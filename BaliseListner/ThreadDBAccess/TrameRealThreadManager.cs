using BaliseListner.DataAccess;
using Collecteur.Core.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BaliseListner.ThreadDBAccess
{
    public static class TrameRealThreadManager
    {

        public static Dictionary<string, TrameRealUpdatWorker> ThreadDictionary = new Dictionary<string, TrameRealUpdatWorker>();
        static TrameRealThreadManager()
        {
            GarbageThread garbageThread = GarbageThread.getInstance(ThreadDictionary);
            ThreadPool.QueueUserWorkItem(garbageThread.GarbageThreadDictionary);
        }
       
        public static void StartUpdate(List<TrameReal> tramesToUpdate)
        {


            lock (ThreadDictionary) {

                foreach (TrameReal trameToUpdate in tramesToUpdate)
                {
                    if(!ThreadDictionary.ContainsKey(trameToUpdate.NisBalise)){

                        TrameRealUpdatWorker TRUW = new TrameRealUpdatWorker(trameToUpdate);

                        ThreadDictionary.Add(trameToUpdate.NisBalise, TRUW);

                        ThreadPool.QueueUserWorkItem(TRUW.DoWork);
                    }
                    else{
                        if (ThreadDictionary[trameToUpdate.NisBalise].Acive)
                            ThreadDictionary[trameToUpdate.NisBalise].NextTrameRealToUpdate=trameToUpdate;
                        else
                        {
                            ThreadDictionary[trameToUpdate.NisBalise].Relaunch(trameToUpdate);
                            ThreadPool.QueueUserWorkItem(ThreadDictionary[trameToUpdate.NisBalise].DoWork);
                        }
                    }
           
                }
            }
       
        
        }
 }
    class GarbageThread
    {

        public  Dictionary<string, TrameRealUpdatWorker> ThreadDictionary;

        static GarbageThread garbageThread ;
        public static GarbageThread getInstance(Dictionary<string, TrameRealUpdatWorker> threadDictionary)
        {
            if(garbageThread == null)
                garbageThread = new GarbageThread(threadDictionary);

            return garbageThread;
               

        }
        GarbageThread(Dictionary<string, TrameRealUpdatWorker> threadDictionary)
        {
            this.ThreadDictionary=threadDictionary;
        }
        public void GarbageThreadDictionary(object state)
        {
            while(true){
                   Thread.Sleep(900000); // 15 min 

                   lock (ThreadDictionary)
                   {
                       
                       List<string> listOfKeys = new List<string>();
                       foreach(string  key in ThreadDictionary.Keys)
                       {
                           if (!ThreadDictionary[key].Acive)
                               listOfKeys.Add(key);
                       }

                       foreach (string key in listOfKeys)
                           ThreadDictionary.Remove(key);
                       Console.WriteLine(DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss]") + " Nettoyage des threads, nbe de threads nettoyés {0}, nbr de threads Actives {1}.", listOfKeys.Count, ThreadDictionary.Count);
                       Logging("ThreadTrameRealGarbage", string.Format("Nettoyage des threads, nbe de threads nettoyés {0}, nbr de threads Actives {1}.", listOfKeys.Count, ThreadDictionary.Count));
                }
            }


        }

        private void Logging(String erreur, String message)
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
            catch (Exception)
            {

            }

        }


    }

   
}
