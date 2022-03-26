using BaliseListner.CollecteurException;
using BaliseListner.Collection;
using BaliseListner.ThreadListener;
using Collecteur.Core.Api;
using log4net;
using OldCollecteur;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BaliseListner.DataAccess
{
    class DataBase
    {
        static readonly ILog logger = LogManager.GetLogger(typeof(DataBase));
        //TODO change the connection string before deployment 
        //public static readonly String connectionString = "Data Source=(local); Initial Catalog=i2beco; Integrated Security=true;Min Pool Size=5; Max Pool Size=200;";
        public static readonly String connectionString = "Data Source=(local); Initial Catalog=I2BGEO; Integrated Security=true;Min Pool Size=5; Max Pool Size=200;";
        //public static readonly String connectionString = "Data Source=127.0.0.1; initial catalog=I2BGEO_; user id=I2BGEO; password=I2BGEO;Min Pool Size=5; Max Pool Size=200;";
        //public static readonly String connectionString = "Data Source=192.168.0.110; initial catalog=i2beco; user id=sa; password=i2bgeo_2019;Min Pool Size=5; Max Pool Size=200;";
        //public static readonly String connectionString = "Data Source=196.41.225.89; initial catalog=I2BGEO; user id=C_GeoTrack; password=BE__6Gfet34;";


        private static DbProviderFactory dbpf;
        private static SqlConnection sqlConnectionForBaliseConnection = new SqlConnection(connectionString);
        private static readonly SqlConnection oConn = new SqlConnection(connectionString);

        private static readonly SqlConnection oConnSonde = new SqlConnection(connectionString);

        private static SqlConnection sqlConnectionForKeyDalasInsert = new SqlConnection(connectionString);
        private static SqlConnection oConnRefrechBalise = new SqlConnection(connectionString);

        private static SqlConnection oConnTrajet = new SqlConnection(connectionString);
        private static SqlConnection oConnTrameBrute = new SqlConnection(connectionString);
        private static SqlConnection oConnTramePOIs = new SqlConnection(connectionString);
        private static SqlConnection oConnCapteurs = new SqlConnection(connectionString);
        private static SqlConnection oConnCapteursSondeTemperateur = new SqlConnection(connectionString);
        private static SqlConnection oConnMission = new SqlConnection(connectionString);
        private static SqlConnection oConnSarens = new SqlConnection(connectionString);

        static DataBase()
        {
            dbpf = DbProviderFactories.GetFactory("System.Data.SqlClient");
        }
        private static SqlConnection openConnection()
        {
            try
            {
                oConn.Open();
            }
            catch (Exception ex)
            {
                PrincipalListner.SyncBase.BaseEchecThreadEvent.Set();
                throw new DataBaseCommunicationException("Echec d'ouvrire une Connexion.", ex);
            }
            return oConn;
        }
        private static SqlConnection openConnectionSonde()
        {
            try
            {
                oConnSonde.Open();
            }
            catch (Exception ex)
            {
                PrincipalListner.SyncBase.BaseEchecThreadEvent.Set();
                throw new DataBaseCommunicationException("Echec d'ouvrire une Connexion.", ex);
            }
            return oConnSonde;
        }
        private static SqlConnection openSarensConnection()
        {
            try
            {
                oConnSarens.Open();
            }
            catch (Exception ex)
            {
                PrincipalListner.SyncBase.BaseEchecThreadEvent.Set();
                throw new DataBaseCommunicationException("Echec d'ouvrire une Connexion.", ex);
            }
            return oConnSarens;
        }
        private static SqlConnection openMissionConnection()
        {
            try
            {
                oConnMission.Open();
            }
            catch (Exception ex)
            {
                PrincipalListner.SyncBase.BaseEchecThreadEvent.Set();
                throw new DataBaseCommunicationException("Echec d'ouvrire une Mission Connexion.", ex);
            }
            return oConnMission;
        }
        private static SqlConnection openConnectionPoisCheck()
        {
            try
            {
                oConnTramePOIs.Open();
            }
            catch (Exception ex)
            {
                PrincipalListner.SyncBase.BaseEchecThreadEvent.Set();
                throw new DataBaseCommunicationException("Echec d'ouvrire une Connexion.", ex);
            }
            return oConnTramePOIs;
        }
        private static SqlConnection openCapteursConnection()
        {
            try
            {
                oConnCapteurs.Open();
            }
            catch (Exception ex)
            {
                PrincipalListner.SyncBase.BaseEchecThreadEvent.Set();
                throw new DataBaseCommunicationException("Echec d'ouvrire une Connexion.", ex);
            }
            return oConnCapteurs;
        }
        private static SqlConnection openCapteursSondeTemperateurConnection()
        {
            try
            {
                oConnCapteursSondeTemperateur.Open();
            }
            catch (Exception ex)
            {
                PrincipalListner.SyncBase.BaseEchecThreadEvent.Set();
                throw new DataBaseCommunicationException("Echec d'ouvrire une Connexion.", ex);
            }
            return oConnCapteursSondeTemperateur;
        }
        private static SqlConnection openForKeyDalasConnection()
        {
            try
            {
                sqlConnectionForKeyDalasInsert.Open();
            }
            catch (Exception ex)
            {
                PrincipalListner.SyncBase.BaseEchecThreadEvent.Set();
                throw new DataBaseCommunicationException("Echec d'ouvrire une Connexion.", ex);
            }
            return sqlConnectionForKeyDalasInsert;
        }
        private static SqlConnection openTrameBruteConnection()
        {
            try
            {
                oConnTrameBrute.Open();
            }
            catch (Exception ex)
            {
                PrincipalListner.SyncBase.BaseEchecThreadEvent.Set();
                throw new DataBaseCommunicationException("Echec d'ouvrire une Connexion trame brute.", ex);
            }
            return oConnTrameBrute;
        }
        private static SqlConnection openTrajetConnection()
        {
            try
            {
                oConnTrajet.Open();
            }
            catch (Exception ex)
            {
                throw new DataBaseCommunicationException("Echec d'ouvrire une Connexion pour Trajet.", ex);
            }
            return oConnTrajet;
        }

        private static SqlConnection openSqlConnectionForBaliseConnection()
        {
            try
            {
                sqlConnectionForBaliseConnection.Open();
            }
            catch (Exception ex)
            {

                throw new DataBaseCommunicationException("Echec d'ouvrire une Connexion pour balise refresh.", ex);

            }
            return sqlConnectionForBaliseConnection;
        }

        public static void insertStatBalise(List<BaliseStat> status)
        {
            if (status == null || status.Count == 0)
                return;
            SqlConnection sqlConnection = null;

            DataTable dataTable = new DataTable("TypeGPRSConnexion");
            try
            {
                dataTable.Columns.Add("Temps", typeof(DateTime));
                dataTable.Columns.Add("Etat", typeof(Boolean));
                dataTable.Columns.Add("NISBalise", typeof(string));
                dataTable.PrimaryKey = new DataColumn[] {dataTable.Columns["Temps"],
                                             dataTable.Columns["NISBalise"]};

                foreach (BaliseStat stat in status)
                {
                    try
                    {
                        //Console.WriteLine("Nbre de trames pas encore traité {0}.", nbrTrame.ToString());
                        dataTable.Rows.Add(stat.dateTime, stat.Connected, stat.NiSBalise);
                    }
                    catch (Exception e)
                    {
                        Logging("GPRSConnexion", "Insertion de GPRSConnexion dupliqués.", e);
                    }

                }

                sqlConnection = openSqlConnectionForBaliseConnection();
                SqlCommand command = sqlConnection.CreateCommand();
                command.CommandTimeout = 300;
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.CommandText = "[dbo].[setConnectionStatus]";

                SqlParameter parameter = new SqlParameter();

                parameter.ParameterName = "@Sample";
                parameter.SqlDbType = SqlDbType.Structured;
                parameter.Value = dataTable;
                command.Parameters.Add(parameter);
                try
                {
                    int numberOfRowsUpdated = command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    command.Cancel();
                    //Console.WriteLine("GPRSConnexion : Erreur  {0}", ex.Message);
                    Logging("GPRSConnexion", "insert erreur ", ex);
                    // OLDModelGeneratorProcessor.addNonInsetedTrajet(dataQueueCopy);
                }
                try
                {
                    sqlConnection.Close();
                }
                catch (Exception)
                {
                    Console.WriteLine("GPRSConnexion : la connexion au serveur BD n'a pas pu se fermé.");
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine("la table de GPRSConnexion n'a pas pu s'initialiser.");
                Logging("GPRSConnexion", "preparation de données erreur ", ex);
                //OLDModelGeneratorProcessor.addNonInsetedTrajet(dataQueueCopy);
            }
            finally
            {
                if (sqlConnection != null)
                    sqlConnection.Close();
            }
        }

        public static void insertDALASKey(IdentDalas key)
        {

            DbCommand oCmd;
            SqlConnection con = null;
            bool exec = false;
            DbParameter paraReturne = null;
            try
            {
                con = openForKeyDalasConnection();

                oCmd = con.CreateCommand();
                oCmd.CommandType = CommandType.StoredProcedure;
                oCmd.CommandText = "autoCarAffectation";

                DbParameter para0 = oCmd.CreateParameter();
                para0.ParameterName = "@nisbalise";
                para0.DbType = DbType.String;
                para0.Size = 32;
                para0.Value = key.Balise;
                oCmd.Parameters.Add(para0);

                DbParameter para1 = oCmd.CreateParameter();
                para1.ParameterName = "@dateDebut";
                para1.DbType = DbType.DateTime;
                para1.Value = key.Datetime;
                oCmd.Parameters.Add(para1);

                DbParameter para2 = oCmd.CreateParameter();
                para2.ParameterName = "@idchauffeur";
                para2.DbType = DbType.String;
                para2.Value = key.DalasKey;
                oCmd.Parameters.Add(para2);

                paraReturne = oCmd.CreateParameter();
                paraReturne.ParameterName = "@message";
                paraReturne.DbType = DbType.String;
                paraReturne.Size = 100;
                paraReturne.Value = "";
                paraReturne.Direction = ParameterDirection.InputOutput;
                oCmd.Parameters.Add(paraReturne);


                try
                {
                    int n = oCmd.ExecuteNonQuery();
                    exec = true;
                }
                catch (Exception ex)
                {
                    oCmd.Cancel();
                    //Console.WriteLine("Erreur dans la connexion au serveur BD du boitier: {0}", key.Balise);
                    Logging("KeyDalas", "erreur d'insertion de Key DALAS ", ex);
                }

                oCmd.Dispose();
                if (!exec)
                {
                    Logging("KeyDalas", "Echec d'insertion, balise : " + key.Balise);
                }
                else
                {
                    Console.WriteLine("Boitier {0} ,key DALAS {1}, execution reussi.", key.Balise, key.DalasKey.ToString());
                }
            }
            catch (Exception ex)
            {
                Logging("KeyDalas", "Echec de preparation de  proc connexion, balise : " + key.Balise, ex);
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                    if (paraReturne != null)
                        LoggingTrace("KeyDalas", String.Format("message de retour de la base {0} ,KEY DALAS :{1},Balise : {2}", (String)paraReturne.Value, key.DalasKey.ToString(), key.Balise));
                }
            }

        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static int GetBalise(bool Update, int Version, ref Dictionary<int, Balise> boitiers)
        {

            SqlCommand oCmd;
            SqlDataReader reader = null;
            SqlConnection RefrechBalise = null;
            bool exec = false;
            try
            {
                using (RefrechBalise = new SqlConnection(connectionString))
                {
                    // RefrechBalise = openRefrechBaliseConnection();
                    RefrechBalise.Open();
                    oCmd = RefrechBalise.CreateCommand();
                    oCmd.CommandType = CommandType.StoredProcedure;
                    oCmd.CommandText = "ChangedBaliseInfo";


                    SqlParameter paraReturne = oCmd.CreateParameter();
                    paraReturne.ParameterName = "@Version";
                    paraReturne.SqlDbType = SqlDbType.Int;
                    paraReturne.Size = 32;
                    paraReturne.Direction = ParameterDirection.InputOutput;
                    paraReturne.Value = Version;
                    oCmd.Parameters.Add(paraReturne);

                    SqlParameter para2 = oCmd.CreateParameter();
                    para2.ParameterName = "@Update";
                    para2.SqlDbType = SqlDbType.Bit;
                    para2.Value = Update;
                    oCmd.Parameters.Add(para2);

                    int i = 0;
                    do
                    {
                        try
                        {
                            reader = oCmd.ExecuteReader();
                            // Version = (int)paraReturne.Value;
                            if (!Update)
                                while (reader.Read())
                                {
                                    Balise balise;
                                    int matricule = reader.GetInt32(0);
                                    String NisBalise = reader.GetString(1);
                                    try
                                    {
                                        balise = new Balise(matricule, NisBalise);
                                        boitiers.Add(balise.Matricule, balise);
                                    }
                                    catch (ArgumentException)
                                    {
                                        //Console.WriteLine("L'element: {0}, Existe déjà.", matricule);
                                        logger.Warn("L'element Existe déjà. matricule :" + matricule + "NISBalise : " + NisBalise);
                                    }
                                }
                            else
                                while (reader.Read())
                                {
                                    try
                                    {
                                        Balise balise;
                                        int matricule = reader.GetInt32(0);
                                        String NisBalise = reader.GetString(1);
                                        String action = reader.GetString(2);
                                        switch (action)
                                        {
                                            case "U":
                                                {
                                                    boitiers.TryGetValue(matricule, out balise);
                                                    if (balise != null)
                                                        balise.Nisbalise = NisBalise;
                                                    else
                                                    {
                                                        balise = new Balise(matricule, NisBalise);
                                                        try
                                                        {
                                                            boitiers.Add(balise.Matricule, balise);
                                                        }
                                                        catch (ArgumentException argEx)
                                                        {
                                                            logger.Error("impossible d'inseré une nouvelle Balise .", argEx);
                                                        }
                                                    }

                                                    break;
                                                }
                                            case "D":
                                                {
                                                    boitiers.Remove(matricule);
                                                    break;
                                                }
                                            case "I":
                                                {
                                                    balise = new Balise(matricule, NisBalise);
                                                    try
                                                    {
                                                        boitiers.Add(balise.Matricule, balise);
                                                    }
                                                    catch (ArgumentException argEx)
                                                    {
                                                        logger.Error("impossible d'inseré une nouvelle Balise .", argEx);
                                                    }
                                                    break;
                                                }
                                            default: throw new CollecteurSQLSyncException("Mise ajour Des Données des Balises Avec des resultats non Attendus ,code Action : " + action);

                                        }

                                    }
                                    catch (Exception ext)
                                    {
                                        throw new CollecteurSQLSyncException(ext);
                                    }
                                }


                            exec = true;
                        }
                        catch (Exception ex)
                        {
                            i++;
                            logger.Error(String.Format("Erreur N° {0} dans la connexion au serveur BD ", i.ToString()), ex);
                            if (reader != null)
                                reader.Close();
                            oCmd.Cancel();
                            //Console.WriteLine("Erreur N° {0} dans la connexion au serveur BD ", i.ToString());
                            Thread.Sleep(1000);
                        }
                    } while (i < 2 && !exec);
                    oCmd.Dispose();
                    if (!exec)
                    {
                        Console.WriteLine(DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss]") + " Echec Synchronisation");
                        throw new DataBaseCommunicationException("Erreur de synchronisation avec la base ");
                    }
                    else
                    {
                        Console.WriteLine(DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss]") + " Syncronisation reussi.");
                        if (RefrechBalise != null)
                            RefrechBalise.Close();
                        Version = (int)paraReturne.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Erreur dans l'execution proc isConnected pour Boities {0}", ex);
                //Console.WriteLine("Erreur de communication avec la base : " + ex.Message);
                throw new DataBaseCommunicationException("Erreur de synchronisation avec la base ", ex);
            }
            finally
            {
                if (reader != null)
                    reader.Close();

                if (RefrechBalise != null)
                    RefrechBalise.Close();
                logger.Debug("Connexion Base Données Fermé  avec succé.");
            }

            return Version;
        }

        public static int GetAvailableMission(bool Update, int Version, ref List<Mission> missions)
        {

            SqlCommand oCmd;
            SqlDataReader reader = null;
            SqlConnection RefrechBalise = null;
            bool exec = false;
            try
            {
                using (RefrechBalise = new SqlConnection(connectionString))
                {
                    // RefrechBalise = openRefrechBaliseConnection();
                    RefrechBalise.Open();
                    oCmd = RefrechBalise.CreateCommand();
                    oCmd.CommandType = CommandType.StoredProcedure;
                    oCmd.CommandText = "ChangedMissionInfo";


                    SqlParameter paraReturne = oCmd.CreateParameter();
                    paraReturne.ParameterName = "@Version";
                    paraReturne.SqlDbType = SqlDbType.Int;
                    paraReturne.Size = 32;
                    paraReturne.Direction = ParameterDirection.InputOutput;
                    paraReturne.Value = Version;
                    oCmd.Parameters.Add(paraReturne);

                    SqlParameter para2 = oCmd.CreateParameter();
                    para2.ParameterName = "@Update";
                    para2.SqlDbType = SqlDbType.Bit;
                    para2.Value = Update;
                    oCmd.Parameters.Add(para2);

                    int i = 0;
                    do
                    {
                        try
                        {
                            reader = oCmd.ExecuteReader();
                            // Version = (int)paraReturne.Value;
                            if (!Update)
                                while (reader.Read())
                                {

                                    Mission mission = new Mission(reader.GetString(5), reader.GetDateTime(1), reader.GetDateTime(2), reader.GetString(0), reader.GetInt32(3), reader.GetString(4), reader.GetBoolean(6));
                                    missions.Add(mission);

                                }
                            else
                                while (reader.Read())
                                {
                                    try
                                    {
                                        Mission mission = new Mission(reader.GetString(5), reader.GetDateTime(1), reader.GetDateTime(2), reader.GetString(0), reader.GetInt32(3), reader.GetString(4), reader.GetBoolean(6));

                                        String action = reader.GetString(7);
                                        switch (action)
                                        {
                                            case "U":
                                                {
                                                    CollectionUtils.remove(mission, missions);
                                                    break;
                                                }
                                            case "D":
                                                {
                                                    CollectionUtils.remove(mission, missions);
                                                    break;
                                                }
                                            case "I":
                                                {
                                                    missions.Add(mission);
                                                    break;
                                                }
                                            default: throw new CollecteurSQLSyncException("Mise ajour Des Données des Missions Avec des resultats non Attendus ,code Action : " + action);

                                        }

                                    }
                                    catch (Exception ext)
                                    {
                                        throw new CollecteurSQLSyncException(ext);
                                    }
                                }


                            exec = true;
                        }
                        catch (Exception ex)
                        {
                            i++;
                            logger.Error(String.Format("Erreur N° {0} dans la connexion au serveur BD ", i.ToString()), ex);
                            if (reader != null)
                                reader.Close();
                            oCmd.Cancel();
                            Console.WriteLine("Erreur N° {0} dans la connexion au serveur BD ", i.ToString());
                            Thread.Sleep(1000);
                        }
                    } while (i < 2 && !exec);
                    oCmd.Dispose();
                    if (!exec)
                    {
                        Console.WriteLine("Echec Synchronisation mission ");
                        throw new DataBaseCommunicationException("Erreur de synchronisation avec la base ");
                    }
                    else
                    {
                        //Console.WriteLine("Syncronisation Mission reussi.");
                        if (RefrechBalise != null)
                            RefrechBalise.Close();
                        Version = (int)paraReturne.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Erreur dans l'execution proc ChangedMissionInfo pour Boities {0}", ex);
                //Console.WriteLine("Erreur de communication avec la base : " + ex.Message);
                throw new DataBaseCommunicationException("Erreur de synchronisation des mission avec la base ", ex);
            }
            finally
            {
                if (reader != null)
                    reader.Close();

                if (RefrechBalise != null)
                    RefrechBalise.Close();
                logger.Debug("Connexion Base Données Fermé  avec succé.");
            }

            return Version;
        }

        public static void insertAllTrame(List<TrameReal> dataQueueCopy)
        {
            SqlConnection sqlConnection = null;
            DataTable dataTable = new DataTable("TypeTrames");

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
                dataTable.Columns.Add("TempsReception", typeof(DateTime));

                dataTable.PrimaryKey = new DataColumn[] {dataTable.Columns["temps"],
                                         dataTable.Columns["NISBalise"]};


                foreach (TrameReal trame in dataQueueCopy)
                {
                    try
                    {
                        //Console.WriteLine("Nbre de trames pas encore traité {0}.", nbrTrame.ToString());
                        dataTable.Rows.Add(trame.Temps, Math.Round(trame.Longitude, 5), Math.Round(trame.Latitude, 5),
                        Math.Round((Decimal)trame.Vitesse, 1), trame.Direction, trame.Temperature,
                        trame.Capteur, trame.Chauffeur, trame.NisBalise, trame.TempsReception);
                    }
                    catch (Exception e)
                    {
                        Logging("TrameDataDupliques", "Insertion de trames dupliqués.", e);
                    }

                }


                bool exec = false;

                sqlConnection = openConnection();
                SqlCommand command = sqlConnection.CreateCommand();
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.CommandText = "[dbo].[InsertDataTrame]";
                command.CommandTimeout = 500;
                SqlParameter parameter = new SqlParameter();

                parameter.ParameterName = "@Sample";
                parameter.SqlDbType = SqlDbType.Structured;
                parameter.Value = dataTable;
                command.Parameters.Add(parameter);



                int i = 0;
                do
                {
                    try
                    {
                        int numberOfRowsUpdated = command.ExecuteNonQuery();
                        exec = true;

                    }
                    catch (Exception ex)
                    {
                        i++;
                        command.Cancel();
                        //Console.WriteLine("Insertion des trames Erreur N° {0}. {1}", i.ToString(), ex.Message);
                        Logging("TrameData", "Insertion des trames Erreur dans une tentative no:" + i.ToString(), ex);
                        Thread.Sleep(1000);

                    }
                } while (i < 2 && !exec);

                if (!exec)
                {

                    Logging("TrameData", "liste des trames restocké dans le depot nbr : " + dataQueueCopy.Count);
                    //Logging("TrameErreur", String.Join(Environment.NewLine, dataQueueCopy));
                    if (dataQueueCopy.Count < 500)
                    {
                        OLDModelGeneratorProcessor.addNonInsetedTrame(dataQueueCopy);
                        dataQueueCopy.Clear();
                    }
                    else
                    {
                        LoggingXML("TrameData", dataTable);
                    }
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
                if (dataQueueCopy.Count < 500)
                    OLDModelGeneratorProcessor.addNonInsetedTrame(dataQueueCopy);
                else
                    LoggingXML("TrameData", dataTable);

                Logging("TrameData", "la table de trames ", ex);
            }
            finally
            {
                if (sqlConnection != null)
                    sqlConnection.Close();
            }

        }
 
        public static void insertAllTrameSonde(List<TrameSonde> dataQueueCopy)
        {
            SqlConnection sqlConnection = null;
            DataTable dataTable = new DataTable("TypeEcoTrames");

            try
            {

                dataTable.Columns.Add("NISBalise", typeof(string));
                dataTable.Columns.Add("Temps", typeof(DateTime));

                dataTable.Columns.Add("Volume1", typeof(Int32));
                dataTable.Columns.Add("Temperature1", typeof(Int16));

                dataTable.Columns.Add("Volume2", typeof(Int32));
                dataTable.Columns.Add("Temperature2", typeof(Int16));

                dataTable.Columns.Add("Volume3", typeof(Int32));
                dataTable.Columns.Add("Temperature3", typeof(Int16));

                dataTable.Columns.Add("Volume4", typeof(Int32));
                dataTable.Columns.Add("Temperature4", typeof(Int16));

                dataTable.Columns.Add("witesse", typeof(Int16));

                dataTable.Columns.Add("Moteur", typeof(byte));

                dataTable.Columns.Add("longitude", typeof(Decimal));
                dataTable.Columns.Add("latitude", typeof(Decimal));

                dataTable.PrimaryKey = new DataColumn[] {dataTable.Columns["Temps"],
                                         dataTable.Columns["NISBalise"]};


                foreach (TrameSonde trame in dataQueueCopy)
                {
                    try
                    {


                        //Console.WriteLine("Nbre de trames pas encore traité {0}.", nbrTrame.ToString());
                        dataTable.Rows.Add(trame.NisBalise, trame.Temps
                            , trame.Volume1, trame.Temperature1
                            , trame.Volume2, trame.Temperature2
                            , trame.Volume3, trame.Temperature3
                            , trame.Volume4, trame.Temperature4
                            , trame.Witesse
                            , trame.Moteur
                            , Math.Round(trame.Longitude, 5)
                               , Math.Round(trame.Latitude, 5)
                            ); ; ;
                    }
                    catch (Exception e)
                    {
                        Logging("TrameSondeDataDupliqus", "Insertion de trames dupliqués.", e);
                    }

                }


                bool exec = false;

                sqlConnection = openConnectionSonde();

                SqlCommand command = sqlConnection.CreateCommand();
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.CommandText = "[dbo].[insertDataSonde]";
                command.CommandTimeout = 500;
                SqlParameter parameter = new SqlParameter();

                parameter.ParameterName = "@Sample";
                parameter.SqlDbType = SqlDbType.Structured;
                parameter.Value = dataTable;
                command.Parameters.Add(parameter);



                int i = 0;
                do
                {
                    try
                    {
                        int numberOfRowsUpdated = command.ExecuteNonQuery();
                        //Console.WriteLine("Nombre de rows updated = " + numberOfRowsUpdated.ToString());
                        exec = true;
                    }
                    catch (Exception ex)
                    {
                        i++;
                        command.Cancel();

                        Logging("TrameSondeData", "Insertion des trames Erreur dans une tentative no:" + i.ToString(), ex);
                        Thread.Sleep(1000);

                    }
                } while (i < 2 && !exec);

                if (!exec)
                {
                    Logging("TrameSondeData", "liste des trames restocké dans le depot nbr : " + dataQueueCopy.Count);

                    if (dataQueueCopy.Count < 500)
                    {
                        OLDModelGeneratorProcessor.addNonInsetedTrameSonde(dataQueueCopy);
                        dataQueueCopy.Clear();
                    }
                    else
                    {
                        LoggingXML("TrameSondeData", dataTable);
                    }
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
                if (dataQueueCopy.Count < 500)
                {
                    OLDModelGeneratorProcessor.addNonInsetedTrameSonde(dataQueueCopy);
                    dataQueueCopy.Clear();
                }
                else
                {
                    LoggingXML("TrameSondeData", dataTable);
                }

                Logging("TrameEcoData", "la table de trames ", ex);
            }
            finally
            {
                if (sqlConnection != null)
                    sqlConnection.Close();
            }

        }

        public static void insertAllTrameMission(List<TrameReal> dataQueueCopy)
        {
            SqlConnection sqlConnection = null;
            DataTable dataTable = new DataTable("TypeTramesMission");

            try
            {
                dataTable.Columns.Add("temps", typeof(DateTime));
                dataTable.Columns.Add("longitude", typeof(Decimal));
                dataTable.Columns.Add("latitude", typeof(Decimal));
                dataTable.Columns.Add("vitesse", typeof(Double));
                dataTable.Columns.Add("NMis", typeof(string));
                dataTable.Columns.Add("NIVehicule", typeof(string));
                dataTable.Columns.Add("NParc", typeof(int));
                dataTable.Columns.Add("NISBalise", typeof(string));

                foreach (TrameReal trame in dataQueueCopy)
                {
                    try
                    {
                        //Console.WriteLine("Nbre de trames pas encore traité {0}.", nbrTrame.ToString());
                        dataTable.Rows.Add(trame.Temps, Math.Round(trame.Latitude, 5), Math.Round(trame.Longitude, 5),
                        Math.Round((Decimal)trame.Vitesse, 1), trame.Mission.NomMis, trame.Mission.Nivehicule,
                        trame.Mission.NParc, trame.NisBalise);
                    }
                    catch (Exception e)
                    {
                        Logging("TrameMission", "Insertion de trames error.", e);
                    }

                }


                bool exec = false;

                sqlConnection = openMissionConnection();
                SqlCommand command = sqlConnection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "[dbo].[InsertTrameMission]";
                command.CommandTimeout = 500;
                SqlParameter parameter = new SqlParameter();

                parameter.ParameterName = "@Sample";
                parameter.SqlDbType = SqlDbType.Structured;
                parameter.Value = dataTable;
                command.Parameters.Add(parameter);

                parameter = command.CreateParameter();
                parameter.ParameterName = "@message";
                parameter.DbType = DbType.String;
                parameter.Size = 100;
                parameter.Value = "";
                parameter.Direction = ParameterDirection.InputOutput;
                command.Parameters.Add(parameter);


                int i = 0;
                do
                {
                    try
                    {
                        int numberOfRowsUpdated = command.ExecuteNonQuery();
                        //Console.WriteLine("Mission rows updated = " + numberOfRowsUpdated.ToString());
                        exec = true;
                    }
                    catch (Exception ex)
                    {
                        i++;
                        command.Cancel();
                        //Console.WriteLine("Insertion des tramesmission Erreur N° {0}. {1}", i.ToString(), ex.Message);
                        Logging("TrameMission", "Insertion des trames mission  Erreur dans une tentative no:" + i.ToString(), ex);
                        Thread.Sleep(1000);

                    }
                } while (i < 2 && !exec);

                if (!exec)
                {
                    //Console.WriteLine("BD Server ne repond pas, sauvegarde du contexte en cours.");
                    Logging("TrameMission", "liste des trames restocké dans le depot nbr : " + dataQueueCopy.Count);
                    //  OLDModelGeneratorProcessor.addNonInsetedTrame(dataQueueCopy);
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
                //Console.WriteLine("la table de trames mission n'a pas pu s'initialiser.");

                Logging("TrameMission", "la table de trames ", ex);
            }
            finally
            {
                if (sqlConnection != null)
                    sqlConnection.Close();
            }

        }

        public static void insertAllTrajets(List<Trajet> dataQueueCopy, int nbrTrajetAdded)
        {

            SqlConnection sqlConnection = null;
            Logging("Trajet", string.Format("nombre de trajet consommé {0}, reste :{1}", dataQueueCopy.Count, nbrTrajetAdded));
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



                foreach (Trajet trajet in dataQueueCopy)
                {
                    try
                    {
                        //Console.WriteLine("Nbre de trames pas encore traité {0}.", nbrTrame.ToString());
                        if (trajet.VMax >= 5 && trajet.Distance > 0)
                            dataTable.Rows.Add(trajet.DateDebut, trajet.LongitudeDebut, trajet.LatitudeDebut, trajet.DateDebut.AddSeconds(trajet.Duree), trajet.LongitudeFin, trajet.LatitudeFin
                                , trajet.VMax, trajet.Duree, trajet.Distance, trajet.NisBalise);
                    }
                    catch (Exception e)
                    {
                        Logging("Trajet", "Insertion de Trajet dupliqués.", e);

                    }

                }


                sqlConnection = openTrajetConnection();
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
                    Console.WriteLine("Trajets : Nombre de rows updated = " + numberOfRowsUpdated.ToString());

                }
                catch (Exception ex)
                {

                    command.Cancel();
                    //Console.WriteLine("Trajets : Erreur  {0}", ex.Message);
                    Logging("Trajet", "insert erreur ", ex);
                    OLDModelGeneratorProcessor.addNonInsetedTrajet(dataQueueCopy);

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
            catch (Exception ex)
            {
                //Console.WriteLine("la table de Trajets n'a pas pu s'initialiser.");
                Logging("Trajet", "preparation de données erreur ", ex);
                OLDModelGeneratorProcessor.addNonInsetedTrajet(dataQueueCopy);
            }
            finally
            {
                if (sqlConnection != null)
                    sqlConnection.Close();
            }
        }

        public static bool insertAllCapteurs(List<TrameReal> dataQueuePart)
        {
            bool exec = false;
            SqlConnection sqlConnection = null;
            DataTable dataTable = new DataTable("TypeTramesData");
            DataTable dataTableOptions = new DataTable("TypeOptionsAux");

            dataTable.Columns.Add("temps", typeof(DateTime));
            dataTable.Columns.Add("longitude", typeof(Decimal));
            dataTable.Columns.Add("latitude", typeof(Decimal));
            dataTable.Columns.Add("vitesse", typeof(Double));
            dataTable.Columns.Add("direction", typeof(Int16));
            dataTable.Columns.Add("Temperature", typeof(Int16));
            dataTable.Columns.Add("Capteur", typeof(string));
            dataTable.Columns.Add("chauffeur", typeof(string));
            dataTable.Columns.Add("NISBalise", typeof(string));

            //dataTable.PrimaryKey = new DataColumn[] {dataTable.Columns["temps"], 
            //                            dataTable.Columns["NISBalise"]};

            //table des options
            dataTableOptions.Columns.Add("temps", typeof(DateTime));
            dataTableOptions.Columns.Add("NISBalise", typeof(string));
            dataTableOptions.Columns.Add("entree", typeof(Int16));
            dataTableOptions.Columns.Add("value", typeof(Int32));


            //dataTableOptions.PrimaryKey = new DataColumn[] {dataTableOptions.Columns["temps"], dataTableOptions.Columns["entree"],
            //                            dataTableOptions.Columns["NISBalise"]};

            //dataTable.Clear();
            String datemin = "01/01/2000";
            foreach (TrameReal trame in dataQueuePart)
            {
                //Console.WriteLine("Nbre de trames pas encore traité {0}.", nbrTrame.ToString());
                try
                {

                    if (trame.Temps > Convert.ToDateTime(datemin) && !String.IsNullOrEmpty(trame.NisBalise))
                    {
                        dataTable.Rows.Add(trame.Temps, Math.Round(trame.Longitude, 5), Math.Round(trame.Latitude, 5),
                            Math.Round((Decimal)trame.Vitesse, 1), trame.Direction, trame.Temperature,
                            trame.Capteur, trame.Chauffeur, trame.NisBalise);

                        if (trame.Options != null)
                        {
                            foreach (KeyValuePair<int, int?> item in trame.Options)
                            {
                                if (item.Key > 0)
                                    dataTableOptions.Rows.Add(trame.Temps, trame.NisBalise, item.Key, item.Value);
                                //, Math.Round(trame.Longitude, 5), Math.Round(trame.Latitude, 5),
                                //Math.Round((Decimal)trame.Vitesse, 1), (trame.Capteur.Substring(3, 1) == "M") ? 1 : 0);
                                Debug.WriteLine(string.Format("l'option est portant la clé {0} a comme valeur {1}", item.Key.ToString(), item.Value.ToString()));
                            }
                        }
                    }

                }
                catch (System.ArgumentException e)
                {
                    Logging("TrameCapteurs", "Insertion de trames Capteurs dupliqués.", e);
                    return exec;
                }
                catch (System.InvalidCastException e)
                {
                    Logging("TrameCapteurs", "Insertion de trames Capteurs dupliqués.", e);
                    return exec;
                }
                catch (System.Data.ConstraintException e)
                {
                    Logging("TrameCapteurs", "Insertion de trames Capteurs dupliqués.", e);
                    return exec;
                }
                catch (System.Data.NoNullAllowedException e)
                {
                    Logging("TrameCapteurs", "Insertion de trames Capteurs dupliqués.", e);
                    return exec;
                }
                catch (System.OverflowException e)
                {
                    Logging("TrameCapteurs", "Insertion de trames Capteurs dupliqués.", e);
                    return exec;
                }
                catch (Exception e)
                {
                    Logging("TrameCapteurs", "Insertion de trames Capteurs dupliqués.", e);
                    return exec;
                }

            }



            sqlConnection = openCapteursConnection();
            SqlCommand command = sqlConnection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "[dbo].[InsertCapteursTrame]";

            SqlParameter parameter = new SqlParameter();
            command.CommandTimeout = 600;
            parameter.ParameterName = "@Sample";
            parameter.SqlDbType = SqlDbType.Structured;
            parameter.Value = dataTable;
            command.Parameters.Add(parameter);

            SqlParameter parameterOptions = new SqlParameter();
            parameterOptions.ParameterName = "@Options";
            parameterOptions.SqlDbType = SqlDbType.Structured;
            parameterOptions.Value = dataTableOptions;
            command.Parameters.Add(parameterOptions);

            try
            {
                int numberOfRowsUpdated = command.ExecuteNonQuery();
                // Console.WriteLine("Nombre de rows updated = " + numberOfRowsUpdated.ToString());
                dataTableOptions.Clear();
                dataTable.Clear();
                exec = true;
                return exec;
            }
            catch (System.InvalidCastException e)
            {
                command.Cancel();
                Logging("TrameCapteurs", "Erreur Exécution Procedure Capteur Trame:", e);
                return exec;

            }
            catch (System.Data.SqlClient.SqlException e)
            {
                command.Cancel();
                Logging("TrameCapteurs", "Erreur Exécution Procedure Capteur Trame:", e);
                return exec;
            }
            catch (System.IO.IOException e)
            {
                command.Cancel();
                Logging("TrameCapteurs", "Erreur Exécution Procedure Capteur Trame:", e);
                return exec;
            }
            catch (System.InvalidOperationException e)
            {
                command.Cancel();
                Logging("TrameCapteurs", "Erreur Exécution Procedure Capteur Trame:", e);
                return exec;
            }
            catch (Exception e)
            {
                command.Cancel();
                Logging("TrameCapteurs", "Erreur Exécution Procedure Capteur Trame:", e);
                return exec;

            }
            finally
            {
                if (sqlConnection != null)
                    sqlConnection.Close();

                //Thread.Sleep(1000);
            }

        }


        public static bool insertAllCapteursSondeTemperateur(List<TrameReal> dataQueuePart)
        {
            bool exec = false;
            SqlConnection sqlConnection = null;
            DataTable dataTable = new DataTable("TypeTramesData");
            DataTable dataTableOptions = new DataTable("TypeOptionsAux");

            dataTable.Columns.Add("temps", typeof(DateTime));
            dataTable.Columns.Add("longitude", typeof(Decimal));
            dataTable.Columns.Add("latitude", typeof(Decimal));
            dataTable.Columns.Add("vitesse", typeof(Double));
            dataTable.Columns.Add("direction", typeof(Int16));
            dataTable.Columns.Add("Temperature", typeof(Int16));
            dataTable.Columns.Add("Capteur", typeof(string));
            dataTable.Columns.Add("chauffeur", typeof(string));
            dataTable.Columns.Add("NISBalise", typeof(string));

            //dataTable.PrimaryKey = new DataColumn[] {dataTable.Columns["temps"], 
            //                            dataTable.Columns["NISBalise"]};

            //table des options
            dataTableOptions.Columns.Add("temps", typeof(DateTime));
            dataTableOptions.Columns.Add("NISBalise", typeof(string));
            dataTableOptions.Columns.Add("entree", typeof(Int16));
            dataTableOptions.Columns.Add("value", typeof(Int32));


            //dataTableOptions.PrimaryKey = new DataColumn[] {dataTableOptions.Columns["temps"], dataTableOptions.Columns["entree"],
            //                            dataTableOptions.Columns["NISBalise"]};

            //dataTable.Clear();
            String datemin = "01/01/2000";
            foreach (TrameReal trame in dataQueuePart)
            {
                //Console.WriteLine("Nbre de trames pas encore traité {0}.", nbrTrame.ToString());
                try
                {

                    if (trame.Temps > Convert.ToDateTime(datemin) && !String.IsNullOrEmpty(trame.NisBalise))
                    {
                        dataTable.Rows.Add(trame.Temps, Math.Round(trame.Longitude, 5), Math.Round(trame.Latitude, 5),
                            Math.Round((Decimal)trame.Vitesse, 1), trame.Direction, trame.Temperature,
                            trame.Capteur, trame.Chauffeur, trame.NisBalise);

                        if (trame.Options != null)
                        {
                            foreach (KeyValuePair<int, int?> item in trame.Options)
                            {
                                if (item.Key > 0)
                                    dataTableOptions.Rows.Add(trame.Temps, trame.NisBalise, item.Key, item.Value);
                                //, Math.Round(trame.Longitude, 5), Math.Round(trame.Latitude, 5),
                                //Math.Round((Decimal)trame.Vitesse, 1), (trame.Capteur.Substring(3, 1) == "M") ? 1 : 0);
                                Debug.WriteLine(string.Format("l'option est portant la clé {0} a comme valeur {1}", item.Key.ToString(), item.Value.ToString()));
                            }
                        }
                    }

                }
                catch (System.ArgumentException e)
                {
                    Logging("TrameCapteurs", "Insertion de trames Capteurs dupliqués.", e);
                    return exec;
                }
                catch (System.InvalidCastException e)
                {
                    Logging("TrameCapteurs", "Insertion de trames Capteurs dupliqués.", e);
                    return exec;
                }
                catch (System.Data.ConstraintException e)
                {
                    Logging("TrameCapteurs", "Insertion de trames Capteurs dupliqués.", e);
                    return exec;
                }
                catch (System.Data.NoNullAllowedException e)
                {
                    Logging("TrameCapteurs", "Insertion de trames Capteurs dupliqués.", e);
                    return exec;
                }
                catch (System.OverflowException e)
                {
                    Logging("TrameCapteurs", "Insertion de trames Capteurs dupliqués.", e);
                    return exec;
                }
                catch (Exception e)
                {
                    Logging("TrameCapteurs", "Insertion de trames Capteurs dupliqués.", e);
                    return exec;
                }

            }

            // le procecdure InsertCapteursTrame est remplace par InsertCapteursNumilog
            // l'objectif est effectuer la division décimale de valeur temperateur sur 10 par le procedure stocke "InsertCapteursNumilog"  et afficher la temperateur avec le virgule 
            // au lieu d'envoyer le resultat de la division euclidienne sur 10 au procedure stocke "InsertCapteursNumilog" 

            sqlConnection = openCapteursSondeTemperateurConnection();
            SqlCommand command = sqlConnection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "[dbo].[InsertCapteursNumilog]";

            SqlParameter parameter = new SqlParameter();
            command.CommandTimeout = 600;
            parameter.ParameterName = "@Sample";
            parameter.SqlDbType = SqlDbType.Structured;
            parameter.Value = dataTable;
            command.Parameters.Add(parameter);

            SqlParameter parameterOptions = new SqlParameter();
            parameterOptions.ParameterName = "@Options";
            parameterOptions.SqlDbType = SqlDbType.Structured;
            parameterOptions.Value = dataTableOptions;
            command.Parameters.Add(parameterOptions);

            try
            {
                int numberOfRowsUpdated = command.ExecuteNonQuery();
                // Console.WriteLine("Nombre de rows updated = " + numberOfRowsUpdated.ToString());
                dataTableOptions.Clear();
                dataTable.Clear();
                exec = true;
                return exec;
            }
            catch (System.InvalidCastException e)
            {
                command.Cancel();
                Logging("TrameCapteurs", "Erreur Exécution Procedure Capteur Trame:", e);
                return exec;

            }
            catch (System.Data.SqlClient.SqlException e)
            {
                command.Cancel();
                Logging("TrameCapteurs", "Erreur Exécution Procedure Capteur Trame:", e);
                return exec;
            }
            catch (System.IO.IOException e)
            {
                command.Cancel();
                Logging("TrameCapteurs", "Erreur Exécution Procedure Capteur Trame:", e);
                return exec;
            }
            catch (System.InvalidOperationException e)
            {
                command.Cancel();
                Logging("TrameCapteurs", "Erreur Exécution Procedure Capteur Trame:", e);
                return exec;
            }
            catch (Exception e)
            {
                command.Cancel();
                Logging("TrameCapteurs", "Erreur Exécution Procedure Capteur Trame:", e);
                return exec;

            }
            finally
            {
                if (sqlConnection != null)
                    sqlConnection.Close();

                //Thread.Sleep(1000);
            }

        }

        public static void insertAllTrameBrute(List<Trame> dataQueueCopy)
        {
            SqlConnection sqlConnection = null;


            try
            {


                sqlConnection = openTrameBruteConnection();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = sqlConnection;
                foreach (Trame trame in dataQueueCopy)
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
            catch (Exception e)
            {
                Logging("TrameBrutte", "Insertion de trames Capteurs dupliqués.", e);
            }

            finally
            {
                if (sqlConnection != null)
                    sqlConnection.Close();
            }
        }

        internal static void insertAllPOIs(List<TrameReal> dataQueueCopy)
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


                foreach (TrameReal trame in dataQueueCopy)
                {
                    try
                    {
                        //Console.WriteLine("Nbre de trames pas encore traité {0}.", nbrTrame.ToString());
                        dataTable.Rows.Add(trame.Temps, Math.Round(trame.Longitude, 5), Math.Round(trame.Latitude, 5),
                        Math.Round((Decimal)trame.Vitesse, 1), trame.Direction, trame.Temperature,
                        trame.Capteur, trame.Chauffeur, trame.NisBalise);
                    }
                    catch (Exception e)
                    {
                        Logging("TramePOIs", "Insertion de trames dupliqués.", e);
                    }

                }


                bool exec = false;

                sqlConnection = openConnectionPoisCheck();
                SqlCommand command = sqlConnection.CreateCommand();
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.CommandText = "[dbo].[InsertPOIsTrame]";
                command.CommandTimeout = 500;
                SqlParameter parameter = new SqlParameter();

                parameter.ParameterName = "@Sample";
                parameter.SqlDbType = SqlDbType.Structured;
                parameter.Value = dataTable;
                command.Parameters.Add(parameter);



                int i = 0;
                do
                {
                    try
                    {
                        int numberOfRowsUpdated = command.ExecuteNonQuery();
                        //Console.WriteLine("Nombre de rows updated TramePOIs = " + numberOfRowsUpdated.ToString());
                        exec = true;
                    }
                    catch (Exception ex)
                    {
                        i++;
                        command.Cancel();
                        //Console.WriteLine("Insertion des trames Erreur N° {0}. {1}", i.ToString(), ex.Message);
                        Logging("TramePOIsErreur", "Insertion des trames POIs Erreur dans une tentative no:" + i.ToString(), ex);
                        Thread.Sleep(1000);

                    }
                } while (i < 2 && !exec);

                if (!exec)
                {
                    Console.WriteLine("BD Server ne repond pas, sauvegarde du contexte en cours.");
                    Logging("TramePOIs", "liste des trames restocké dans le depot nbr : " + dataQueueCopy.Count);
                    Logging("TramePOIsErreur", String.Join(Environment.NewLine, dataQueueCopy));
                    //  OLDModelGeneratorProcessor.addNonInsetedTrame(dataQueueCopy);
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
                //Console.WriteLine("la table de trames n'a pas pu s'initialiser.");
                //OLDModelGeneratorProcessor.addNonInsetedTrame(dataQueueCopy);
                Logging("TramePOIsErreur", "la table de trames ", ex);
            }
            finally
            {
                if (sqlConnection != null)
                    sqlConnection.Close();
            }

        }

        //internal static void updateTrailerID(TrailerID IDTrailerCopy)
        //{
        //    DbCommand oCmd;
        //    SqlConnection con = null;
        //    bool exec = false;
        //    DbParameter paraReturne = null;
        //    try
        //    {
        //        con = openForKeyDalasConnection();

        //        oCmd = con.CreateCommand();
        //        oCmd.CommandType = CommandType.StoredProcedure;
        //        oCmd.CommandText = "autoTrailerAffectation";

        //        DbParameter para0 = oCmd.CreateParameter();
        //        para0.ParameterName = "@nisbalise";
        //        para0.DbType = DbType.String;
        //        para0.Size = 32;
        //        para0.Value = IDTrailerCopy.Balise;
        //        oCmd.Parameters.Add(para0);

        //        DbParameter para1 = oCmd.CreateParameter();
        //        para1.ParameterName = "@dateDebut";
        //        para1.DbType = DbType.DateTime;
        //        para1.Value = IDTrailerCopy.Datetime;
        //        oCmd.Parameters.Add(para1);

        //        DbParameter para2 = oCmd.CreateParameter();
        //        para2.ParameterName = "@idTrailer";
        //        para2.DbType = DbType.String;
        //        para2.Value = IDTrailerCopy.IDTrailer;
        //        oCmd.Parameters.Add(para2);

        //        DbParameter para3 = oCmd.CreateParameter();
        //        para3.ParameterName = "@idTrailer";
        //        para3.DbType = DbType.Byte;
        //        para3.Value = IDTrailerCopy.isNewID();
        //        oCmd.Parameters.Add(para3);

        //        paraReturne = oCmd.CreateParameter();
        //        paraReturne.ParameterName = "@message";
        //        paraReturne.DbType = DbType.String;
        //        paraReturne.Size = 100;
        //        paraReturne.Value = "";
        //        paraReturne.Direction = ParameterDirection.InputOutput;
        //        oCmd.Parameters.Add(paraReturne);


        //        try
        //        {
        //            int n = oCmd.ExecuteNonQuery();
        //            exec = true;
        //        }
        //        catch (Exception ex)
        //        {
        //            oCmd.Cancel();
        //            //Console.WriteLine("Erreur dans la connexion au serveur BD du boitier: {0}", IDTrailerCopy.Balise);
        //            Logging("TrailerID", "erreur d'insertion de IDTrailerCopy DALAS ", ex);
        //        }

        //        oCmd.Dispose();
        //        if (!exec)
        //        {
        //            Logging("TrailerID", "Echec d'insertion, balise : " + IDTrailerCopy.Balise);
        //        }
        //        else
        //        {
        //            Console.WriteLine("Boitier {0} ,IDTrailer {1}, execution reussi.", IDTrailerCopy.Balise, IDTrailerCopy.IDTrailer);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Logging("TrailerID", "Echec de preparation de  proc connexion, balise : " + IDTrailerCopy.Balise, ex);
        //    }
        //    finally
        //    {
        //        if (con != null)
        //        {
        //            con.Close();
        //            if (paraReturne != null)
        //                LoggingTrace("TrailerID", String.Format("message de retour de la base {0} ,IDTrailer :{1},Balise : {2}", (String)paraReturne.Value, IDTrailerCopy.IDTrailer, IDTrailerCopy.Balise));
        //        }
        //    }

        //}

        internal static void insertTrailerID(TrailerID IDTrailerCopy)
        {
            DbCommand oCmd;
            SqlConnection con = null;
            bool exec = false;
            DbParameter paraReturne = null;
            try
            {
                con = openForKeyDalasConnection();

                oCmd = con.CreateCommand();
                oCmd.CommandType = CommandType.StoredProcedure;
                oCmd.CommandText = "autoTrailerAffectation";

                DbParameter para0 = oCmd.CreateParameter();
                para0.ParameterName = "@nisbalise";
                para0.DbType = DbType.String;
                para0.Size = 32;
                para0.Value = IDTrailerCopy.Balise;
                oCmd.Parameters.Add(para0);

                DbParameter para1 = oCmd.CreateParameter();
                para1.ParameterName = "@dateDebut";
                para1.DbType = DbType.DateTime;
                para1.Value = IDTrailerCopy.Datetime;
                oCmd.Parameters.Add(para1);

                DbParameter para2 = oCmd.CreateParameter();
                para2.ParameterName = "@idTrailer";
                para2.DbType = DbType.String;
                para2.Value = IDTrailerCopy.IDTrailer;
                oCmd.Parameters.Add(para2);

                DbParameter para3 = oCmd.CreateParameter();
                para3.ParameterName = "@new";
                para3.DbType = DbType.Byte;
                para3.Value = IDTrailerCopy.isNewID();
                oCmd.Parameters.Add(para3);

                paraReturne = oCmd.CreateParameter();
                paraReturne.ParameterName = "@message";
                paraReturne.DbType = DbType.String;
                paraReturne.Size = 100;
                paraReturne.Value = "";
                paraReturne.Direction = ParameterDirection.InputOutput;
                oCmd.Parameters.Add(paraReturne);


                try
                {
                    int n = oCmd.ExecuteNonQuery();
                    exec = true;
                }
                catch (Exception ex)
                {
                    oCmd.Cancel();
                    //Console.WriteLine("Erreur dans la connexion au serveur BD du boitier: {0}", IDTrailerCopy.Balise);
                    Logging("TrailerID", "erreur d'insertion de IDTrailerCopy DALAS ", ex);
                }

                oCmd.Dispose();
                if (!exec)
                {
                    Logging("TrailerID", "Echec d'insertion, balise : " + IDTrailerCopy.Balise);
                }
                else
                {
                    Console.WriteLine("Boitier {0} ,IDTrailer {1}, execution reussi.", IDTrailerCopy.Balise, IDTrailerCopy.IDTrailer);
                }
            }
            catch (Exception ex)
            {
                Logging("TrailerID", "Echec de preparation de  proc connexion, balise : " + IDTrailerCopy.Balise, ex);
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                    if (paraReturne != null)
                        LoggingTrace("TrailerID", String.Format("message de retour de la base {0} ,IDTrailer :{1},Balise : {2}", (String)paraReturne.Value, IDTrailerCopy.IDTrailer, IDTrailerCopy.Balise));
                }
            }
        }

        internal static void insertTrameSarens(List<TrameReal> dataQueuePart)
        {
            //bool exec = false;
            DataTable dataTable = new DataTable("TypeSarensTrame");
            //DataTable dataTableOptions = new DataTable("TypeOptionsAux");

            dataTable.Columns.Add("Temps", typeof(DateTime));
            dataTable.Columns.Add("NISBalise", typeof(string));
            dataTable.Columns.Add("Lat", typeof(Decimal));
            dataTable.Columns.Add("Lng", typeof(Decimal));
            dataTable.Columns.Add("Speed", typeof(Decimal));
            dataTable.Columns.Add("Heading", typeof(Int16));
            dataTable.Columns.Add("hdop", typeof(Int16));
            dataTable.Columns.Add("Odometre", typeof(int));
            dataTable.Columns.Add("Mainpower", typeof(Int16));
            dataTable.Columns.Add("backuppower", typeof(Int16));
            dataTable.Columns.Add("Input_1", typeof(byte));
            dataTable.Columns.Add("Input_2", typeof(byte));
            dataTable.Columns.Add("Input_3", typeof(byte));
            dataTable.Columns.Add("Analog_1", typeof(Int16));
            dataTable.Columns.Add("Analog_2", typeof(Int16));
            dataTable.Columns.Add("Moteur", typeof(byte));


            //dataTable.PrimaryKey = new DataColumn[] {dataTable.Columns["temps"], 
            //                            dataTable.Columns["NISBalise"]};

            //dataTable.Clear();
            const String datemin = "01/01/2000";
            foreach (TrameReal trame in dataQueuePart)
            {
                //Console.WriteLine("Nbre de trames pas encore traité {0}.", nbrTrame.ToString());
                try
                {

                    if (trame.Temps > Convert.ToDateTime(datemin) && !String.IsNullOrEmpty(trame.NisBalise))
                    {
                        byte Input1 = 0, Input2 = 0, Input3 = 0;
                        Int16 hdop = -1, mainpower = -1, backuppower = -1, Analog1 = -1, Analog2 = -1;
                        int odometre = -1;

                        if (trame.Options != null)
                        {
                            foreach (KeyValuePair<int, int?> item in trame.Options)
                            {
                                switch (item.Key)
                                {
                                    case 80:
                                        Input1 = (byte)item.Value;
                                        break;
                                    case 81:
                                        Input2 = (byte)item.Value;
                                        break;
                                    case 82:
                                        Input3 = (byte)item.Value;
                                        break;
                                    case 83:
                                        Analog1 = (Int16)item.Value;
                                        break;
                                    case 84:
                                        Analog2 = (Int16)item.Value;
                                        break;
                                    case 71:
                                        mainpower = (Int16)item.Value;
                                        break;
                                    case 72:
                                        backuppower = (Int16)item.Value;
                                        break;
                                    default:
                                        break;
                                }

                                Debug.WriteLine(string.Format("l'option portant la clé {0} a comme valeur {1}", item.Key.ToString(), item.Value.ToString()));

                            }
                        }

                        dataTable.Rows.Add(trame.Temps, trame.NisBalise, Math.Round(trame.Longitude, 5), Math.Round(trame.Latitude, 5),
                            Math.Round((Decimal)trame.Vitesse, 1), trame.Direction, hdop, odometre,
                            mainpower, backuppower, Input1, Input2, Input3, Analog1, Analog2, (byte)(trame.Capteur.EndsWith("M", true, null) || trame.Capteur.EndsWith("B", true, null) ? 1 : 0));
                    }

                }
                catch (System.ArgumentException e)
                {
                    Logging("TrameSarens", "Insertion de trames Sarens dupliqués.", e);
                    return;
                }
                catch (System.InvalidCastException e)
                {
                    Logging("TrameSarens", "Insertion de trames Sarens dupliqués.", e);
                    return;
                }
                catch (System.Data.ConstraintException e)
                {
                    Logging("TrameSarens", "Insertion de trames Sarens dupliqués.", e);
                    return;
                }
                catch (System.Data.NoNullAllowedException e)
                {
                    Logging("TrameSarens", "Insertion de trames Sarens dupliqués.", e);
                    return;
                }
                catch (System.OverflowException e)
                {
                    Logging("TrameSarens", "Insertion de trames Sarens dupliqués.", e);
                    return;
                }
                catch (Exception e)
                {
                    Logging("TrameSarens", "Insertion de trames Sarens dupliqués.", e);
                    return;
                }

            }

            SqlConnection sqlSarensConnection = openSarensConnection();
            SqlCommand command = sqlSarensConnection.CreateCommand();
            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.CommandText = "[dbo].[InsertSarensTrame]";

            SqlParameter parameter = new SqlParameter();
            command.CommandTimeout = 600;
            parameter.ParameterName = "@Sample";
            parameter.SqlDbType = SqlDbType.Structured;
            parameter.Value = dataTable;
            command.Parameters.Add(parameter);

            try
            {
                int numberOfRowsUpdated = command.ExecuteNonQuery();
                //Console.WriteLine("Nombre de rows updated = " + numberOfRowsUpdated.ToString());
                dataTable.Clear();
                return;
            }
            catch (System.InvalidCastException e)
            {
                command.Cancel();
                Logging("TrameSarens", "Erreur Exécution Procedure Capteur Trame:", e);
                return;
            }
            catch (System.Data.SqlClient.SqlException e)
            {
                command.Cancel();
                Logging("TrameSarens", "Erreur Exécution Procedure Capteur Trame:", e);
                return;
            }
            catch (System.IO.IOException e)
            {
                command.Cancel();
                Logging("TrameSarens", "Erreur Exécution Procedure Capteur Trame:", e);
                return;
            }
            catch (System.InvalidOperationException e)
            {
                command.Cancel();
                Logging("TrameSarens", "Erreur Exécution Procedure Capteur Trame:", e);
                return;
            }
            catch (Exception e)
            {
                command.Cancel();
                Logging("TrameSarens", "Erreur Exécution Procedure Capteur Trame:", e);
                return;

            }
            finally
            {
                if (sqlSarensConnection != null)
                    sqlSarensConnection.Close();
                //Thread.Sleep(1000);
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
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void Logging(String erreur, String message)
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
                    sw.Close();
                }
            }
            catch (Exception)
            {

            }
        }


        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void LoggingXML(String filename, DataTable table)
        {
            String path = "Log\\NonInsetedData\\" + DateTime.Now.ToString("yyyy-MM-dd");
            Int32 unixTimestamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;


            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                table.WriteXml(path + "\\" + filename + "-" + unixTimestamp.ToString() + ".log");


            }
            catch (Exception)
            {

            }
        }


        [MethodImpl(MethodImplOptions.Synchronized)]
        private static void LoggingTrace(String trace, String message)
        {
            String path = "Log\\" + DateTime.Now.ToString("yyyy-MM-dd");
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                using (StreamWriter sw = File.AppendText(path + "\\trace_" + trace + ".log"))
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


