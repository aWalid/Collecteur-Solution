
using BaliseListner.DataAccess;
using BaliseListner.Generator;
using BaliseListner.ThreadDBAccess;
using BaliseListner.ThreadListener;
using Collecteur.Core.Api;
using Collecteur.Core.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OldCollecteur
{
    public static class OLDModelGeneratorProcessor
    {
        public static int nbrAdded;
        public static int nbrTrameRealAdded;
        private static int nbrTrajetAdded;
        private static int nbrStatAdded;
        private static int nbrcapteurAdded;
        private static int nbrcapteurSondeTemperateurAdded;
        private static int nbrTrameBruteAdded;
        private static int nbrTramePOIAdded;
        private static int nbrTrameKeyAdded;
        private static int nbrTrameMissionAdded;
        private static int nbrTrameSarensAdded;
        private static int nbrTrailerIDAdded;

        private static int nbrTrameEcoAdded;
        private static int nbrTrameSondeAdded;
        static OLDModelGeneratorProcessor()
        {

            nbrAdded = 0;
            nbrStatAdded = 0;
            nbrTrajetAdded = 0;
            nbrTrameRealAdded = 0;
            nbrcapteurAdded = 0;
            nbrTramePOIAdded = 0;
            nbrTrameBruteAdded = 0;
            nbrTrameKeyAdded = 0;
            nbrTrailerIDAdded = 0;
            nbrTrameMissionAdded = 0;
            nbrTrameSarensAdded = 0;

            

            Thread threadTrameConsomater = new Thread(ComsommeTrame);
            threadTrameConsomater.Start();
            // à arréter inutilisable
            Thread threadStatConsomater = new Thread(consommeStat);
            threadStatConsomater.Start();
            Thread threadTrameRealConsomater = new Thread(consommeTrameReal);
            threadTrameRealConsomater.Start();
            Thread threadTrajetConsomater = new Thread(consommeTrajet);
            threadTrajetConsomater.Start();
            Thread threadCapteursConsomer = new Thread(consommeCapteurs);
            threadCapteursConsomer.Start();
            // à arréter inutilisable
            Thread threadTrameBruteConsomer = new Thread(consommeTRameBrute);
            threadTrameBruteConsomer.Start();
            Thread threadTrameDalasConsomer = new Thread(consommeTRameDalas);
            threadTrameDalasConsomer.Start();
            // à arréter si possible
            Thread threadTramePOIConsomer = new Thread(consommeTramePOI);
            threadTramePOIConsomer.Start();
            Thread threadTrameMissionConsomer = new Thread(consommeTrameMission);
            threadTrameMissionConsomer.Start();
            Thread threadTrailerIDConsomer = new Thread(consommeTrailerID);
            threadTrailerIDConsomer.Start();

            Thread threadTrameSarensConsomer = new Thread(consommeTrameSarens);
            threadTrameSarensConsomer.Start();

            /*Eco-Conduite 20200119 par oualid*/
            nbrTrameEcoAdded = 0;
            Thread threadTrameEcoConsomer = new Thread(ConsommeTrameEco);
            threadTrameEcoConsomer.Start();


            /*Sonde 20211026 par oualid*/
            nbrTrameSondeAdded = 0;
            Thread threadTrameSondeConsomer = new Thread(ConsommeTrameSonde);
            threadTrameSondeConsomer.Start();

            nbrcapteurSondeTemperateurAdded = 0;
            Thread threadCapteursSondeTemperateurConsomer = new Thread(consommeCapteursSondeTemperateur);
            threadCapteursSondeTemperateurConsomer.Start();

           

        }

        private static void consommeTrameSarens()
        {
            while (nbrTrameSarensAdded > 0 || WaitHandle.WaitAny(trameSarensConsomeSyncEvents.EventArray) != 1)
            {
                Queue<TrameReal> dataQueueCopy = null;

                lock (((ICollection)SarensQueue).SyncRoot)
                {
                    try
                    {
                        dataQueueCopy = new Queue<TrameReal>(SarensQueue.ToArray());
                        SarensQueue.Clear();
                        if (PrincipalListner.config.Debug)
                            Console.WriteLine("Trames Sarens consomés nbr :{0}.", nbrTrameSarensAdded);
                        nbrTrameSarensAdded = 0;
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Buffer n'a pas pu lire les données.");
                    }
                }
                DataBase.insertTrameSarens(dataQueueCopy.ToList());
            }
        }

        private static void consommeTrameMission()
        {
            while (nbrTrameMissionAdded > 0 || WaitHandle.WaitAny(TrameMissionConsomeSyncEvents.EventArray) != 1)
            {

                Queue<TrameReal> dataQueueCopy = null;

                lock (((ICollection)trameMissionQueue).SyncRoot)
                {
                    try
                    {
                        dataQueueCopy = new Queue<TrameReal>(trameMissionQueue.ToArray());
                        trameMissionQueue.Clear();
                        if (PrincipalListner.config.Debug)
                            Console.WriteLine("Trame Missions consomé nbr :{0}.", nbrTrameMissionAdded);
                        nbrTrameMissionAdded = 0;
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Buffer n'a pas pu lire les données.");
                    }
                }
                MissionInsertLancer.go(dataQueueCopy.ToList());
            }
        }
        private static void consommeTramePOI()
        {
            while (nbrTramePOIAdded > 0 || WaitHandle.WaitAny(TramePOIConsomeSyncEvents.EventArray) != 1)
            {

                Queue<TrameReal> dataQueueCopy = null;

                lock (((ICollection)tramePOIQueue).SyncRoot)
                {
                    try
                    {
                        dataQueueCopy = new Queue<TrameReal>(tramePOIQueue.ToArray());
                        tramePOIQueue.Clear();
                        if (PrincipalListner.config.Debug)
                            Console.WriteLine("Trame POIs consomé nbr :{0}.", nbrTramePOIAdded);
                        nbrTramePOIAdded = 0;
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Buffer n'a pas pu lire les données.");
                    }
                }
                DataBase.insertAllPOIs(dataQueueCopy.ToList());
            }
        }

        private static void addTramesMission(TrameReal trame)
        {
            if (trame == null)
                return;

            lock (((ICollection)trameMissionQueue).SyncRoot)
            {

                trameMissionQueue.Enqueue(trame);
                TrameMissionConsomeSyncEvents.NewItemEvent.Set();
                nbrTrameMissionAdded++;
            }

        }
        private static void addTramesPOIs(TrameReal trame)
        {
            if (trame == null)
                return;

            lock (((ICollection)tramePOIQueue).SyncRoot)
            {

                tramePOIQueue.Enqueue(trame);
                TramePOIConsomeSyncEvents.NewItemEvent.Set();
                nbrTramePOIAdded++;
            }

        }
        private static void consommeCapteursSondeTemperateur()
        {
            while (nbrcapteurSondeTemperateurAdded > 0 || WaitHandle.WaitAny(trameCapteursSondeTemperateurConsomeSyncEvents.EventArray) != 1)
            {
                Queue<TrameReal> SortedQueue = null;

                lock (((ICollection)trameCapteursSondeTemperateur).SyncRoot)
                {
                    SortedQueue = new Queue<TrameReal>(trameCapteursSondeTemperateur.OrderBy(trameReal => trameReal.Temps).ToArray());
                    trameCapteursSondeTemperateur.Clear();
                    if (PrincipalListner.config.Debug)
                        Console.WriteLine("Trame Capteurs sonde temperateur consomé nbr :{0}.", nbrcapteurAdded);
                    nbrcapteurSondeTemperateurAdded = 0;
                }

                try
                {
                    int nbritem = SortedQueue.Count; //int n = 0; bool exit = false;
                    //int i = 0; bool reussie = false;
                    int max = 5000;

                    while (Math.Min(nbritem, max) > 0)
                    {
                        int min = Math.Min(nbritem, max); // nombre de tentatives
                        try
                        {
                            if (DataBase.insertAllCapteursSondeTemperateur(SortedQueue.ToList().GetRange(0, min)))
                            {
                                TrameReal removedTrame = null;
                                for (int i = 0; i < min; i++)
                                    removedTrame = new TrameReal(SortedQueue.Dequeue());
                                //.RemoveRange(0, min);
                                nbritem -= min;
                                //Console.WriteLine("Tbdillllllllllllllllllllllllllllllllllllllllllll");
                            }
                            else
                            {
                                break;
                            }
                        }
                        catch (ArgumentOutOfRangeException are)
                        {
                            Console.WriteLine("Exception List Capteurs Creation: " + are.Message);
                            break;
                        }
                        catch (ArgumentException ae)
                        {
                            Console.WriteLine("Exception List Capteurs Creation: " + ae.Message);
                            break;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Exception List Capteurs Creation: " + e.Message);
                            break;
                        }
                        finally
                        {
                            Thread.Sleep(2000);
                        }

                    }

                }
                catch (Exception)
                {
                    Console.WriteLine("Buffer n'a pas pu lire les données.");
                }
                finally
                {  
                   
                    if (SortedQueue.Count > 0)
                        OLDModelGeneratorProcessor.addTramesCapteursSondeTemperateurNotInserted(SortedQueue.ToList());
                }
            }
        }


        private static void consommeCapteurs()
        {
            while (nbrcapteurAdded > 0 || WaitHandle.WaitAny(trameCapteursConsomeSyncEvents.EventArray) != 1)
            {
                Queue<TrameReal> SortedQueue = null;

                lock (((ICollection)trameCapteurs).SyncRoot)
                {
                    SortedQueue = new Queue<TrameReal>(trameCapteurs.OrderBy(trameReal => trameReal.Temps).ToArray());
                    trameCapteurs.Clear();
                    if (PrincipalListner.config.Debug)
                        Console.WriteLine("Trame Capteurs consomé nbr :{0}.", nbrcapteurAdded);
                    nbrcapteurAdded = 0;
                }

                try
                {
                    int nbritem = SortedQueue.Count; //int n = 0; bool exit = false;
                    //int i = 0; bool reussie = false;
                    int max = 5000;

                    while (Math.Min(nbritem, max) > 0)
                    {
                        int min = Math.Min(nbritem, max); // nombre de tentatives
                        try
                        {
                            if (DataBase.insertAllCapteurs(SortedQueue.ToList().GetRange(0, min)))
                            {
                                TrameReal removedTrame = null;
                                for (int i = 0; i < min; i++)
                                    removedTrame = new TrameReal(SortedQueue.Dequeue());
                                //.RemoveRange(0, min);
                                nbritem -= min;
                            }
                            else
                            {
                                break;
                            }
                        }
                        catch (ArgumentOutOfRangeException are)
                        {
                            Console.WriteLine("Exception List Capteurs Creation: " + are.Message);
                            break;
                        }
                        catch (ArgumentException ae)
                        {
                            Console.WriteLine("Exception List Capteurs Creation: " + ae.Message);
                            break;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Exception List Capteurs Creation: " + e.Message);
                            break;
                        }
                        finally
                        {
                            Thread.Sleep(2000);
                        }

                    }

                }
                catch (Exception)
                {
                    Console.WriteLine("Buffer n'a pas pu lire les données.");
                }
                finally
                {
                    if (SortedQueue.Count > 0)
                        OLDModelGeneratorProcessor.addTramesCapteursNotInserted(SortedQueue.ToList());
                }
            }
        }
        private static void consommeTRameDalas()
        {
            while (nbrTrameKeyAdded > 0 || WaitHandle.WaitAny(IdentDalasConsomeSyncEvents.EventArray) != 1)
            {

                IdentDalas dataQueueCopy = null;

                lock (((ICollection)queueIdentDalas).SyncRoot)
                {
                    try
                    {

                        dataQueueCopy = queueIdentDalas.Dequeue();
                        if (PrincipalListner.config.Debug)
                            Console.WriteLine("Trame Dalas Key consomé nbr non consumé :{0}.", nbrTrameKeyAdded - 1);
                        nbrTrameKeyAdded--;
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Buffer n'a pas pu lire les données.");
                    }
                }
                if (dataQueueCopy != null)
                    DataBase.insertDALASKey(dataQueueCopy);
            }
        }

        private static void consommeTrailerID()
        {
            while (nbrTrailerIDAdded > 0 || WaitHandle.WaitAny(TrailerIDConsomeSyncEvents.EventArray) != 1)
            {

                TrailerID IDTrailerCopy = null;

                lock (((ICollection)queueTrailerID).SyncRoot)
                {
                    try
                    {
                        IDTrailerCopy = new TrailerID(queueTrailerID.Dequeue());
                        nbrTrailerIDAdded--;
                        if (PrincipalListner.config.Debug)
                            Console.WriteLine("Nbr Trailer ID Restant: {0}.", nbrTrailerIDAdded);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Trailer ID File n'a pas pu lire les données.");
                    }
                }
                if (IDTrailerCopy != null)
                    DataBase.insertTrailerID(IDTrailerCopy);

            }
        }


        public static void addTramesCapteursNotInserted(List<TrameReal> listTrames)
        {
            if (listTrames == null || listTrames.Count == 0)
                return;

            lock (((ICollection)trameCapteurs).SyncRoot)
            {
                foreach (TrameReal trame in listTrames)
                    trameCapteurs.Enqueue(trame);
                trameCapteursConsomeSyncEvents.NewItemEvent.Set();
                nbrcapteurAdded = nbrcapteurAdded + listTrames.Count;
            }

        }
        public static void addTramesCapteursSondeTemperateurNotInserted(List<TrameReal> listTrames)
        {
            if (listTrames == null || listTrames.Count == 0)
                return;

            lock (((ICollection)trameCapteursSondeTemperateur).SyncRoot)
            {
                foreach (TrameReal trame in listTrames)
                    trameCapteursSondeTemperateur.Enqueue(trame);
                trameCapteursSondeTemperateurConsomeSyncEvents.NewItemEvent.Set();
                nbrcapteurSondeTemperateurAdded = nbrcapteurSondeTemperateurAdded + listTrames.Count;
            }

        }
        private static void addTramesCapteurs(TrameReal trame)
        {
            if (trame == null)
                return;

            lock (((ICollection)trameCapteurs).SyncRoot)
            {

                trameCapteurs.Enqueue(trame);
                trameCapteursConsomeSyncEvents.NewItemEvent.Set();
                nbrcapteurAdded++;
            }

        }

        private static void addTramesCapteursSondeTemperateur(TrameReal trame)
        {
            if (trame == null)
                return;

            lock (((ICollection)trameCapteursSondeTemperateur).SyncRoot)
            {

                trameCapteursSondeTemperateur.Enqueue(trame);
                trameCapteursSondeTemperateurConsomeSyncEvents.NewItemEvent.Set();
                nbrcapteurSondeTemperateurAdded++;
            }

        }

        public static void addTramesConcoxCapteurs(TrameReal tr)
        {
            addTramesCapteurs(tr);
        }

        public static void addTramesIdentDalas(IdentDalas trame)
        {
            if (trame == null)
                return;

            lock (((ICollection)queueIdentDalas).SyncRoot)
            {

                queueIdentDalas.Enqueue(trame);
                IdentDalasConsomeSyncEvents.NewItemEvent.Set();
                nbrTrameKeyAdded++;
            }

        }

        public static void addTramesTrailerID(TrailerID trame)
        {
            if (trame == null)
                return;

            lock (((ICollection)queueTrailerID).SyncRoot)
            {

                queueTrailerID.Enqueue(trame);
                TrailerIDConsomeSyncEvents.NewItemEvent.Set();
                nbrTrailerIDAdded++;
            }

        }

        private static Dictionary<String, TrameReal> queueTrameRealUpdater = new Dictionary<String, TrameReal>();
        private static Queue<TrameReal> queue = new Queue<TrameReal>();
        private static Queue<TrameEco> queueEco = new Queue<TrameEco>();
        private static Queue<TrameSonde> queueSonde = new Queue<TrameSonde>();

        private static Queue<TrameReal> SarensQueue = new Queue<TrameReal>();
        private static Queue<IdentDalas> queueIdentDalas = new Queue<IdentDalas>();
        // File des Trailer ID
        private static Queue<TrailerID> queueTrailerID = new Queue<TrailerID>();
        private static Queue<BaliseStat> statBalise = new Queue<BaliseStat>();
        private static Queue<TrameReal> trameCapteurs = new Queue<TrameReal>();   
        private static Queue<TrameReal> trameCapteursSondeTemperateur = new Queue<TrameReal>();
        private static Queue<Trame> trameBruteQueue = new Queue<Trame>();
        private static Queue<TrameReal> tramePOIQueue = new Queue<TrameReal>();
        private static Queue<TrameReal> trameMissionQueue = new Queue<TrameReal>();
        private static Queue<Trajet> Trajets = new Queue<Trajet>();

        private static SyncEvents statConsomeSyncEvents = new SyncEvents();
        private static SyncEvents TrajetSyncEvents = new SyncEvents();
        private static SyncEvents trameConsomeSyncEvents = new SyncEvents();
        private static SyncEvents trameEcoConsomeSyncEvents = new SyncEvents();
        private static SyncEvents trameSondeConsomeSyncEvents = new SyncEvents();
        private static SyncEvents trameRealUpdaterConsomeSyncEvents = new SyncEvents();
        private static SyncEvents trameCapteursConsomeSyncEvents = new SyncEvents();   
        private static SyncEvents trameCapteursSondeTemperateurConsomeSyncEvents = new SyncEvents();
        private static SyncEvents trameSarensConsomeSyncEvents = new SyncEvents();
        private static SyncEvents trameTrameBruteSyncEvents = new SyncEvents();
        private static SyncEvents IdentDalasConsomeSyncEvents = new SyncEvents();
        private static SyncEvents TramePOIConsomeSyncEvents = new SyncEvents();
        private static SyncEvents TrameMissionConsomeSyncEvents = new SyncEvents();
        // Trailer ID Evenement 
        private static SyncEvents TrailerIDConsomeSyncEvents = new SyncEvents();

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void addEcoTrame(TrameEco teco)
        {

            if (teco == null)
                return;


            lock (((ICollection)queueEco).SyncRoot)
            {
                queueEco.Enqueue(teco);
                trameEcoConsomeSyncEvents.NewItemEvent.Set();
                nbrTrameEcoAdded++;
            }



        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void addSondeTrame(TrameSonde teso)
        {

            if (teso == null)
                return;


            lock (((ICollection)queueSonde).SyncRoot)
            {
                queueSonde.Enqueue(teso);
                trameSondeConsomeSyncEvents.NewItemEvent.Set();
                nbrTrameSondeAdded++;
            }



        }
        
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void addTrame(TrameReal trame , bool notSondetemperateur = true )
        {
            TrameReal trameRealAdded = null;
            if (trame == null)
                return;

            addTramesPOIs(trame);

            if (notSondetemperateur)
                addTramesCapteurs(trame);
            else
                addTramesCapteursSondeTemperateur(trame);

            addTramesMission(trame);
            lock (((ICollection)queue).SyncRoot)
            {
                queue.Enqueue(trame);
                trameConsomeSyncEvents.NewItemEvent.Set();
                nbrAdded++;
            }

            if (trame.Balise.TrajeEnCours == null)
            {
                trame.Balise.TrajeEnCours = new Trajet(trame);
            }
            else
                if (trame.Balise.TrajeEnCours.UpdateTrajet(trame, ref trameRealAdded))
            {
                if (trame.Balise.TrajeEnCours.VMax >= 10 && trame.Balise.TrajeEnCours.Duree >= 60)
                    addTrajet((Trajet)trame.Balise.TrajeEnCours.Clone());
                trame.Balise.TrajeEnCours.initializeTrajet(trame);

            }
            else if (trameRealAdded != null)
            {
                addTramesPOIs(trameRealAdded);
                lock (((ICollection)queue).SyncRoot)
                {
                    queue.Enqueue(trameRealAdded);
                    trameConsomeSyncEvents.NewItemEvent.Set();
                    nbrAdded++;
                }
            }

        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void addTrameMidNightSplitTrajet(TrameReal trame)
        {
            TrameReal trameRealAdded = null;
            if (trame == null)
                return;

            addTramesPOIs(trame);
            addTramesCapteurs(trame);
            addTramesMission(trame);
            lock (((ICollection)queue).SyncRoot)
            {
                queue.Enqueue(trame);
                trameConsomeSyncEvents.NewItemEvent.Set();
                nbrAdded++;
            }

            if (trame.Balise.TrajeEnCours == null)
            {
                trame.Balise.TrajeEnCours = new Trajet(trame);
            }
            else
                if (trame.Balise.TrajeEnCours.UpdateTrajetMidNightSplitTrajet(trame, ref trameRealAdded))
            {
                if (trame.Balise.TrajeEnCours.VMax >= 10 && trame.Balise.TrajeEnCours.Duree >= 60)
                    addTrajet((Trajet)trame.Balise.TrajeEnCours.Clone());
                trame.Balise.TrajeEnCours.initializeTrajet(trame);

            }
            else if (trameRealAdded != null)
            {
                addTramesPOIs(trameRealAdded);
                lock (((ICollection)queue).SyncRoot)
                {
                    queue.Enqueue(trameRealAdded);
                    trameConsomeSyncEvents.NewItemEvent.Set();
                    nbrAdded++;
                }
            }

        }
        public static void addTrameReal(TrameReal trame)
        {
            if (trame == null)
                return;

            lock (((ICollection)queueTrameRealUpdater).SyncRoot)
            {
                try
                {
                    queueTrameRealUpdater.Add(trame.Balise.Nisbalise, trame);

                }
                catch (ArgumentException)
                {
                    queueTrameRealUpdater.Remove(trame.Balise.Nisbalise);
                    queueTrameRealUpdater.Add(trame.Balise.Nisbalise, trame);
                }

                trameRealUpdaterConsomeSyncEvents.NewItemEvent.Set();
                nbrTrameRealAdded++;
            }

        }
        private static void addTrajet(Trajet trajet)
        {
            if (trajet == null)
                return;

            lock (((ICollection)Trajets).SyncRoot)
            {
                Trajets.Enqueue(trajet);
                TrajetSyncEvents.NewItemEvent.Set();
                nbrTrajetAdded++;
            }

        }
        public static void addNonInsetedTrajet(List<Trajet> listLrajets)
        {
            if (listLrajets == null || listLrajets.Count == 0)
                return;

            lock (((ICollection)Trajets).SyncRoot)
            {
                foreach (Trajet trajet in listLrajets)
                    Trajets.Enqueue(trajet);
                TrajetSyncEvents.NewItemEvent.Set();
                nbrTrajetAdded = nbrTrajetAdded + listLrajets.Count;
            }

        }


        public static void addNonInsetedTrame(List<TrameReal> listTrames)
        {
            if (listTrames == null || listTrames.Count == 0)
                return;

            lock (((ICollection)queue).SyncRoot)
            {
                foreach (TrameReal trame in listTrames)
                    queue.Enqueue(trame);
                nbrAdded = nbrAdded + listTrames.Count;
                trameConsomeSyncEvents.NewItemEvent.Set();
            }

        }

        public static void addNonInsetedTrameEco(List<TrameEco> listTrames)
        {
            if (listTrames == null || listTrames.Count == 0)
                return;

            lock (((ICollection)queueEco).SyncRoot)
            {
                foreach (TrameEco trame in listTrames)
                    queueEco.Enqueue(trame);
                nbrTrameEcoAdded = nbrTrameEcoAdded + listTrames.Count;
                trameEcoConsomeSyncEvents.NewItemEvent.Set();

            }

        }

        public static void addNonInsetedTrameSonde(List<TrameSonde> listTrames)
        {
            if (listTrames == null || listTrames.Count == 0)
                return;

            lock (((ICollection)queueSonde).SyncRoot)
            {
                foreach (TrameSonde trame in listTrames)
                    queueSonde.Enqueue(trame);
                nbrTrameSondeAdded = nbrTrameSondeAdded + listTrames.Count;
                trameSondeConsomeSyncEvents.NewItemEvent.Set();
                trameSondeConsomeSyncEvents.NewItemEvent.Set();

            }

        }
        public static void addFirstTrame(Trame trame)
        {

            addStatBalise(new BaliseStat(trame.balise, true, System.DateTime.Now));

        }

        public static void addDeconnect(Balise boitier)
        {
            if (boitier == null || boitier.Nisbalise == null || boitier.Nisbalise.Length == 0)
                return;
            if (PrincipalListner.config.Debug)
                Console.WriteLine("la Balise {0} vient de se déconnecter", boitier.Nisbalise);
            try
            {
                if (PrincipalListner.connections.findConnectionByNISBalise(boitier))
                    return;
            }
            catch (Exception e)
            {
                Console.WriteLine("Boitier: " + boitier.Nisbalise + "- Exception dans la recherche des connection addDeconnect() : " + e.Message);
                return;
            }

            addStatBalise(new BaliseStat(boitier, false, System.DateTime.Now));
        }
        public static void addConnect(Balise boitier)
        {
            if (boitier == null || boitier.Nisbalise == null || boitier.Nisbalise.Length == 0)
                return;
            if (PrincipalListner.config.Debug)
                Console.WriteLine("la Balise {0} vient de se Connecter.", boitier.Nisbalise);
            try
            {
                if (PrincipalListner.connections.findConnectionByNISBalise(boitier))
                    return;
            }
            catch (Exception e)
            {
                Console.WriteLine("Boitier: " + boitier.Nisbalise + "- Exception dans la recherche des connection addConnect() : " + e.Message);
                return;
            }

            addStatBalise(new BaliseStat(boitier, true, System.DateTime.Now));
        }
        private static void ComsommeTrame()
        {
            while (nbrAdded > 0 || WaitHandle.WaitAny(trameConsomeSyncEvents.EventArray) != 1)
            {

                Queue<TrameReal> dataQueueCopy = null;

                lock (((ICollection)queue).SyncRoot)
                {
                    try
                    {
                        dataQueueCopy = new Queue<TrameReal>(queue.ToArray());
                        queue.Clear();
                        if (PrincipalListner.config.Debug)
                            Console.WriteLine("Trame consomé nbr :{0}.", nbrAdded);
                        nbrAdded = 0;
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Buffer n'a pas pu lire les données.");
                    }
                }
                DataBase.insertAllTrame(dataQueueCopy.ToList());
                //TrameInsertThread trameInserted = new TrameInsertThread(dataQueueCopy.ToList());
                //ThreadPool.QueueUserWorkItem(trameInserted.insertAllTrame);

            }
        }


        private static void ConsommeTrameEco()
        {
            while (nbrTrameEcoAdded > 0 || WaitHandle.WaitAny(trameEcoConsomeSyncEvents.EventArray) != 1)
            {

                Queue<TrameEco> dataQueueCopy = null;

                lock (((ICollection)queueEco).SyncRoot)
                {
                    try
                    {
                        dataQueueCopy = new Queue<TrameEco>(queueEco.ToArray());
                        queueEco.Clear();
                        if (PrincipalListner.config.Debug)
                            Console.WriteLine("Trame eco consomé nbr :{0}.", nbrTrameEcoAdded);
                        nbrTrameEcoAdded = 0;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("insertAllTrameEcoErreur:\t" + ex.Message);
                        DataBase.Logging("insertAllTrameEcoErreur", ex.ToString());
                    }
                }

                //try
                //{
                DataBase.insertAllTrameEco(dataQueueCopy.ToList());
                //}
                //catch (Exception ex)
                //{
                //    Console.WriteLine(ex.Message);
                //    DataBase.Logging("insertAllTrameEcoErreur", "1: " + ex.ToString());
                //}
                //TrameInsertThread trameInserted = new TrameInsertThread(dataQueueCopy.ToList());
                //ThreadPool.QueueUserWorkItem(trameInserted.insertAllTrame);

            }
        }
        //
        //  
        private static void ConsommeTrameSonde()
        {
            while (nbrTrameSondeAdded > 0 || WaitHandle.WaitAny(trameSondeConsomeSyncEvents.EventArray) != 1)
            {

                Queue<TrameSonde> dataQueueCopy = null;

                lock (((ICollection)queueSonde).SyncRoot)
                {
                    try
                    {
                        dataQueueCopy = new Queue<TrameSonde>(queueSonde.ToArray());
                        queueSonde.Clear();
                        if (PrincipalListner.config.Debug)
                            Console.WriteLine("Trame sonde consomé nbr :{0}.", nbrTrameEcoAdded);
                        nbrTrameSondeAdded = 0;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        DataBase.Logging("insertAllTrameSondeErreur", "0: " + ex.ToString());
                    }
                }

                try
                {
                    DataBase.insertAllTrameSonde(dataQueueCopy.ToList());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    DataBase.Logging("insertAllTrameSondeErreur", "1: " + ex.ToString());
                }


            }
        }
        //
        private static void consommeTRameBrute()
        {
            while (nbrTrameBruteAdded > 0 || WaitHandle.WaitAny(trameTrameBruteSyncEvents.EventArray) != 1)
            {

                Queue<Trame> dataQueueCopy = null;

                lock (((ICollection)trameBruteQueue).SyncRoot)
                {
                    try
                    {
                        dataQueueCopy = new Queue<Trame>(trameBruteQueue.ToArray());
                        trameBruteQueue.Clear();
                        if (PrincipalListner.config.Debug)
                            Console.WriteLine("Trame Brute consomé nbr :{0}.", nbrAdded);
                        nbrTrameBruteAdded = 0;
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Buffer n'a pas pu lire les données trame brutte.");
                    }
                }

                DataBase.insertAllTrameBrute(dataQueueCopy.ToList());
            }
        }
        private static void consommeTrameReal()
        {
            while (nbrTrameRealAdded > 0 || WaitHandle.WaitAny(trameRealUpdaterConsomeSyncEvents.EventArray) != 1)
            {
                bool taked = false;
                List<TrameReal> dataQueueCopy = null;

                lock (((ICollection)queueTrameRealUpdater).SyncRoot)
                {
                    try
                    {

                        dataQueueCopy = new List<TrameReal>();
                        taked = true;
                        int i = 0;
                        foreach (string NISBalise in queueTrameRealUpdater.Keys)
                        {
                            if (queueTrameRealUpdater.Count == 0 || i > 50)
                                break;
                            TrameReal trame;

                            queueTrameRealUpdater.TryGetValue(NISBalise, out trame);
                            if (trame == null)
                                continue;
                            dataQueueCopy.Add(trame);


                            i++;
                        }
                        foreach (TrameReal trame in dataQueueCopy)
                            queueTrameRealUpdater.Remove(trame.Balise.Nisbalise);
                        nbrTrameRealAdded = queueTrameRealUpdater.Count;
                        if (PrincipalListner.config.Debug)
                            Console.WriteLine("---Trame Real  Consomées {0} non Consomées {1}: ", dataQueueCopy.Count, nbrTrameRealAdded);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Buffer n'a pas pu lire les données. {0}", e.Message);

                    }
                }


                if (taked)
                {
                    TrameRealThreadManager.StartUpdate(dataQueueCopy);
                }

            }
        }




        private static void addStatBalise(BaliseStat stat)
        {
            lock (((ICollection)statBalise).SyncRoot)
            {
                nbrStatAdded++;
                statBalise.Enqueue(stat);
                statConsomeSyncEvents.NewItemEvent.Set();
            }
        }
        //////public static void addTrameBrute(Trame trame)
        //////{
        //////    /*lock (((ICollection)trameBruteQueue).SyncRoot)
        //////    {
        //////        if (trame.TrameValue.Trim().Length == 0 || trame.TrameValue.Trim() == "#")
        //////            return;
        //////        nbrTrameBruteAdded++;
        //////        trameBruteQueue.Enqueue(trame);
        //////        trameTrameBruteSyncEvents.NewItemEvent.Set();
        //////    }*/
        //////}
        private static void consommeStat()
        {


            while (nbrStatAdded > 0 || WaitHandle.WaitAny(statConsomeSyncEvents.EventArray) != 1)
            {
                List<BaliseStat> dataQueueCopy = null;
                lock (((ICollection)statBalise).SyncRoot)
                {

                    if (statBalise.Count > 0)
                    {

                        dataQueueCopy = new List<BaliseStat>();
                        dataQueueCopy.AddRange(statBalise.ToList());
                        statBalise.Clear();
                        nbrStatAdded = 0;
                        if (PrincipalListner.config.Debug)
                            Console.WriteLine("State balise Connection : {0}", dataQueueCopy.Count);
                    }
                }
                DataBase.insertStatBalise(dataQueueCopy);


            }
        }
        private static void consommeTrajet()
        {

            while (nbrTrajetAdded > 0 || WaitHandle.WaitAny(TrajetSyncEvents.EventArray) != 1)
            {

                List<Trajet> dataQueueCopy = null;

                lock (((ICollection)Trajets).SyncRoot)
                {
                    try
                    {

                        dataQueueCopy = new List<Trajet>();

                        for (int i = 0; i < 100; i++)
                        {
                            if (Trajets.Count == 0)
                                break;
                            dataQueueCopy.Add(Trajets.Dequeue());
                        }
                        nbrTrajetAdded = Trajets.Count;
                        if (PrincipalListner.config.Debug)
                            Console.WriteLine("*********Trajet   Consomés : {0}. non Consumées :{1}", dataQueueCopy.Count, nbrTrajetAdded);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Buffer n'a pas pu lire les données.");
                    }
                }

                //DataBase.insertAllTrajets(dataQueueCopy.ToList(),nbrTrajetAdded);
                TrajetsInsertThread trameTrajetUpdater = new TrajetsInsertThread(dataQueueCopy, nbrTrajetAdded);
                ThreadPool.QueueUserWorkItem(trameTrajetUpdater.insertAllTrjets);

            }
        }


        public static void addSarensTrame(TrameReal trame)
        {
            if (trame == null)
                return;

            lock (((ICollection)SarensQueue).SyncRoot)
            {
                SarensQueue.Enqueue(trame);
                trameSarensConsomeSyncEvents.NewItemEvent.Set();
                nbrTrameSarensAdded++;
            }

        }

        public static void addNonInsetedTrameReal(List<TrameReal> listTrames)
        {
            if (listTrames == null || listTrames.Count == 0)
                return;

            lock (((ICollection)queueTrameRealUpdater).SyncRoot)
            {
                foreach (TrameReal trame in listTrames)
                {

                    if (!queueTrameRealUpdater.ContainsKey(trame.Balise.Nisbalise))
                        queueTrameRealUpdater.Add(trame.Balise.Nisbalise, trame);


                }
                nbrTrameRealAdded = queueTrameRealUpdater.Count;
                trameRealUpdaterConsomeSyncEvents.NewItemEvent.Set();

            }
        }




    }
}
