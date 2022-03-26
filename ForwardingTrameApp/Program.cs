
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Security.Principal;
using System.Net.NetworkInformation;
using log4net;
using BaliseListner.ThreadListener;

namespace BaliseListner
{
  

    class MainClass
    {

        private static readonly ILog logger = LogManager.GetLogger(typeof(MainClass));
        static void Main(string[] args)
        {
           
            log4net.Config.XmlConfigurator.Configure();
            ThreadLancer.StartCollecteur();
        }
    }
}

