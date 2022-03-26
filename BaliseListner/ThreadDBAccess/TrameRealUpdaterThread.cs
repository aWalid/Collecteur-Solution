
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
     
    public class TrameRealUpdatWorker
    {
        
        private TrameReal trameRealToUpdate;
        private TrameReal nextTrameRealToUpdate;
        public bool Acive = true;
        public TrameReal NextTrameRealToUpdate
        {
            get { return nextTrameRealToUpdate; }
            set { nextTrameRealToUpdate = value; }
        }
        public TrameRealUpdatWorker(TrameReal trameReal)
        {
            this.trameRealToUpdate = trameReal;
        }

        public void Relaunch(TrameReal trameReal){
            Acive = true;
            trameRealToUpdate = trameReal;
        }
        public void DoWork(object state)
        {
            while (trameRealToUpdate != null)
            {
                lock (trameRealToUpdate)
                {
                 bool success  = UpdateTrameReal();
                     if (success)
                     {
                         trameRealToUpdate = NextTrameRealToUpdate;
                         NextTrameRealToUpdate = null;
                         continue;
                     }
                     else
                     {
                         if (NextTrameRealToUpdate == null)
                         {
                             break;
                         }
                         else
                         {
                             trameRealToUpdate = NextTrameRealToUpdate;
                             NextTrameRealToUpdate = null;
                             continue;
                         }
                     }


                }

            }
            Acive = false;
            

        }
        private bool UpdateTrameReal()
        {
            bool success=false;
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
                dataTable.PrimaryKey = new DataColumn[] { dataTable.Columns["NISBalise"] };


                   try
                    {
                     
                            dataTable.Rows.Add(trameRealToUpdate.Temps, Math.Round(trameRealToUpdate.Longitude,5), Math.Round(trameRealToUpdate.Latitude,5),
                                Math.Round((Decimal)trameRealToUpdate.Vitesse,1), trameRealToUpdate.Direction, trameRealToUpdate.Temperature,
                                trameRealToUpdate.Capteur, trameRealToUpdate.Chauffeur, trameRealToUpdate.NisBalise);
                        

                    }
                    catch (Exception e)
                    {
                        Logging("TrameReal", "impossible d'inserer des donnees dans la table Sample. ", e);
                        return success;
                    }




                   using (sqlConnection = new SqlConnection(DataBase.connectionString + ""))
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
                       // Console.WriteLine("tramesReel : Nombre de rows updated = " + numberOfRowsUpdated.ToString());
                    }
                    catch (Exception ex)
                    {

                       // Console.WriteLine("tramesReel : Erreur  {0}.", ex.Message);
                        Logging("TrameReal", " Erreur dans une tentative d'insertion ", ex);
                      //  OLDModelGeneratorProcessor.addNonInsetedTrameReal(dataQueueCopy);
                        command.Cancel();
                        return success;
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
                return success;


            }
            finally
            {
                if (sqlConnection != null)
                    sqlConnection.Close();
            }
            return true;
        }

       
        private  void Logging(String erreur, String message, Exception exp)
        {
            String path = "Log\\" + DateTime.Now.ToString("yyyy-MM-dd");
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
        private  void Logging(String erreur, String message)
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

       
    }
}
