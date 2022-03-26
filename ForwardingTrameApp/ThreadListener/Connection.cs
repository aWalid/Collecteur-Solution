
using BaliseListner.CollecteurException;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BaliseListner.ThreadListener
{
    class Connection 
    {
            private readonly static Object lockObject = new Object();
            private int atsForordPort;
            private Socket Socket;
            public byte[] BufferIn;
            public byte[] BufferOut;
            public byte[] BufferOutAts;
            private StreamWriter StreamWriterAts;
            private StreamWriter StreamWriterGeotrack;
            TcpClient SocketATSCollecteur = new TcpClient("192.168.1.53", 30004);
            TcpClient SocketGEOTRACKCollecteur = new TcpClient("192.168.0.110", 30002);
            private int dataSize = 0;
         
            
             //proprite pour old Collecteur
           
            public Connection(Socket Socket,int atsForwordPort)
            {
                this.atsForordPort = atsForwordPort;
                this.Socket = Socket;
                Socket.Blocking = false;
                BufferIn = new byte[1024];
                BufferOut = new byte[1024];
                BufferOutAts = new byte[1024];
                
                // Sr = new StreamReader(FluxClient);
                StreamWriterAts = new StreamWriter(SocketATSCollecteur.GetStream());
                StreamWriterGeotrack = new StreamWriter(SocketGEOTRACKCollecteur.GetStream());
                  }
       
            public void StartCommunication()
            {
                this.Socket.BeginReceive(BufferIn, 0, BufferIn.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), this);
                 //  this.Socket.BeginSend(BufferOut, 1024, dataSize, SocketFlags.None, new AsyncCallback(SendCallback), this);
                SocketATSCollecteur.Client.BeginReceive(BufferOutAts, 0, BufferOutAts.Length, SocketFlags.None, new AsyncCallback(AtsCollecteurReceiveCallback), this);
            }
            private void ReceiveCallback(IAsyncResult result)
            {
                try
                {
                    int bytesRead = Socket.EndReceive(result);
                    if (0 != bytesRead)
                    {
                        string text = Encoding.ASCII.GetString(BufferIn, 0, bytesRead); ;
                        Logging(text,true);
                        StreamWriterAts.WriteLine(BufferIn);
                        StreamWriterGeotrack.WriteLine(BufferIn);
                        StreamWriterAts.Flush();
                        StreamWriterGeotrack.Flush();
                       // Socket.Send(Encoding.ASCII.GetBytes(OK), OK.Length, SocketFlags.None);
                        
                        Socket.BeginReceive(BufferIn, 0, BufferIn.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), this);
                       
                    }
                }
               
                catch (SocketException e)
                {

                    //  Console.WriteLine("La connection: {0} a été fermé. pour cause: {1}.", Socket.Handle.ToString(), e.ToString());
                    this.Socket.Close();
                 
                }
            }
            private void AtsCollecteurReceiveCallback(IAsyncResult result)
            {
                try
                {
                    int bytesRead = SocketATSCollecteur.Client.EndReceive(result);
                    if (0 != bytesRead)
                    {
                        string text = Encoding.ASCII.GetString(BufferOutAts, 0, bytesRead); ;
                        Logging(text, false);
                       
                        Socket.Send(BufferOutAts, BufferOutAts.Length, SocketFlags.None);

                        SocketATSCollecteur.Client.BeginReceive(BufferOutAts, 0, BufferOutAts.Length, SocketFlags.None, new AsyncCallback(AtsCollecteurReceiveCallback), this);

                    }
                }

                catch (SocketException e)
                {

                    //  Console.WriteLine("La connection: {0} a été fermé. pour cause: {1}.", Socket.Handle.ToString(), e.ToString());
                    this.Socket.Close();

                }
            }
        
    public  void CloseConnection()
       {
           if (this.Socket == null)
               return;
         
           this.Socket.Close();
           
       }
    private void SendCallback(IAsyncResult result)
    {
        try
        {
                 int bytesRead = Socket.EndSend(result);
                 //if ( BufferOut.Length>0)
                 //   { 
                 //       SocketError codeErreur;
                 //       this.Socket.Send(BufferOut, 0, BufferOut.Length, SocketFlags.None, out codeErreur);
                 //   }

                //Console.WriteLine("la connexion: {0}, a envoyé: {1}.", Socket.Handle.ToString(), BufferOut);



              //  this.Socket.BeginSend(BufferOut, 0, dataSize, SocketFlags.None, new AsyncCallback(SendCallback), this);
       }   

        
        catch (SocketException se)
        {
            
           // Console.WriteLine("La connection: {0} a été fermé. pour cause: {1}.", Socket.Handle.ToString(), se.ToString());
             CloseConnection();  
        }
        catch (Exception e)
        {
            
            //Console.WriteLine("La connection: {0} a été fermé. pour cause: {1}.", Socket.Handle.ToString(), e.ToString());
            CloseConnection();
        }
       
    }
    private  void Logging(String message,bool receiveFromBalise)
    {
        string recieve;
        if (receiveFromBalise)
        {
            recieve = " Reception ";
        }
        else
        {
            recieve = " Evoie ";
        }
        lock (lockObject) { 
        using (StreamWriter sw = File.AppendText("Log\\Balises-" + this.Socket.Handle))
        {
            sw.WriteLine(DateTime.Now + "  " + recieve + ": " + message);
        }
        }
    }
     private void Logging(String message,Exception exp)
    {
        lock (lockObject)
        {
            using (StreamWriter sw = File.AppendText("Log\\Balise-" + this.Socket.Handle))
            {
                sw.WriteLine(DateTime.Now + ": " + message);
                sw.WriteLine(exp.Message);
            }
        }
    }

    

    }
}
