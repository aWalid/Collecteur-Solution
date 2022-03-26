
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
using System.IO;

namespace BaliseListner.ThreadListener
{
   public static class  ThreadLancer
    {
       private static readonly ILog logger = LogManager.GetLogger(typeof(ThreadLancer));
      
       private static Config config;
       private static int sleepTime = 300000;
       public static void StartCollecteur()
       {
           try
           {
               Config configCollecteur30004 = new Config(getLocalAdress(),30004);
               Config configCollecteur30006 = new Config(getLocalAdress(), 30006);
               PrincipalListner principalListnerFor30004 = PrincipalListner.getPrincipalListner(configCollecteur30004);
               principalListnerFor30004.Start();
               PrincipalListner principalListnerFor30006 = PrincipalListner.getPrincipalListner(configCollecteur30006);
               principalListnerFor30006.Start();
               while (Console.ReadLine()!="exit") 
               {
                 
                 
                   //Thread.Sleep(sleepTime);
               } 
              
           }
           catch (InitialisationException exp)
           {
               logger.Fatal("Grave : echec de demarage de collecteur,probleme d'inisialisation cause :", exp);
               Console.WriteLine(exp);

           }catch (Exception e) {
               logger.Fatal("Grave : echec de demarage de collecteur,erreur inconnue cause :", e);
               Console.WriteLine(e);
               
           }
           
         
       }
       
       private static IPAddress getLocalAdress()
       {
           IPAddress ipAddress=null;


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
           if (ipAddress == null) { 
               logger.Error("adresse null ,impossibe de recupéré une adresse IP valide.");
               throw  new InitialisationException(" impossibe de recupéré une adresse IP valide.");
           }
           else
           logger.Info("adresse IP local recupéré "+ipAddress.ToString());
           return ipAddress;
       }

    }
    class PrincipalListner
   {
       private static readonly ILog logger = LogManager.GetLogger(typeof(PrincipalListner));
       private static PrincipalListner principalListner;
       private  Config configObject;
       private Socket serverSocket;
       private  readonly object serverLock = new object();
       private  List<Connection> connections = new List<Connection>();
       public static PrincipalListner getPrincipalListner(Config conf)
       {


           principalListner = new PrincipalListner(conf);
               logger.Debug("Nouvelle instance de PrincipalListner est cree. ");
         

           logger.Info("Une instance de PrincipalListner est recupere .");
           return principalListner;
           
       }

       private PrincipalListner(Config conf)
       {
           configObject = conf;
       }
      
       public void Start()
       {

          Console.Write("Démarrage du Collecteur... ");
           
               SetupServerSocket();
               serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), serverSocket);
               logger.Debug("Socket server demarre avec succe : ");
               Console.WriteLine("OK. en Ecoute.");
               
           
       }

       private void AcceptCallback(IAsyncResult result)
       {
          

           Console.WriteLine("Accept!");
           Connection connection = new Connection(((Socket)result.AsyncState).EndAccept(result), configObject.Port);
           try
           {
               // fin Accept
               Console.WriteLine(result.ToString());
               lock (connections) connections.Add(connection);
               Console.WriteLine("Nouvelle connexion de: " + connection.ToString());
               if (connection != null)
                   connection.StartCommunication();
               // se remettre en ecoute pour un nouveau Accept
               serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), result.AsyncState);
           }
           catch (SocketException exc)
           {
               
               Console.WriteLine("Socket exception: " + exc.SocketErrorCode);
               logger.Error("erreur dans la comminication ,cause: ", exc);
           }
           catch (Exception exc)
           {
              
               Console.WriteLine("Exception: " + exc);
               logger.Error("erreur inconnue ,cause: ", exc);
           }
       }
       

     
      
         private  void SetupServerSocket()
        {
            
                IPEndPoint myEndpoint = new IPEndPoint(configObject.getIPAddress(), configObject.Port);

                // Create the socket, bind it, and start listening
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                serverSocket.Blocking = false;

                serverSocket.Bind(myEndpoint);
                //serverSocket.Listen((int)SocketOptionName.MaxConnections);
                serverSocket.Listen(1000);
                Console.WriteLine("Server Ip Adresse: " + configObject.getIPAddress() + ", Port: " + configObject.Port.ToString());

            }






       
   }
}
