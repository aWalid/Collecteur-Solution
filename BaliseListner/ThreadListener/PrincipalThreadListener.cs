using BaliseListner.DataAccess;
using BaliseListner.CollecteurException;
using log4net;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XMLSerializer;
using XMLSerializer.SerializeException;
using BaliseListner.ThreadListener;
using BaliseListner.Generator;
using System.IO;
using System.Runtime.InteropServices;
using Collecteur.Core.Api;
using Collecteur.Core.Events;
using System.Runtime.CompilerServices;
using BaliseListner.ErrorsManager;

namespace BaliseListner.ThreadListener
{
    public static class ThreadLancer
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(ThreadLancer));

        private static Config config;
        private static int sleepTime = 300000;
        private static SyncEvents syncBase;
        private static PrincipalListner principalListner;
        public static void StartCollecteur()
        {
            try
            {

                initialisation();
                logger.Info(DateTime.Now.ToString("HH:mm:ss") + " Starting Principal Listener ...");
                principalListner = PrincipalListner.getPrincipalListner(config);
                principalListner.Start();
                syncBase = PrincipalListner.SyncBase;
                //TrameConsumer.threadTrameSarensReader();
                while (WaitHandle.WaitAny(syncBase.EventArray, sleepTime) != 1)
                {

                    try
                    {
                        principalListner.RefreshData();

                         

                        Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " Refresh All balises finished.");
                        sleepTime = 300000;
                        if (!PrincipalListner.baseActive)
                            syncBase.NewItemEvent.Set();
                        syncBase.BaseEchecThreadEvent.Reset();
                        PrincipalListner.baseActive = true;

                    }
                    catch (DataBaseCommunicationException baseCommException)
                    {

                        logger.Error("Base de données inactive", baseCommException);
                        sleepTime = 2000;
                        principalListner.BDHorsServices();

                    }
                    //Thread.Sleep(sleepTime);
                }

            }
            catch (InitialisationException exp)
            {
                logger.Fatal("Grave : echec de demarage de collecteur,probleme d'inisialisation cause :", exp);
                Console.WriteLine(exp);
            }
            catch (Exception e)
            {
                logger.Fatal("Grave : echec de demarage de collecteur,erreur inconnue cause :", e);
                Console.WriteLine(e);
            }
        }
        private static void initialisation()
        {
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            try
            {
                config = Utils.loadXMLtoObject<Config>(Path.Combine(Environment.CurrentDirectory, "config.xml"));
                try
                {
                    config.setIPAddress(IPAddress.Parse(config.IPAddressString));
                    if (config.Port == 0)
                    {
                        config.Port = 30002;
                        logger.Warn("le numero de port dans le fichier de configuration est invalide .");

                    }
                }
                catch (Exception exp)
                {

                    logger.Warn("l'adresse Ip dans le fichier de configuration est incorrect.", exp);
                    config.setIPAddress(getLocalAdress());

                }

                logger.Info("Le collecteur charge  les parametres depuit config.xml.");

            }
            catch (DeSerializeObjectException e)
            {
                config = new Config("", 30002, true);
                config.setIPAddress(getLocalAdress());
                logger.Warn("Le collecteur charge  les parametres par defaut.", e);
            }

            logger.Info(String.Format("Le collecteur initialisé avec l'adresse IP :{0} et numero de port : {1}", config.getIPAddress(), config.Port));
        }
        private static IPAddress getLocalAdress()
        {
            IPAddress ipAddress = null;

            foreach (NetworkInterface adapter in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (adapter.NetworkInterfaceType == NetworkInterfaceType.Ethernet
                    || adapter.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                {
                    IPInterfaceProperties ipProperties = adapter.GetIPProperties();
                    foreach (UnicastIPAddressInformation address in ipProperties.UnicastAddresses)
                    {
                        if (address.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork
                            && adapter.OperationalStatus == OperationalStatus.Up)
                        {
                            ipAddress = address.Address;
                            break;
                        }
                    }
                }
            }
            if (ipAddress == null)
            {
                logger.Error("adresse null ,impossibe de recupéré une adresse IP valide.");
                throw new InitialisationException(" impossibe de recupéré une adresse IP valide.");
            }
            else
                logger.Info("adresse IP local recupéré " + ipAddress.ToString());
            return ipAddress;
        }
        internal static void DeconnectionBalises()
        {
            try
            {
                syncBase.ExitThreadEvent.Set();
                List<BaliseStat> listStatBalise = new List<BaliseStat>();
                foreach (Connection con in PrincipalListner.connections)
                {
                    if (con.boitier == null || con.boitier.Nisbalise.Length == 0)
                        continue;
                    listStatBalise.Add(new BaliseStat(con.boitier, false, DateTime.Now));
                    con.CloseConnection();
                }
                DataBase.insertStatBalise(listStatBalise);
                PrincipalListner.Logging("FermetureCollecteur", string.Format("insertion des etats de balises dans la base, nbr de etat {0}", listStatBalise.Count));
            }
            catch (Exception e)
            {
                PrincipalListner.Logging("FermetureCollecteur", string.Format("Message d'exp : " + e.Message));
            }

        }
        internal static void StopCollecteur()
        {
            principalListner.SetQuitRequested();
            DeconnectionBalises();
            principalListner.StopListener();
            List<Trajet> dataQueueCopy = new List<Trajet>();
            foreach (Balise balise in PrincipalListner.boitiers.Values)
            {
                if (balise.TrajeEnCours != null /*&& balise.TrajeEnCours.Duree > 0*/)
                {
                    try
                    {
                        if (balise.TrajeEnCours.VMax >= 10 && balise.TrajeEnCours.Duree >= 60)
                            dataQueueCopy.Add(balise.TrajeEnCours);
                    }
                    catch (Exception e)
                    {
                        logger.Error(e.Message);
                    }

                }

            }
            DataBase.insertAllTrajets(dataQueueCopy, dataQueueCopy.Count);
        }
    }
    class PrincipalListner
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(PrincipalListner));
        private static PrincipalListner principalListner;
        public static Dictionary<int, Balise> boitiers;
        public static readonly SyncEvents SyncBase = new SyncEvents();
        public int Version;
        public static bool baseActive = true;

        private static bool _quitRequested = false;
        private static object _syncLock = new object();
        private static AutoResetEvent _waitHandle = new AutoResetEvent(false);

        public static Config config;
        private TcpListener serverSocket;
        private readonly object serverLock = new object();
        // private readonly object lockObject = new object();
        private readonly static Object lockObject = new Object();

        public static ConnectionManager<Connection> connections = new ConnectionManager<Connection>();

        public static PrincipalListner getPrincipalListner(Config conf)
        {
            if (principalListner == null)
            {
                config = conf;
                principalListner = new PrincipalListner();
                //TrameSerializer.deserialiseAllNotTransfferedTrame();
                logger.Debug("Nouvelle instance de PrincipalListner est cree. ");
            }

            logger.Info("Une instance de PrincipalListner est recupere .");
            return principalListner;

        }

        public void SetQuitRequested()
        {
            lock (_syncLock)
            {
                _quitRequested = true;
            }
        }
        private PrincipalListner()
        {
            boitiers = new Dictionary<int, Balise>();
            Version = DataBase.GetBalise(false, 0, ref boitiers);
            // A supprimer
            foreach (Balise balise in boitiers.Values)
            {
                Trajet trajet;
                try
                {
                    string path = @"temp\Trajet\" + balise.Nisbalise + ".bin";
                    if (!File.Exists(Path.Combine(Environment.CurrentDirectory, path)))
                        continue;
                    trajet = Utils.loadBinarytoObject<Trajet>(Path.Combine(Environment.CurrentDirectory, path));
                    if (trajet != null)
                    {
                        balise.TrajeEnCours = new Trajet(trajet);
                        Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " Chargement du Trajet pour la Balise: {0} distance: {1}.", balise.Nisbalise, trajet.Distance);
                        logger.Info("Chargement du Trajet pour la Balise : " + balise.Nisbalise + " distance : " + trajet.Distance);
                        File.Delete(Path.Combine(Environment.CurrentDirectory, path));
                    }
                }
                catch (DeSerializeObjectException e)
                {
                    //   Console.WriteLine("impossible de deserialise Trajet Balise : " + balise.Nisbalise, e);
                    logger.Warn("impossible de deserialise Trajet Balise : " + balise.Nisbalise, e);
                }

            }

        }

        public void RefreshData()
        {

            Version = DataBase.GetBalise(true, Version, ref boitiers);

        }



        public void Start()
        {

            Console.Write(DateTime.Now.ToString("HH:mm:ss") + " Démarrage du Collecteur... ");
            SetupServerSocket();

            serverSocket.BeginAcceptTcpClient(new AsyncCallback(AcceptCallback), serverSocket);
            logger.Debug("Socket server demarre avec succe : ");
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " OK. en Ecoute.");


        }
        public void StopListener()
        {
            _waitHandle.WaitOne();
            serverSocket.Stop();
        }
        private void AcceptCallback(IAsyncResult result)
        {
            if (!baseActive)
                WaitHandle.WaitAny(SyncBase.EventArray);
            if (_quitRequested)
            {
                _waitHandle.Set();
                return;
            }
            try
            {

                TcpClient clientSocket = serverSocket.EndAcceptTcpClient(result);
                clientSocket.NoDelay = true; //Desactiver l'algorithme de Nagle.
                Connection connection = new Connection(clientSocket, config.Debug);

                lock (connections)
                {
                    connections.Add(connection);
                }

                connection.StartCommunication();

            }
            catch (SocketException exc)
            {
                Console.WriteLine("Socket exception: " + exc.SocketErrorCode);
                Logging("SocketException", exc.Message);
            }
            catch (Exception exc)
            {
                Console.WriteLine("Exception: " + exc);
                Logging("AcceptCallback", exc.Message);
            }

            serverSocket.BeginAcceptTcpClient(new AsyncCallback(AcceptCallback), result.AsyncState);
        }

        private void SetupServerSocket()
        {

            IPEndPoint myEndpoint = new IPEndPoint(config.getIPAddress(), config.Port);
            Console.WriteLine("Server Ip Adresse: " + config.getIPAddress() + ", Port: " + config.Port.ToString());

            // Create the socket, bind it, and start listening
            serverSocket = new TcpListener(myEndpoint);
            //  serverSocket.Blocking = false;

            //serverSocket.Bind(myEndpoint);
            //serverSocket.Listen((int)SocketOptionName.MaxConnections);
            serverSocket.Start(10000);


            ThreadPool.QueueUserWorkItem(checkConnectionThread);

            Console.WriteLine("Server Ip Adresse: " + config.getIPAddress() + ", Port: " + config.Port.ToString());

        }

        private void checkConnectionThread(object lockObject)
        {
            while (true)
            {
                Thread.Sleep(1800000);

                int NbrCxnActive = 0;
                int NbrCxnDeleted = 0;
                lock (connections)
                {
                    int nbconnection = connections.Count;
                    int i = 0;
                    while (i < nbconnection)
                    {
                        try
                        {
                            Connection con = connections.ElementAt(i);
                            if (con == null)
                            {
                                connections.RemoveAt(i);
                                NbrCxnDeleted++;
                                nbconnection--;
                                continue;
                            }
                            if (!con.IsConnected())
                            {
                                con.CloseConnection();
                                connections.RemoveAt(i);
                                NbrCxnDeleted++;
                                nbconnection--;
                                continue;

                            }
                            i++;

                        }
                        catch (Exception e)
                        {
                            Logging("ConnectionManager", string.Format("Exception dans la Vérification de connexion ,trace d'erreur : ", e.Message));
                        }

                    }

                    NbrCxnActive = connections.Count;
                }
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " Le processus de garbage des connections est terminé, nbr de connections supprimées: {0}, nbr de conections activees: {1}.", NbrCxnDeleted, NbrCxnActive);
                Logging("ConnectionsGarbage", string.Format("Le nbr de connections supprimées: {0}, nbr de conections activees: {1}.", NbrCxnDeleted, NbrCxnActive));
            }
        }
        internal void BDHorsServices()
        {
            SyncBase.NewItemEvent.Reset();
            if (baseActive)
                SyncBase.BaseEchecThreadEvent.Set();
            baseActive = false;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private static void Logging(String erreur, String message, Exception exp)
        {
            String path = "Log\\" + DateTime.Now.ToString("yyyy-MM-dd");
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                using (StreamWriter sw = File.AppendText(path + "\\Trace_" + erreur + ".log"))
                {
                    sw.WriteLine(DateTime.Now + ": " + message);
                    sw.WriteLine(exp.Message);
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception dans l'ouverture du fichier Log: {0}", e.Message);
            }

        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void Logging(String erreur, String message)
        {

            String path = "Log\\" + DateTime.Now.ToString("yyyy-MM-dd");
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                using (StreamWriter sw = File.AppendText(path + "\\Trace_" + erreur + ".log"))
                {
                    sw.WriteLine(DateTime.Now + ": " + message);
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception dans l'ouverture du fichier Log: {0}", e.Message);
            }
        }


    }

}
