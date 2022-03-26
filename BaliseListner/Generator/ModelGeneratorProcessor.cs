using BaliseListner.DataAccess;
using Collecteur.Core.Api;
using Collecteur.Core.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BaliseListner.Generator
{
    public static class ModelGeneratorProcessor
    {    
        static ModelGeneratorProcessor(){
            //Thread threadTrameConsomater = new Thread(ComsommeTrame);
            //threadTrameConsomater.Start();

            //Thread threadStatConsomater = new Thread(consommeStat);
            //threadStatConsomater.Start();
        }
   
        private static Queue<Trame> queue = new Queue<Trame>();
        private static Queue<BaliseStat> statBalise = new Queue<BaliseStat>();
        private static SyncEvents statConsomeSyncEvents = new SyncEvents();
        private static SyncEvents trameConsomeSyncEvents = new SyncEvents();

        public static void addTrame(Trame trame)
        {
            lock (((ICollection)queue).SyncRoot)
            {   
                queue.Enqueue(trame);
                trameConsomeSyncEvents.NewItemEvent.Set();    
            }
        }
        public static void addFirstTrame(Trame trame)
        {
            addStatBalise(new BaliseStat(trame.balise, true, System.DateTime.Now));
            addTrame(trame);
        }

        public static void addDeconnect(Balise Balise)
        {
            if (Balise == null || Balise.Nisbalise.Length == 0)
                return;
            addStatBalise(new BaliseStat(Balise, false, System.DateTime.Now));
        }
        private  static void ComsommeTrame()
        {
           Trame trame = null;
            while (WaitHandle.WaitAny(trameConsomeSyncEvents.EventArray) != 1)
            {  
                lock (((ICollection)queue).SyncRoot)
                {
                    trame = queue.Dequeue();
                }
              //  DataBase.InsertTrame(trame); 
                Console.WriteLine("trame consomé :{0}", trame.TrameValue);
               
            }           
        }
        //

        private static void addStatBalise(BaliseStat stat)
        {
            lock (((ICollection)statBalise).SyncRoot)
            {
                statBalise.Enqueue(stat);
                statConsomeSyncEvents.NewItemEvent.Set();
            }
        }

        private static void consommeStat()
        {
            BaliseStat stat = null;
            while (WaitHandle.WaitAny(statConsomeSyncEvents.EventArray) != 1)
            {
                lock (((ICollection)statBalise).SyncRoot)
                {
                    stat = statBalise.Dequeue();
                }
               // DataBase.InsertTrame(trame);
                Console.WriteLine("state consomé Boitier:{0}, connecte :{1}",stat.NiSBalise ,stat.Connected);

            }    
        }
    }
}
