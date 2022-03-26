
using System;

using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Security.Principal;
using System.Net.NetworkInformation;
using Collecteur.Core.Api;

namespace OldCollecteur
{
    public class Collect
    {
        private static object serverLock = new object();
        private static object log = new object();
        private static object cBuffer = new object();
        private static object dBuffer = new object();
        //private static Mutex _login = new Mutex();
        //private static bool showText = true;
        private static string OK = "!\r\n";
        private static string NO = "#\r\n";
       
       

            // public List<Balise> listeTrames;

        private static void dataFilter(string trame, string currentDate, string boitier, ref Collecteur.Core.Api.TrameReal tramereal)
            {
                if (trame != null)
                {
                    DateTime TempsReel;
                    try
                    {
                        TempsReel = Convert.ToDateTime(currentDate + ' ' + trame.Substring(3, 2) + ':' + trame.Substring(5, 2) + ':' + trame.Substring(7, 2));
                        // Console.WriteLine("TempsReel: " + TempsReel);
                        if ((TempsReel - DateTime.Now).TotalHours < 24)
                        {
                            // Console.WriteLine("Date invalide: {0}", TempsReel);
                            String Capteur = trame.Substring(32, 4);
                            //Console.WriteLine("Capteur: " + Capteur);

                            String Moteur = trame.Substring(35, 1);
                            //Console.WriteLine("Etat Moteur: " + Moteur);
                            try
                            {
                                bool result;
                                decimal LatitudeReel;
                                //Console.WriteLine(trame.Substring(9, 8));
                                result = Decimal.TryParse(trame.Substring(9, 2), out LatitudeReel);
                                if (result)
                                {
                                    Decimal latDecimal;
                                    bool b = Decimal.TryParse(trame.Substring(11, 2) + ',' + trame.Substring(13, 4), out latDecimal);
                                    if (b)
                                    {
                                        LatitudeReel += latDecimal / 60;
                                    }
                                    else
                                    {
                                        LatitudeReel = 0;
                                    }
                                }
                                else
                                {
                                    LatitudeReel = 0;
                                }
                                switch (trame.Substring(36, 1))
                                {
                                    case "N":
                                        break;
                                    case "S": LatitudeReel *= -1;
                                        break;
                                    default: LatitudeReel = 0;
                                        Console.WriteLine("Trame Invalide pour cause {0} non pri en compte:", trame.Substring(36));
                                        break;
                                }

                                //Console.WriteLine("LatitudeReel: " + LatitudeReel);
                                Decimal LongitudeReel;
                                //Console.WriteLine(trame.Substring(17, 8));
                                result = Decimal.TryParse(trame.Substring(17, 2), out LongitudeReel);
                                if (result)
                                {
                                    Decimal longDecimal;
                                    bool b = Decimal.TryParse(trame.Substring(19, 2) + ',' + trame.Substring(21, 4), out longDecimal);
                                    if (b)
                                    {
                                        LongitudeReel += longDecimal / 60;
                                    }
                                    else
                                    {
                                        LongitudeReel = 0;
                                    }
                                }
                                else
                                {
                                    LongitudeReel = 0;
                                }
                                switch (trame.Substring(37, 1))
                                {
                                    case "E":
                                        break;
                                    case "W": LongitudeReel *= -1;
                                        break;
                                    default: LongitudeReel = 0;
                                        Console.WriteLine("Trame Invalide pour cause {0} non pri en compte:", trame.Substring(37));
                                        break;
                                }

                                double vitesseReel;
                                result = double.TryParse(trame.Substring(25, 3) + ',' + trame.Substring(28, 1), out vitesseReel);
                                if (result)
                                {
                                    vitesseReel = 1.852 * vitesseReel;
                                }
                                else
                                {
                                    //Console.WriteLine("Erreur vitesseReel: {0}", vitesseReel);
                                    vitesseReel = 0;
                                }
                                Int16 Temperature;
                                result = Int16.TryParse(trame.Substring(29, 3), out Temperature);
                                if (!result)
                                {
                                    //Console.WriteLine("Erreur Temperature: {0} ", Temperature);
                                    Temperature = 0;
                                }

                                Int16 directionReel;
                                result = Int16.TryParse(trame.Substring(32, 3), out directionReel);
                                if (!result)
                                {
                                    //Console.WriteLine("Erreur directionReel: {0} ", directionReel);
                                    directionReel = 0;
                                }

                                if (LongitudeReel != 0 && LatitudeReel != 0)
                                {
                                    if (tramereal == null || TempsReel > tramereal.Temps)
                                    { 
                                        TrameReal tr = new TrameReal(boitier, TempsReel, LatitudeReel, LongitudeReel, vitesseReel, Temperature, Capteur, directionReel);
                                        tramereal = tr;
                                        OLDModelGeneratorProcessor.addTrame(tr);
                                    }
                                }
                            }
                            catch (FormatException)
                            {
                                Console.WriteLine("Unable to convert '{0}' to a Decimal.");

                            }

                            catch (OverflowException)
                            {
                                Console.WriteLine("'{0}' is outside the range of a Decimal.");
                            }
                            
                        }

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Format de date invalide" + e);
                        Logging(trame+" currente date : "+currentDate+" balise : "+ boitier,"DateNonValide");
                        
                    }
                }
            }


