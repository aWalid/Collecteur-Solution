using BaliseListner.DataAccess;
using Collecteur.Core.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BaliseListner.ThreadDBAccess
{
    public  static class MissionInsertLancer 
    {
       private static  MissionInsertExecutor executor = MissionInsertExecutor.getInstance();

        public static void go(List<TrameReal> listTrame)
        {

            executor.start(listTrame);

        }
       

    }

     class MissionInsertExecutor
    {
        private static MissionInsertExecutor executor;
        public static MissionInsertExecutor getInstance(){
            if(executor == null)
                executor = new MissionInsertExecutor();
            return executor;

        }



        private List<Mission> missions = new List<Mission>();
        DateTime lastUpdate = DateTime.Now;
        int  version = 0;
        bool update = false;

        public void start(List<TrameReal> listTrame)
        {
            if (!update)
            {
              version=  DataBase.GetAvailableMission(update, version, ref missions);
              update = true;
              lastUpdate = DateTime.Now;
            }
            else if ((DateTime.Now - lastUpdate).TotalMinutes >= 10)
            {
                    version = DataBase.GetAvailableMission(update, version, ref missions);
                    lastUpdate = DateTime.Now;
                
            }

            List<TrameReal> listTrames = new List<TrameReal>();
            foreach(TrameReal tr in listTrame){
                if (isTrameMission(tr))
                    listTrames.Add(tr);
                   
            }
            if (listTrames.Count > 0)
                DataBase.insertAllTrameMission(listTrames);


        }

        private bool isTrameMission(TrameReal trame)
        {
            foreach (Mission mission in missions)
            {
                if (trame.Balise.Nisbalise == mission.balise)
                    if (mission.Start <= trame.Temps && mission.End >= trame.Temps)
                    {
                        trame.Mission = mission;
                        return true;
                    }
                        
            }
            return false;
        }
    }
}
