using BaliseListner.Api;
using BaliseListner.OldCollecteur;
using ModelBoitier;
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
     
    public class TrameBruteThread
    {
       // private static String connectionStringPooled = "Data Source=(local); Initial Catalog=I2BGEO; Integrated Security=true;Min Pool Size=10;";
        private static String connectionStringPooled = DataBase.connectionString +";Min Pool Size=10";

        private List<Trame> dataTrameQueueCopy;

        public TrameBruteThread(List<Trame> dataQueueCopy)
        {
            this.dataTrameQueueCopy = dataQueueCopy;
        }
        public  void insertAllTrameBrute(object state)
        {
            SqlConnection sqlConnection = null;


            try
            {
                using (sqlConnection = new SqlConnection(connectionStringPooled))
                {

                    sqlConnection.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = sqlConnection;
                    foreach (Trame trame in dataTrameQueueCopy)
                    {
                        try
                        {

                            cmd.CommandText = trame.toSQL();
                            cmd.ExecuteNonQuery();
                        }
                        catch (Exception e)
                        {
                            cmd.Cancel();
                            Logging("TrameBrutte", "trames Brute non inseré.", e);
                        }

                    }

                }
            }
            catch (Exception e)
            {
                Logging("TrameBrutte", "Insertion de trames Capteurs a echouie", e);
            }

            finally
            {
                if (sqlConnection != null)
                    sqlConnection.Close();
            }
       
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private static void Logging(String erreur, String message)
        {

            String path = "Log\\" + DateTime.Now.ToString("dd-MM-yyyy");
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
            String path = "Log\\" + DateTime.Now.ToString("dd-MM-yyyy");
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