        private static string ProcessTrame(string[] trame, Socket socket, string boitier, string currentDate, ref TrameReal tramereal)
            {
                int nbrTrame = trame.Length;
                bool okSend = true;
                
                
                for (int k = 0; k < nbrTrame; k++)
                {
                    string trameK= trame[k].Trim().TrimEnd('\r');

                    if (trameK.ToUpper().Contains("$GT") || trameK.ToUpper().Contains("$GK") || trameK.ToUpper().Contains("$GM"))
                    {
                        switch (trameK.Length)
                        {
                            case 39:
                            case 49:
                            case 53:
                                dataFilter(trameK, currentDate, boitier, ref tramereal);
                                    break;

                            case 45:
                                    try
                                    {
                                        string newDate = ("20" + trameK.Substring(42, 2) + '-' + trameK.Substring(40, 2) + '-' + trameK.Substring(38, 2));
                                        //Console.WriteLine("Date Currente: {0} ", newDate);
                                        currentDate = newDate;
                                        //Console.WriteLine("Date Currente dans instance connection: {0} ", currentDate);
                                        dataFilter(trameK, currentDate, boitier, ref tramereal);
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine("Format de date invalid: {0}", e.ToString());
                                        okSend = false;
                                        socket.Send(Encoding.ASCII.GetBytes(NO), NO.Length, SocketFlags.None);
                                        break;
                                    }
                                    break;

                            case 55:
                                    try
                                    {
                                        string newDate = ("20" + trameK.Substring(52, 2) + '-' + trameK.Substring(50, 2) + '-' + trameK.Substring(48, 2));
                                        currentDate = newDate;
                                        //Console.WriteLine("Date Currente dans instance connection: {0} ", currentDate);
                                        dataFilter(trameK, currentDate, boitier, ref tramereal);
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine("Format de date invalid: {0}", e.ToString());
                                        okSend = false;
                                        socket.Send(Encoding.ASCII.GetBytes(NO), NO.Length, SocketFlags.None);
                                        break;
                                    }
                                    break;
                                    
                            case 59:
                                    try
                                    {
                                        string newDate = ("20" + trameK.Substring(56, 2) + '-' + trameK.Substring(54, 2) + '-' + trameK.Substring(52, 2));
                                        //Console.WriteLine("Date Currente: {0} ", newDate);
                                        currentDate = newDate;
                                        //Console.WriteLine("Date Currente dans instance connection: {0} ", currentDate);
                                        dataFilter(trameK, currentDate, boitier, ref tramereal);
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine("Format de date invalid: {0}", e.ToString());
                                        okSend = false;
                                        socket.Send(Encoding.ASCII.GetBytes(NO), NO.Length, SocketFlags.None);
                                        break;
                                    }
                                    break;

                            default:
                                    Console.WriteLine("Type de trames pas encore traité: {0}", trameK);
                                    Console.WriteLine("Longeur de la trame est: {0}", trameK.Length);
                                    break;
                        }

                    }
                    else if (!trameK.Contains("PING") && !trameK.Contains("$") && !trameK.Contains("#"))
                    {
                        Console.WriteLine("{0} est Trame non reconnu", trameK);
                        //okSend = false;
                    }
                    
                }
                if (okSend)
                {
                    Logging(trame.ToString(), boitier);
                    Thread.Sleep(10000);
                    socket.Send(Encoding.ASCII.GetBytes(OK), OK.Length, SocketFlags.None);
                }
                else
                {
                    socket.Send(Encoding.ASCII.GetBytes(NO), NO.Length, SocketFlags.None);
                }
                return currentDate;
            }


