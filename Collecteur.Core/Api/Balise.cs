
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using XMLSerializer;
using System.Net.Sockets;
using System.IO;

namespace Collecteur.Core.Api
{
    [Serializable]
    public class Balise : Serialise
    {
        private int matricule;
        public BaliseInfo baliseInfo { get; set; }
        public int Matricule
        {
            get { return matricule; }
            set { matricule = value; }
        }
        private Trajet trajeEnCours;

        public Trajet TrajeEnCours
        {
            get { return trajeEnCours; }
            set { trajeEnCours = value; }
        }
        public bool Stat { get; set; }
        private int lastID;

        [NonSerialized]
        private List<String> trameNonValide;


        [NonSerialized]
        private Socket activeSocket;
        public Socket ActiveSocket
        {
            get { return activeSocket; }
            set
            {
                activeSocket = value;
            }
        }

        public String[] listCommande { get; set; }
        
        public List<String> TrameNonValide
        {
            get { return trameNonValide; }
            set { trameNonValide = value; }
        }

        public int LastID
        {
            get { return lastID; }
            set { lastID = value; }
        }


        private string puce;

        public string Puce
        {
            get { return puce; }
            set { puce = value; }
        }

        private string nisbalise;

        public string Nisbalise
        {
            get { return nisbalise; }
            set { nisbalise = value; }
        }

        private TrameReal trameReel;

        public TrameReal TrameReel
        {
            get { return trameReel; }
            set { trameReel = value; }
        }

        private string trameValue;

        public string TrameValue
        {
            get { return trameValue; }
            set { trameValue = value; }
        }


        public String toSQL()
        {
            String insertRequest = "INSERT INTO [dbo].[T_Depot] ([trameBrute],[NISBalise],[gpsDate]) VALUES ";
            bool first = true;
            foreach (String unitTrame in this.TrameValue.Split(this.baliseInfo.trameSeparator, System.StringSplitOptions.RemoveEmptyEntries))
            {
                if (unitTrame.Trim() == "#")
                    continue;
                insertRequest += (first) ? ("") : ",";
                first = false;
                insertRequest += "('" + unitTrame + "','" + this.Nisbalise + "', GETDATE() )";
            }
            return insertRequest;
        }

        public Balise(Socket s)
        {
            this.baliseInfo = new BaliseInfo();
            this.Stat = true;
            this.activeSocket = s;
            this.Nisbalise = null;
            this.TrameReel = null;
            TrajeEnCours = new Trajet();
        }
       
        override
        public string ToString()
        {
            return this.Nisbalise;

        }
        public Balise(int mat, string nis, string commande)
        {
            matricule = mat;
            this.listCommande = commande.Split('|');
            nisbalise = nis;
            lastID = 0;
            this.Stat = false;
            this.TrameReel = null;
            this.baliseInfo = new BaliseInfo();
            this.activeSocket = null;
        }
        public Balise(int mat, string nis)
        {
            matricule = mat;
            this.Stat = false;
            nisbalise = nis;
            lastID = 0;
            this.TrameReel = null;
            this.baliseInfo = new BaliseInfo();
            this.activeSocket = null;
        }

