using BaliseListner.CollecteurException;
using BaliseListner.DataAccess;
using BaliseListner.Generator;
using Collecteur.Core.Api;
using OldCollecteur;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BaliseListner.ThreadListener
{
    class Connection
    {
        private readonly static string OK = "!\r\n";
        private readonly static string NO = "#\r\n";
        private readonly static Object lockObject = new Object();
        private Socket Socket;
        private TcpClient tcpClient;

        public byte[] BufferIn;
        public byte[] BufferOut;
        public Balise boitier;
        private int dataSize = 0;
        public int matricule;
        private DateTime LastTimeReception;
        private bool isModeDebug;
        private TrameReal trameReal = null;
        private bool ConexionClosed = false;
        private string RemoteHote = "non identifie";
        //proprite pour old Collecteur
        public string currentDate;



        /// <Code pour tester crystage de données: boitier atrack>
        ///  PassWord = mohamedmohame
        ///  private byte[] ATPassWord = { 0x6d, 0x6f, 0x68, 0x61, 0x6d, 0x65, 0x64, 0x6d, 0x6f, 0x68, 0x61, 0x6d, 0x65, 0x64, 0x00, 0x00 };
        /// </Code pour tester crystage de données: boitier atrack>


        // <Code pour tester crystage de données: boitier atrack>
        //  PassWord = mohamedmohame

        // </Code pour tester crystage de données: boitier atrack>


        public Connection(TcpClient tcpClient, bool modeDebug)
        {
            this.tcpClient = tcpClient;
            this.Socket = tcpClient.Client;
            Socket.Blocking = false;
            BufferIn = new byte[3096];
            BufferOut = new byte[8];
            isModeDebug = modeDebug;
            RemoteHote = Socket.RemoteEndPoint.ToString();

            //BufferOut = System.Text.Encoding.ASCII.GetBytes("Salut ");
            //boitier = null;
            //matricule = 0;
        }

        public void StartCommunication()
        {
            //Logging("reception des données avec la taille de buffer: " + tcpClient.ReceiveBufferSize);
            //Logging("reception des données timeout : " + tcpClient.ReceiveTimeout);
            //Logging("reception des données avec la taille recue  : " + tcpClient.Available);
            BufferIn = new byte[tcpClient.ReceiveBufferSize];
            this.Socket.BeginReceive(BufferIn, 0, BufferIn.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), this);
            //  this.Socket.BeginSend(BufferOut, 1024, dataSize, SocketFlags.None, new AsyncCallback(SendCallback), this);

        }

        private void ReceiveCallback(IAsyncResult result)
        {
            if (!PrincipalListner.baseActive)
                WaitHandle.SignalAndWait(PrincipalListner.SyncBase.BaseEchecThreadEvent, PrincipalListner.SyncBase.NewItemEvent);
            DateTime receptionTime = DateTime.Now;
            String textt = "";
            try
            {
                int bytesRead = Socket.EndReceive(result);

                if (0 != bytesRead)
                {
                    byte[] BufferConnect = (byte[])BufferIn.Clone();
                    textt = Encoding.ASCII.GetString(BufferConnect, 0, bytesRead);

                    if (BufferConnect[0].Equals(0xFE) && BufferConnect[1].Equals(0x02))
                    {
                        if (boitier == null)
                        {

                            LoggingCommunication("Connection", $"TC\t{RemoteHote}\t{BitConverter.ToString(BufferConnect.ToList().GetRange(0, bytesRead).ToArray()).Replace("-", "")}"); //Try to connected
                            matricule = getMatriculeBoitieFromBinaryFormat(BufferConnect.ToList().GetRange(2, 8).ToArray());
                            Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} Atrack Balise:\t{matricule} try to connected.");
                            PrincipalListner.boitiers.TryGetValue(matricule, out boitier);
                            if (boitier == null)
                                throw new BaliseIdentificationException($"Matricule {matricule} n'est pas ajouté dans la base.");
                            OLDModelGeneratorProcessor.addConnect(boitier);

                            //LoggingCommunication("Connection", $"Connection: Succes\t{RemoteHote}\t{boitier.Nisbalise}\t{boitier.Matricule}");
                        }

                        #region    Atrack crypté            
                        ///  string textDecoder = "";              
                        #endregion

                        string trame = "";
                        if (bytesRead > 12)
                        {

                            trame = Encoding.ASCII.GetString(BufferIn, 12, bytesRead - 12);// text.Substring(text.IndexOf("@"));
                    
                            #region    Atrack crypté
                            //if (boitier.Nisbalise == "359316071459900")
                            //{
                            //    int DataEcryptedPos = getDataEcryptedPos(BufferConnect, 11);
                            //    if (DataEcryptedPos != -1)
                            //    {
                            //        textDecoder = Encoding.ASCII.GetString(BufferIn.ToList().GetRange(0, DataEcryptedPos).ToArray()) + DecryptStringFromBytes_Aes(BufferIn.ToList().GetRange(DataEcryptedPos, bytesRead - DataEcryptedPos).ToArray(), ATPassWord);
                            //        trame = textDecoder.Substring(text.IndexOf("@"));
                            //  }
                            // }  
                            #endregion
                            
                            currentDate = Collect.splitTrame(new Trame(boitier, trame, receptionTime), Socket, currentDate, ref trameReal);
                        }

                        if (trameReal != null)
                            OLDModelGeneratorProcessor.addTrameReal(trameReal);
                     
                        #region    Atrack crypté

                        //if (boitier.Nisbalise == "359316071459900")
                        //{
                        //    Logging("Crypté   : " + text, boitier.Nisbalise);
                        //    if (textDecoder != "")
                        //        Logging("Décrypté : " + textDecoder, boitier.Nisbalise);
                        //}
                        //else
                        #endregion

                        Logging($"{BitConverter.ToString(BufferConnect.ToList().GetRange(0, 12).ToArray()).Replace("-", "")}{trame}", boitier.Nisbalise);
                        Socket.Send(Encoding.ASCII.GetBytes(OK), OK.Length, SocketFlags.None);
                        Socket.BeginReceive(BufferIn, 0, BufferIn.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), this);

                    }

                    // Concox
                    else 
                    
                    if ((BufferConnect[0].Equals(0x78) && BufferConnect[1].Equals(0x78)) || (BufferConnect[0].Equals(0x79) && BufferConnect[1].Equals(0x79)))// trame GT06N GT800 WeTrack
                    {
                        if (boitier == null)
                        {
                            LoggingCommunication("Connection", $"TC\t{RemoteHote}\t{BitConverter.ToString(BufferConnect.ToList().GetRange(0, bytesRead).ToArray()).Replace("-", "")}"); //Try to connected
                            String NisBalise;
                            NisBalise = BitConverter.ToString(BufferConnect.ToList().GetRange(4, 8).ToArray()).Replace("-", "").Substring(1);
                            Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} Concox Balise:\t{NisBalise} try to connected.");
                            boitier = PrincipalListner.boitiers.FirstOrDefault(kvp => kvp.Value.Nisbalise == NisBalise).Value;
                            if (boitier == null)
                                throw new BaliseIdentificationException(String.Format("Matricule : {0} n'est pas ajouté dans la base.", matricule));
                            OLDModelGeneratorProcessor.addConnect(boitier);
                            byte[] SendBuffer = getBufferToSend(BufferConnect);
                            Logging("Reception: " + BitConverter.ToString(BufferConnect.ToList().GetRange(0, bytesRead).ToArray()).Replace("-", ""), boitier.Nisbalise);
                            Logging("Envoie   : " + BitConverter.ToString(SendBuffer).Replace("-", ""), boitier.Nisbalise);
                            Socket.Send(SendBuffer, SendBuffer.Length, SocketFlags.None);

                            //LoggingCommunication("Connection", $"Connection: Succes\t{RemoteHote}\t{boitier.Nisbalise}\t{boitier.Matricule}");
                        }
                        else
                        {
                            Logging("Reception: " + BitConverter.ToString(BufferConnect.ToList().GetRange(0, bytesRead).ToArray()).Replace("-", ""), boitier.Nisbalise);
                            if (bytesRead >= 5)
                            {
                                string trame = BitConverter.ToString(BufferConnect.ToList().GetRange(0, bytesRead).ToArray()).Replace("-", "");
                                currentDate = Collect.splitTrame(new Trame(boitier, trame, receptionTime), Socket, currentDate, ref trameReal);
                            }
                            if (trameReal != null)
                                OLDModelGeneratorProcessor.addTrameReal(trameReal);
                        }

                        Socket.BeginReceive(BufferIn, 0, BufferIn.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), this);
                    }

                    #region GT800 Crypté
                    //else if ((BufferConnect[0].Equals(0x80) && BufferConnect[1].Equals(0x80)))
                    //{
                    //    try
                    //    {
                    //        if (boitier == null)
                    //        {
                    //            LoggingCommunication("Connection", $"TC\t{RemoteHote}\t{BitConverter.ToString(BufferConnect.ToList().GetRange(0, bytesRead).ToArray()).Replace("-", "")}"); //Try to connected
                    //            string NisBalise;
                    //            byte[] BufferDecrypted = Cryptage.DecrytTrameConcox(BufferConnect.ToList().GetRange(0, bytesRead).ToArray(), Cryptage.getKey(""));

                    //            NisBalise = BitConverter.ToString(BufferDecrypted.ToList().GetRange(4, 8).ToArray()).Replace("-", "").Substring(1);
                    //            Console.WriteLine($"{DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss]")}\tConcox crypté Balise:\t{NisBalise} try to connected.");
                    //            boitier = PrincipalListner.boitiers.FirstOrDefault(kvp => kvp.Value.Nisbalise == NisBalise).Value;

                    //            if (boitier == null)
                    //                throw new BaliseIdentificationException(String.Format("Matricule : {0} n'est pas ajouté dans la base.", matricule));

                    //            //LoggingCommunication("Connection", $"CloseConnection: Close succes\t{RemoteHote}\t{boitier.Matricule}\t{boitier.Matricule}");

                    //            OLDModelGeneratorProcessor.addConnect(boitier);
                    //            BufferDecrypted[0] = 0x78;
                    //            BufferDecrypted[1] = 0x78;
                    //            Logging("Reception\t" + BitConverter.ToString(BufferConnect.ToList().GetRange(0, bytesRead).ToArray()).Replace("-", ""), boitier.Nisbalise);
                    //            Logging("Decryptage\t" + BitConverter.ToString(BufferDecrypted).Replace("-", ""), boitier.Nisbalise);


                    //            byte[] Reponse = getBufferToSend(BufferDecrypted);
                    //            byte[] ReponseCrypted = Cryptage.CrypteConcoxReponseMsg(Reponse, Cryptage.getKey(boitier.Nisbalise));

                    //            Reponse[0] = 0x78;
                    //            Reponse[1] = 0x78;

                    //            Logging("Envoie\t\t: " + BitConverter.ToString(ReponseCrypted).Replace("-", ""), boitier.Nisbalise + "-ReponseServer");
                    //            Logging("Decryptage\t: " + BitConverter.ToString(Reponse).Replace("-", ""), boitier.Nisbalise + "-ReponseServer");

                    //            Socket.Send(ReponseCrypted, ReponseCrypted.Length, SocketFlags.None);
                    //        }
                    //        else
                    //        {

                    //            Logging("Reception\t: " + BitConverter.ToString(BufferConnect.ToList().GetRange(0, bytesRead).ToArray()).Replace("-", ""), boitier.Nisbalise);

                    //            if (bytesRead >= 5)
                    //            {
                    //                int LenghtTrame = 0;
                    //                int bytesTraite = 0;
                    //                do
                    //                {

                    //                    string str = BitConverter.ToString(BufferConnect.ToList().GetRange(bytesTraite + 2, 2).ToArray()).Replace("-", "");
                    //                    LenghtTrame = int.Parse(str, System.Globalization.NumberStyles.AllowHexSpecifier) + 6;

                    //                    byte[] TrameDecrypted = Cryptage.DecrytTrameConcox(BufferConnect.ToList().GetRange(bytesTraite, LenghtTrame).ToArray(), Cryptage.getKey(boitier.Nisbalise));

                    //                    byte[] b = new byte[TrameDecrypted.Length];
                    //                    Array.Copy(TrameDecrypted, b, TrameDecrypted.Length);

                    //                    if (b[0] == 0x80)
                    //                    {
                    //                        b[0] = 0x78;
                    //                        b[1] = 0x78;
                    //                    }

                    //                    Logging("Decryptage\t: " + BitConverter.ToString(b).Replace("-", ""), boitier.Nisbalise);

                    //                    if (TrameDecrypted.Length >= 5)
                    //                    {
                    //                        string trame = BitConverter.ToString(TrameDecrypted.ToList().GetRange(0, TrameDecrypted.Length).ToArray()).Replace("-", "");
                    //                        currentDate = Collect.splitTrame(new Trame(boitier, trame, receptionTime), Socket, currentDate, ref trameReal);
                    //                    }


                    //                    if (trameReal != null)
                    //                        OLDModelGeneratorProcessor.addTrameReal(trameReal);


                    //                    bytesTraite += LenghtTrame;

                    //                } while (bytesTraite < bytesRead);

                    //            }
                    //        }

                    //        // OLDModelGeneratorProcessor.addTrameBrute(new Trame(boitier,text));

                    //        Socket.BeginReceive(BufferIn, 0, BufferIn.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), this);

                    //    }
                    //    catch (Exception ex) // this instruction will be delete when teste is complete
                    //    {
                    //        Logging("Reception : " + BitConverter.ToString(BufferConnect.ToList().GetRange(0, bytesRead).ToArray()).Replace("-", ""), "Erreur Cryptage");
                    //        Logging("Reception : " + ex.ToString(), "Erreur Cryptage");
                    //        CloseConnection();
                    //    }


                    //}
                    #endregion

                    #region  Teltonika
                    //else if ((BufferConnect[0].Equals(0x00) && BufferConnect[1].Equals(0x0F)) || (BufferConnect[0].Equals(0x00) && BufferConnect[1].Equals(0x00)))
                    //{
                    //    if (boitier == null)
                    //    {
                    //        LoggingCommunication("Connection", $"TC\t{RemoteHote}\t{BitConverter.ToString(BufferConnect.ToList().GetRange(0, bytesRead).ToArray()).Replace("-", "")}"); //Try to connected
                    //        string NisBalise = Encoding.ASCII.GetString(BufferConnect.ToList().GetRange(2, 15).ToArray());
                    //        Console.WriteLine($"{DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss]")}\tTeltonika Balise:\t{NisBalise} try to connected.");
                    //        boitier = PrincipalListner.boitiers.FirstOrDefault(kvp => kvp.Value.Nisbalise == NisBalise).Value;

                    //        if (boitier == null)
                    //        {
                    //            Socket.Send(new byte[] { 0x00 }, 1, SocketFlags.None);
                    //            throw new BaliseIdentificationException(String.Format("Matricule : {0} n'est pas ajouté dans la base.", matricule));
                    //        }
                    //        Socket.Send(new byte[] { 0x01 }, 1, SocketFlags.None);
                    //        OLDModelGeneratorProcessor.addConnect(boitier);
                    //        Logging("Reception: " + BitConverter.ToString(BufferConnect.ToList().GetRange(0, bytesRead).ToArray()).Replace("-", ""), boitier.Nisbalise);

                    //       // LoggingCommunication("Connection", $"Connection: Succes\t{RemoteHote}\t{boitier.Nisbalise}\t{boitier.Matricule}");
                    //    }
                    //    else
                    //    {

                    //        string trame = BitConverter.ToString(BufferConnect.ToList().GetRange(0, bytesRead).ToArray()).Replace("-", "");
                    //        currentDate = Collect.splitTrame(new Trame(boitier, trame, receptionTime), Socket, currentDate, ref trameReal);

                    //        if (trameReal != null)
                    //            OLDModelGeneratorProcessor.addTrameReal(trameReal);
                    //        Logging("Reception: " + BitConverter.ToString(BufferConnect.ToList().GetRange(0, bytesRead).ToArray()).Replace("-", ""), boitier.Nisbalise);
                    //    }

                    //    Socket.BeginReceive(BufferIn, 0, BufferIn.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), this);

                    //}
                    #endregion

                    // Trame Atrack without login Or VGO 
                    else
                    {

                        string text = Encoding.ASCII.GetString(BufferIn, 0, bytesRead);
                        if (boitier == null)
                        {
                            LoggingCommunication("Connection", $"TC\t{RemoteHote}\t{BitConverter.ToString(BufferConnect.ToList().GetRange(0, bytesRead).ToArray()).Replace("-", "")}");//Try to connected

                            string trame = "";
                            matricule = getMatriculeBoitie(text, out trame);
                            Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")}Other Balise:\t{matricule} try to connected.");

                            PrincipalListner.boitiers.TryGetValue(matricule, out boitier);
                            if (boitier == null)
                                throw new BaliseIdentificationException(String.Format("Matricule : {0} n'est pas ajouté dans la base.", matricule));
                            OLDModelGeneratorProcessor.addConnect(boitier);

                            //LoggingCommunication("Connection", $"Connection: Succes\t{RemoteHote}\t{boitier.Nisbalise}\t{boitier.Matricule}");

                            if (bytesRead > 12 && text.StartsWith("@"))
                            {
                                #region    Atrack crypté
                                ///if (boitier.Nisbalise == "359316071459900")
                                ///{
                                ///    int DataEcryptedPos = getDataEcryptedPos(BufferConnect, 0);
                                ///    if (DataEcryptedPos != -1)
                                ///    {
                                ///        textDecoder = Encoding.ASCII.GetString(BufferIn.ToList().GetRange(0, DataEcryptedPos).ToArray()) + DecryptStringFromBytes_Aes(BufferIn.ToList().GetRange(DataEcryptedPos, bytesRead - DataEcryptedPos).ToArray(), ATPassWord);
                                ///        currentDate = Collect.splitTrame(new Trame(boitier, textDecoder, receptionTime), Socket, currentDate, ref trameReal);
                                ///    }
                                ///}
                                ///else
                                #endregion     
                               
                                currentDate = Collect.splitTrame(new Trame(boitier, text, receptionTime), Socket, currentDate, ref trameReal);
                            }
                            if (trameReal != null)
                                OLDModelGeneratorProcessor.addTrameReal(trameReal);

                        }
                        else
                        {
                            if (text.Trim().Length > 0)

                                #region    Atrack crypté
                                ///if (boitier.Nisbalise == "359316071459900")
                                ///{
                                ///    int DataEcryptedPos = getDataEcryptedPos(BufferConnect, 0);
                                ///    if (DataEcryptedPos != -1)
                                ///    {
                                ///        textDecoder = Encoding.ASCII.GetString(BufferIn.ToList().GetRange(0, DataEcryptedPos).ToArray()) + DecryptStringFromBytes_Aes(BufferIn.ToList().GetRange(DataEcryptedPos, bytesRead - DataEcryptedPos).ToArray(), ATPassWord);
                                ///        currentDate = Collect.splitTrame(new Trame(boitier, textDecoder, receptionTime), Socket, currentDate, ref trameReal);
                                ///    }
                                ///}
                                ///else                              
                                #endregion

                                currentDate = Collect.splitTrame(new Trame(boitier, text, receptionTime), Socket, currentDate, ref trameReal);
                            if (trameReal != null)
                                OLDModelGeneratorProcessor.addTrameReal(trameReal);
                        }

                        #region    Atrack crypté
                    
           
                        /// if (boitier.Nisbalise == "359316071459900") 
                        /// { 
                        ///    Logging("Crypté   : " + text, boitier.Nisbalise); 
                        ///    if (textDecoder != "")
                        ///         Logging("Décrypté : " + textDecoder, boitier.Nisbalise);    
                        /// }
                        /// else
               
    #endregion

                        Logging($"{text}", boitier.Nisbalise);
                        Socket.Send(Encoding.ASCII.GetBytes(OK), OK.Length, SocketFlags.None);
                        Socket.BeginReceive(BufferIn, 0, BufferIn.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), this);
                    }
                }
            }

            catch (BaliseIdentificationException bIdenExcp)
            {
                Logging("Fermeture de la connexion , impossible d'identifier le boitie ,cause : " + bIdenExcp.Message);
                LoggingCommunication("Connection", $"bIdenExcp\t{RemoteHote}.\t{bIdenExcp.Message}");
                CloseConnection();
            }
            catch (SocketException e)
            {
                LoggingCommunication("Connection", $"SE\t{RemoteHote}.\t{e}");
                CloseConnection();
            }
            catch (Exception e)
            {
                if (!ConexionClosed)
                    LoggingCommunication("Connection", $"E\t{RemoteHote}.\t{e}");
                CloseConnection();
            }

            LastTimeReception = DateTime.Now;
        }
        #region    Atrack crypté     
        static int getDataEcryptedPos(byte[] data, int startInddex)
        {

            int Pos = startInddex;
            int nbCama = 0;
            do
            {
                nbCama += data[Pos].Equals(0x2c) ? +1 : 0;
                Pos += 1;
            }
            while ((nbCama < 5) && (Pos < data.Length));

            if (Pos < data.Length)
                return Pos;
            else
                return -1;
        }
        #endregion
        private byte[] getBufferToSend(byte[] BufferConnect)
        {
            byte[] BufferToSend = new byte[10];

            byte[] BufferData = new byte[4];
            BufferData[0] = 0x05;
            BufferData[1] = BufferConnect[3];
            BufferData[2] = BufferConnect[17];
            BufferData[3] = BufferConnect[18];


            byte[] sum = BitConverter.GetBytes(Checksum.crc16(Checksum.CRC16_X25, BufferData));
            BufferToSend[0] = BufferConnect[0];
            BufferToSend[1] = BufferConnect[1];
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
        public void CloseConnection()
        {
            if (this.tcpClient == null || ConexionClosed)
            {
                //LoggingCommunication("Connection", $"CC: WCB\t{RemoteHote}\t{boitier.Matricule}\t{boitier.Matricule}"); // Was closed befor
                return;
            }

            ConexionClosed = true;

            try
            {
                this.tcpClient.Close();
                //LoggingCommunication("Connection", $"CC: CS\t{RemoteHote}\t{boitier.Matricule}\t{boitier.Matricule}"); // Close succes
            }
            catch
            {
                //LoggingCommunication("Connection", $"CC: E:\t{RemoteHote}\t{boitier.Matricule}\t{boitier.Matricule}.\r\n{e.Message}"); //Exception
            }
            if (boitier != null)
                OLDModelGeneratorProcessor.addDeconnect(boitier);
        }
        //////////private void SendCallback(IAsyncResult result)
        //////////{
        //////////    try
        //////////    {
        //////////        int bytesRead = Socket.EndSend(result);
        //////////        //if ( BufferOut.Length>0)
        //////////        //   { 
        //////////        //       SocketError codeErreur;
        //////////        //       this.Socket.Send(BufferOut, 0, BufferOut.Length, SocketFlags.None, out codeErreur);
        //////////        //   }

        //////////        //Console.WriteLine("la connexion: {0}, a envoyé: {1}.", Socket.Handle.ToString(), BufferOut);



        //////////        //  this.Socket.BeginSend(BufferOut, 0, dataSize, SocketFlags.None, new AsyncCallback(SendCallback), this);
        //////////    }


        //////////    catch (SocketException se)
        //////////    {

        //////////        // Console.WriteLine("La connection: {0} a été fermé. pour cause: {1}.", Socket.Handle.ToString(), se.ToString());
        //////////        CloseConnection();
        //////////    }
        //////////    catch (Exception e)
        //////////    {

        //////////        //Console.WriteLine("La connection: {0} a été fermé. pour cause: {1}.", Socket.Handle.ToString(), e.ToString());
        //////////        CloseConnection();
        //////////    }

        //////////}
        public bool IsConnected()
        {
            try
            {
                if (this.tcpClient == null || this.tcpClient.Client == null)
                    return false;
                if (!tcpClient.Connected)
                    return false;
                if ((DateTime.Now - LastTimeReception).TotalMinutes > 15)
                    return false;

                return true;
            }
            catch (Exception e)
            {
                LoggingCommunication("Connection", $"erreur dans la recuperation de l'etat de connection.\r\n{e.Message}");
                return false;
            }

        }
        private int getMatriculeBoitie(String trame, out String trameConnect)
        {
            int matricule = 0;
            String matriculeString;
            if (trame.Contains("$GCONNECT"))
            {
                char[] separator = { ' ' };
                String[] trameDecoup = trame.Split(separator, System.StringSplitOptions.RemoveEmptyEntries);
                try
                {
                    matriculeString = trameDecoup[0];
                    trameConnect = trameDecoup[1];
                }
                catch (Exception exp)
                {
                    throw new BaliseIdentificationException("trame de connecion : " + trame, exp);
                }
            }
            else if (trame.StartsWith("$I2BCONNECT"))
            {
                try
                {
                    char[] separator = { ',' };
                    String[] trameDecoup = trame.Split(separator, System.StringSplitOptions.RemoveEmptyEntries);
                    trameConnect = trameDecoup[0];
                    matriculeString = trameDecoup[1];
                }
                catch (Exception exp)
                {

                    throw new BaliseIdentificationException(exp);
                }
            }
            else
            {
                throw new BaliseIdentificationException("Impossible d'identifier la balise ,donnée recue :" + trame.ToString());
            }
            try
            {
                matricule = Int32.Parse(matriculeString);
            }
            catch (Exception exp)
            {
                throw new BaliseIdentificationException(exp);
            }
            return matricule;

        }
        private int getMatriculeBoitieFromBinaryFormat(byte[] data)
        {
            int matricule = 0;

            Array.Reverse(data);

            try
            {
                matricule = BitConverter.ToInt32(data, 0);


            }
            catch (Exception exp)
            {

                throw new BaliseIdentificationException("matricule binary  de connecion : " + data.ToString(), exp);
            }

            return matricule;

        }
        private void Logging(String message)
        {

            try
            {

                String path = "Log\\" + DateTime.Now.ToString("yyyy-MM-dd");

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                using (StreamWriter sw = File.AppendText(path + "\\Balises-" + ((this.boitier != null) ? (this.boitier.Matricule) + "" : "Innconu") + ".log"))
                {
                    sw.WriteLine(DateTime.Now + ": " + message);
                    sw.WriteLine(" @IP  : " + RemoteHote);
                    sw.Close();
                }

            }
            catch (Exception)
            {

            }

        }
        public static void Logging(String message, string balise)
        {
            try
            {
                String path = "Log\\Trame\\" + DateTime.Now.ToString("yyyy-MM-dd");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                using (StreamWriter sw = File.AppendText(path + "\\Balises-" + balise + ".log"))
                {
                    sw.WriteLine(DateTime.Now + " " + message);
                    sw.Close();
                }
            }
            catch (Exception)
            {


            }
        }
        private void LoggingConnection(string type, String message)
        {
            try
            {

                String path = "Log\\" + DateTime.Now.ToString("yyyy-MM-dd") + "\\connection" + type;

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }


                using (StreamWriter sw = File.AppendText(path + "\\Balises-" + ((this.boitier != null) ? (this.boitier.Matricule) + "" : "Innconu") + ".log"))
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
        private static void LoggingCommunication(String erreur, String message)
        {

            String path = "Log\\" + DateTime.Now.ToString("yyyy-MM-dd");
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                using (StreamWriter sw = File.AppendText(path + "\\Trace_" + erreur + ".log"))
                {
                    sw.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " " + message);
                    sw.Close();
                }
            }
            catch (Exception)
            {

            }
        }
        internal string getRemoteConnection()
        {
            if (this.Socket != null)
            {
                try
                {
                    return this.Socket.RemoteEndPoint.ToString();
                }
                catch (Exception e)
                {

                    return "hôte non identifie: " + e.Message;
                }
            }
            else
                return "hôte non identifie";
        }
        override public string ToString()
        {
            if (boitier != null)
                return boitier.Nisbalise;
            return "null";
        }
    }

}
