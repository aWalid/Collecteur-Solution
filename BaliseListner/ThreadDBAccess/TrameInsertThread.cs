using BaliseListner.ThreadListener;
using Collecteur.Core.Api;
using OldCollecteur;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace BaliseListner.DataAccess
{
     
    public class TrameInsertThread
    {
       // private static String connectionStringPooled = "Data Source=(local); Initial Catalog=I2BGEO; Integrated Security=true;Min Pool Size=10;";
        private static String connectionStringPooled = DataBase.connectionString +";Min Pool Size=1";

        private List<TrameReal> dataTrameQueueCopy;

        public TrameInsertThread(List<TrameReal> dataQueueCopy)
        {
            this.dataTrameQueueCopy = dataQueueCopy;
        }
        
        [MethodImpl(MethodImplOptions.Synchronized)]
        private static void Logging(String erreur, String message)
        {

            String path = "Log\\" + DateTime.Now.ToString("yyyy-MM-dd");
              try
              {
                  if (!Directory.Exists(path))
                  {
                      Directory.CreateDirectory(path);
                  }
                  using (StreamWriter sw = File.AppendText(path+"\\Erreur_" + erreur + ".log"))
                  {
                      sw.WriteLine(DateTime.Now + ": " + message);
                      sw.Close();
                  }
              }
              catch (Exception)
              {

              }
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
                using (StreamWriter sw = File.AppendText(path + "\\Erreur_" + erreur + ".log"))
                {
                    sw.WriteLine(DateTime.Now + ": " + message);
                    sw.WriteLine("Trace d'erreur :" + exp.Message);

                    sw.Close();
                }
            }
            catch (Exception)
            {

            }

        }

       
    }
}
