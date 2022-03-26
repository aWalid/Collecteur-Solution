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
     
    public class TrajetsInsertThread
    {
       // private static String connectionStringPooled = "Data Source=(local); Initial Catalog=I2BGEO; Integrated Security=true;Min Pool Size=10;";
        private static String connectionStringPooled = DataBase.connectionString +"";

        private List<Trajet> dataTrajetQueueCopy;
        private int nbrTrajetAdded;
        public TrajetsInsertThread(List<Trajet> dataQueueCopy, int nbrTrajetAdded)
        {
            this.nbrTrajetAdded = nbrTrajetAdded;
            this.dataTrajetQueueCopy = dataQueueCopy;
        }
        public void insertAllTrjets(object state)
        {
            if (PrincipalListner.config.Debug)
                Logging("Trajet", string.Format("nombre de trajet consommé {0}, reste :{1}", dataTrajetQueueCopy.Count, nbrTrajetAdded));
         
            SqlConnection sqlConnection = null;
            DataTable dataTable = new DataTable("TypeTrajet");
            try
            {
                dataTable.Columns.Add("dateDebut", typeof(DateTime));

                dataTable.Columns.Add("LonDebut", typeof(Decimal));
                dataTable.Columns.Add("LatDebut", typeof(Decimal));
                dataTable.Columns.Add("dateFin", typeof(DateTime));
                dataTable.Columns.Add("LonFin", typeof(Decimal));
                dataTable.Columns.Add("LatFin", typeof(Decimal));
                dataTable.Columns.Add("vitesseMax", typeof(Decimal));
                dataTable.Columns.Add("duree", typeof(Int32));
                dataTable.Columns.Add("distance", typeof(Decimal));
                dataTable.Columns.Add("NISBalise", typeof(string));
                dataTable.PrimaryKey = new DataColumn[] {dataTable.Columns["dateDebut"], 
                                         dataTable.Columns["NISBalise"]};



                foreach (Trajet trajet in dataTrajetQueueCopy)
                {
                    try
                    {
                        //Console.WriteLine("Nbre de trames pas encore traité {0}.", nbrTrame.ToString());
                        dataTable.Rows.Add(trajet.DateDebut,Math.Round( trajet.LongitudeDebut,5), Math.Round(trajet.LatitudeDebut,5), trajet.DateDebut.AddSeconds(trajet.Duree),Math.Round( trajet.LongitudeFin,5), Math.Round(trajet.LatitudeFin,5)
                            , Math.Round(trajet.VMax,1), trajet.Duree, Math.Round(trajet.Distance,2), trajet.NisBalise);
                    }
                    catch (Exception e)
                    {
                        Logging("Trajet", "Insertion de Trajet dupliqués.", e);

                    }

                }

                using (sqlConnection = new SqlConnection(connectionStringPooled))
                {
                    sqlConnection.Open();

                    SqlCommand command = sqlConnection.CreateCommand();
                    command.CommandTimeout = 300;
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.CommandText = "[dbo].[insertTrajet]";

                    SqlParameter parameter = new SqlParameter();

                    parameter.ParameterName = "@Sample";
                    parameter.SqlDbType = SqlDbType.Structured;
                    parameter.Value = dataTable;
                    command.Parameters.Add(parameter);


                    try
                    {
                        int numberOfRowsUpdated = command.ExecuteNonQuery();
                        if (PrincipalListner.config.Debug)
                            Console.WriteLine("Trajets : Nombre de rows updated = " + numberOfRowsUpdated.ToString());

                    }
                    catch (Exception ex)
                    {

                        command.Cancel();
                        Console.WriteLine("Trajets : Erreur  {0}", ex.Message);
                        Logging("Trajet", "insert erreur ", ex);
                        OLDModelGeneratorProcessor.addNonInsetedTrajet(dataTrajetQueueCopy);

                    }
                    try
                    {
                        sqlConnection.Close();
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Trajets : la connexion au serveur BD n'a pas pu se fermé.");
                    }

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("la table de Trajets n'a pas pu s'initialiser.");
                Logging("Trajet", "preparation de données erreur ", ex);
                OLDModelGeneratorProcessor.addNonInsetedTrajet(dataTrajetQueueCopy);
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
                    sw.WriteLine(exp.Message);
                    sw.Close();
                }
            }
            catch (Exception)
            {

            }

        }

       
    }
}
