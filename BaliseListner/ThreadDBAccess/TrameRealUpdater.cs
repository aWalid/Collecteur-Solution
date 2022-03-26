
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

    public class TrameRealUpdater
    {
       // private static String connectionStringPooled = "Data Source=(local); Initial Catalog=I2BGEO; Integrated Security=true;Min Pool Size=10;";
        private static String connectionStringPooled = DataBase.connectionString +"";
       
        private List<TrameReal> dataQueueCopy;

        public TrameRealUpdater(List<TrameReal> dataQueueCopy)
        {
            this.dataQueueCopy = dataQueueCopy;
        }
        public void insertAllTrameRealUpdater(object state)
        {

            SqlConnection sqlConnection = null;
            DataTable dataTable = new DataTable("TypeTramesData");
            try
            {
                dataTable.Columns.Add("temps", typeof(DateTime));
                dataTable.Columns.Add("longitude", typeof(Decimal));
                dataTable.Columns.Add("latitude", typeof(Decimal));
                dataTable.Columns.Add("vitesse", typeof(Decimal));
                dataTable.Columns.Add("direction", typeof(Int16));
                dataTable.Columns.Add("Temperature", typeof(Int16));
                dataTable.Columns.Add("Capteur", typeof(string));
                dataTable.Columns.Add("chauffeur", typeof(string));
                dataTable.Columns.Add("NISBalise", typeof(string));
                dataTable.Columns.Add("tempsReception", typeof(DateTime));
                dataTable.PrimaryKey = new DataColumn[] { dataTable.Columns["NISBalise"] };


                foreach (TrameReal trame in dataQueueCopy)
                {
                    try
                    {
                        DataRow rw = dataTable.Rows.Find(trame.NisBalise);

                        int idx;
                        if (rw != null && (idx = dataTable.Rows.IndexOf(rw)) >= 0)
                        {
                            try
                            {
                                DateTime dt;
                                bool rs = DateTime.TryParse(rw["temps"].ToString(), out dt);

                                if (rs)
                                {
                                    if (trame.Temps > dt)
                                    {
                                        dataTable.Rows[idx]["temps"] = trame.Temps;
                                        dataTable.Rows[idx]["longitude"] = trame.Longitude;
                                        dataTable.Rows[idx]["latitude"] = trame.Latitude;
                                        dataTable.Rows[idx]["vitesse"] = trame.Vitesse;
                                        dataTable.Rows[idx]["direction"] = trame.Direction;
                                        dataTable.Rows[idx]["Temperature"] = trame.Temperature;
                                        dataTable.Rows[idx]["Capteur"] = trame.Capteur;
                                        dataTable.Rows[idx]["chauffeur"] = trame.Chauffeur;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Logging("TrameReal", " Erreur conversion date:", ex);
                            }
                        }
                        else
                        {
                            dataTable.Rows.Add(trame.Temps, Math.Round(trame.Longitude,5), Math.Round(trame.Latitude,5),
                                Math.Round((Decimal)trame.Vitesse,1), trame.Direction, trame.Temperature,
                                trame.Capteur, trame.Chauffeur, trame.NisBalise);
                        }

                    }
                    catch (Exception e)
                    {
                        Logging("TrameReal", "la table de tramesReel n'a pas pu s'initialiser. ", e);
                    }
                }



                using (sqlConnection = new SqlConnection(connectionStringPooled))
                {
                    sqlConnection.Open();
                    SqlCommand command = sqlConnection.CreateCommand();
                    command.CommandTimeout = 150;
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.CommandText = "[dbo].[updateTrameReel]";

                    SqlParameter parameter = new SqlParameter();

                    parameter.ParameterName = "@Sample";
                    parameter.SqlDbType = SqlDbType.Structured;
                    parameter.Value = dataTable;
                    command.Parameters.Add(parameter);
                    try
                    {

                        int numberOfRowsUpdated = command.ExecuteNonQuery();
                        Console.WriteLine("tramesReel : Nombre de rows updated = " + numberOfRowsUpdated.ToString());
                    }
                    catch (Exception ex)
                    {

                       // Console.WriteLine("tramesReel : Erreur  {0}.", ex.Message);
                        Logging("TrameReal", " Erreur dans une tentative d'insertion ", ex);
                        OLDModelGeneratorProcessor.addNonInsetedTrameReal(dataQueueCopy);
                        command.Cancel();
                    }
                    try
                    {
                        sqlConnection.Close();
                    }
                    catch (Exception ex)
                    {
                       // Console.WriteLine("tramesReel : la connexion au serveur BD n'a pas pu se fermé.");
                        Logging("TrameReal", " la connexion au serveur BD n'a pas pu se fermé.", ex);
                    }

                }





            }
            catch (Exception e)
            {
                Console.WriteLine("echec thread trame reel.");
                Logging("TrameReal", "echec thread trame reel.", e);
               // OLDModelGeneratorProcessor.addNonInsetedTrameReal(dataQueueCopy);


            }
            finally
            {
                if (sqlConnection != null)
                    sqlConnection.Close();
            }
        }
        private static void Logging(String erreur, String message, Exception exp)
        {
            String path = "Log\\" + DateTime.Now.ToString("dd-MM-yyyy");
            try {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                 using (StreamWriter sw = File.AppendText(path +"\\Erreur_" + erreur +  ".log"))
            {
                sw.WriteLine(DateTime.Now + ": " + message);
                sw.WriteLine(exp.Message);
                sw.Close();
            }
            }catch(Exception){

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

       
    }
}
