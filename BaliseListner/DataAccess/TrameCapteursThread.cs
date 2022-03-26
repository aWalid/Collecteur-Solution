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
     
    public class TrameCapteursThread
    {
       // private static String connectionStringPooled = "Data Source=(local); Initial Catalog=I2BGEO; Integrated Security=true;Min Pool Size=10;";
        private static String connectionStringPooled = DataBase.connectionString +";Min Pool Size=10";
       
        private List<TrameReal> dataQueueCopy;

        public TrameCapteursThread(List<TrameReal> dataQueueCopy)
        {
            this.dataQueueCopy = dataQueueCopy;
        }
        public void insertAllTrameCapteurs(object state)
        {

            SqlConnection sqlConnection = null;
            DataTable dataTable = new DataTable("TypeTramesData");

            try
            {
                dataTable.Columns.Add("temps", typeof(DateTime));
                dataTable.Columns.Add("longitude", typeof(Decimal));
                dataTable.Columns.Add("latitude", typeof(Decimal));
                dataTable.Columns.Add("vitesse", typeof(Double));
                dataTable.Columns.Add("direction", typeof(Int16));
                dataTable.Columns.Add("Temperature", typeof(Int16));
                dataTable.Columns.Add("Capteur", typeof(string));
                dataTable.Columns.Add("chauffeur", typeof(string));
                dataTable.Columns.Add("NISBalise", typeof(string));

                dataTable.PrimaryKey = new DataColumn[] {dataTable.Columns["temps"], 
                                         dataTable.Columns["NISBalise"]};


                foreach (TrameReal boitier in dataQueueCopy)
                {
                    try
                    {
                        //Console.WriteLine("Nbre de trames pas encore traité {0}.", nbrTrame.ToString());
                        dataTable.Rows.Add(boitier.Temps, boitier.Longitude, boitier.Latitude,
                        (Decimal)boitier.Vitesse, boitier.Direction, boitier.Temperature,
                        boitier.Capteur, boitier.Chauffeur, boitier.Balise.Nisbalise);
                    }
                    catch (Exception e)
                    {
                        Logging("TrameCapteurs", "Insertion de trames Capteurs dupliqués.", e);
                    }

                }

                bool exec = false;
                using (sqlConnection = new SqlConnection(connectionStringPooled))
                {
                    sqlConnection.Open();
                    SqlCommand command = sqlConnection.CreateCommand();
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.CommandText = "[dbo].[InsertCapteursTrame]";

                    SqlParameter parameter = new SqlParameter();
                    command.CommandTimeout = 120;
                    parameter.ParameterName = "@Sample";
                    parameter.SqlDbType = SqlDbType.Structured;
                    parameter.Value = dataTable;
                    command.Parameters.Add(parameter);




                    try
                    {
                        int numberOfRowsUpdated = command.ExecuteNonQuery();
                        Console.WriteLine("Nombre de rows updated = " + numberOfRowsUpdated.ToString());
                        exec = true;
                    }
                    catch (Exception ex)
                    {

                        command.Cancel();
                        Console.WriteLine("Insertion des trames Capteurs Erreur : {0}", ex.Message);
                        Logging("TrameCapteurs", "Insertion des trames Capteurs Erreur ", ex);


                    }
                }

                if (!exec)
                {
                    Console.WriteLine("BD Server ne repond pas, sauvegarde du contexte en cours.");
                    Logging("TrameCapteurs", "liste des trames restocké dans le depot nbr : " + dataQueueCopy.Count);
                    OLDModelGeneratorProcessor.addTramesCapteursNotInserted(dataQueueCopy);
                    Logging("TrameCapteurs", string.Join("Trames non inserée", dataQueueCopy.Select(d => d.ToString()).ToArray()));
                   
                }



                try
                {
                    sqlConnection.Close();
                }
                catch (Exception)
                {
                    Console.WriteLine("la connexion au serveur BD n'a pas pu se fermé.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("la table de trames n'a pas pu s'initialiser.");
                OLDModelGeneratorProcessor.addTramesCapteursNotInserted(dataQueueCopy);
                Logging("TrameCapteurs", "la table de trames n'a pas pu s'initialiser.", ex);
                Logging("TrameCapteurs", string.Join("Trames non inserée", dataQueueCopy.Select(d => d.ToString()).ToArray()));
                   
            }
            finally
            {
                if (sqlConnection != null)
                    sqlConnection.Close();
            }

               
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
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
                sw.WriteLine("Trace d'erreur :" +exp.Message);
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