            public static string SplitTrame(Trame tramee, Socket socket, string currentDate ,ref TrameReal tramereal)
            {
                String trames = tramee.TrameValue;
                Int16 matricule = tramee.balise.Matricule;
                string boitier = tramee.balise.Nisbalise;
              
                char[] trameseparator = { '\n' };
                string[] trame = null;

                if (trames.ToUpper().Contains("$GT"))
                {
                    if (!String.IsNullOrEmpty(boitier))
                    {
                        trame = trames.Split(trameseparator, System.StringSplitOptions.RemoveEmptyEntries);
                        if (currentDate != null)
                        {
                            currentDate = ProcessTrame(trame, socket, boitier, currentDate, ref tramereal);
                        }
                        else
                        {
                            if (trame[0].Length < 45)
                            {
                                Console.WriteLine("Date non identifié");
                                socket.Send(Encoding.ASCII.GetBytes(NO), NO.Length, SocketFlags.None);
                            }
                            else
                            {
                                try
                                {
                                    currentDate = ("20" + trame[0].Substring(42, 2) + '-' + trame[0].Substring(40, 2) + '-' + trame[0].Substring(38, 2));
                                    currentDate = ProcessTrame(trame, socket, boitier, currentDate, ref tramereal);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("Format de date invalid: {0}", e.ToString());
                                    socket.Send(Encoding.ASCII.GetBytes(NO), NO.Length, SocketFlags.None);
                                }
                            }
                        }
                    }
                }
                else if (trames.ToUpper().Contains("$GK"))
                {
                    if (!String.IsNullOrEmpty(boitier))
                    {
                        trame = trames.Split(trameseparator, System.StringSplitOptions.RemoveEmptyEntries);

                        if (currentDate != null)
                        {
                            currentDate = ProcessTrame(trame, socket, boitier, currentDate, ref tramereal);
                        }
                        else
                        {
                            if (trame[0].Length < 58)
                            {
                                Console.WriteLine("Date non identifié: {0} - {1}", trame[0], trame[0].Length.ToString());
                                socket.Send(Encoding.ASCII.GetBytes(NO), NO.Length, SocketFlags.None);
                            }
                            else
                            {
                                try
                                {
                                    currentDate = ("20" + trame[0].Substring(56, 2) + '-' + trame[0].Substring(54, 2) + '-' + trame[0].Substring(52, 2));
                                    //Console.WriteLine("Date Currente: {0} ", this.currentDate);
                                    currentDate = ProcessTrame(trame, socket, boitier, currentDate, ref tramereal);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("Format de date invalid: {0}", e.ToString());
                                    socket.Send(Encoding.ASCII.GetBytes(NO), NO.Length, SocketFlags.None);
                                }
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Boitier non identifié");
                        socket.Send(Encoding.ASCII.GetBytes(NO), NO.Length, SocketFlags.None);
                    }
                }
                else if (trames.ToUpper().Contains("$GM")) 
                {

                    if (!String.IsNullOrEmpty(boitier))
                    {
                        trame = trames.Split(trameseparator, System.StringSplitOptions.RemoveEmptyEntries);

                        if (currentDate != null)
                        {
                            currentDate = ProcessTrame(trame, socket, boitier, currentDate, ref tramereal);
                        }
                        else
                        {
                            if (trame[0].Length < 54)
                            {
                                Console.WriteLine("Date non identifié: {0} - {1}", trame[0], trame[0].Length.ToString());
                                socket.Send(Encoding.ASCII.GetBytes(NO), NO.Length, SocketFlags.None);
                            }
                            else
                            {
                                try
                                {
                                    currentDate = ("20" + trame[0].Substring(52, 2) + '-' + trame[0].Substring(50, 2) + '-' + trame[0].Substring(48, 2));
                                    //Console.WriteLine("Date Currente: {0} ", this.currentDate);
                                    currentDate = ProcessTrame(trame, socket, boitier, currentDate, ref tramereal);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("Format de date invalid: {0}", e.ToString());
                                    socket.Send(Encoding.ASCII.GetBytes(NO), NO.Length, SocketFlags.None);
                                }
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Boitier non identifié");
                        socket.Send(Encoding.ASCII.GetBytes(NO), NO.Length, SocketFlags.None);
                    }
                }
                //************* Trame $GPRMC  ***************************************
                else if (trames.StartsWith("$GPRMC"))
                {
                   String[] listTrame = trames.Split(trameseparator, System.StringSplitOptions.RemoveEmptyEntries);
                   Logging(trames, boitier);
                   foreach (String oneTrame in listTrame)
                   {
                        String[] data = oneTrame.Split(',');
                        if (data.Length < 13 || data[2] == "V")
                        {


                           // Console.WriteLine("Trame non Valide {0}", oneTrame);
                           // this.Socket.Send(Encoding.ASCII.GetBytes(OK), OK.Length, SocketFlags.None);
                        }
                        else
                        {
                            String newTrame = "$GT";
                            String endStrng;
                            try { 
                            newTrame += data[1].Substring(0, 6);
                            endStrng = data[4] + data[6];
                            newTrame += data[3].Replace(".", "");
                            newTrame += data[5].Replace(".", "").Substring(1, 8);
                            currentDate = ("20" + data[9].Substring(4, 2) + '-' + data[9].Substring(2, 2) + '-' + data[9].Substring(0, 2));
                            newTrame += data[7].Replace(".", "");
                            newTrame += data[8].Substring(0, 3);
                            newTrame += data[12].Substring(0, 3);
                            endStrng = data[12].Substring(4, 1) + endStrng;
                            newTrame += endStrng;
                            for (int i = 13; i < data.Length; i++)
                            {
                                newTrame += data[i];
                            }
                            //Console.WriteLine("Trame GPRMC recu: {0}", newTrame);
                            dataFilter(newTrame, currentDate, boitier, ref tramereal); 
                            }
                            catch (Exception)
                            {
                                Console.WriteLine("Trame non Valide {0}", oneTrame);
                            }
                           
                        }
                    }
                }
                else if (trames.StartsWith("$I2B"))
                {
                    String[] listTrame = trames.Split(trameseparator, System.StringSplitOptions.RemoveEmptyEntries);
                    Logging(trames, boitier);
                    foreach (String oneTrame in listTrame)
                    {
                        String[] data = oneTrame.Split(',');
                        if (data.Length < 10)
                        {

                             Console.WriteLine("Trame non Valide {0}", oneTrame);
                            // this.Socket.Send(Encoding.ASCII.GetBytes(OK), OK.Length, SocketFlags.None);
                        }
                        else
                        {
                            try
                            {
                                spliteTrameI2BFirmware(data, boitier,ref tramereal);
                            }
                            catch (Exception)
                            {
                                Console.WriteLine("Trame non Valide {0}", oneTrame);
                            }

                        }
                    }
                }
                else if (!trames.Contains("$"))
                {
                    Console.WriteLine("Trame non reconnu");
                    Logging(trames, boitier);
                    socket.Send(Encoding.ASCII.GetBytes(NO), NO.Length, SocketFlags.None);
                }
                return currentDate;
            }
        

       

        //public void saveContext()
        //{
        //    while (!_conBuffer.IsCompleted && !_dataBuffer.IsCompleted)
        //    {
        //        Console.Write("-");
        //        try
        //        {
        //            Thread.Sleep(30000);
        //        }
        //        catch (Exception) { }
        //    }
        //}

   
        

      

     
        
        private static void Logging(String message, string balise)
        {
            using (StreamWriter sw = File.AppendText("Log\\Balise_" + balise + ".log"))
            {
                sw.WriteLine(DateTime.Now + ": "+message);
            }
        }

        private static void Logging(String message)
        {
            using (StreamWriter sw = File.AppendText("Log\\Balises.log"))
            {
                sw.WriteLine(DateTime.Now + ": " + message);
            }
        }
        
        private static void LogDBError(String message, string balise)
        {
            using (StreamWriter sw = File.AppendText("Log\\BDError_" + balise + ".log"))
            {
                sw.WriteLine(DateTime.Now + ": " + message);
            }
        }


        private static void spliteTrameI2BFirmware(String[] info,string idBalise,ref TrameReal trameReal)
        {
              DateTime TempsReel  = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                try
                {
                    TempsReel = TempsReel.AddSeconds(Int64.Parse(info[1]));
                    // Console.WriteLine("TempsReel: " + TempsReel);
                    if ((TempsReel - DateTime.Now).TotalHours < 24)
                    {
                        // Console.WriteLine("Date invalide: {0}", TempsReel);
                        String Capteur = info[8];
                        //Console.WriteLine("Capteur: " + Capteur);

                        String Moteur = info[7];
                        //Console.WriteLine("Etat Moteur: " + Moteur);
                        try
                        {
                            bool result;
                            Decimal LatitudeReel;
                            //Console.WriteLine(trame.Substring(9, 8));
                            result = Decimal.TryParse(info[2].Replace('.', ','), out LatitudeReel);
                            if (!result)
                            {
                                LatitudeReel = 0;
                            }
                           
                            

                            //Console.WriteLine("LatitudeReel: " + LatitudeReel);
                            Decimal LongitudeReel;
                            //Console.WriteLine(trame.Substring(17, 8));
                            result = Decimal.TryParse(info[3].Replace('.', ','), out LongitudeReel);
                            if (!result)
                            {
                              
                                LongitudeReel = 0;
                      
                            }
                            
                            double vitesseReel;
                            result = double.TryParse(info[6].Replace('.', ','), out vitesseReel);
                            if (result)
                            {
                                vitesseReel = 3.6* vitesseReel;
                            }
                            else
                            {
                                //Console.WriteLine("Erreur vitesseReel: {0}", vitesseReel);
                                vitesseReel = 0;
                            }
                            Int16 Temperature;
                            result = Int16.TryParse(info[9], out Temperature);
                            if (!result)
                            {
                                //Console.WriteLine("Erreur Temperature: {0} ", Temperature);
                                Temperature = 0;
                            }

                            Int16 directionReel;
                            Double direction;
                            result = Double.TryParse(info[5], out direction);
                            if (!result)
                            {
                                //Console.WriteLine("Erreur directionReel: {0} ", directionReel);
                                directionReel = 0;
                            }
                            else
                            {
                                directionReel = (Int16)direction;
                            }

                            if (LongitudeReel != 0 && LatitudeReel != 0)
                            {
                                TrameReal tr = new TrameReal(idBalise, TempsReel, LatitudeReel, LongitudeReel, vitesseReel, Temperature, Capteur, directionReel);
                                OLDModelGeneratorProcessor.addTrame(tr);
                                if (trameReal == null || trameReal.Temps < tr.Temps)
                                {
                                    trameReal = tr;
                                }
                            }
                        }
                        catch (FormatException)
                        {
                            Console.WriteLine("Unable to convert '{0}' to a Decimal.");
                        }

                        catch (OverflowException)
                        {
                            Console.WriteLine("'{0}' is outside the range of a Decimal.");
                        }

                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine("Format de date invalide" + e);
                }
            

        }
        private static void LoggingByte(Byte[] message)
        {
            string d = DateTime.Now + ": ";
            using (FileStream fs = new FileStream("Log\\Balises.log", FileMode.Append))
            {    
                foreach (byte b in d)
                {
                    fs.WriteByte(b);   
                }
                fs.Write(message, 0, message.Length);
            }
        }

     
    

   
    }
}