        public Balise(string nis)
        {
           
            this.Stat = false;
            nisbalise = nis;
            lastID = 0;
            this.TrameReel = null;
            this.baliseInfo = new BaliseInfo();
            this.activeSocket = null;
        }
        /*
        private void SocketCLOSE()
        {
            try
            {
                //LoggingConnection("Info", string.Format("(SocketCLOSE Method)about to close socket from of NISBalise: {0} from IP {1}", this.Nisbalise, this.activeSocket.RemoteEndPoint.ToString()));
                if (this.activeSocket != null)
                {
                    this.activeSocket.Shutdown(SocketShutdown.Both);
                    this.activeSocket.Close();
                }
            }
            catch { }
            this.activeSocket = null;
        }

        private void SocketCLOSE(Socket s)
        {
            this.Stat = false;
            try
            {
                //LoggingConnection("Info", string.Format("(SocketCLOSE(s) Method)about to close socket from of NISBalise: {0} from IP {1}", this.Nisbalise, this.activeSocket.RemoteEndPoint.ToString()));
                if (this.activeSocket != null)
                {
                    this.activeSocket.Shutdown(SocketShutdown.Both);
                    this.activeSocket.Close();
                }
            }
            catch { }
            this.activeSocket = null;
        }

        //public bool SocketCheck(Socket s)
        //{
        //    try
        //    {
        //        if (this.activeSocket.Handle == s.Handle)
        //            return true;
        //        return false;

        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine("Socket equals check error: " + e.Message);
        //        return false;
        //    }
        //    catch 
        //    {
        //        return false;
        //    }
            
        //}

        public void SocketUpdate(Socket s)
        {
            if (this.activeSocket != null)
            {
                //LoggingConnection("Info", string.Format("(SocketUpdate Method)about to close socket from of NISBalise: {0} from IP {1}",this.Nisbalise,this.activeSocket.RemoteEndPoint.ToString()));
                SocketCLOSE();
                //LoggingConnection("Info", string.Format("(SocketUpdate Method)Socket closed from of NISBalise: {0} from IP {1}", this.Nisbalise, this.activeSocket.RemoteEndPoint.ToString()));
            }
            //LoggingConnection("Info", string.Format("(SocketUpdate Method)Socket closed from of NISBalise: {0} from IP {1}", this.Nisbalise, s.RemoteEndPoint.ToString()));
            try
            {
                //LoggingConnection("Info", string.Format("(SocketUpdate Method)new socket assigned to NISBalise: {0}. new socket  IP {1}", this.Nisbalise == null ? "" : this.Nisbalise, s.RemoteEndPoint.ToString()));
                this.activeSocket = s;
                this.Stat = true;
            }
            catch(Exception e)
            {
                //LoggingConnection("Info", e.Message);
            }
            
        }

        public void SetNullSocket()
        {
            this.Stat = false;
            try
            {
                //LoggingConnection("Info", string.Format("(SetNullSocket Method)about to close socket from of NISBalise: {0} from IP {1}", this.Nisbalise, this.activeSocket.RemoteEndPoint.ToString()));
                if (this.activeSocket != null)
                {
                    this.activeSocket.Shutdown(SocketShutdown.Both);
                    this.activeSocket.Close();
                }
                    
                //LoggingConnection("Info", string.Format("(SetNullSocket Method)about to close socket from of NISBalise: {0} from IP {1}", this.Nisbalise, this.activeSocket.RemoteEndPoint.ToString()));
            }
            catch { }
            
            this.activeSocket = null;
        }
       
        public string getIPAdresse()
        {
            try
            {
                if (this.activeSocket != null)
                    return this.activeSocket.RemoteEndPoint.ToString();

                return ""; 
            }
            catch
            {
                return "";
            }
                          
        }

        */
        //#region private methods
        //private void Logging(String message)
        //{

        //    try
        //    {

        //        String path = "Log\\" + DateTime.Now.ToString("dd-MM-yyyy");

        //        if (!Directory.Exists(path))
        //        {
        //            Directory.CreateDirectory(path);
        //        }

        //        using (StreamWriter sw = File.AppendText(path + "\\Balises-" + ((this != null) ? (this.Matricule) + "" : "Innconu") + ".log"))
        //        {
        //            sw.WriteLine(DateTime.Now + ": " + message);
        //            sw.WriteLine(" @IP  : " + this.ActiveSocket.RemoteEndPoint.ToString());
        //            sw.Close();
        //        }

        //    }
        //    catch (Exception)
        //    {

        //    }

        //}
        //private static void Logging(String message, string balise)
        //{
        //    try
        //    {

        //        String path = "Log\\Trame\\" + DateTime.Now.ToString("dd-MM-yyyy");

        //        if (!Directory.Exists(path))
        //        {
        //            Directory.CreateDirectory(path);
        //        }


        //        using (StreamWriter sw = File.AppendText(path + "\\Balises-" + balise + ".log"))
        //        {
        //            sw.WriteLine(DateTime.Now + ": " + message);
        //            sw.Close();
        //        }
        //    }
        //    catch (Exception)
        //    {


        //    }
        //}
        //private void LoggingConnection(string type, String message)
        //{
        //    try
        //    {

        //        String path = "Log\\" + DateTime.Now.ToString("dd-MM-yyyy") + "\\connection" + type;

        //        if (!Directory.Exists(path))
        //        {
        //            Directory.CreateDirectory(path);
        //        }


        //        using (StreamWriter sw = File.AppendText(path + "\\Balises-" + ((this != null) ? (this.Matricule) + "" : "Innconu") + ".log"))
        //        {
        //            sw.WriteLine(DateTime.Now + ": " + message);
        //            sw.Close();
        //        }
        //    }
        //    catch (Exception)
        //    {


        //    }
        //}
        //#endregion
    }
}
