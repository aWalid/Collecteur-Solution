using BaliseListner.Api;
using BaliseListner.OldCollecteur;
using ModelBoitier;
using OldCollecteur;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace BaliseListner.DataAccess
{
     
    public class StateBaliseThread
    {
       // private static String connectionStringPooled = "Data Source=(local); Initial Catalog=I2BGEO; Integrated Security=true;Min Pool Size=10;";
        private static String connectionStringPooled = DataBase.connectionString +";Min Pool Size=10";

        private BaliseStat stat ;

        public StateBaliseThread(BaliseStat state)
        {
            this.stat = state;
        }
        public  void insertEtatBalise(object stateThread)
        {
            SqlConnection sqlConnection = null;


            try
            {
                using (sqlConnection = new SqlConnection(connectionStringPooled))
                {
                    DbCommand oCmd;
                    bool exec = false;

                    sqlConnection.Open();

                    oCmd = sqlConnection.CreateCommand();
                    oCmd.CommandType = CommandType.StoredProcedure;
                    oCmd.CommandText = "isConnected";

                    DbParameter para0 = oCmd.CreateParameter();
                    para0.ParameterName = "@balise";
                    para0.DbType = DbType.String;
                    para0.Size = 32;
                    para0.Value = stat.NiSbalise;
                    oCmd.Parameters.Add(para0);

                    DbParameter para1 = oCmd.CreateParameter();
                    para1.ParameterName = "@moment";
                    para1.DbType = DbType.DateTime;
                    para1.Value = stat.dateTime;
                    oCmd.Parameters.Add(para1);

                    DbParameter para2 = oCmd.CreateParameter();
                    para2.ParameterName = "@etat";
                    para2.DbType = DbType.Boolean;
                    para2.Value = stat.Connected;
                    oCmd.Parameters.Add(para2);

                    try
                    {
                        int n = oCmd.ExecuteNonQuery();
                        Console.WriteLine("Execution Réussi: {0}", n);
                        exec = true;
                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine("Erreur dans la connexion au serveur BD du boitier: {0}", stat.NiSbalise);
                        Logging("StatBalise", "erreur d'insertion de l'etat de balise  . ", ex);


                    }

                    oCmd.Dispose();
                    if (!exec)
                    {
                        Console.WriteLine("Echec execution proc connexion, sauvegarde du contexte {0}", stat.NiSbalise);
                        Logging("StatBalise", "Echec execution proc connexion, balise : " + stat.NiSbalise);

                    }
                    else
                    {
                        Console.WriteLine("Boitier {0} Etat Connexion {1} reussi.", stat.NiSbalise, stat.Connected.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur dans l'execution proc isConnected : " + ex.Message);
                Logging("StatBalise", "Echec execution proc connexion, balise : " + stat.NiSbalise, ex);


            }
                    finally
                    {
                        if (sqlConnection !=null)
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
