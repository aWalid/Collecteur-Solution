
using System;
using System.Linq;
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
using System.Runtime.CompilerServices;
using Collecteur.Core.Api;
using BaliseListner.ThreadListener;
using CONCOXParser;
using CONCOXParser.Types;
using BaliseListner.Generator;
using System.Text.RegularExpressions;
using System.Diagnostics;

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

        private static Regex regConcox = new Regex(@"(7878|7979).*?0D0A($|(?=7878)|(?=7979))", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        private static String[] getConcoxTrames7878(string str)
        {
            return (from Match m in regConcox.Matches(str) where m.Value.StartsWith("7878") select m.Value).ToArray();//only data,check time keep alive, login trames
        }

        // public List<Balise> listeTrames;

        private static void dataFilter(string trame, string currentDate, Balise boitier, ref TrameReal tramereal, DateTime receptionDateTime)
        {
            if (trame != null)
            {
                DateTime TempsReel;
                try
                {
                    TempsReel = Convert.ToDateTime(currentDate + ' ' + trame.Substring(3, 2) + ':' + trame.Substring(5, 2) + ':' + trame.Substring(7, 2));
                    // Console.WriteLine("TempsReel: " + TempsReel);
                    if ((TempsReel - DateTime.Now).TotalHours < 720)
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
                                case "S":
                                    LatitudeReel *= -1;
                                    break;
                                default:
                                    LatitudeReel = 0;
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
                                case "W":
                                    LongitudeReel *= -1;
                                    break;
                                default:
                                    LongitudeReel = 0;
                                    if (PrincipalListner.config.Debug)
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
                                if ((vitesseReel > 999) || (vitesseReel < 0))
                                {
                                    LogDBError("Vitesse Hors limite ,vitesse : " + vitesseReel, boitier.Nisbalise);
                                }
                                else
                                {
                                    if (Math.Abs(LongitudeReel) > 90 || Math.Abs(LatitudeReel) > 90)
                                    {
                                        LogDBError("Position Hors limite,  : " + LongitudeReel + ',' + LatitudeReel, boitier.Nisbalise);
                                    }
                                    else
                                    {
                                        if (tramereal == null || TempsReel > tramereal.Temps)
                                        {

                                            TrameReal tr = new TrameReal(boitier, TempsReel, LatitudeReel, LongitudeReel, vitesseReel, Temperature, Capteur, directionReel, null, receptionDateTime);
                                            tramereal = tr;
                                            OLDModelGeneratorProcessor.addTrame(tr);
                                        }
                                    }
                                }
                            }
                        }
                        catch (FormatException ef)
                        {
                            Console.WriteLine("Unable to convert '{0}' to a Decimal.", ef.Message);
                        }

                        catch (OverflowException ofe)
                        {
                            Console.WriteLine("'{0}' is outside the range of a Decimal.", ofe.Message);
                        }

                    }

                }
                catch (Exception)
                {
                    // Console.WriteLine("Format de date invalide" + e);
                    LogDBError("DateNonValide trame: " + trame + " currente date : " + currentDate, boitier.Nisbalise);

                }
            }
        }

        private static string processTrame(String[] trame, Socket socket, Balise boitier, string currentDate, ref TrameReal tramereal, DateTime receptionDateTime)
        {
            int nbrTrame = trame.Length;
            bool okSend = true;


            for (int k = 0; k < nbrTrame; k++)
            {
                string trameK = trame[k].Trim().TrimEnd('\r');
                if (trameK.ToUpper().Contains("$GT") || trameK.ToUpper().Contains("$GK") || trameK.ToUpper().Contains("$GM"))
                {

                    if (trameK.Length >= 38)
                    {
                        if (trameK.Length >= 44)
                        {

                            string newDate = ("20" + trameK.Substring(trameK.Length - 2, 2) + '-' + trameK.Substring(trameK.Length - 4, 2) + '-' + trameK.Substring(trameK.Length - 6, 2));
                            //  Console.WriteLine("Date Currente: {0} ", newDate);
                            try
                            {
                                Convert.ToDateTime(newDate);
                                currentDate = newDate;

                            }
                            catch (Exception e)
                            {
                                if (currentDate == null)
                                {
                                    okSend = false;
                                    //socket.Send(Encoding.ASCII.GetBytes(NO), NO.Length, SocketFlags.None);
                                    LogDBError("DateNonValide trame: " + trame[k] + " currente date : " + newDate + " Erreur  :" + e.Message, boitier.Nisbalise);
                                    continue;
                                }

                            }
                            if (trameK.Contains("IDD") && trameK.Length <= 58)
                            {
                                // spliteTrameDALAS(boitier,trame[k], currentDate);
                                okSend = true;
                                continue;
                            }

                            //  Console.WriteLine("Date Currente dans instance connection: {0} ", currentDate);
                        }
                        if (trameK.Contains("IDD"))
                        {
                            //spliteTrameDALAS(boitier,trame[k], currentDate);
                        }
                        else
                        {
                            dataFilter(trameK, currentDate, boitier, ref tramereal, receptionDateTime);
                        }
                        okSend = true;


                    }
                    else
                    {
                        if (PrincipalListner.config.Debug)
                            Console.WriteLine("Type de trames pas encore traité: {0},Longeur de la trame est: {1}", trameK, trameK.Length);
                        LogDBError("Type de trames pas encore traité :  " + trameK, boitier.Nisbalise);
                    }



                }
                else if (!trameK.Contains("PING") && !trameK.Contains("$") && !trameK.Contains("#"))
                {
                    if (PrincipalListner.config.Debug)
                        Console.WriteLine("{0} est Trame non reconnu", trameK);
                    //okSend = false;
                }

            }
            if (okSend)
            {

                socket.Send(Encoding.ASCII.GetBytes(OK), OK.Length, SocketFlags.None);
            }
            else
            {
                socket.Send(Encoding.ASCII.GetBytes(NO), NO.Length, SocketFlags.None);
            }
            return currentDate;
        }

        private static void spliteTrameDALAS(Balise balise, string trame, string Date)
        {


            DateTime TempsReel;
            try
            {
                TempsReel = Convert.ToDateTime(Date + ' ' + trame.Substring(3, 2) + ':' + trame.Substring(5, 2) + ':' + trame.Substring(7, 2));
                String key = trame.Substring(9, 16);
                OLDModelGeneratorProcessor.addTramesIdentDalas(new IdentDalas(balise, TempsReel, key));
            }
            catch (Exception e)
            {
                LogDBError("DateNonValide trame: " + trame + " currente date : " + Date + " Erreur  :" + e.Message, balise.Nisbalise);

            }

        }

        public static string splitTrame(Trame tramee, Socket socket, string currentDate, ref TrameReal tramereal)
        {
            String trames = tramee.TrameValue;
            int matricule = tramee.balise.Matricule;
            Balise boitier = tramee.balise;
            DateTime receptionDateTime = tramee.ReceptionDateTime;
            char[] Trameseparator = { '\n', '\r' };
            string[] trame = null;
            //Logging(trames, boitier.Nisbalise);
            if (trames.ToUpper().Contains("$GT"))
            {
                if (!String.IsNullOrEmpty(boitier.Nisbalise))
                {
                    trame = trames.Split(Trameseparator, System.StringSplitOptions.RemoveEmptyEntries);
                    if (currentDate != null)
                    {
                        currentDate = processTrame(trame, socket, boitier, currentDate, ref tramereal, receptionDateTime);
                    }
                    else
                    {
                        if (trame[0].Length < 44)
                        {
                            if (PrincipalListner.config.Debug)
                                Console.WriteLine("Date non identifié");
                            LogDBError("Date non identifié ,Trames Recus :  " + trames, boitier.Nisbalise);
                            socket.Send(Encoding.ASCII.GetBytes(NO), NO.Length, SocketFlags.None);

                        }
                        else
                        {
                            try
                            {
                                currentDate = ("20" + trame[0].Substring(42, 2) + '-' + trame[0].Substring(40, 2) + '-' + trame[0].Substring(38, 2));
                                currentDate = processTrame(trame, socket, boitier, currentDate, ref tramereal, receptionDateTime);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Format de date invalid: {0}", e.ToString());
                                LogDBError("Format de date invalid :  " + e.ToString(), boitier.Nisbalise);
                                socket.Send(Encoding.ASCII.GetBytes(NO), NO.Length, SocketFlags.None);
                            }
                        }
                    }
                }
            }
            else if (trames.ToUpper().Contains("$GK"))
            {
                if (!String.IsNullOrEmpty(boitier.Nisbalise))
                {
                    trame = trames.Split(Trameseparator, System.StringSplitOptions.RemoveEmptyEntries);

                    if (currentDate != null)
                    {
                        currentDate = processTrame(trame, socket, boitier, currentDate, ref tramereal, receptionDateTime);
                    }
                    else
                    {
                        if (trame[0].Length < 58)
                        {
                            // Console.WriteLine("Date non identifié: {0} - {1}", trame[0], trame[0].Length.ToString());
                            LogDBError("Date non identifié :  " + trames, boitier.Nisbalise);
                            socket.Send(Encoding.ASCII.GetBytes(NO), NO.Length, SocketFlags.None);
                        }
                        else
                        {
                            try
                            {
                                currentDate = ("20" + trame[0].Substring(56, 2) + '-' + trame[0].Substring(54, 2) + '-' + trame[0].Substring(52, 2));
                                //Console.WriteLine("Date Currente: {0} ", this.currentDate);
                                currentDate = processTrame(trame, socket, boitier, currentDate, ref tramereal, receptionDateTime);
                            }
                            catch (Exception e)
                            {
                                // Console.WriteLine("Format de date invalid: {0}", e.ToString());
                                LogDBError("Format de date invalid :  " + e.Message, boitier.Nisbalise);
                                socket.Send(Encoding.ASCII.GetBytes(NO), NO.Length, SocketFlags.None);
                            }
                        }
                    }
                }
                else
                {
                    // Console.WriteLine("Boitier non identifié");
                    LogDBError("Boitier non identifié", boitier.Nisbalise);
                    socket.Send(Encoding.ASCII.GetBytes(NO), NO.Length, SocketFlags.None);
                }
            }
            else if (trames.ToUpper().Contains("$GM"))
            {

                if (!String.IsNullOrEmpty(boitier.Nisbalise))
                {
                    trame = trames.Split(Trameseparator, System.StringSplitOptions.RemoveEmptyEntries);

                    if (currentDate != null)
                    {
                        currentDate = processTrame(trame, socket, boitier, currentDate, ref tramereal, receptionDateTime);
                    }
                    else
                    {
                        if (trame[0].Length < 54)
                        {
                            //    Console.WriteLine("Date non identifié: {0} - {1}", trame[0], trame[0].Length.ToString());
                            LogDBError("Date non identifié :  " + trames, boitier.Nisbalise);
                            socket.Send(Encoding.ASCII.GetBytes(NO), NO.Length, SocketFlags.None);
                        }
                        else
                        {
                            try
                            {
                                currentDate = ("20" + trame[0].Substring(52, 2) + '-' + trame[0].Substring(50, 2) + '-' + trame[0].Substring(48, 2));
                                //Console.WriteLine("Date Currente: {0} ", this.currentDate);
                                currentDate = processTrame(trame, socket, boitier, currentDate, ref tramereal, receptionDateTime);
                            }
                            catch (Exception e)
                            {
                                LogDBError("Format de date invalid :  " + e.Message, boitier.Nisbalise);
                                // Console.WriteLine("Format de date invalid: {0}", e.ToString());
                                socket.Send(Encoding.ASCII.GetBytes(NO), NO.Length, SocketFlags.None);
                            }
                        }
                    }
                }
                else
                {
                    // Console.WriteLine("Boitier non identifié");
                    socket.Send(Encoding.ASCII.GetBytes(NO), NO.Length, SocketFlags.None);
                    LogDBError("Boitier non identifié : trames recus " + trames, boitier.Nisbalise);
                }
            }
            else if (trames.StartsWith("$GPRMC"))
            {
                String[] listTrame = trames.Split(Trameseparator, System.StringSplitOptions.RemoveEmptyEntries);

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
                        try
                        {
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
                            dataFilter(newTrame, currentDate, boitier, ref tramereal, receptionDateTime);
                        }
                        catch (Exception)
                        {
                            // Console.WriteLine("Trame non Valide {0}", oneTrame);
                            LogDBError("Trame non Valide  " + oneTrame, boitier.Nisbalise);

                        }
                    }
                }
            }

            // $I2B, $I2S, @S, @U se trouve seulement dans le port 30000 pricipale
            else if (trames.StartsWith("$I2B"))
            {
                String[] listTrame = trames.Split(Trameseparator, System.StringSplitOptions.RemoveEmptyEntries);

                // Logging(trames, boitier.Nisbalise);

                foreach (String oneTrame in listTrame)
                {
                    String[] data = oneTrame.Split(',');
                    if (data.Length < 10)
                    {

                        // Console.WriteLine("Trame non Valide {0}", oneTrame);
                        LogDBError("Trame  Non Valide, Trame : " + oneTrame, boitier.Nisbalise);
                        // this.Socket.Send(Encoding.ASCII.GetBytes(OK), OK.Length, SocketFlags.None);
                    }
                    else
                    {
                        try
                        {
                            spliteTrameI2BFirmware(data, boitier, ref tramereal, receptionDateTime);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Trame non Valide {0}", oneTrame);
                            LogDBError("Trame  Non Valide, Trame : " + oneTrame + "Exception : " + e.Message, boitier.Nisbalise);
                        }

                    }
                }
            }
            else if (trames.StartsWith("$I2S"))
            {
                String[] listTrame = trames.Split(Trameseparator, System.StringSplitOptions.RemoveEmptyEntries);

                // Logging(trames, boitier.Nisbalise);

                foreach (String oneTrame in listTrame)
                {
                    String[] data = oneTrame.Split(',');
                    if (data.Length < 10)
                    {

                        // Console.WriteLine("Trame non Valide {0}", oneTrame);
                        LogDBError("Trame  Non Valide, Trame : " + oneTrame, boitier.Nisbalise);
                        // this.Socket.Send(Encoding.ASCII.GetBytes(OK), OK.Length, SocketFlags.None);
                    }
                    else
                    {
                        try
                        {
                            spliteTrameI2BFirmwareVersionBelge(data, boitier, ref tramereal, receptionDateTime);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Trame non Valide {0}", oneTrame);
                            LogDBError("Trame  Non Valide, Trame : " + oneTrame + "Exception : " + e.Message, boitier.Nisbalise);
                        }

                    }
                }
            }
            else if (trames.Contains("@S,"))
            {
                String[] listTrame = trames.Split(Trameseparator, System.StringSplitOptions.RemoveEmptyEntries);

                foreach (String oneTrame in listTrame)
                {
                    if (oneTrame.Length == 0)
                        continue;
                    String[] data = oneTrame.Split(',');

                    if (!data[0].Contains("@S"))
                    {
                        String[] dataTrame = (String[])data.Clone();
                        data = new string[dataTrame.Length + 5];
                        dataTrame.CopyTo(data, 5);
                    }

                    if (data.Length < 20)
                    {
                        //  Console.WriteLine("Trame non Valide {0}", oneTrame);
                        LogDBError("Trame  Non Valide, Trame : " + trame, boitier.Nisbalise);
                        // this.Socket.Send(Encoding.ASCII.GetBytes(OK), OK.Length, SocketFlags.None);
                    }
                    else try
                        {
                            spliteTrameATrackForSarens(data, boitier, ref tramereal, receptionDateTime);
                        }
                        catch (Exception)
                        {
                            LogDBError("Trame  Non Valide, Trame : " + oneTrame, boitier.Nisbalise);
                            Console.WriteLine("Trame non Valide {0}", oneTrame);
                        }

                }
            }
            else if (trames.Contains("@U,"))
            {
                String[] listTrame = trames.Split(Trameseparator, System.StringSplitOptions.RemoveEmptyEntries);

                foreach (String oneTrame in listTrame)
                {
                    if (oneTrame.Length == 0)
                        continue;
                    String[] data = oneTrame.Split(',');

                    if (!data[0].Contains("@U"))
                    {
                        String[] dataTrame = (String[])data.Clone();
                        data = new string[dataTrame.Length + 5];
                        dataTrame.CopyTo(data, 5);
                    }


                    if (data.Length < 20)
                    {

                        //  Console.WriteLine("Trame non Valide {0}", oneTrame);
                        LogDBError("Trame  Non Valide, Trame : " + trame, boitier.Nisbalise);
                        // this.Socket.Send(Encoding.ASCII.GetBytes(OK), OK.Length, SocketFlags.None);
                    }
                    else try
                        {
                            spliteTrameATrackUltrason(data, boitier, ref tramereal, receptionDateTime);
                        }
                        catch (Exception)
                        {
                            LogDBError("Trame  Non Valide, Trame : " + oneTrame, boitier.Nisbalise);
                            Console.WriteLine("Trame non Valide {0}", oneTrame);
                        }

                }
            }

            else if (trames.Contains("@P,"))
            {

                String[] listTrame = trames.Split(Trameseparator, System.StringSplitOptions.RemoveEmptyEntries);

                foreach (String oneTrame in listTrame)
                {
                    if (oneTrame.Length == 0)
                        continue;
                    String[] data = oneTrame.Split(',');

                    if (data[0].Substring(data[0].Length - 2) != "@P")
                    {
                        String[] dataTrame = (String[])data.Clone();
                        data = new string[dataTrame.Length + 5];
                        dataTrame.CopyTo(data, 5);
                    }

                    if (data.Length < 20)
                    {

                        //  Console.WriteLine("Trame non Valide {0}", oneTrame);
                        LogDBError("Trame  Non Valide, Trame : " + trame, boitier.Nisbalise);
                        // this.Socket.Send(Encoding.ASCII.GetBytes(OK), OK.Length, SocketFlags.None);
                    }
                    else
                        try
                        {
                            spliteTrameATrack(data, boitier, ref tramereal, receptionDateTime);
                        }
                        catch (Exception)
                        {
                            LogDBError("Trame  Non Valide, Trame : " + oneTrame, boitier.Nisbalise);
                            Console.WriteLine("Trame non Valide {0}", oneTrame);
                        }

                }
            }
            else if (trames.Contains("@F,"))
            {
                const string pattern = @"[^(\u0000-\u0009)\u001A(\u0020-\u007F)]+";
                Regex rgx = new Regex(pattern);
                String[] listTrame = rgx.Split(trames);
                //String[] listTrame = trames.Split(Trameseparator, System.StringSplitOptions.RemoveEmptyEntries);

                foreach (String oneTrame in listTrame)
                {
                    if (oneTrame.Length == 0)
                        continue;
                    String[] data = oneTrame.Split(',');

                    if (data[0].Substring(data[0].Length - 2) != "@F")
                    {
                        String[] dataTrame = (String[])data.Clone();
                        data = new string[dataTrame.Length + 5];
                        dataTrame.CopyTo(data, 5);
                    }

                    if (data.Length < 20)
                    {
                        LogDBError("Trame  Non Valide, Trame : " + trame, boitier.Nisbalise);
                        // this.Socket.Send(Encoding.ASCII.GetBytes(OK), OK.Length, SocketFlags.None);
                    }
                    else
                        try
                        {
                            spliteTrameATrackCAN(data, boitier, ref tramereal, receptionDateTime);
                        }
                        catch (Exception)
                        {
                            LogDBError("Trame  Non Valide, Trame : " + oneTrame, boitier.Nisbalise);
                            // Console.WriteLine("Trame non Valide {0}", oneTrame);
                        }

                }
            }
            else if (trames.Contains("@E,"))
            {
                String[] listTrame = trames.Split(Trameseparator, System.StringSplitOptions.RemoveEmptyEntries);

                foreach (String oneTrame in listTrame)
                {
                    if (oneTrame.Length == 0)
                        continue;
                    String[] data = oneTrame.Split(',');

                    if (data[0].Substring(data[0].Length - 2) != "@E")
                    {
                        String[] dataTrame = (String[])data.Clone();
                        data = new string[dataTrame.Length + 5];
                        dataTrame.CopyTo(data, 5);
                    }


                    if (data.Length < 20)
                    {

                        //  Console.WriteLine("Trame non Valide {0}", oneTrame);
                        LogDBError("Trame  Non Valide, Trame : " + trame, boitier.Nisbalise);
                        // this.Socket.Send(Encoding.ASCII.GetBytes(OK), OK.Length, SocketFlags.None);
                    }
                    else try
                        {
                            spliteTrameATrackETUSA(data, boitier, ref tramereal, receptionDateTime);
                        }
                        catch (Exception)
                        {
                            LogDBError("Trame  Non Valide, Trame : " + oneTrame, boitier.Nisbalise);
                            Console.WriteLine("Trame non Valide {0}", oneTrame);
                        }

                }
            }
            //TODO: Balise AX9
            else if (trames.Contains("@X,"))
            {
                String[] listTrame = trames.Split(Trameseparator, System.StringSplitOptions.RemoveEmptyEntries);

                foreach (String oneTrame in listTrame)
                {
                    if (oneTrame.Length == 0)
                        continue;
                    String[] data = oneTrame.Split(',');

                    if (data[0].Substring(data[0].Length - 2) != "@X")
                    {
                        String[] dataTrame = (String[])data.Clone();
                        data = new string[dataTrame.Length + 5];
                        dataTrame.CopyTo(data, 5);
                    }


                    if (data.Length < 20)
                    {

                        //  Console.WriteLine("Trame non Valide {0}", oneTrame);
                        LogDBError("Trame  Non Valide, Trame : " + trame, boitier.Nisbalise);
                        // this.Socket.Send(Encoding.ASCII.GetBytes(OK), OK.Length, SocketFlags.None);
                    }
                    else
                        try
                        {
                            spliteTrameATrackAX9(data, boitier, ref tramereal, receptionDateTime);
                        }
                        catch (Exception ex)
                        {
                            LogDBError("Trame  Non Valide, Trame : " + oneTrame, boitier.Nisbalise);

                        }

                }
            }
            else if (trames.StartsWith("7878") || trames.StartsWith("7979"))
            {
                Byte[] sendBuffer;

                //trames = trames.Replace("7878", ",");
                //String[] listTrame = trames.Split(",".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);


                String[] listTrame = getConcoxTrames7878(trames);//only data,check time keep alive, login trames

                foreach (String oneTrame in listTrame)
                {
                    sendBuffer = spliteTrameConcox(oneTrame, boitier, ref tramereal, receptionDateTime);
                    //string typeTrame = oneTrame.Substring(2, 2);
                    string typeTrame = oneTrame.Substring(6, 2);

                    if (typeTrame.Equals("01", StringComparison.OrdinalIgnoreCase) || typeTrame.Equals("13", StringComparison.OrdinalIgnoreCase) || typeTrame.Equals("23", StringComparison.OrdinalIgnoreCase)
                        || typeTrame.Equals("26", StringComparison.OrdinalIgnoreCase) || typeTrame.Equals("8A", StringComparison.OrdinalIgnoreCase) || typeTrame.Equals("16", StringComparison.OrdinalIgnoreCase))
                    {
                        socket.Send(sendBuffer, sendBuffer.Length, SocketFlags.None);
                        Connection.Logging("Envoie   : " + BitConverter.ToString(sendBuffer).Replace("-", ""), boitier.Nisbalise);
                    }
                }
            }
            ///<Commented by ali le >
            ///else if (trames.StartsWith("7979"))
            ///{
            ///    Byte[] sendBuffer;
            ///    trames = trames.Replace("yy", ",");
            ///    String[] listTrame = trames.Split(",".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
            ///    foreach (String oneTrame in listTrame)
            ///    {
            ///        byte[] bufferToSend;
            ///        bufferToSend = getConcoxDataBufferToSend(oneTrame);
            ///        if (bufferToSend != null)
            ///        {
            ///            string hex = BitConverter.ToString(bufferToSend).Replace("-", "");
            ///            Connection.Logging("7979" + hex, boitier.Nisbalise);
            ///            if (bufferToSend.Length > 7)
            ///            {
            ///                if (hex.Substring(4, 4).Equals("3F00", StringComparison.OrdinalIgnoreCase))
            ///                {
            ///                    try
            ///                    {
            ///                        Console.WriteLine("Balise {0} Analog Data: HEX={1} DEC={2}.", boitier.Nisbalise, hex.Substring(8, 4), int.Parse(hex.Substring(8, 4), System.Globalization.NumberStyles.HexNumber) / 100);
            ///                        //Console.WriteLine("Balise {0} Analog Data: DEC={1}", boitier.Nisbalise, int.Parse(BitConverter.ToString(analog).Replace("-", ""), System.Globalization.NumberStyles.HexNumber));
            ///                    }
            ///                    catch (Exception e)
            ///                    {
            ///                        Console.WriteLine(e.ToString());
            ///                    }
            ///                }
            ///                else if (hex.Substring(4, 4).Equals("3F05", StringComparison.OrdinalIgnoreCase))
            ///                {
            ///                    try
            ///                    {
            ///                        Console.WriteLine("Balise {0} Digital Data: HEX={1} DEC={2}.", boitier.Nisbalise, hex.Substring(8, 4), int.Parse(hex.Substring(8, 4), System.Globalization.NumberStyles.HexNumber));
            ///                        //Console.WriteLine("Balise {0} Analog Data: DEC={1}", boitier.Nisbalise, int.Parse(BitConverter.ToString(analog).Replace("-", ""), System.Globalization.NumberStyles.HexNumber));
            ///                    }
            ///                    catch (Exception e)
            ///                    {
            ///                        Console.WriteLine(e.ToString());
            ///                    }
            ///                }
            ///            }
            ///        }
            ///    }
            ///} 
            ///</Commented by ali le >

            else if (trames.Contains("@e,"))
            {
                String[] listTrame = trames.Split(Trameseparator, System.StringSplitOptions.RemoveEmptyEntries);

                foreach (String oneTrame in listTrame)
                {
                    if (oneTrame.Length == 0)
                        continue;
                    String[] data = oneTrame.Split(',');

                    if (data[0].Substring(data[0].Length - 2) != "@e")
                    {
                        String[] dataTrame = (String[])data.Clone();
                        data = new string[dataTrame.Length + 5];
                        dataTrame.CopyTo(data, 5);
                    }

                    if (data.Length < 20)
                    {

                        //  Console.WriteLine("Trame non Valide {0}", oneTrame);
                        LogDBError("Trame  Non Valide, Trame : " + trame, boitier.Nisbalise);
                        // this.Socket.Send(Encoding.ASCII.GetBytes(OK), OK.Length, SocketFlags.None);
                    }
                    else
                        try
                        {
                            spliteTrameEchoATrack(data, boitier, ref tramereal, receptionDateTime);
                        }
                        catch (Exception ex)
                        {

                            LogDBError("Trame  Non Valide, Trame : " + oneTrame, boitier.Nisbalise);
                            LogDBError("\n Exception : " + ex.ToString(), boitier.Nisbalise);
                            Console.WriteLine("Trame non Valide {0}", oneTrame);
                        }

                }
            }
            else if (trames.Contains("@T,"))// Sonde de température "chambre de froid" example de client numilog,la vergule de temperateur est important
            {
                String[] listTrame = trames.Split(Trameseparator, System.StringSplitOptions.RemoveEmptyEntries);

                foreach (String oneTrame in listTrame)
                {
                    if (oneTrame.Length == 0)
                        continue;
                    String[] data = oneTrame.Split(',');

                    if (data[0].Substring(data[0].Length - 2) != "@T")
                    {
                        String[] dataTrame = (String[])data.Clone();
                        data = new string[dataTrame.Length + 5];
                        dataTrame.CopyTo(data, 5);
                    }

                    if (data.Length < 20)
                    {

                        //  Console.WriteLine("Trame non Valide {0}", oneTrame);
                        LogDBError("Trame  Non Valide, Trame : " + trame, boitier.Nisbalise);
                        // this.Socket.Send(Encoding.ASCII.GetBytes(OK), OK.Length, SocketFlags.None);
                    }
                    else
                        try
                        {
                            spliteTrameSondeTemperature(data, boitier, ref tramereal, receptionDateTime);
                        }
                        catch (Exception)
                        {
                            LogDBError("Trame  Non Valide, Trame : " + oneTrame, boitier.Nisbalise);
                            Console.WriteLine("Trame non Valide {0}", oneTrame);
                        }

                }
            }
            else if (trames.Contains("@L,"))// Sonde niveuacarburant
            {
                String[] listTrame = trames.Split(Trameseparator, System.StringSplitOptions.RemoveEmptyEntries);

                foreach (String oneTrame in listTrame)
                {
                    if (oneTrame.Length == 0)
                        continue;
                    String[] data = oneTrame.Split(',');

                    if (data[0].Substring(data[0].Length - 2) != "@L")
                    {
                        String[] dataTrame = (String[])data.Clone();
                        data = new string[dataTrame.Length + 5];
                        dataTrame.CopyTo(data, 5);
                    }

                    if (data.Length < 20)
                    {

                        //  Console.WriteLine("Trame non Valide {0}", oneTrame);
                        LogDBError("Trame  Non Valide, Trame : " + trame, boitier.Nisbalise);
                        // this.Socket.Send(Encoding.ASCII.GetBytes(OK), OK.Length, SocketFlags.None);
                    }
                    else
                        try
                        {

                            spliteTrameSondeNiveauCarbrant(data, boitier, ref tramereal, receptionDateTime);
                        }
                        catch (Exception)
                        {
                            LogDBError("Trame  Non Valide, Trame : " + oneTrame, boitier.Nisbalise);
                            Console.WriteLine("Trame non Valide {0}", oneTrame);
                        }

                }
            }
            else if (trames.Contains("@C,"))
            {
                const string pattern = @"[^(\u0000-\u0009)\u001A(\u0020-\u007F)]+";
                Regex rgx = new Regex(pattern);
                String[] listTrame = rgx.Split(trames);
                //String[] listTrame = trames.Split(Trameseparator, System.StringSplitOptions.RemoveEmptyEntries);

                foreach (String oneTrame in listTrame)
                {
                    if (oneTrame.Length == 0)
                        continue;
                    String[] data = oneTrame.Split(',');

                    if (data[0].Substring(data[0].Length - 2) != "@C")
                    {
                        String[] dataTrame = (String[])data.Clone();
                        data = new string[dataTrame.Length + 5];
                        dataTrame.CopyTo(data, 5);
                    }


                    if (data.Length < 20)
                    {

                        //  Console.WriteLine("Trame non Valide {0}", oneTrame);
                        LogDBError("Trame  Non Valide, Trame : " + trame, boitier.Nisbalise);
                        // this.Socket.Send(Encoding.ASCII.GetBytes(OK), OK.Length, SocketFlags.None);
                    }
                    else try
                        {
                            spliteTrameATrackCANEco(data, boitier, ref tramereal, receptionDateTime);
                        }
                        catch (Exception)
                        {
                            LogDBError("Trame  Non Valide, Trame : " + oneTrame, boitier.Nisbalise);
                            Console.WriteLine("Trame non Valide {0}", oneTrame);
                        }

                }
            }
            else if (trames.StartsWith("8080"))
            {
                Byte[] Reponse, ReponseCrypted;

                Reponse = spliteTrameConcox(trames, boitier, ref tramereal, receptionDateTime);



                string typeTrame = trames.Substring(6, 2);

                if (typeTrame.Equals("01", StringComparison.OrdinalIgnoreCase) || typeTrame.Equals("13", StringComparison.OrdinalIgnoreCase) || typeTrame.Equals("23", StringComparison.OrdinalIgnoreCase)
                    || typeTrame.Equals("26", StringComparison.OrdinalIgnoreCase) || typeTrame.Equals("8A", StringComparison.OrdinalIgnoreCase) || typeTrame.Equals("16", StringComparison.OrdinalIgnoreCase))
                {

                    ReponseCrypted = Cryptage.CrypteConcoxReponseMsg(Reponse, Cryptage.getKey(boitier.Nisbalise));

                    Reponse[0] = 0x78;
                    Reponse[1] = 0x78;

                    Connection.Logging("Envoie\t\t: " + BitConverter.ToString(ReponseCrypted).Replace("-", ""), boitier.Nisbalise + "-ReponseServer");
                    Connection.Logging("Decryptage\t: " + BitConverter.ToString(Reponse).Replace("-", ""), boitier.Nisbalise + "-ReponseServer");


                    socket.Send(ReponseCrypted, ReponseCrypted.Length, SocketFlags.None);
                }

            }
            ///Teltonika
            else if (trames.StartsWith("00000000"))
            {
                try
                {
                    //structure de package est : 00000000 + Longeur de package(4 bytes) + 08 +  data(n bytes)  + nombre des trames(1byte) + 0000 + CRC calculé(2bytes)
                    string[] splitPackage = regTeltonikaPacKageFMB9XX.Split(trames);
                    //résultat de Regex "splitPackage" est de structeur : [stringeempty,Longeur de package,nombre des trames,trames data,nombre des trames,stringeempty

                    if ((splitPackage.Length == 7) && (splitPackage[2] == splitPackage[4]))
                    {
                        //  lengthTrame = Longeur de package - (lengeur codecid (1 octet) + lengeur nombre des trames de  debut (= 1octet) * 2 )
                        int lengthTrame = (StringHexToInt(splitPackage[1]) - 3) / StringHexToInt(splitPackage[2]) * 2;
                        string[] listTrame = TeltonikaSplit(splitPackage[3], lengthTrame);

                        foreach (string oneTrame in listTrame)
                        {
                            spliteTrameTeltonikaFMB9XX(oneTrame, boitier, ref tramereal, receptionDateTime);
                        }

                        socket.Send(new byte[] { 0x00, 0x00, 0x00, StringToByteArray(splitPackage[2])[0] }, 4, SocketFlags.None);
                    }
                    else
                    {
                        LogDBError("Trame  Non Traité, Trame : " + trames, boitier.Nisbalise);
                        socket.Send(new byte[] { 0x00, 0x00, 0x00, 0x00 }, 4, SocketFlags.None);
                    }
                }
                catch (Exception ex)
                {
                    LogDBError("Trame  Non Traité, Trame : " + trames, boitier.Nisbalise);
                    socket.Send(new byte[] { 0x00, 0x00, 0x00, 0x00 }, 4, SocketFlags.None);
                }

            }
            else //if (!trames.Contains("$"))
            {
                //  Console.WriteLine("Trame non reconnu");
                // Logging(trames, boitier.Nisbalise);
                LogDBError("Trame  Non Traité, Trame : " + trames, boitier.Nisbalise);

                socket.Send(Encoding.ASCII.GetBytes(NO), NO.Length, SocketFlags.None);
            }

            return currentDate;
        }

        private static byte[] getConcoxDataBufferToSend(string oneTrame)
        {
            byte[] BufferData;
            // UTF8Encoding enc = new UTF8Encoding();
            try
            {
                // BufferData = enc.GetBytes(oneTrame);
                BufferData = Encoding.ASCII.GetBytes(oneTrame);
            }
            catch (ArgumentNullException en)
            {
                Console.WriteLine(en.ToString());
                return null;
            }
            catch (System.Text.EncoderFallbackException efb)
            {
                Console.WriteLine(efb.ToString());
                return null;
            }

            return BufferData;
        }

        private static void spliteTrameATrackETUSA(String[] info, Balise balise, ref TrameReal trameReal, DateTime receptionDateTime)
        {
            DateTime TempsReel = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            try
            {
                TempsReel = TempsReel.AddSeconds(Int64.Parse(info[5]));
                // Console.WriteLine("TempsReel: " + TempsReel);
                if ((TempsReel - DateTime.Now).TotalHours < 720)
                {
                    // Console.WriteLine("Date invalide: {0}", TempsReel);
                    String Capteur = "000";
                    int Carburant = Convert.ToInt32(info.Length < 25 ? info[17] : info[24]);      //valeur en miliVolt position 24 dans la trame Ak1
                    Carburant = Carburant / 25;                     //echantillonage 
                    if (Carburant < 1000)
                    {
                        Capteur = convertAndformat(Carburant);      // convertir en chaine de caracter sur 3 positions
                    }
                    //Console.WriteLine("Capteur: " + Capteur);

                    int Moteur = Convert.ToInt16((info[14]));
                    //Console.WriteLine("Etat Moteur: " + Moteur);
                    if (Moteur % 2 == 1)
                        Capteur = Capteur + "M";
                    else
                        Capteur = Capteur + "A";

                    try
                    {
                        bool result;
                        Decimal LatitudeReel;
                        //Console.WriteLine(trame.Substring(9, 8));
                        result = Decimal.TryParse(info[9], out LatitudeReel);
                        if (!result)
                        {
                            LatitudeReel = 0;
                        }
                        LatitudeReel = LatitudeReel / 1000000;
                        //Console.WriteLine("LatitudeReel: " + LatitudeReel);
                        Decimal LongitudeReel;
                        //Console.WriteLine(trame.Substring(17, 8));
                        result = Decimal.TryParse(info[8], out LongitudeReel);
                        if (!result)
                        {

                            LongitudeReel = 0;

                        }
                        LongitudeReel = LongitudeReel / 1000000;
                        double vitesseReel;
                        result = double.TryParse(info[15].Replace('.', ','), out vitesseReel);
                        if (!result)
                        {
                            //Console.WriteLine("Erreur vitesseReel: {0}", vitesseReel);
                            vitesseReel = 0;
                        }
                        Int16 Temperature;
                        result = Int16.TryParse(info.Length < 20 ? "0" : info[19], out Temperature);
                        if (!result)
                        {
                            //Console.WriteLine("Erreur Temperature: {0} ", Temperature);
                            Temperature = 0;
                        }


                        if (info[18] != null && info[18].Trim().Length >= 12)
                            OLDModelGeneratorProcessor.addTramesIdentDalas(new IdentDalas(balise, TempsReel, info[18].Trim()));

                        Int16 directionReel;
                        Double direction;
                        result = Double.TryParse(info[10], out direction);
                        if (!result)
                        {
                            //Console.WriteLine("Erreur directionReel: {0} ", directionReel);
                            directionReel = 0;
                        }
                        else
                        {
                            if (direction > 360 || direction < 0)
                                directionReel = 0;
                            else
                                directionReel = (Int16)direction;
                        }

                        OptionsAux options = new OptionsAux();
                        try
                        {
                            Int16 moteur;
                            result = Int16.TryParse(info[14], out moteur);
                            if (!result)
                            {
                                moteur = 0;
                            }
                            else
                            {
                                moteur = (Int16)(moteur % 2);
                            }

                            Int16 MVolt; //Main voltage (Batterie Vehicule)
                            result = Int16.TryParse(info.Length < 23 ? "0" : info[22], out MVolt);
                            if (!result)
                            {
                                MVolt = 0;
                            }

                            Int16 BVolt; //Backup voltage (Batterie Boitier)
                            result = Int16.TryParse(info.Length < 24 ? "0" : info[23], out BVolt);
                            if (!result)
                            {
                                BVolt = 0;
                            }

                            Int16 Temp; //Temperature Moteur
                            result = Int16.TryParse(info.Length < 26 ? "0" : info[25], out Temp);
                            if (!result)
                            {
                                Temp = 0;
                            }

                            Int16 TempAlerte; //Temoin Alerte Temperature
                            result = Int16.TryParse(info.Length < 27 ? "0" : info[26], out TempAlerte);
                            if (!result)
                            {
                                TempAlerte = 0;
                            }
                            // Si le Moteur est eteint TempsAlerte doit etre fixer à 0
                            if (moteur == 0)
                            {
                                TempAlerte = 0;
                            }

                            //options[64] = Convert.ToInt32(info.Length < 25 ? info[17] : info[24]); //valeur Carburant
                            //options[65] = Temperature;
                            //options[66] = moteur;
                            options[71] = MVolt;//Main voltage (Batterie Vehicule)
                            options[72] = BVolt;//Backup Voltage (Batterie Balise)
                            if (Temp > 0)
                                options[85] = Temp;// Valeur Temperature moteur
                            options[86] = TempAlerte;// Temoin température (0/1)
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Conversion Invalid Capteur .", e.Message);
                            LogDBError("Conversion Invalid Capteur  : " + info + " \n Exception : " + e.Message, balise.Nisbalise);
                        }

                        if (LongitudeReel != 0 && LatitudeReel != 0)
                        {
                            if ((vitesseReel > 999) || (vitesseReel < 0))
                            {
                                LogDBError("Vitesse Hors limite", balise.Nisbalise);
                            }
                            else
                            {
                                if (Math.Abs(LongitudeReel) > 90 || Math.Abs(LatitudeReel) > 90)
                                {
                                    LogDBError("Position Hors limite,  : " + LongitudeReel + " et " + LatitudeReel, balise.Nisbalise);
                                }
                                else
                                {
                                    TrameReal tr = new TrameReal(balise, TempsReel, LatitudeReel, LongitudeReel, vitesseReel, Temperature, Capteur, directionReel, options, receptionDateTime);

                                    if (trameReal == null || trameReal.Temps < tr.Temps)
                                    {
                                        OLDModelGeneratorProcessor.addTrame(tr);
                                        trameReal = tr;
                                    }
                                }
                            }
                        }
                    }
                    catch (FormatException fe)
                    {
                        Console.WriteLine("Unable to convert '{0}' to a Decimal.", fe.Message);
                        LogDBError("Trame  Non Valide, Trame : " + info + " \n Exception : " + fe.Message, balise.Nisbalise);
                    }

                    catch (OverflowException ofe)
                    {
                        Console.WriteLine("'{0}' is outside the range of a Decimal.", ofe.Message);
                        LogDBError("Trame  Non Valide, Trame : " + info + "Exception : " + ofe.Message, balise.Nisbalise);
                    }

                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Format de date invalide" + e);
                LogDBError("Trame  Non Valide, Trame : " + info + "Exception : " + e.Message, balise.Nisbalise);
            }


        }

        private static byte[] spliteTrameConcox(string p, Balise boitier, ref TrameReal tramereal, DateTime receptionDateTime)
        {

            ConcoxTrameParser trameParser = new ConcoxTrameParser();
            ConcoxTrame trame = trameParser.ParseTrame(p);

            if ((trame.Type == ConcoxTrameType.GT800Data || trame.Type == ConcoxTrameType.WeTrackData || trame.Type == ConcoxTrameType.WeTrackDataAlarm) && trame.DataLocationTrame.Date != null && trame.DataLocationTrame.Longitude != 0 && trame.DataLocationTrame.Latitude != 0)
            {
                if (trame.DataLocationTrame.Speed > 300)
                {
                    LogDBError("Vitesse Hors limite", boitier.Nisbalise);
                }
                else
                {
                    if (Math.Abs(trame.DataLocationTrame.Longitude) > 90 || Math.Abs(trame.DataLocationTrame.Latitude) > 90)
                    {
                        LogDBError("Position Hors limite,  : " + trame.DataLocationTrame.Longitude + " et " + trame.DataLocationTrame.Latitude, boitier.Nisbalise);
                    }
                    else
                    {
                        string capteur = "000" + (trame.DataLocationTrame.Ignition > 0 ? "M" : "A"); // Carburant + Moteur
                        TrameReal tr = new TrameReal(boitier, trame.DataLocationTrame.Date.Value, trame.DataLocationTrame.Latitude, trame.DataLocationTrame.Longitude, trame.DataLocationTrame.Speed, 0, capteur, trame.DataLocationTrame.cap, null, receptionDateTime);
                        //TrameReal tr = new TrameReal(boitier, tramereal, trame.DataLocationTrame.Latitude, trame.DataLocationTrame.Longitude, trame.DataLocationTrame.Speed, i, "asd", i, null, receptionDateTime);
                        if (tr.Temps < DateTime.Now.AddHours(12))
                            if (tramereal == null || tramereal.Temps < tr.Temps)
                            {
                                OLDModelGeneratorProcessor.addTrame(tr);

                                tramereal = tr;


                                if (boitier.EcoConduite)
                                {
                                    // do something
                                    TrameEco echo = new TrameEco(tr.Temps.AddHours(1), tr.Direction, (int)Math.Truncate(tr.Vitesse), boitier.Nisbalise, (byte)(trame.DataLocationTrame.Ignition > 0 ? 1 : 0), -1, -1, null, null, null, null);
                                    OLDModelGeneratorProcessor.addEcoTrame(echo);

                                    //if (trame.Type == ConcoxTrameType.WeTrackDataAlarm)
                                    //{
                                    //    Connection.Logging("" + trame.DataLocationTrame.Alerm.ToString(), boitier.Nisbalise);
                                    //}

                                }
                            }
                    }
                }
            }
            else if ((trame.Type == ConcoxTrameType.Status) && (boitier.EcoConduite))
            {
                if (tramereal != null)
                {
                    string Voltage_Level = p.Substring(10, 2);
                    int Voltag = 0;

                    if (Voltage_Level == "06")
                        Voltag = 41;
                    else if (Voltage_Level == "05")
                        Voltag = 39;
                    else if (Voltage_Level == "04")
                        Voltag = 38;
                    else if (Voltage_Level == "03")
                        Voltag = 37;
                    else if (Voltage_Level == "02")
                        Voltag = 36;
                    else if (Voltage_Level == "01")
                        Voltag = 35;
                    else if (Voltage_Level == "00")
                        Voltag = 33;

                    TrameReal r = new TrameReal(tramereal);
                    OptionsAux ops = new OptionsAux();
                    ops[72] = Voltag;
                    r.Options = ops;
                    OLDModelGeneratorProcessor.addTramesConcoxCapteurs(r);
                }

            }

            byte protocol;
            switch (trame.Type)
            {
                case ConcoxTrameType.Unknown:
                    {
                        protocol = (byte)trame.OtherTrame.NumeroProtocol;
                        break;
                    }
                case ConcoxTrameType.eTime:
                    {
                        protocol = 0x8A;
                        break;
                    }
                default:
                    {
                        protocol = (byte)trame.Type;
                        break;
                    }

            }


            String numTrameString = p.Substring(p.Length - 12, 4);
            int numTrame = int.Parse(numTrameString, System.Globalization.NumberStyles.AllowHexSpecifier);
            byte[] bufferSend;
            if (protocol == 0x8A)
            {
                eTimeTrame checktime = new eTimeTrame();
                //Console.WriteLine("Concox Trame Type: {2}, is recepted: {0}, response: {1}", p, checktime.eTime, protocol.ToString());
                bufferSend = geteTimeBufferToSend(checktime, numTrame);
            }
            else
            {
                //Console.WriteLine("Concox Trame Type: {0}, is recepted: {1}", protocol.ToString(), p);
                bufferSend = getBufferToSend(protocol, numTrame);
            }

            if (PrincipalListner.config.Debug)
            {
                //Connection.Logging("Envoie : " + BitConverter.ToString(bufferSend).Replace("-", ""), boitier.Nisbalise);
            }

            return bufferSend;
        }

        private static byte[] geteTimeBufferToSend(eTimeTrame eCheckTime, int numTrame)
        {
            byte[] BufferToSend = new byte[16];

            byte[] BufferData = new byte[10];
            BufferData[0] = 0x0B;
            BufferData[1] = (byte)eCheckTime.NumeroProtocol;
            BufferData[2] = Convert.ToByte(Int32.Parse(eCheckTime.eTime.Substring(0, 2)));
            BufferData[3] = Convert.ToByte(Int32.Parse(eCheckTime.eTime.Substring(2, 2)));
            BufferData[4] = Convert.ToByte(Int32.Parse(eCheckTime.eTime.Substring(4, 2)));
            BufferData[5] = Convert.ToByte(Int32.Parse(eCheckTime.eTime.Substring(6, 2)));
            BufferData[6] = Convert.ToByte(Int32.Parse(eCheckTime.eTime.Substring(8, 2)));
            BufferData[7] = Convert.ToByte(Int32.Parse(eCheckTime.eTime.Substring(10, 2)));
            BufferData[8] = BitConverter.GetBytes(numTrame)[1];
            BufferData[9] = BitConverter.GetBytes(numTrame)[0];

            byte[] sum = BitConverter.GetBytes(Checksum.crc16(Checksum.CRC16_X25, BufferData));
            BufferToSend[0] = 0x78;
            BufferToSend[1] = 0x78;
            BufferToSend[2] = BufferData[0];
            BufferToSend[3] = BufferData[1];
            BufferToSend[4] = BufferData[2];
            BufferToSend[5] = BufferData[3];
            BufferToSend[6] = BufferData[4];
            BufferToSend[7] = BufferData[5];
            BufferToSend[8] = BufferData[6];
            BufferToSend[9] = BufferData[7];
            BufferToSend[10] = BufferData[8];
            BufferToSend[11] = BufferData[9];
            BufferToSend[12] = sum[1];
            BufferToSend[13] = sum[0];
            BufferToSend[14] = 0x0d;
            BufferToSend[15] = 0x0a;

            return BufferToSend;

        }

        private static byte[] getBufferToSend(byte protocolId, int numTrame)
        {
            byte[] BufferToSend = new byte[10];

            byte[] BufferData = new byte[4];
            BufferData[0] = 0x05;
            BufferData[1] = protocolId;
            BufferData[2] = BitConverter.GetBytes(numTrame)[1];
            BufferData[3] = BitConverter.GetBytes(numTrame)[0];


            byte[] sum = BitConverter.GetBytes(Checksum.crc16(Checksum.CRC16_X25, BufferData));
            BufferToSend[0] = 0x78;
            BufferToSend[1] = 0x78;
            BufferToSend[2] = BufferData[0];
            BufferToSend[3] = BufferData[1];
            BufferToSend[4] = BufferData[2];
            BufferToSend[5] = BufferData[3];
            BufferToSend[6] = sum[1];
            BufferToSend[7] = sum[0];
            BufferToSend[8] = 0x0d;
            BufferToSend[9] = 0x0a;

            return BufferToSend;

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

        //[MethodImpl(MethodImplOptions.Synchronized)]
        //private static void Logging(String message, string balise)
        //    {
        //        try { 
        //    using (StreamWriter sw = File.AppendText("Log\\Balises-" + DateTime.Now.ToString("dd-MM-yyyy") +"-" + balise + ".log"))
        //    {
        //        sw.WriteLine(DateTime.Now + ": "+message);
        //        sw.Close();
        //    }
        //        }
        //        catch (Exception)
        //        {


        //        }
        //}

        private static void spliteTrameI2BFirmware(String[] info, Balise balise, ref TrameReal trameReal, DateTime receptionDateTime)
        {
            DateTime TempsReel = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            try
            {
                TempsReel = TempsReel.AddSeconds(Int64.Parse(info[1]));
                // Console.WriteLine("TempsReel: " + TempsReel);
                if ((TempsReel - DateTime.Now).TotalHours < 720)
                {
                    // Console.WriteLine("Date invalide: {0}", TempsReel);
                    String Capteur = info[8];
                    //Console.WriteLine("Capteur: " + Capteur);
                    if (Capteur.Length == 2)
                        Capteur = "0" + Capteur;
                    else if (Capteur.Length == 1)
                    {
                        Capteur = "00" + Capteur;
                    }
                    string Moteur = info[7];
                    if (Moteur == "1")
                        Capteur = Capteur + 'M';
                    else if (Moteur == "0")
                    {
                        Capteur = Capteur + 'A';
                    }

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
                        if (!result)
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
                            if (direction > 360 || direction < 0)
                                directionReel = 0;
                            else
                                directionReel = (Int16)direction;
                        }
                        //if (info.Length > 11 && info[11].Trim() != "00")
                        //    OLDModelGeneratorProcessor.addTramesIdentDalas(new IdentDalas(balise, TempsReel, info[11])); 
                        if (LongitudeReel != 0 && LatitudeReel != 0)
                        {
                            if ((vitesseReel > 999) || (vitesseReel < 0))
                            {
                                LogDBError("Vitesse Hors limite, Vitesse : " + vitesseReel, balise.Nisbalise);
                            }
                            else
                            {
                                if (Math.Abs(LongitudeReel) > 90 || Math.Abs(LatitudeReel) > 90)
                                {
                                    LogDBError("Position Hors limite,  : " + LongitudeReel + ',' + LatitudeReel, balise.Nisbalise);
                                }
                                else
                                {
                                    TrameReal tr = new TrameReal(balise, TempsReel, LatitudeReel, LongitudeReel, vitesseReel, Temperature, Capteur, directionReel, null, receptionDateTime);

                                    if (trameReal == null || trameReal.Temps < tr.Temps)
                                    {
                                        OLDModelGeneratorProcessor.addTrame(tr);
                                        trameReal = tr;
                                    }

                                }
                            }
                        }
                    }
                    catch (FormatException fe)
                    {
                        Console.WriteLine("Unable to convert '{0}' to a Decimal.", fe.Message);
                        LogDBError("Trame  Non Valide, Trame : " + info, balise.Nisbalise);

                    }

                    catch (OverflowException ofe)
                    {
                        Console.WriteLine("'{0}' is outside the range of a Decimal.", ofe.Message);
                        LogDBError("Trame  Non Valide, Trame : " + info, balise.Nisbalise);
                    }

                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Format de date invalide" + e);
                LogDBError("Trame  Non Valide, Trame : " + info, balise.Nisbalise);
            }


        }
        private static void spliteTrameI2BFirmwareVersionBelge(String[] info, Balise balise, ref TrameReal trameReal, DateTime receptionDateTime)
        {
            DateTime TempsReel = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            try
            {
                OptionsAux options = new OptionsAux();
                TempsReel = TempsReel.AddSeconds(Int64.Parse(info[1]));
                // Console.WriteLine("TempsReel: " + TempsReel);
                if ((TempsReel - DateTime.Now).TotalHours < 720)
                {
                    // Console.WriteLine("Date invalide: {0}", TempsReel);

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


                        int entree = 80;
                        int valueOption;
                        for (int i = 4; i < info.Length; i++)
                        {
                            result = Int32.TryParse(info[i], out valueOption);
                            if (!result)
                            {
                                //Console.WriteLine("Erreur directionReel: {0} ", directionReel);
                                entree++;

                            }
                            else
                            {
                                options[entree++] = valueOption;
                            }

                        }

                        TrameReal tr = new TrameReal(balise, TempsReel, LatitudeReel, LongitudeReel, 0, 0, "000A", 0, options, receptionDateTime);

                        if (LongitudeReel != 0 && LatitudeReel != 0)
                        {

                            if (Math.Abs(LongitudeReel) > 90 || Math.Abs(LatitudeReel) > 90)
                            {
                                LogDBError("Position Hors limite,  : " + LongitudeReel + ',' + LatitudeReel, balise.Nisbalise);
                            }
                            else
                            {

                                if (trameReal == null || trameReal.Temps < tr.Temps)
                                {
                                    OLDModelGeneratorProcessor.addTrame(tr);
                                    trameReal = tr;
                                }
                            }
                        }
                        OLDModelGeneratorProcessor.addSarensTrame(tr);
                        //TrameConsumer.addTrame(tr);

                    }
                    catch (FormatException fe)
                    {
                        Console.WriteLine("Unable to convert '{0}' to a Decimal.", fe.Message);
                        LogDBError("Trame  Non Valide, Trame : " + info, balise.Nisbalise);

                    }

                    catch (OverflowException ofe)
                    {
                        Console.WriteLine("'{0}' is outside the range of a Decimal.", ofe.Message);
                        LogDBError("Trame  Non Valide, Trame : " + info, balise.Nisbalise);
                    }

                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Format de date invalide" + e);
                LogDBError("Trame  Non Valide, Trame : " + info, balise.Nisbalise);
            }


        }
        private static void spliteTrameATrackUltrason(String[] info, Balise balise, ref TrameReal trameReal, DateTime receptionDateTime)
        {
            DateTime TempsReel = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            try
            {

                TempsReel = TempsReel.AddSeconds(Int64.Parse(info[5]));
                // Console.WriteLine("TempsReel: " + TempsReel);
                if ((TempsReel - DateTime.Now).TotalHours < 720)
                {
                    // Console.WriteLine("Date invalide: {0}", TempsReel);
                    String Capteur = "000";
                    int Carburant = Convert.ToInt32(info.Length < 25 ? info[17] : info[24]);      //valeur en miliVolt position 24 dans la trame Ak1
                    Carburant = Carburant / 25;                     //echantillonage 
                    if (Carburant < 1000)
                    {
                        Capteur = convertAndformat(Carburant);      // convertir en chaine de caracter sur 3 positions
                    }
                    //Console.WriteLine("Capteur: " + Capteur);

                    int Moteur = Convert.ToInt16((info[14]));
                    //Console.WriteLine("Etat Moteur: " + Moteur);
                    if (Moteur % 2 == 1)
                        Capteur = Capteur + "M";
                    else
                        Capteur = Capteur + "A";

                    try
                    {
                        bool result;
                        Decimal LatitudeReel;
                        //Console.WriteLine(trame.Substring(9, 8));
                        result = Decimal.TryParse(info[9], out LatitudeReel);
                        if (!result)
                        {
                            LatitudeReel = 0;
                        }
                        LatitudeReel = LatitudeReel / 1000000;
                        //Console.WriteLine("LatitudeReel: " + LatitudeReel);
                        Decimal LongitudeReel;
                        //Console.WriteLine(trame.Substring(17, 8));
                        result = Decimal.TryParse(info[8], out LongitudeReel);
                        if (!result)
                        {

                            LongitudeReel = 0;

                        }
                        LongitudeReel = LongitudeReel / 1000000;
                        double vitesseReel;
                        result = double.TryParse(info[15].Replace('.', ','), out vitesseReel);
                        if (!result)
                        {
                            //Console.WriteLine("Erreur vitesseReel: {0}", vitesseReel);
                            vitesseReel = 0;
                        }
                        Int16 Temperature;
                        result = Int16.TryParse(info[19], out Temperature);
                        if (!result)
                        {
                            //Console.WriteLine("Erreur Temperature: {0} ", Temperature);
                            Temperature = 0;
                        }
                        else
                            Temperature = (Int16)(Temperature / 10);

                        if (info[18] != null && info[18].Trim().Length >= 12)
                            OLDModelGeneratorProcessor.addTramesIdentDalas(new IdentDalas(balise, TempsReel, info[18].Trim()));

                        Int16 directionReel;
                        Double direction;
                        result = Double.TryParse(info[10], out direction);
                        if (!result)
                        {
                            //Console.WriteLine("Erreur directionReel: {0} ", directionReel);
                            directionReel = 0;
                        }
                        else
                        {
                            if (direction > 360 || direction < 0)
                                directionReel = 0;
                            else
                                directionReel = (Int16)direction;
                        }

                        OptionsAux options = new OptionsAux();
                        try
                        {
                            Int16 moteur;
                            result = Int16.TryParse(info[14], out moteur);
                            if (!result)
                            {
                                moteur = 0;
                            }

                            Int16 MVolt; //Main voltage (Batterie Vehicule)
                            result = Int16.TryParse(info.Length < 23 ? "0" : info[22], out MVolt);
                            if (!result)
                            {
                                MVolt = 0;
                            }

                            Int16 BVolt; //Backup voltage (Batterie Boitier)
                            result = Int16.TryParse(info.Length < 24 ? "0" : info[23], out BVolt);
                            if (!result)
                            {
                                BVolt = 0;
                            }

                            Int16 Ultrason; //Capteur (Ultrason)
                            result = Int16.TryParse(info.Length < 25 ? "0" : info[24], out Ultrason);
                            if (!result)
                            {
                                Ultrason = 0;
                            }

                            Int32 Odometre; //Odometre
                            result = Int32.TryParse(info[12], out Odometre);
                            if (!result)
                            {
                                Odometre = 0;
                            }



                            //options[64] = Convert.ToInt32(info.Length < 25 ? info[17] : info[24]); //valeur Carburant
                            //options[65] = Temperature;
                            //options[66] = moteur;
                            options[71] = MVolt;//Main voltage (Batterie Vehicule)
                            options[72] = BVolt;//Backup Voltage (Batterie Balise)
                            options[73] = Ultrason;// Valeur Carburant Ultrason
                            options[74] = Odometre;// Odometre
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Conversion Invalid Capteur .", e.Message);
                            LogDBError("Conversion Invalid Capteur  : " + info + " \n Exception : " + e.Message, balise.Nisbalise);
                        }

                        if (LongitudeReel != 0 && LatitudeReel != 0)
                        {
                            if ((vitesseReel > 999) || (vitesseReel < 0))
                            {
                                LogDBError("Vitesse Hors limite", balise.Nisbalise);
                            }
                            else
                            {
                                if (Math.Abs(LongitudeReel) > 90 || Math.Abs(LatitudeReel) > 90)
                                {
                                    LogDBError("Position Hors limite,  : " + LongitudeReel + " et " + LatitudeReel, balise.Nisbalise);
                                }
                                else
                                {
                                    TrameReal tr = new TrameReal(balise, TempsReel, LatitudeReel, LongitudeReel, vitesseReel, Temperature, Capteur, directionReel, options, receptionDateTime);

                                    if (trameReal == null || trameReal.Temps < tr.Temps)
                                    {
                                        OLDModelGeneratorProcessor.addTrame(tr);
                                        trameReal = tr;
                                    }
                                }
                            }
                        }
                    }
                    catch (FormatException fe)
                    {
                        Console.WriteLine("Unable to convert '{0}' to a Decimal.", fe.Message);
                        LogDBError("Trame  Non Valide, Trame : " + info + " \n Exception : " + fe.Message, balise.Nisbalise);
                    }

                    catch (OverflowException ofe)
                    {
                        Console.WriteLine("'{0}' is outside the range of a Decimal.", ofe.Message);
                        LogDBError("Trame  Non Valide, Trame : " + info + "Exception : " + ofe.Message, balise.Nisbalise);
                    }

                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Format de date invalide" + e);
                LogDBError("Trame  Non Valide, Trame : " + info + "Exception : " + e.Message, balise.Nisbalise);
            }


        }

        private static void spliteTrameATrackCAN(String[] info, Balise balise, ref TrameReal trameReal, DateTime receptionDateTime)
        {
            DateTime TempsReel = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            try
            {
                TempsReel = TempsReel.AddSeconds(Int64.Parse(info[5]));

                if ((TempsReel - DateTime.Now).TotalHours < 720)
                {

                    String Capteur = "000";
                    int Carburant = Convert.ToInt32(info[17]);      //valeur en miliVolt position 17 dans la trame Ak1
                    Carburant = Carburant / 25;                     //echantillonage 
                    if (Carburant < 1000)
                    {
                        Capteur = convertAndformat(Carburant);      // convertir en chaine de caracter sur 3 positions
                    }
                    //Console.WriteLine("Capteur: " + Capteur);

                    int Moteur = Convert.ToInt16((info[14]));
                    if (Moteur % 2 == 1)
                        Capteur = Capteur + "M";
                    else
                        Capteur = Capteur + "A";

                    try
                    {
                        bool result;
                        Decimal LatitudeReel;
                        //Console.WriteLine(trame.Substring(9, 8));
                        result = Decimal.TryParse(info[9], out LatitudeReel);
                        if (!result)
                        {
                            LatitudeReel = 0;
                        }
                        LatitudeReel = LatitudeReel / 1000000;
                        //Console.WriteLine("LatitudeReel: " + LatitudeReel);
                        Decimal LongitudeReel;
                        //Console.WriteLine(trame.Substring(17, 8));
                        result = Decimal.TryParse(info[8], out LongitudeReel);
                        if (!result)
                        {
                            LongitudeReel = 0;
                        }
                        LongitudeReel = LongitudeReel / 1000000;
                        double vitesseReel;
                        result = double.TryParse(info[15].Replace('.', ','), out vitesseReel);
                        if (!result)
                        {
                            vitesseReel = 0;
                        }

                        Int16 Temperature;
                        result = Int16.TryParse(info.Length < 20 ? "0" : info[19], out Temperature);
                        if (!result)
                        {
                            Temperature = 0;
                        }
                        else
                            Temperature = (Int16)(Temperature - 40);

                        if (info[18] != null && info[18].Trim().Length >= 12)
                            OLDModelGeneratorProcessor.addTramesIdentDalas(new IdentDalas(balise, TempsReel, info[18].Trim()));

                        Int16 directionReel;
                        Double direction;
                        result = Double.TryParse(info[10], out direction);
                        if (!result)
                        {
                            directionReel = 0;
                        }
                        else
                        {
                            if (direction > 360 || direction < 0)
                                directionReel = 0;
                            else
                                directionReel = (Int16)direction;
                        }

                        string trailer = string.Empty; //Trailer ID --1
                        int i = 1;

                        if (info.Length == 39) //info.Length == 39 LA Trame contient Trailer ID
                        {
                            i = 0;
                            trailer = (info.Length < 25 ? string.Empty : String.IsNullOrWhiteSpace(info[24]) ? string.Empty : info[24].Trim());
                            try
                            {
                                if (trameReal != null)
                                {
                                    //if ((trailer == null || trailer == "") && (trameReal.TrailerID != "" || trameReal.TrailerID != null))
                                    if ((trailer == string.Empty) && (trameReal.TrailerID != string.Empty))
                                    {// Update Trailer ID
                                        Console.WriteLine("Trailer ID:{0} is Detached.", trameReal.TrailerID);
                                        OLDModelGeneratorProcessor.addTramesTrailerID(new TrailerID(balise.Nisbalise, TempsReel, trameReal.TrailerID, trameReal.TrailerID));
                                    }
                                    //else if (trailer != null && trailer != "")
                                    else if (trailer != string.Empty)
                                    {// Insert Trailer ID
                                        if (trameReal.TrailerID == string.Empty)
                                        {
                                            Console.WriteLine("Trailer ID: {0} is Attached.", trailer);
                                            OLDModelGeneratorProcessor.addTramesTrailerID(new TrailerID(balise.Nisbalise, TempsReel, trailer, trameReal.TrailerID));
                                        }
                                    }
                                }
                                else if (trailer != string.Empty)
                                {
                                    Console.WriteLine("Trailer ID:{0}. First Trame Received", trailer);
                                    OLDModelGeneratorProcessor.addTramesTrailerID(new TrailerID(balise.Nisbalise, TempsReel, trailer, null));
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("New Trailer ID: {0}, Old Trailer ID {1}.", trailer, (trameReal == null ? "NULL" : trameReal.TrailerID));
                            }
                        }


                        OptionsAux options = new OptionsAux();
                        try
                        {
                            Int16 moteur;
                            result = Int16.TryParse(info[14], out moteur);
                            if (!result)
                            {
                                moteur = 0;
                            }

                            Int16 MVolt; //Main voltage (Batterie Vehicule)
                            result = Int16.TryParse(info.Length < 23 ? "0" : info[22], out MVolt);
                            if (!result)
                            {
                                MVolt = 0;
                            }

                            Int16 BVolt; //Backup voltage (Batterie Boitier)
                            result = Int16.TryParse(info.Length < 24 ? "0" : info[23], out BVolt);
                            if (!result)
                            {
                                BVolt = 0;
                            }


                            double FuelLevel; //Niveau Carburant %
                            result = double.TryParse(info.Length < 26 - i ? "0" : info[25 - i], out FuelLevel);
                            if (!result)
                            {
                                FuelLevel = 0;
                            }
                            else
                            {
                                FuelLevel *= 0.4;
                            }

                            double FuelRate; //Consommation Carburant L/h
                            result = double.TryParse(info.Length < 27 - i ? "0" : info[26 - i], out FuelRate);
                            if (!result)
                            {
                                FuelRate = 0;
                            }
                            else
                            {
                                FuelRate *= 0.05;
                            }

                            double FuelEco; //Consommation Carburant Km/L
                            result = double.TryParse(info.Length < 28 - i ? "0" : info[27 - i], out FuelEco);
                            if (!result)
                            {
                                FuelEco = 0;
                            }
                            else
                            {
                                FuelEco = FuelEco / 512;
                            }

                            double FuelUsed;
                            result = double.TryParse(info.Length < 29 - i ? "0" : info[28 - i], out FuelUsed);
                            if (!result)
                            {
                                FuelUsed = 0;
                            }
                            else
                            {
                                FuelUsed = FuelUsed / 2;
                            }

                            Int32 Kilometrage;
                            result = Int32.TryParse(info.Length < 30 - i ? "0" : info[29 - i], out Kilometrage);
                            if (!result)
                            {
                                Kilometrage = 0;
                            }

                            Int32 TempMoteur;
                            result = Int32.TryParse(info.Length < 31 - i ? "0" : info[30 - i], out TempMoteur);
                            if (!result)
                            {
                                //Console.WriteLine("Erreur Temperature: {0} ", Temperature);
                                TempMoteur = 0;
                            }
                            else
                            {
                                TempMoteur = (Int32)(TempMoteur - 40);
                            }

                            Int32 RPM;
                            result = Int32.TryParse(info.Length < 32 - i ? "0" : info[31 - i], out RPM);
                            if (!result)
                            {
                                //Console.WriteLine("Erreur Temperature: {0} ", Temperature);
                                RPM = 0;
                            }

                            double TFEngin;  //temps de fonctionnement Moteur
                            result = Double.TryParse(info.Length < 33 - i ? "0" : info[32 - i], out TFEngin);
                            if (!result)
                            {
                                TFEngin = 0;
                            }
                            else
                            {
                                TFEngin *= 0.05;
                            }



                            //options[64] = Convert.ToInt32(info.Length < 25 ? info[17] : info[24]); //valeur Carburant
                            //options[65] = Temperature;
                            //options[66] = moteur;
                            options[71] = MVolt;//Main voltage (Batterie Vehicule)
                            options[72] = BVolt;//Backup Voltage (Batterie Balise)
                            try
                            {
                                options[75] = Convert.ToInt32(FuelLevel); // Valeur Niveau Carburant
                            }
                            catch (Exception e)
                            {
                                options[75] = 0;
                                Console.WriteLine("Niveau Carburant exception: {0}", e.Message);
                            }
                            try
                            {
                                options[76] = Convert.ToInt32(FuelRate); // Valeur Consommation Carburant
                            }
                            catch (Exception e)
                            {
                                options[76] = 0;
                                Console.WriteLine("Consommation Carburant exception: {0}", e.Message);
                            }

                            try
                            {
                                options[77] = Convert.ToInt32(FuelUsed); // Valeur Consommation Carburant
                            }
                            catch (Exception e)
                            {
                                options[77] = 0;
                                Console.WriteLine("Consommation Carburant Total exception: {0}", e.Message);
                            }

                            try
                            {
                                options[78] = Convert.ToInt32(FuelEco); // Valeur Consommation Carburant
                            }
                            catch (Exception e)
                            {
                                options[78] = 0;
                                Console.WriteLine("Carburant Economy Km/l exception: {0}", e.Message);
                            }

                            try
                            {
                                if (TempMoteur > 0)
                                    options[79] = TempMoteur; // Temperature Coolant Engin
                            }
                            catch (Exception e)
                            {
                                //options[79] = 0;
                                Console.WriteLine(" Temperature Coolant Engin exception: {0}", e.Message);
                            }


                            try
                            {
                                options[87] = Convert.ToInt32(RPM); // RMP Tours Minute
                            }
                            catch (Exception e)
                            {
                                options[87] = 0;
                                Console.WriteLine(" RPM exception: {0}", e.Message);
                            }


                            try
                            {
                                options[89] = Convert.ToInt32(TFEngin); // Temps de Fonctionnement Moteur
                            }
                            catch (Exception e)
                            {
                                options[89] = 0;
                                Console.WriteLine(" TFEngin exception: {0}", e.Message);
                            }

                            try
                            {
                                options[90] = Convert.ToInt32(Kilometrage); // Kilometrage Vehicule
                            }
                            catch (Exception e)
                            {
                                options[90] = 0;
                                Console.WriteLine(" Kilomtrage exception: {0}", e.Message);
                            }

                            if (i == 0) // i ==0 ma3netha @F fih Trialer ID wzid ghir client BL il demande les poits
                            {
                                Int32 AxelWeight0;
                                Int32.TryParse(info.Length < 35 ? "0" : info[34], out AxelWeight0);
                                Int32 AxelWeight1;
                                Int32.TryParse(info.Length < 36 ? "0" : info[35], out AxelWeight1);
                                Int32 AxelWeight2;
                                Int32.TryParse(info.Length < 37 ? "0" : info[36], out AxelWeight2);
                                Int32 AxelWeight3;
                                Int32.TryParse(info.Length < 38 ? "0" : info[37], out AxelWeight3);
                                Int32 AxelWeight4;
                                Int32.TryParse(info.Length < 39 ? "0" : info[38], out AxelWeight4);

                                options[80] = AxelWeight0;
                                options[81] = AxelWeight1;
                                options[82] = AxelWeight2;
                                options[83] = AxelWeight3;
                                options[84] = AxelWeight4;

                            }

                            //options[76] = Odometre;// Odometre
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Conversion Invalid Capteur .", e.Message);
                            LogDBError("Conversion Invalid Capteur  : " + info + " \n Exception : " + e.Message, balise.Nisbalise);
                        }

                        if (LongitudeReel != 0 && LatitudeReel != 0)
                        {
                            if ((vitesseReel > 999) || (vitesseReel < 0))
                            {
                                LogDBError("Vitesse Hors limite", balise.Nisbalise);
                            }
                            else
                            {
                                if (Math.Abs(LongitudeReel) > 90 || Math.Abs(LatitudeReel) > 90)
                                {
                                    LogDBError("Position Hors limite,  : " + LongitudeReel + " et " + LatitudeReel, balise.Nisbalise);
                                }
                                else
                                {
                                    TrameReal tr = new TrameReal(balise, TempsReel, LatitudeReel, LongitudeReel, vitesseReel, Temperature, Capteur, directionReel, trailer, options, receptionDateTime);

                                    if (trameReal == null || trameReal.Temps < tr.Temps)
                                    {
                                        OLDModelGeneratorProcessor.addTrame(tr);
                                        trameReal = tr;
                                    }
                                }
                            }
                        }
                    }
                    catch (FormatException fe)
                    {
                        Console.WriteLine("Unable to convert '{0}' to a Decimal.", fe.Message);
                        LogDBError("Trame  Non Valide, Trame : " + info + " \n Exception : " + fe.Message, balise.Nisbalise);
                    }

                    catch (OverflowException ofe)
                    {
                        Console.WriteLine("'{0}' is outside the range of a Decimal.", ofe.Message);
                        LogDBError("Trame  Non Valide, Trame : " + info + "Exception : " + ofe.Message, balise.Nisbalise);
                    }

                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Format de date invalide" + e);
                LogDBError("Trame  Non Valide, Trame : " + info + "Exception : " + e.Message, balise.Nisbalise);
            }


        }

        private static void spliteTrameATrackCANEco(String[] info, Balise balise, ref TrameReal trameReal, DateTime receptionDateTime)
        {
            DateTime TempsReel = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            try
            {
                TempsReel = TempsReel.AddSeconds(Int64.Parse(info[5]));
                // Console.WriteLine("TempsReel: " + TempsReel);
                if ((TempsReel - DateTime.Now).TotalHours < 720)
                {
                    // Console.WriteLine("Date invalide: {0}", TempsReel);
                    String Capteur = "000";
                    int Carburant = Convert.ToInt32(info[17]);      //valeur en miliVolt position 17 dans la trame Ak1
                    Carburant = Carburant / 25;                     //echantillonage 
                    if (Carburant < 1000)
                    {
                        Capteur = convertAndformat(Carburant);      // convertir en chaine de caracter sur 3 positions
                    }
                    //Console.WriteLine("Capteur: " + Capteur);

                    int Moteur = Convert.ToInt16((info[14]));
                    if (Moteur % 2 == 1)
                        Capteur += "M";
                    else
                        Capteur += "A";

                    try
                    {
                        bool result;
                        Decimal LatitudeReel;
                        //Console.WriteLine(trame.Substring(9, 8));
                        result = Decimal.TryParse(info[9], out LatitudeReel);
                        if (!result)
                        {
                            LatitudeReel = 0;
                        }
                        LatitudeReel = LatitudeReel / 1000000;
                        //Console.WriteLine("LatitudeReel: " + LatitudeReel);
                        Decimal LongitudeReel;
                        //Console.WriteLine(trame.Substring(17, 8));
                        result = Decimal.TryParse(info[8], out LongitudeReel);
                        if (!result)
                        {

                            LongitudeReel = 0;

                        }
                        LongitudeReel = LongitudeReel / 1000000;
                        double vitesseReel;
                        result = double.TryParse(info[15].Replace('.', ','), out vitesseReel);
                        if (!result)
                        {
                            //Console.WriteLine("Erreur vitesseReel: {0}", vitesseReel);
                            vitesseReel = 0;
                        }

                        Int16 Temperature;
                        result = Int16.TryParse(info.Length < 20 ? "0" : info[19], out Temperature);
                        if (!result)
                        {
                            //Console.WriteLine("Erreur Temperature: {0} ", Temperature);
                            Temperature = 0;
                        }
                        else
                            Temperature = (Int16)(Temperature - 40);

                        if (info[18] != null && info[18].Trim().Length >= 12)
                            OLDModelGeneratorProcessor.addTramesIdentDalas(new IdentDalas(balise, TempsReel, info[18].Trim()));

                        Int16 directionReel;
                        Double direction;
                        result = Double.TryParse(info[10], out direction);
                        if (!result)
                        {
                            //Console.WriteLine("Erreur directionReel: {0} ", directionReel);
                            directionReel = 0;
                        }
                        else
                        {
                            if (direction > 360 || direction < 0)
                                directionReel = 0;
                            else
                                directionReel = (Int16)direction;
                        }

                        //--2
                        string trailer = (info.Length < 25 ? string.Empty : String.IsNullOrWhiteSpace(info[24]) ? string.Empty : info[24].Trim());
                        try
                        {
                            if (trameReal != null)
                            {
                                //if ((trailer == null || trailer == "") && (trameReal.TrailerID != "" || trameReal.TrailerID != null))
                                if ((trailer == string.Empty) && (trameReal.TrailerID != string.Empty))
                                {// Update Trailer ID
                                    Console.WriteLine("Trailer ID:{0} is Detached.", trameReal.TrailerID);
                                    OLDModelGeneratorProcessor.addTramesTrailerID(new TrailerID(balise.Nisbalise, TempsReel, trameReal.TrailerID, trameReal.TrailerID));
                                }
                                //else if (trailer != null && trailer != "")
                                else if ((trailer != string.Empty) && (trameReal.TrailerID == string.Empty))
                                {// Insert Trailer ID
                                    Console.WriteLine("Trailer ID: {0} is Attached.", trailer);
                                    OLDModelGeneratorProcessor.addTramesTrailerID(new TrailerID(balise.Nisbalise, TempsReel, trailer, trameReal.TrailerID));
                                }
                            }
                            else if (trailer != string.Empty)
                            {
                                Console.WriteLine("Trailer ID:{0}. First Trame Received", trailer);
                                OLDModelGeneratorProcessor.addTramesTrailerID(new TrailerID(balise.Nisbalise, TempsReel, trailer, string.Empty));
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("New Trailer ID: {0}, Old Trailer ID {1}.", trailer, (trameReal == null ? "NULL" : trameReal.TrailerID));
                        }


                        int index = 0; //ali 09/06/2021

                        if (info.Length == 40)
                            index = 1;

                        OptionsAux options = new OptionsAux();
                        try
                        {
                            Int16 moteur;
                            result = Int16.TryParse(info[14], out moteur);
                            if (!result)
                            {
                                moteur = 0;
                            }

                            Int16 MVolt; //Main voltage (Batterie Vehicule)
                            result = Int16.TryParse(info.Length < 23 ? "0" : info[22], out MVolt);
                            if (!result)
                            {
                                MVolt = 0;
                            }

                            Int16 BVolt; //Backup voltage (Batterie Boitier)
                            result = Int16.TryParse(info.Length < 24 ? "0" : info[23], out BVolt);
                            if (!result)
                            {
                                BVolt = 0;
                            }


                            double FuelLevel; //Niveau Carburant %
                            result = double.TryParse(info.Length < 26 + index ? "0" : info[25 + index], out FuelLevel);
                            if (!result)
                            {
                                FuelLevel = 0;
                            }
                            else
                            {
                                FuelLevel *= 0.4;
                            }

                            double FuelRate; //Consommation Carburant L/h
                            result = double.TryParse(info.Length < 27 + index ? "0" : info[26 + index], out FuelRate);
                            if (!result)
                            {
                                FuelRate = 0;
                            }
                            else
                            {
                                FuelRate *= 0.05;
                            }

                            double FuelEco; //Consommation Carburant Km/L
                            result = double.TryParse(info.Length < 28 + index ? "0" : info[27 + index], out FuelEco);
                            if (!result)
                            {
                                FuelEco = 0;
                            }
                            else
                            {
                                FuelEco = FuelEco / 512;
                            }

                            double FuelUsed;
                            result = double.TryParse(info.Length < 29 + index ? "0" : info[28 + index], out FuelUsed);
                            if (!result)
                            {
                                FuelUsed = 0;
                            }
                            else
                            {
                                FuelUsed = FuelUsed / 2;
                            }

                            Int32 Kilometrage;
                            result = Int32.TryParse(info.Length < 30 + index ? "0" : info[29 + index], out Kilometrage);
                            if (!result)
                            {
                                Kilometrage = 0;
                            }

                            Int32 TempMoteur;
                            result = Int32.TryParse(info.Length < 31 + index ? "0" : info[30 + index], out TempMoteur);
                            if (!result)
                            {
                                //Console.WriteLine("Erreur Temperature: {0} ", Temperature);
                                TempMoteur = 0;
                            }
                            else
                            {
                                TempMoteur = (Int32)(TempMoteur - 40);
                            }

                            Int32 RPM;
                            result = Int32.TryParse(info.Length < 32 + index ? "0" : info[31 + index], out RPM);
                            if (!result)
                            {
                                //Console.WriteLine("Erreur Temperature: {0} ", Temperature);
                                RPM = 0;
                            }

                            double TFEngin;  //temps de fonctionnement Moteur
                            result = Double.TryParse(info.Length < 33 + index ? "0" : info[32 + index], out TFEngin);
                            if (!result)
                            {
                                TFEngin = 0;
                            }
                            else
                            {
                                TFEngin *= 0.05;
                            }


                            Int32 AxelWeight0;
                            Int32.TryParse(info.Length < 35 + index ? "0" : info[34 + index], out AxelWeight0);
                            Int32 AxelWeight1;
                            Int32.TryParse(info.Length < 36 + index ? "0" : info[35 + index], out AxelWeight1);
                            Int32 AxelWeight2;
                            Int32.TryParse(info.Length < 37 + index ? "0" : info[36 + index], out AxelWeight2);
                            Int32 AxelWeight3;
                            Int32.TryParse(info.Length < 38 + index ? "0" : info[37 + index], out AxelWeight3);
                            Int32 AxelWeight4;
                            Int32.TryParse(info.Length < 39 + index ? "0" : info[38 + index], out AxelWeight4);
                            //options[64] = Convert.ToInt32(info.Length < 25 ? info[17] : info[24]); //valeur Carburant
                            //options[65] = Temperature;
                            //options[66] = moteur;
                            options[71] = MVolt;//Main voltage (Batterie Vehicule)
                            options[72] = BVolt;//Backup Voltage (Batterie Balise)
                            try
                            {
                                options[75] = Convert.ToInt32(FuelLevel); // Valeur Niveau Carburant
                            }
                            catch (Exception e)
                            {
                                options[75] = 0;
                                Console.WriteLine("Niveau Carburant exception: {0}", e.Message);
                            }
                            try
                            {
                                options[76] = Convert.ToInt32(FuelRate); // Valeur Consommation Carburant
                            }
                            catch (Exception e)
                            {
                                options[76] = 0;
                                Console.WriteLine("Consommation Carburant exception: {0}", e.Message);
                            }

                            try
                            {
                                options[77] = Convert.ToInt32(FuelUsed); // Valeur Consommation Carburant
                            }
                            catch (Exception e)
                            {
                                options[77] = 0;
                                Console.WriteLine("Consommation Carburant Total exception: {0}", e.Message);
                            }

                            try
                            {
                                options[78] = Convert.ToInt32(FuelEco); // Valeur Consommation Carburant
                            }
                            catch (Exception e)
                            {
                                options[78] = 0;
                                Console.WriteLine("Carburant Economy Km/l exception: {0}", e.Message);
                            }

                            try
                            {
                                if (TempMoteur > 0)
                                    options[79] = TempMoteur; // Temperature Coolant Engin
                            }
                            catch (Exception e)
                            {
                                //options[79] = 0;
                                Console.WriteLine(" Temperature Coolant Engin exception: {0}", e.Message);
                            }


                            try
                            {
                                options[87] = Convert.ToInt32(RPM); // RMP Tours Minute
                            }
                            catch (Exception e)
                            {
                                options[87] = 0;
                                Console.WriteLine(" RPM exception: {0}", e.Message);
                            }


                            try
                            {
                                options[89] = Convert.ToInt32(TFEngin); // Temps de Fonctionnement Moteur
                            }
                            catch (Exception e)
                            {
                                options[89] = 0;
                                Console.WriteLine(" TFEngin exception: {0}", e.Message);
                            }


                            try
                            {
                                options[90] = Convert.ToInt32(Kilometrage); // Kilometrage Vehicule
                            }
                            catch (Exception e)
                            {
                                options[90] = 0;
                                Console.WriteLine(" Kilomtrage exception: {0}", e.Message);
                            }

                            options[80] = AxelWeight0;
                            options[81] = AxelWeight1;
                            options[82] = AxelWeight2;
                            options[83] = AxelWeight3;
                            options[84] = AxelWeight4;



                            //options[76] = Odometre;// Odometre
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Conversion Invalid Capteur .", e.Message);
                            LogDBError("Conversion Invalid Capteur  : " + info + " \n Exception : " + e.Message, balise.Nisbalise);
                        }

                        if (LongitudeReel != 0 && LatitudeReel != 0)
                        {
                            if ((vitesseReel > 999) || (vitesseReel < 0))
                            {
                                LogDBError("Vitesse Hors limite", balise.Nisbalise);
                            }
                            else
                            {
                                if (Math.Abs(LongitudeReel) > 90 || Math.Abs(LatitudeReel) > 90)
                                {
                                    LogDBError("Position Hors limite,  : " + LongitudeReel + " et " + LatitudeReel, balise.Nisbalise);
                                }
                                else
                                {
                                    TrameReal tr = new TrameReal(balise, TempsReel, LatitudeReel, LongitudeReel, vitesseReel, Temperature, Capteur, directionReel, trailer, options, receptionDateTime);

                                    if (trameReal == null || trameReal.Temps < tr.Temps)
                                    {
                                        if (!balise.EcoConduite)
                                        {
                                            OLDModelGeneratorProcessor.addTrame(tr);
                                            trameReal = tr;
                                        }
                                        else if (info.Length == 40) // verification lengeur de trame 
                                        {
                                            OLDModelGeneratorProcessor.addTrameMidNightSplitTrajet(tr);
                                            trameReal = tr;
                                            string G_Force, Hix;
                                            Int16? Gx = null;
                                            Int16? Gy = null;
                                            Int16? Gz = null;
                                            Int16? CCarb = null;
                                            try
                                            {

                                                G_Force = (info.Length < 26 ? "" : String.IsNullOrWhiteSpace(info[25]) ? "" : info[25].Trim());
                                                if (G_Force != "")
                                                {
                                                    Hix = G_Force.Substring(0, 4);
                                                    Gx = Convert.ToInt16("0x" + Hix, 16);

                                                    Hix = G_Force.Substring(4, 4);
                                                    Gy = Convert.ToInt16("0x" + Hix, 16);

                                                    Hix = G_Force.Substring(8, 4);
                                                    Gz = Convert.ToInt16("0x" + Hix, 16);
                                                }

                                                Int16 rpm = -1;
                                                string rpm_s = (info.Length < 33 ? "" : String.IsNullOrWhiteSpace(info[32]) ? "" : info[32].Trim());

                                                if (!Int16.TryParse(rpm_s, out rpm))
                                                    rpm = -1;

                                                TrameEco echo = new TrameEco(tr.Temps.AddHours(1), tr.Direction, (int)Math.Truncate(tr.Vitesse), balise.Nisbalise, (byte)(Moteur % 2 == 1 ? 1 : 0), rpm, Temperature, Gx, Gy, Gz, CCarb);
                                                OLDModelGeneratorProcessor.addEcoTrame(echo);

                                            }
                                            catch (Exception ex)
                                            {
                                                Console.WriteLine("Echo conduit G_Force Invalide Values: '{0}' to a Decimal.", ex.Message);
                                                LogDBError("Echo confuit G_Force Invalide Values" + info + " \n Exception : " + ex.Message, balise.Nisbalise);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (FormatException fe)
                    {
                        Console.WriteLine("Unable to convert '{0}' to a Decimal.", fe.Message);
                        LogDBError("Trame  Non Valide, Trame : " + info + " \n Exception : " + fe.Message, balise.Nisbalise);
                    }

                    catch (OverflowException ofe)
                    {
                        Console.WriteLine("'{0}' is outside the range of a Decimal.", ofe.Message);
                        LogDBError("Trame  Non Valide, Trame : " + info + "Exception : " + ofe.Message, balise.Nisbalise);
                    }

                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Format de date invalide" + e);
                LogDBError("Trame  Non Valide, Trame : " + info + "Exception : " + e.Message, balise.Nisbalise);
            }


        }

        private static void spliteTrameATrack(String[] info, Balise balise, ref TrameReal trameReal, DateTime receptionDateTime)
        {
            DateTime TempsReel = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            try
            {

                TempsReel = TempsReel.AddSeconds(Int64.Parse(info[5]));
                // Console.WriteLine("TempsReel: " + TempsReel);
                if ((TempsReel - DateTime.Now).TotalHours < 720)
                {
                    // Console.WriteLine("Date invalide: {0}", TempsReel);
                    String Capteur = "000";
                    int Carburant = Convert.ToInt32(info.Length < 25 ? info[17] : info[24]);      //valeur en miliVolt position 24 dans la trame Ak1
                    Carburant = Carburant / 25;                     //echantillonage 
                    if (Carburant < 1000)
                    {
                        Capteur = convertAndformat(Carburant);      // convertir en chaine de caracter sur 3 positions
                    }
                    //Console.WriteLine("Capteur: " + Capteur);

                    int Moteur = Convert.ToInt16((info[14]));
                    //Console.WriteLine("Etat Moteur: " + Moteur);
                    if (Moteur % 2 == 1)
                        Capteur = Capteur + "M";
                    else
                        Capteur = Capteur + "A";

                    try
                    {
                        bool result;
                        Decimal LatitudeReel;
                        //Console.WriteLine(trame.Substring(9, 8));
                        result = Decimal.TryParse(info[9], out LatitudeReel);
                        if (!result)
                        {
                            LatitudeReel = 0;
                        }
                        LatitudeReel = LatitudeReel / 1000000;
                        //Console.WriteLine("LatitudeReel: " + LatitudeReel);
                        Decimal LongitudeReel;
                        //Console.WriteLine(trame.Substring(17, 8));
                        result = Decimal.TryParse(info[8], out LongitudeReel);
                        if (!result)
                        {
                            LongitudeReel = 0;
                        }
                        LongitudeReel = LongitudeReel / 1000000;
                        double vitesseReel;
                        result = double.TryParse(info[15].Replace('.', ','), out vitesseReel);
                        if (!result)
                        {
                            //Console.WriteLine("Erreur vitesseReel: {0}", vitesseReel);
                            vitesseReel = 0;
                        }
                        Int16 Temperature;
                        result = Int16.TryParse(info[19], out Temperature);
                        if (!result)
                        {
                            //Console.WriteLine("Erreur Temperature: {0} ", Temperature);
                            Temperature = 0;
                        }
                        else
                            Temperature = (Int16)(Temperature / 10);

                        if (info[18] != null && info[18].Trim().Length >= 12)
                            OLDModelGeneratorProcessor.addTramesIdentDalas(new IdentDalas(balise, TempsReel, info[18].Trim()));

                        Int16 directionReel;
                        Double direction;
                        result = Double.TryParse(info[10], out direction);
                        if (!result)
                        {
                            //Console.WriteLine("Erreur directionReel: {0} ", directionReel);
                            directionReel = 0;
                        }
                        else
                        {
                            if (direction > 360 || direction < 0)
                                directionReel = 0;
                            else
                                directionReel = (Int16)direction;
                        }


                        string trailer; //Trailer ID
                        trailer = (info.Length < 25 ? string.Empty : String.IsNullOrWhiteSpace(info[24]) ? string.Empty : info[24].Trim());
                        try
                        {
                            if (trameReal != null)
                            {
                                //if ((trailer == null || trailer == "") && (trameReal.TrailerID != "" || trameReal.TrailerID != null))
                                if ((trailer == string.Empty) && (trameReal.TrailerID != string.Empty))
                                {// Update Trailer ID
                                    Console.WriteLine("Trailer ID:{0} is Detached.", trameReal.TrailerID);
                                    OLDModelGeneratorProcessor.addTramesTrailerID(new TrailerID(balise.Nisbalise, TempsReel, trameReal.TrailerID, trameReal.TrailerID));
                                }
                                //else if (trailer != null && trailer != "")
                                else if ((trailer != string.Empty) && (trameReal.TrailerID == string.Empty))
                                {// Insert Trailer ID

                                    Console.WriteLine("Trailer ID: {0} is Attached.", trailer);
                                    OLDModelGeneratorProcessor.addTramesTrailerID(new TrailerID(balise.Nisbalise, TempsReel, trailer, trameReal.TrailerID));
                                }
                            }
                            else
                            {
                                Console.WriteLine("Trailer ID:{0}. First Trame Received", trailer);
                                if (trailer != string.Empty)
                                    OLDModelGeneratorProcessor.addTramesTrailerID(new TrailerID(balise.Nisbalise, TempsReel, trailer, string.Empty));
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("New Trailer ID: {0}, Old Trailer ID {1}.", trailer, (trameReal == null ? "NULL" : trameReal.TrailerID));
                        }

                        OptionsAux options = new OptionsAux();
                        try
                        {
                            Int16 moteur;
                            result = Int16.TryParse(info[14], out moteur);
                            if (!result)
                            {
                                moteur = 0;
                            }

                            Int16 MVolt; //Main voltage (Batterie Vehicule)
                            result = Int16.TryParse(info.Length < 23 ? "0" : info[22], out MVolt);
                            if (!result)
                            {
                                MVolt = 0;
                            }

                            Int16 BVolt; //Backup voltage (Batterie Boitier)
                            result = Int16.TryParse(info.Length < 24 ? "0" : info[23], out BVolt);
                            if (!result)
                            {
                                BVolt = 0;
                            }

                            Int16 Ultrason; //Capteur (Ultrason)
                            result = Int16.TryParse(info.Length < 25 ? "0" : info[24], out Ultrason);
                            if (!result)
                            {
                                Ultrason = 0;
                            }

                            Int32 Odometre; //Odometre
                            result = Int32.TryParse(info[12], out Odometre);
                            if (!result)
                            {
                                Odometre = 0;
                            }


                            //options[64] = Convert.ToInt32(info.Length < 25 ? info[17] : info[24]); //valeur Carburant
                            //options[65] = Temperature;
                            //options[66] = moteur;
                            options[71] = MVolt;//Main voltage (Batterie Vehicule)
                            options[72] = BVolt;//Backup Voltage (Batterie Balise)
                            //options[73] = Ultrason;// Valeur Carburant Ultrason
                            options[74] = Odometre;// Odometre
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Conversion Invalid Capteur .", e.Message);
                            LogDBError("Conversion Invalid Capteur  : " + info + " \n Exception : " + e.Message, balise.Nisbalise);
                        }

                        if (LongitudeReel != 0 && LatitudeReel != 0)
                        {
                            if ((vitesseReel > 999) || (vitesseReel < 0))
                            {
                                LogDBError("Vitesse Hors limite", balise.Nisbalise);
                            }
                            else
                            {
                                if (Math.Abs(LongitudeReel) > 90 || Math.Abs(LatitudeReel) > 90)
                                {
                                    LogDBError("Position Hors limite,  : " + LongitudeReel + " et " + LatitudeReel, balise.Nisbalise);
                                }
                                else
                                {

                                    TrameReal tr = new TrameReal(balise, TempsReel, LatitudeReel, LongitudeReel, vitesseReel, Temperature, Capteur, directionReel, trailer, options, receptionDateTime);

                                    //if (info[4] == "226894")
                                    //{
                                    //if (trameReal == null)
                                    //{
                                    //    TrameReal tr2 = new TrameReal(balise, TempsReel, Convert.ToDecimal(36720508.0 / 1000000), Convert.ToDecimal(3195408.0 / 1000000), 0, Temperature, Capteur, directionReel, trailer, options, receptionDateTime);
                                    //    OLDModelGeneratorProcessor.addTrame(tr2);
                                    //    trameReal = tr2;
                                    //}
                                    //else
                                    //{
                                    //    if ((tr.Temps - trameReal.Temps).TotalMinutes >= 29)
                                    //    {
                                    //        trameReal.Temps = trameReal.Temps.AddMinutes(29);
                                    //        trameReal.Temps = trameReal.Temps.AddSeconds(37);
                                    //        trameReal.TempsReception = trameReal.Temps;
                                    //        OLDModelGeneratorProcessor.addTrame(trameReal);
                                    //    }
                                    //}
                                    //    TrameReal tr2 = new TrameReal(balise, TempsReel, Convert.ToDecimal(36720508.0 / 1000000), Convert.ToDecimal(3195408.0 / 1000000), 0, Temperature, Capteur, directionReel, trailer, options, receptionDateTime);
                                    //    if (trameReal == null || trameReal.Temps < tr2.Temps)
                                    //    {
                                    //        OLDModelGeneratorProcessor.addTrame(tr2);
                                    //        trameReal = tr2;
                                    //    }

                                    //}
                                    //else
                                    //{
                                    if (trameReal == null || trameReal.Temps < tr.Temps)
                                    {
                                        OLDModelGeneratorProcessor.addTrame(tr);
                                        trameReal = tr;
                                    }
                                    //}

                                }
                            }
                        }
                    }
                    catch (FormatException fe)
                    {
                        Console.WriteLine("Unable to convert '{0}' to a Decimal.", fe.Message);
                        LogDBError("Trame  Non Valide, Trame : " + info + " \n Exception : " + fe.Message, balise.Nisbalise);
                    }

                    catch (OverflowException ofe)
                    {
                        Console.WriteLine("'{0}' is outside the range of a Decimal.", ofe.Message);
                        LogDBError("Trame  Non Valide, Trame : " + info + "Exception : " + ofe.Message, balise.Nisbalise);
                    }
                }
            }
            catch (FormatException exp)
            {
                Console.WriteLine("Format de date invalide" + exp);
                PrincipalListner.Logging("FormatException", string.Format(balise.Nisbalise + " - Format de date invalide : ", exp.Message));
            }
            catch (Exception e)
            {
                Console.WriteLine("Format de date invalide" + e);
                LogDBError("Trame  Non Valide, Trame : " + info + "Exception : " + e.Message, balise.Nisbalise);
            }
        }

        private static void spliteTrameSondeTemperature(String[] info, Balise balise, ref TrameReal trameReal, DateTime receptionDateTime)
        {
            DateTime TempsReel = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            try
            {

                TempsReel = TempsReel.AddSeconds(Int64.Parse(info[5]));

                if ((TempsReel - DateTime.Now).TotalHours < 720)
                {

                    String Capteur = "000";
                    int Carburant = Convert.ToInt32(info[17]);   //Convert.ToInt32(info.Length < 25 ? info[17] : info[24]);      valeur en miliVolt position 24 dans la trame Ak1
                    Carburant = Carburant / 25;                     //echantillonage 
                    if (Carburant < 1000)
                    {
                        Capteur = convertAndformat(Carburant);      // convertir en chaine de caracter sur 3 positions
                    }
                    //Console.WriteLine("Capteur: " + Capteur);

                    int Moteur = Convert.ToInt16((info[14]));
                    //Console.WriteLine("Etat Moteur: " + Moteur);
                    if (Moteur % 2 == 1)
                        Capteur += "M";
                    else
                        Capteur += "A";

                    try
                    {
                        bool result;
                        Decimal LatitudeReel;
                        //Console.WriteLine(trame.Substring(9, 8));
                        result = Decimal.TryParse(info[9], out LatitudeReel);
                        if (!result) LatitudeReel = 0;
                        LatitudeReel /= 1000000;

                        Decimal LongitudeReel;
                        result = Decimal.TryParse(info[8], out LongitudeReel);
                        if (!result) LongitudeReel = 0;
                        LongitudeReel /= 1000000;

                        double vitesseReel;
                        result = double.TryParse(info[15].Replace('.', ','), out vitesseReel);
                        if (!result)
                        {
                            vitesseReel = 0;
                        }

                        Int16 Temperature;
                        result = Int16.TryParse(info[19], out Temperature);
                        if (!result) Temperature = 2000;

                        Int16 Temperature2;
                        result = Int16.TryParse(info[20], out Temperature2);
                        if (!result) Temperature2 = 2000;

                        Int16 directionReel;
                        Double direction;
                        result = Double.TryParse(info[10], out direction);
                        if (!result)
                        {
                            directionReel = 0;
                        }
                        else
                        {
                            if (direction > 360 || direction < 0)
                                directionReel = 0;
                            else
                                directionReel = (Int16)direction;
                        }

                        OptionsAux options = new OptionsAux();
                        try
                        {
                            Int16 moteur; result = Int16.TryParse(info[14], out moteur); if (!result) moteur = 0;
                            Int16 MVolt; result = Int16.TryParse(info.Length < 23 ? "0" : info[22], out MVolt); MVolt = 0; //Main voltage (Batterie Vehicule)
                            Int16 BVolt; result = Int16.TryParse(info.Length < 24 ? "0" : info[23], out BVolt); if (!result) BVolt = 0;//Backup voltage (Batterie Boitier)
                            Int32 Odometre; result = Int32.TryParse(info[12], out Odometre); if (!result) Odometre = 0;//Odometre

                            if (info.Length > 24)
                            {
                                Int16 moteur2;
                                result = Int16.TryParse(info[24], out moteur2);
                                if (result)
                                    options[166] = moteur2;
                            }


                            //options[64] = Convert.ToInt32(info.Length < 25 ? info[17] : info[24]); //valeur Carburant
                            options[65] = Temperature;
                            options[165] = Temperature2;
                            //options[66] = moteur;
                            options[71] = MVolt;//Main voltage (Batterie Vehicule)
                            options[72] = BVolt;//Backup Voltage (Batterie Balise)
                            options[74] = Odometre;// Odometre
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Conversion Invalid Capteur .", e.Message);
                            LogDBError("Conversion Invalid Capteur  : " + info + " \n Exception : " + e.Message, balise.Nisbalise);
                        }

                        if (LongitudeReel != 0 && LatitudeReel != 0)
                        {
                            if ((vitesseReel > 999) || (vitesseReel < 0))
                            {
                                LogDBError("Vitesse Hors limite", balise.Nisbalise);
                            }
                            else
                            {
                                if (Math.Abs(LongitudeReel) > 90 || Math.Abs(LatitudeReel) > 90)
                                {
                                    LogDBError("Position Hors limite,  : " + LongitudeReel + " et " + LatitudeReel, balise.Nisbalise);
                                }
                                else
                                {


                                    TrameReal tr = new TrameReal(balise, TempsReel, LatitudeReel, LongitudeReel, vitesseReel, (Int16)(Temperature / 10), Capteur, directionReel, options, receptionDateTime);

                                    if (trameReal == null || trameReal.Temps < tr.Temps)
                                    {
                                        OLDModelGeneratorProcessor.addTrame(tr, false);
                                        trameReal = tr;
                                    }

                                }
                            }
                        }
                    }
                    catch (FormatException fe)
                    {
                        Console.WriteLine("Unable to convert '{0}' to a Decimal.", fe.Message);
                        LogDBError("Trame  Non Valide, Trame : " + info + " \n Exception : " + fe.Message, balise.Nisbalise);
                    }

                    catch (OverflowException ofe)
                    {
                        Console.WriteLine("'{0}' is outside the range of a Decimal.", ofe.Message);
                        LogDBError("Trame  Non Valide, Trame : " + info + "Exception : " + ofe.Message, balise.Nisbalise);
                    }
                }
            }
            catch (FormatException exp)
            {
                Console.WriteLine("Format de date invalide" + exp);
                PrincipalListner.Logging("FormatException", string.Format(balise.Nisbalise + " - Format de date invalide : ", exp.Message));
            }
            catch (Exception e)
            {
                Console.WriteLine("Format de date invalide" + e);
                LogDBError("Trame  Non Valide, Trame : " + info + "Exception : " + e.Message, balise.Nisbalise);
            }
        }

        private static void spliteTrameSondeNiveauCarbrant(String[] info, Balise balise, ref TrameReal trameReal, DateTime receptionDateTime)
        {
            DateTime TempsReel = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            try
            {

                TempsReel = TempsReel.AddSeconds(Int64.Parse(info[5]));
                // Console.WriteLine("TempsReel: " + TempsReel);
                if ((TempsReel - DateTime.Now).TotalHours < 720)
                {
                    // Console.WriteLine("Date invalide: {0}", TempsReel);
                    String Capteur = "000";
                    int Carburant = Convert.ToInt32(info[17]);   //Convert.ToInt32(info.Length < 25 ? info[17] : info[24]);      valeur en miliVolt position 24 dans la trame Ak1
                    Carburant = Carburant / 25;                     //echantillonage 
                    if (Carburant < 1000)
                    {
                        Capteur = convertAndformat(Carburant);      // convertir en chaine de caracter sur 3 positions
                    }
                    //Console.WriteLine("Capteur: " + Capteur);

                    int Moteur = Convert.ToInt16((info[14]));
                    //Console.WriteLine("Etat Moteur: " + Moteur);
                    if (Moteur % 2 == 1)
                        Capteur = Capteur + "M";
                    else
                        Capteur = Capteur + "A";

                    try
                    {
                        bool result;
                        Decimal LatitudeReel;
                        //Console.WriteLine(trame.Substring(9, 8));
                        result = Decimal.TryParse(info[9], out LatitudeReel);
                        if (!result)
                        {
                            LatitudeReel = 0;
                        }
                        LatitudeReel = LatitudeReel / 1000000;
                        //Console.WriteLine("LatitudeReel: " + LatitudeReel);
                        Decimal LongitudeReel;
                        //Console.WriteLine(trame.Substring(17, 8));
                        result = Decimal.TryParse(info[8], out LongitudeReel);
                        if (!result)
                        {
                            LongitudeReel = 0;
                        }
                        LongitudeReel = LongitudeReel / 1000000;
                        double vitesseReel;
                        result = double.TryParse(info[15].Replace('.', ','), out vitesseReel);
                        if (!result)
                        {
                            vitesseReel = 0;
                        }
                        Int16 Temperature;
                        result = Int16.TryParse(info[19], out Temperature);
                        if (!result)
                        {
                            Temperature = 2000;
                        }


                        Int16 Temperature2;
                        result = Int16.TryParse(info[20], out Temperature2);
                        if (!result)
                        {
                            Temperature2 = 2000;
                        }



                        if (info[18] != null && info[18].Trim().Length >= 12)
                            OLDModelGeneratorProcessor.addTramesIdentDalas(new IdentDalas(balise, TempsReel, info[18].Trim()));

                        Int16 directionReel;
                        Double direction;
                        result = Double.TryParse(info[10], out direction);
                        if (!result)
                        {
                            directionReel = 0;
                        }
                        else
                        {
                            if (direction > 360 || direction < 0)
                                directionReel = 0;
                            else
                                directionReel = (Int16)direction;
                        }

                        OptionsAux options = new OptionsAux();
                        try
                        {
                            Int16 moteur;
                            result = Int16.TryParse(info[14], out moteur);
                            if (!result)
                            {
                                moteur = 0;
                            }

                            Int16 MVolt; //Main voltage (Batterie Vehicule)
                            result = Int16.TryParse(info.Length < 23 ? "0" : info[22], out MVolt);
                            if (!result)
                            {
                                MVolt = 0;
                            }

                            Int16 BVolt; //Backup voltage (Batterie Boitier)
                            result = Int16.TryParse(info.Length < 24 ? "0" : info[23], out BVolt);
                            if (!result)
                            {
                                BVolt = 0;
                            }

                            Int32 Odometre; //Odometre
                            result = Int32.TryParse(info[12], out Odometre);
                            if (!result)
                            {
                                Odometre = 0;
                            }

                            if (info.Length > 24)
                            {
                                Int16 moteur2;
                                result = Int16.TryParse(info[24], out moteur2);
                                if (result)
                                    options[166] = moteur2;

                            }


                            //options[64] = Convert.ToInt32(info.Length < 25 ? info[17] : info[24]); //valeur Carburant
                            options[65] = Temperature;
                            options[165] = Temperature2;
                            //options[66] = moteur;

                            options[71] = MVolt;//Main voltage (Batterie Vehicule)
                            options[72] = BVolt;//Backup Voltage (Batterie Balise)
                            //options[73] = Ultrason;// Valeur Carburant Ultrason
                            options[74] = Odometre;// Odometre
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Conversion Invalid Capteur .", e.Message);
                            LogDBError("Conversion Invalid Capteur  : " + info + " \n Exception : " + e.Message, balise.Nisbalise);
                        }

                        if (LongitudeReel != 0 && LatitudeReel != 0)
                        {
                            if ((vitesseReel > 999) || (vitesseReel < 0))
                            {
                                LogDBError("Vitesse Hors limite", balise.Nisbalise);
                            }
                            else
                            {
                                if (Math.Abs(LongitudeReel) > 90 || Math.Abs(LatitudeReel) > 90)
                                {
                                    LogDBError("Position Hors limite,  : " + LongitudeReel + " et " + LatitudeReel, balise.Nisbalise);
                                }
                                else
                                {

                                    TrameReal tr = new TrameReal(balise, TempsReel, LatitudeReel, LongitudeReel, vitesseReel, (Int16)(Temperature / 10), Capteur, directionReel, options, receptionDateTime);

                                    if (trameReal == null || trameReal.Temps < tr.Temps)
                                    {
                                        OLDModelGeneratorProcessor.addTrame(tr);
                                        trameReal = tr;

                                        if (info.Length >= 25)
                                        {

                                            TrameSonde tramesonde = new TrameSonde();
                                            bool hasSonde = false; // hasSonde == true ----> il y a a mois une informartion recu par un des sondes installé   

                                            string sondeFuelLevelInfo = info[24].Trim();
                                            if (sondeFuelLevelInfo != "")
                                            {
                                                try
                                                {
                                                    string sondeTemperatureFuel = sondeFuelLevelInfo.Substring(6, 2);
                                                    string sondeLevelFuel = sondeFuelLevelInfo.Substring(10, 2) + sondeFuelLevelInfo.Substring(8, 2);
                                                    tramesonde.Temperature1 = (Int16)(StringprefixedHexToInt(sondeTemperatureFuel) * 10);
                                                    tramesonde.Volume1 = (Int32)(StringprefixedHexToInt(sondeLevelFuel) * 10);

                                                    hasSonde = true;
                                                }
                                                catch (Exception e)
                                                {
                                                    Console.WriteLine("Sonde conversion erreur position info1: " + sondeFuelLevelInfo + ", " + e.Message);
                                                }
                                            }

                                            if (info.Length >= 26)
                                            {
                                                sondeFuelLevelInfo = info[25].Trim();
                                                if (sondeFuelLevelInfo != "")
                                                {
                                                    try
                                                    {
                                                        string sondeTemperatureFuel = sondeFuelLevelInfo.Substring(6, 2);
                                                        string sondeLevelFuel = sondeFuelLevelInfo.Substring(10, 2) + sondeFuelLevelInfo.Substring(8, 2);
                                                        tramesonde.Temperature2 = (Int16)(StringprefixedHexToInt(sondeTemperatureFuel) * 10);
                                                        tramesonde.Volume2 = (Int32)(StringprefixedHexToInt(sondeLevelFuel) * 10);

                                                        hasSonde = true;
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        Console.WriteLine("Sonde conversion erreur position info2: " + sondeFuelLevelInfo + ", " + e.Message);
                                                    }
                                                }
                                            }
                                            if (info.Length >= 27)
                                            {
                                                sondeFuelLevelInfo = info[26].Trim();
                                                if (sondeFuelLevelInfo != "")
                                                {
                                                    try
                                                    {
                                                        string sondeTemperatureFuel = sondeFuelLevelInfo.Substring(6, 2);
                                                        string sondeLevelFuel = sondeFuelLevelInfo.Substring(10, 2) + sondeFuelLevelInfo.Substring(8, 2);
                                                        tramesonde.Temperature3 = (Int16)(StringprefixedHexToInt(sondeTemperatureFuel) * 10);
                                                        tramesonde.Volume3 = (Int32)(StringprefixedHexToInt(sondeLevelFuel) * 10);

                                                        hasSonde = true;
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        Console.WriteLine("Sonde conversion erreur position info3: " + sondeFuelLevelInfo + ", " + e.Message);
                                                    }
                                                }
                                            }
                                            if (info.Length >= 28)
                                            {
                                                sondeFuelLevelInfo = info[27].Trim();
                                                if (sondeFuelLevelInfo != "")
                                                {
                                                    try
                                                    {
                                                        string sondeTemperatureFuel = sondeFuelLevelInfo.Substring(6, 2);
                                                        string sondeLevelFuel = sondeFuelLevelInfo.Substring(10, 2) + sondeFuelLevelInfo.Substring(8, 2);
                                                        tramesonde.Temperature4 = (Int16)(StringprefixedHexToInt(sondeTemperatureFuel) * 10);
                                                        tramesonde.Volume4 = (Int32)(StringprefixedHexToInt(sondeLevelFuel) * 10);

                                                        hasSonde = true;
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        Console.WriteLine("Sonde conversion erreur position info4: " + sondeFuelLevelInfo + ", " + e.Message);
                                                    }
                                                }
                                            }


                                            if (hasSonde)
                                            {
                                                int m = 0;
                                                m = Convert.ToInt16(Moteur);
                                                tramesonde.Temps = tr.Temps.AddHours(1);
                                                tramesonde.NisBalise = balise.Nisbalise;
                                                tramesonde.Witesse = (Int16)Math.Truncate(tr.Vitesse * 10);// witesse = la pertie entier de vitesse * 10 
                                                tramesonde.Moteur = (byte)(m % 2 == 1 ? 1 : 0);
                                                tramesonde.Latitude = tr.Latitude;
                                                tramesonde.Longitude = tr.Longitude;
                                                OLDModelGeneratorProcessor.addSondeTrame(tramesonde);
                                            }

                                        }
                                    }

                                }
                            }
                        }
                    }
                    catch (FormatException fe)
                    {
                        Console.WriteLine("Unable to convert '{0}' to a Decimal.", fe.Message);
                        LogDBError("Trame  Non Valide, Trame : " + info + " \n Exception : " + fe.Message, balise.Nisbalise);
                    }

                    catch (OverflowException ofe)
                    {
                        Console.WriteLine("'{0}' is outside the range of a Decimal.", ofe.Message);
                        LogDBError("Trame  Non Valide, Trame : " + info + "Exception : " + ofe.Message, balise.Nisbalise);
                    }
                }
            }
            catch (FormatException exp)
            {
                Console.WriteLine("Format de date invalide" + exp);
                PrincipalListner.Logging("FormatException", string.Format(balise.Nisbalise + " - Format de date invalide : ", exp.Message));
            }
            catch (Exception e)
            {
                Console.WriteLine("Format de date invalide" + e);
                LogDBError("Trame  Non Valide, Trame : " + info + "Exception : " + e.Message, balise.Nisbalise);
            }
        }

        private static void spliteTrameEchoATrack(String[] info, Balise balise, ref TrameReal trameReal, DateTime receptionDateTime)
        {
            DateTime TempsReel = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            try
            {

                TempsReel = TempsReel.AddSeconds(Int64.Parse(info[5]));
                // Console.WriteLine("TempsReel: " + TempsReel);
                if ((TempsReel - DateTime.Now).TotalHours < 720)
                {
                    // Console.WriteLine("Date invalide: {0}", TempsReel);
                    String Capteur = "000";
                    int Carburant = Convert.ToInt32(info[17]); //Convert.ToInt32(info.Length < 25 ? info[17] : info[24]);      valeur en miliVolt position 24 dans la trame Ak1
                    Carburant = Carburant / 25;                     //echantillonage 
                    if (Carburant < 1000)
                    {
                        Capteur = convertAndformat(Carburant);      // convertir en chaine de caracter sur 3 positions
                    }
                    //Console.WriteLine("Capteur: " + Capteur);

                    int Moteur = Convert.ToInt16((info[14]));
                    //Console.WriteLine("Etat Moteur: " + Moteur);
                    if (Moteur % 2 == 1)
                        Capteur = Capteur + "M";
                    else
                        Capteur = Capteur + "A";

                    try
                    {
                        bool result;
                        Decimal LatitudeReel;
                        //Console.WriteLine(trame.Substring(9, 8));
                        result = Decimal.TryParse(info[9], out LatitudeReel);
                        if (!result)
                        {
                            LatitudeReel = 0;
                        }
                        LatitudeReel = LatitudeReel / 1000000;
                        //Console.WriteLine("LatitudeReel: " + LatitudeReel);
                        Decimal LongitudeReel;
                        //Console.WriteLine(trame.Substring(17, 8));
                        result = Decimal.TryParse(info[8], out LongitudeReel);
                        if (!result)
                        {
                            LongitudeReel = 0;
                        }
                        LongitudeReel = LongitudeReel / 1000000;
                        double vitesseReel;
                        result = double.TryParse(info[15].Replace('.', ','), out vitesseReel);
                        if (!result)
                        {
                            //Console.WriteLine("Erreur vitesseReel: {0}", vitesseReel);
                            vitesseReel = 0;
                        }
                        Int16 Temperature;
                        result = Int16.TryParse(info[19], out Temperature);
                        if (!result)
                        {
                            //Console.WriteLine("Erreur Temperature: {0} ", Temperature);
                            Temperature = 2000;
                        }


                        if (info[18] != null && info[18].Trim().Length >= 12)
                            OLDModelGeneratorProcessor.addTramesIdentDalas(new IdentDalas(balise, TempsReel, info[18].Trim()));

                        Int16 directionReel;
                        Double direction;
                        result = Double.TryParse(info[10], out direction);
                        if (!result)
                        {
                            //Console.WriteLine("Erreur directionReel: {0} ", directionReel);
                            directionReel = 0;
                        }
                        else
                        {
                            if (direction > 360 || direction < 0)
                                directionReel = 0;
                            else
                                directionReel = (Int16)direction;
                        }

                        int index = 0;
                        if (balise.EcoConduite)
                        {
                            index = 1; // 
                        }

                        OptionsAux options = new OptionsAux();
                        try
                        {
                            Int16 moteur;
                            result = Int16.TryParse(info[14], out moteur);
                            if (!result)
                            {
                                moteur = 0;
                            }

                            Int16 MVolt; //Main voltage (Batterie Vehicule)
                            result = Int16.TryParse(info.Length < 23 ? "0" : info[22], out MVolt);
                            if (!result)
                            {
                                MVolt = 0;
                            }

                            Int16 BVolt; //Backup voltage (Batterie Boitier)
                            result = Int16.TryParse(info.Length < 24 ? "0" : info[23], out BVolt);
                            if (!result)
                            {
                                BVolt = 0;
                            }


                            Int32 TempMoteur;
                            result = Int32.TryParse(info.Length < 26 + index ? "0" : info[25 + index], out TempMoteur);
                            if (!result)
                            {

                                TempMoteur = 0;
                            }

                            Int32 FuelUsed;
                            result = Int32.TryParse(info.Length < 27 + index ? "0" : info[26 + index], out FuelUsed);

                            if (!result)
                            {

                                FuelUsed = 0;
                            }
                            else
                                FuelUsed = (Int32)(FuelUsed / 10);


                            double FuelLevel; //Niveau Carburant %
                            result = double.TryParse(info.Length < 28 + index ? "0" : info[27 + index], out FuelLevel);
                            if (!result)
                            {
                                FuelLevel = 0;
                            }

                            Int32 Rpm;
                            result = Int32.TryParse(info.Length < 31 + index ? "0" : info[30 + index], out Rpm);
                            if (!result)
                            {

                                Rpm = 0;
                            }


                            options[71] = MVolt;//Main voltage (Batterie Vehicule)
                            options[72] = BVolt;//Backup Voltage (Batterie Balise)
                            try
                            {
                                options[75] = Convert.ToInt16(FuelLevel); // Valeur Niveau Carburant
                            }
                            catch (Exception e)
                            {
                                options[75] = 0;
                                Console.WriteLine("Valeur Carburant exception: {0}", e.Message);
                            }

                            try
                            {
                                options[77] = Convert.ToInt32(FuelUsed); // Valeur Fuel Used (L)
                            }
                            catch (Exception e)
                            {
                                options[77] = 0;
                                Console.WriteLine("Valeur Fuel Used exception: {0}", e.Message);
                            }

                            try
                            {
                                options[87] = Convert.ToInt32(Rpm); // Valeur RPM (Tours minute)
                            }
                            catch (Exception e)
                            {
                                options[87] = 0;
                                Console.WriteLine("Valeur RPM exception: {0}", e.Message);
                            }

                            try
                            {
                                if (TempMoteur > 0)
                                    options[79] = TempMoteur; // Valeur Temperature Moteur
                            }
                            catch (Exception e)
                            {
                                //options[79] = 0;
                                Console.WriteLine("Valeur Temperature Moteur exception: {0}", e.Message);
                            }

                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Conversion Invalid Capteur .", e.Message);
                            LogDBError("Conversion Invalid Capteur  : " + info + " \n Exception : " + e.Message, balise.Nisbalise);
                        }

                        if (LongitudeReel != 0 && LatitudeReel != 0)
                        {
                            if (vitesseReel > 999 || vitesseReel < 0)
                            {
                                LogDBError("Vitesse Hors limite", balise.Nisbalise);
                            }
                            else
                            {
                                if (Math.Abs(LongitudeReel) > 90 || Math.Abs(LatitudeReel) > 90)
                                {
                                    LogDBError("Position Hors limite,  : " + LongitudeReel + " et " + LatitudeReel, balise.Nisbalise);
                                }
                                else
                                {
                                    TrameReal tr = new TrameReal(balise, TempsReel, LatitudeReel, LongitudeReel, vitesseReel, (Int16)(Temperature / 10), Capteur, directionReel, options, receptionDateTime);

                                    if (trameReal == null || trameReal.Temps < tr.Temps)
                                    {
                                        if (!balise.EcoConduite)
                                        {
                                            OLDModelGeneratorProcessor.addTrame(tr);
                                            trameReal = tr;
                                        }
                                        else
                                        {
                                            OLDModelGeneratorProcessor.addTrameMidNightSplitTrajet(tr);
                                            trameReal = tr;
                                            string G_Force, Hix;
                                            Int16? Gx = null;
                                            Int16? Gy = null;
                                            Int16? Gz = null;
                                            Int16? CCarb = null;
                                            try
                                            {
                                                G_Force = (info.Length < 25 ? "" : String.IsNullOrWhiteSpace(info[24]) ? "" : info[24].Trim());

                                                if (G_Force != "")
                                                {
                                                    Hix = G_Force.Substring(0, 4);
                                                    Gx = Convert.ToInt16("0x" + Hix, 16);

                                                    Hix = G_Force.Substring(4, 4);
                                                    Gy = Convert.ToInt16("0x" + Hix, 16);

                                                    Hix = G_Force.Substring(8, 4);
                                                    Gz = Convert.ToInt16("0x" + Hix, 16);
                                                }


                                                int m = 0;
                                                m = Convert.ToInt16(Moteur);

                                                Int16 rpm = -1;

                                                string rpm_s = (info.Length < 32 ? "" : String.IsNullOrWhiteSpace(info[31]) ? "" : info[31].Trim());
                                                if (rpm_s != "")
                                                {
                                                    rpm = Convert.ToInt16(rpm_s);
                                                }

                                                TrameEco echo = new TrameEco(tr.Temps.AddHours(1), tr.Direction, (int)Math.Truncate(tr.Vitesse), balise.Nisbalise, (byte)(m % 2 == 1 ? 1 : 0), rpm, Temperature, Gx, Gy, Gz, CCarb);
                                                OLDModelGeneratorProcessor.addEcoTrame(echo);

                                                //if (info[11] == "114")
                                                //{
                                                //    Connection.Logging("Début évènement Freinage brusque", balise.Nisbalise);
                                                //    Console.WriteLine("Début évènement Freinage brusque: " + balise.Nisbalise);
                                                //}

                                                //if (info[11] == "115")
                                                //{
                                                //    Connection.Logging("Fin évènement Freinage brusque", balise.Nisbalise);
                                                //    Console.WriteLine("Fin évènement Freinage brusque: " + balise.Nisbalise);
                                                //}

                                                //if (info[11] == "116")
                                                //{
                                                //    Connection.Logging("Début évènement Accélération  brusque", balise.Nisbalise);
                                                //    Console.WriteLine("Début évènement Accélération  brusque: " + balise.Nisbalise);
                                                //}

                                                //if (info[11] == "117")
                                                //{
                                                //    Connection.Logging("Fin évènement Accélération  brusque", balise.Nisbalise);
                                                //    Console.WriteLine("Fin évènement Accélération  brusque: " + balise.Nisbalise);
                                                //}

                                                //if (info[11] == "118")
                                                //{
                                                //    Connection.Logging("Début évènement Virage  brusque", balise.Nisbalise);
                                                //    Console.WriteLine("Début évènement Virage  brusque: " + balise.Nisbalise);
                                                //}

                                                //if (info[11] == "119")
                                                //{
                                                //    Connection.Logging("Fin évènement Virage  brusque", balise.Nisbalise);
                                                //    Console.WriteLine("Fin évènement Virage  brusque: " + balise.Nisbalise);
                                                //}

                                            }
                                            catch (Exception ex)
                                            {
                                                Console.WriteLine("Echo conduit G_Force Invalide Values: '{0}' to a Decimal.", ex.Message);
                                                LogDBError("Echo confuit G_Force Invalide Values" + info + " \n Exception : " + ex.Message, balise.Nisbalise);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (FormatException fe)
                    {
                        Console.WriteLine("Unable to convert '{0}' to a Decimal.", fe.Message);
                        LogDBError("Trame  Non Valide, Trame : " + info + " \n Exception : " + fe.Message, balise.Nisbalise);
                    }

                    catch (OverflowException ofe)
                    {
                        Console.WriteLine("'{0}' is outside the range of a Decimal.", ofe.Message);
                        LogDBError("Trame  Non Valide, Trame : " + info + "Exception : " + ofe.Message, balise.Nisbalise);
                    }
                }
            }
            catch (FormatException exp)
            {
                Console.WriteLine("Format de date invalide" + exp);
                PrincipalListner.Logging("FormatException", string.Format(balise.Nisbalise + " - Format de date invalide : ", exp.Message));
            }
            catch (Exception e)
            {
                Console.WriteLine("Format de date invalide" + e);
                LogDBError("Trame  Non Valide, Trame : " + info + "Exception : " + e.Message, balise.Nisbalise);
            }
        }

        private static void spliteTrameATrackForSarens(String[] info, Balise balise, ref TrameReal trameReal, DateTime receptionDateTime)
        {
            DateTime TempsReel = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            try
            {
                TempsReel = TempsReel.AddSeconds(Int64.Parse(info[5]));
                // Console.WriteLine("TempsReel: " + TempsReel);
                if ((TempsReel - DateTime.Now).TotalHours < 720)
                {
                    // Console.WriteLine("Date invalide: {0}", TempsReel);
                    String Capteur = "000"; //info[24];
                                            //Console.WriteLine("Capteur: " + Capteur);

                    int Moteur = Convert.ToInt16((info[14]));
                    //Console.WriteLine("Etat Moteur: " + Moteur);
                    if (Moteur % 2 == 1)
                        Capteur = Capteur + "M";
                    else
                        Capteur = Capteur + "A";

                    try
                    {
                        bool result;
                        Decimal LatitudeReel;
                        //Console.WriteLine(trame.Substring(9, 8));
                        result = Decimal.TryParse(info[9], out LatitudeReel);
                        if (!result)
                        {
                            LatitudeReel = 0;
                        }
                        LatitudeReel = LatitudeReel / 1000000;
                        //Console.WriteLine("LatitudeReel: " + LatitudeReel);
                        Decimal LongitudeReel;
                        //Console.WriteLine(trame.Substring(17, 8));
                        result = Decimal.TryParse(info[8], out LongitudeReel);
                        if (!result)
                        {
                            LongitudeReel = 0;
                        }
                        LongitudeReel = LongitudeReel / 1000000;
                        double vitesseReel;
                        result = double.TryParse(info[15].Replace('.', ','), out vitesseReel);
                        if (!result)
                        {
                            //Console.WriteLine("Erreur vitesseReel: {0}", vitesseReel);
                            vitesseReel = 0;
                        }
                        Int16 Temperature;
                        result = Int16.TryParse(info[19], out Temperature);
                        if (!result)
                        {
                            //Console.WriteLine("Erreur Temperature: {0} ", Temperature);
                            Temperature = 0;
                        }
                        else
                            Temperature = (Int16)(Temperature / 10);

                        if (info[18] != null && info[18].Trim().Length >= 12)
                            OLDModelGeneratorProcessor.addTramesIdentDalas(new IdentDalas(balise, TempsReel, info[18].Trim()));

                        Int16 directionReel;
                        Double direction;
                        result = Double.TryParse(info[10], out direction);
                        if (!result)
                        {
                            //Console.WriteLine("Erreur directionReel: {0} ", directionReel);
                            directionReel = 0;
                        }
                        else
                        {
                            if (direction > 360 || direction < 0)
                                directionReel = 0;
                            else
                                directionReel = (Int16)direction;
                        }
                        OptionsAux options = new OptionsAux();
                        int entree = 80;
                        int valueOption;
                        for (int i = 24; i < info.Length; i++)
                        {
                            result = Int32.TryParse(info[i], out valueOption);
                            if (!result)
                            {
                                //Console.WriteLine("Erreur directionReel: {0} ", directionReel);
                                entree++;
                            }
                            else
                            {
                                options[entree++] = valueOption;
                            }
                        }
                        if (LongitudeReel != 0 && LatitudeReel != 0)
                        {
                            if ((vitesseReel > 999) || (vitesseReel < 0))
                            {
                                LogDBError("Vitesse Hors limite", balise.Nisbalise);
                            }
                            else
                            {
                                if (Math.Abs(LongitudeReel) > 90 || Math.Abs(LatitudeReel) > 90)
                                {
                                    LogDBError("Position Hors limite,  : " + LongitudeReel + " et " + LatitudeReel, balise.Nisbalise);
                                }
                                else
                                {
                                    TrameReal tr = new TrameReal(balise, TempsReel, LatitudeReel, LongitudeReel, vitesseReel, Temperature, Capteur, directionReel, options, receptionDateTime);

                                    if (trameReal == null || trameReal.Temps < tr.Temps)
                                    {
                                        OLDModelGeneratorProcessor.addTrame(tr);
                                        trameReal = tr;
                                    }
                                    OLDModelGeneratorProcessor.addSarensTrame(tr);
                                    //TrameConsumer.addTrame(tr);
                                }
                            }
                        }
                    }
                    catch (FormatException fe)
                    {
                        Console.WriteLine("Unable to convert '{0}' to a Decimal.", fe.Message);
                        LogDBError("Trame  Non Valide, Trame : " + info + " \n Exception : " + fe.Message, balise.Nisbalise);
                    }
                    catch (OverflowException ofe)
                    {
                        Console.WriteLine("'{0}' is outside the range of a Decimal.", ofe.Message);
                        LogDBError("Trame  Non Valide, Trame : " + info + "Exception : " + ofe.Message, balise.Nisbalise);
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Format de date invalide" + e.Message);
                LogDBError("Trame  Non Valide, Trame : " + info + "Exception : " + e.Message, balise.Nisbalise);
            }


        }

        private static void spliteTrameATrackAX9(String[] info, Balise balise, ref TrameReal trameReal, DateTime receptionDateTime)
        {
            DateTime TempsReel = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            try
            {

                TempsReel = TempsReel.AddSeconds(Int64.Parse(info[5]));
                // Console.WriteLine("TempsReel: " + TempsReel);
                if ((TempsReel - DateTime.Now).TotalHours < 720)
                {
                    // Console.WriteLine("Date invalide: {0}", TempsReel);
                    String Capteur = "000";
                    int Carburant = Convert.ToInt32(info[17]);      //valeur en miliVolt position 17 dans la trame Ak1
                    Carburant = Carburant / 25;                     //echantillonage 
                    if (Carburant < 1000)
                    {
                        Capteur = convertAndformat(Carburant);      // convertir en chaine de caractere sur 3 positions
                    }
                    //Console.WriteLine("Capteur: " + Capteur);

                    int Moteur = Convert.ToInt16((info[14]));
                    //Console.WriteLine("Etat Moteur: " + Moteur);
                    if (Moteur % 2 == 1)
                        Capteur = Capteur + "M";
                    else
                        Capteur = Capteur + "A";

                    try
                    {
                        bool result;
                        Decimal LatitudeReel;
                        //Console.WriteLine(trame.Substring(9, 8));
                        result = Decimal.TryParse(info[9], out LatitudeReel);
                        if (!result)
                        {
                            LatitudeReel = 0;
                        }
                        LatitudeReel = LatitudeReel / 1000000;
                        //Console.WriteLine("LatitudeReel: " + LatitudeReel);
                        Decimal LongitudeReel;
                        //Console.WriteLine(trame.Substring(17, 8));
                        result = Decimal.TryParse(info[8], out LongitudeReel);
                        if (!result)
                        {

                            LongitudeReel = 0;

                        }
                        LongitudeReel = LongitudeReel / 1000000;
                        double vitesseReel;
                        result = double.TryParse(info[15].Replace('.', ','), out vitesseReel);
                        if (!result)
                        {
                            //Console.WriteLine("Erreur vitesseReel: {0}", vitesseReel);
                            vitesseReel = 0;
                        }

                        if (info[18] != null && info[18].Trim().Length >= 12)
                            OLDModelGeneratorProcessor.addTramesIdentDalas(new IdentDalas(balise, TempsReel, info[18].Trim()));

                        Int16 directionReel;
                        Double direction;
                        result = Double.TryParse(info[10], out direction);
                        if (!result)
                        {
                            //Console.WriteLine("Erreur directionReel: {0} ", directionReel);
                            directionReel = 0;
                        }
                        else
                        {
                            if (direction > 360 || direction < 0)
                                directionReel = 0;
                            else
                                directionReel = (Int16)direction;
                        }


                        Int16 Temperature;
                        result = Int16.TryParse(info[19], out Temperature);
                        if (!result)
                        {
                            //Console.WriteLine("Erreur Temperature: {0} ", Temperature);
                            Temperature = 0;
                        }
                        else
                            Temperature = (Int16)(Temperature / 10);

                        OptionsAux options = new OptionsAux();
                        try
                        {
                            Int16 moteur;
                            result = Int16.TryParse(info[14], out moteur);
                            if (!result)
                            {
                                moteur = 0;
                            }

                            Int16 MVolt; //Main voltage (Batterie Vehicule)
                            result = Int16.TryParse(info.Length < 23 ? "0" : info[22], out MVolt);
                            if (!result)
                            {
                                MVolt = 0;
                            }

                            Int16 BVolt; //Backup voltage (Batterie Boitier)
                            result = Int16.TryParse(info.Length < 24 ? "0" : info[23], out BVolt);
                            if (!result)
                            {
                                BVolt = 0;
                            }



                            Int32 TempMoteur;
                            result = Int32.TryParse(info.Length < 26 ? "0" : info[25], out TempMoteur);
                            if (!result)
                            {
                                //Console.WriteLine("Erreur Temperature: {0} ", Temperature);
                                TempMoteur = 0;
                            }

                            Int32 FuelUsed;
                            result = Int32.TryParse(info.Length < 27 ? "0" : info[26], out FuelUsed);

                            if (!result)
                            {
                                //Console.WriteLine("Erreur Temperature: {0} ", Temperature);
                                FuelUsed = 0;
                            }
                            else
                                FuelUsed = (Int32)(FuelUsed / 10);


                            double FuelLevel; //Niveau Carburant %
                            result = double.TryParse(info.Length < 28 ? "0" : info[27], out FuelLevel);
                            if (!result)
                            {
                                FuelLevel = 0;
                            }

                            Int32 Rpm;
                            result = Int32.TryParse(info.Length < 31 ? "0" : info[30], out Rpm);
                            if (!result)
                            {
                                //Console.WriteLine("Erreur Temperature: {0} ", Temperature);
                                Rpm = 0;
                            }


                            options[71] = MVolt;//Main voltage (Batterie Vehicule)
                            options[72] = BVolt;//Backup Voltage (Batterie Balise)
                            try
                            {
                                options[75] = Convert.ToInt16(FuelLevel); // Valeur Niveau Carburant
                            }
                            catch (Exception e)
                            {
                                options[75] = 0;
                                Console.WriteLine("Valeur Carburant exception: {0}", e.Message);
                            }

                            try
                            {
                                options[77] = Convert.ToInt32(FuelUsed); // Valeur Fuel Used (L)
                            }
                            catch (Exception e)
                            {
                                options[77] = 0;
                                Console.WriteLine("Valeur Fuel Used exception: {0}", e.Message);
                            }

                            try
                            {
                                options[87] = Convert.ToInt32(Rpm); // Valeur RPM (Tours minute)
                            }
                            catch (Exception e)
                            {
                                options[87] = 0;
                                Console.WriteLine("Valeur RPM exception: {0}", e.Message);
                            }

                            try
                            {
                                if (TempMoteur > 0)
                                    options[79] = TempMoteur; // Valeur Temperature Moteur
                            }
                            catch (Exception e)
                            {
                                //options[79] = 0;
                                Console.WriteLine("Valeur Temperature Moteur exception: {0}", e.Message);
                            }

                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Conversion Invalid Capteur .", e.Message);
                            LogDBError("Conversion Invalid Capteur  : " + info + " \n Exception : " + e.Message, balise.Nisbalise);
                        }

                        if (LongitudeReel != 0 && LatitudeReel != 0)
                        {
                            if ((vitesseReel > 999) || (vitesseReel < 0))
                            {
                                LogDBError("Vitesse Hors limite", balise.Nisbalise);
                            }
                            else
                            {
                                if (Math.Abs(LongitudeReel) > 90 || Math.Abs(LatitudeReel) > 90)
                                {
                                    LogDBError("Position Hors limite,  : " + LongitudeReel + " et " + LatitudeReel, balise.Nisbalise);
                                }
                                else
                                {
                                    TrameReal tr = new TrameReal(balise, TempsReel, LatitudeReel, LongitudeReel, vitesseReel, Temperature, Capteur, directionReel, options, receptionDateTime);
                                    //0, 0, "000A", 0
                                    if (trameReal == null || trameReal.Temps < tr.Temps)
                                    {
                                        OLDModelGeneratorProcessor.addTrame(tr);
                                        trameReal = tr;
                                    }
                                }
                            }
                        }
                    }
                    catch (FormatException fe)
                    {
                        Console.WriteLine("Unable to convert '{0}' to a Decimal.", fe.Message);
                        LogDBError("Trame  Non Valide, Trame : " + info + " \n Exception : " + fe.Message, balise.Nisbalise);
                    }

                    catch (OverflowException ofe)
                    {
                        Console.WriteLine("'{0}' is outside the range of a Decimal.", ofe.Message);
                        LogDBError("Trame  Non Valide, Trame : " + info + "Exception : " + ofe.Message, balise.Nisbalise);
                    }

                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Format de date invalide" + e);
                LogDBError("Trame  Non Valide, Trame : " + info + "Exception : " + e.Message, balise.Nisbalise);
            }


        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private static void LogDBError(String message, string balise)
        {
            try
            {
                String path = "Log\\" + DateTime.Now.ToString("yyyy-MM-dd") + "\\TrameParser";

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                using (StreamWriter sw = File.AppendText(path + "\\traitementTrameErreur_" + balise + ".log"))
                {
                    sw.WriteLine(DateTime.Now + ": " + message);
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("impossible de logger l'erreur {0}", e.Message);

            }
        }

        private static String convertAndformat(int value)
        {
            String Carburant = "";
            if (value < 10)
            {
                Carburant = "00" + Convert.ToString(value);
            }
            else if (value < 100)
            {
                Carburant = "0" + Convert.ToString(value);
            }
            else
            {
                Carburant = Convert.ToString(value);
            }

            return Carburant;

        }

        #region "Teltonika"
        private static int StringHexToInt(string StringHex)
        {
            return int.Parse(StringHex, System.Globalization.NumberStyles.HexNumber);
        }
        private static Regex regTeltonikaPacKageFMB9XX = new Regex(@"00000000(.{8})08(.{2})(.*)(.{2})(.{8})", RegexOptions.Compiled);

        static string[] TeltonikaSplit(string str, int chunkSize)
        {
            return Enumerable.Range(0, str.Length / chunkSize)
                .Select(i => str.Substring(i * chunkSize, chunkSize)).ToArray();
        }
        private static int StringprefixedHexToInt(string prefixedHex)
        {
            return Convert.ToInt32("0x" + prefixedHex, 16);
        }
        private static long StringHexTolong(string StringHex)
        {
            return long.Parse(StringHex, System.Globalization.NumberStyles.HexNumber);
        }
        private static void spliteTrameTeltonikaFMB9XX(string tramestr, Balise balise, ref TrameReal trameReal, DateTime receptionDateTime)
        {

            try
            {

                DateTime AvlEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);


                DateTime Temps = AvlEpoch.AddMilliseconds(StringHexTolong(tramestr.Substring(0, 16))).ToLocalTime();
                //DateTime AvlEpoch2 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                //Console.WriteLine(AvlEpoch.ToString() + "\t" + AvlEpoch.AddMilliseconds(StringHexTolong(tramestr.Substring(0, 16))).ToString()
                //    + "\t" + AvlEpoch.AddMilliseconds(StringHexTolong(tramestr.Substring(0, 16))).ToLocalTime().ToString());

                //Console.WriteLine(AvlEpoch2.ToString() + "\t" + AvlEpoch2.AddMilliseconds(StringHexTolong(tramestr.Substring(0, 16))).ToString()
                //   + "\t" + AvlEpoch2.AddMilliseconds(StringHexTolong(tramestr.Substring(0, 16))).ToLocalTime().ToString());

                //Console.WriteLine(AvlEpoch2.AddMilliseconds(StringHexTolong(tramestr.Substring(0, 16))).AddHours(1));

                //Temps = AvlEpoch2.AddMilliseconds(StringHexTolong(tramestr.Substring(0, 16))).AddHours(1);

                decimal Longitude = ((decimal)StringprefixedHexToInt(tramestr.Substring(18, 8)) / 10000000);
                //Console.WriteLine(Longitude);

                decimal Latitude = ((decimal)StringprefixedHexToInt(tramestr.Substring(26, 8)) / 10000000);
                //Console.WriteLine(Latitude);

                short Direction = (short)StringHexToInt(tramestr.Substring(38, 4));

                int Vitesse = StringHexToInt(tramestr.Substring(44, 4));

                int Elemnt1bCount = StringHexToInt(tramestr.Substring(52, 2));
                string ignition = string.Empty;

                for (int i = 0; i < Elemnt1bCount; i++)
                {
                    //Console.WriteLine(string.Format("ID IO:{0}, Value IO:{1}", tramestr.Substring(54 + i * 4, 2), tramestr.Substring(54 + i * 4 + 2, 2)));
                    if (tramestr.Substring(54 + i * 4, 2) == "EF")
                    {
                        ignition = tramestr.Substring(54 + i * 4 + 2, 2);
                        break;
                    }
                }

                string Capteur = string.Empty;
                if (!string.IsNullOrWhiteSpace(ignition))
                    Capteur = "000" + (ignition == "01" ? "M" : "A");

                //Console.WriteLine(string.Format("Temps:{0}, Longitude:{1}, Latitude:{2}, Direction:{3}, Vitesse:{4}, ACC:{5}"
                //        , Temps, Longitude, Latitude, Direction, Vitesse, Capteur));

                TrameReal tr = new TrameReal(balise, Temps, Latitude, Longitude, Vitesse, 0, Capteur, Direction, null, receptionDateTime);

                if (trameReal == null || trameReal.Temps < tr.Temps)
                {
                    OLDModelGeneratorProcessor.addTrame(tr);
                    trameReal = tr;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Format trame Teltonika invalide" + e);
                LogDBError("Trame  Non Valide, Trame : " + tramestr + "Exception : " + e.Message, balise.Nisbalise);
            }
        }
        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }
        #endregion

    }
}
