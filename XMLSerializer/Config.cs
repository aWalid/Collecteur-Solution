using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;


namespace XMLSerializer
{
   
   public class Config : Serialise
    {

         
        public String IPAddressString { set;get; }
        public int Port { set; get; }

        [NonSerialized]
        private IPAddress IPAddresse;

       

        public bool Debug { set; get; }

        public bool Prod { set; get; }
        public Config()
        {

        }
        public Config(String adress, int port, bool debug,bool prod)
        {
            this.IPAddressString = adress;
            this.Port = port;
            this.Debug = debug;
            this.Prod = prod;

        }
       public Config(String adress,int port,bool debug)
        {
            this.IPAddressString = adress;
            this.Port = port;
            this.Debug = debug;
            this.Prod = false;
           
        }
        public Config(String adress, int port)
        {
            this.IPAddressString = adress;
            this.Port = port;
            this.Debug = false;
            this.Prod = false;
        }
        public Config(IPAddress adress, int port)
        {
            this.IPAddresse = adress;
            this.Port = port;
            this.Debug = false;
            this.Prod = false;

        }
       public IPAddress getIPAddress(){

           return IPAddresse;
       }
       public void setIPAddress(IPAddress ipAddress){

           this.IPAddresse = ipAddress;
       }
        
    }
}
