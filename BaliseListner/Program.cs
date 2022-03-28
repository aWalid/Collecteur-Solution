
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
using System.Runtime.InteropServices;


namespace BaliseListner
{


    class MainClass
    {
         private static readonly ILog logger = LogManager.GetLogger(typeof(MainClass));   
        static MainClass()
        {
            Console.CancelKeyPress += new ConsoleCancelEventHandler(Console_CancelKeyPress);
        }
        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            if ((e.SpecialKey == ConsoleSpecialKey.ControlBreak) || (e.SpecialKey == ConsoleSpecialKey.ControlC))
            {
                Console.WriteLine("Asynchronous shutdown Started");
                ThreadLancer.StopCollecteur();
                Console.WriteLine("Asynchronous shutdown Ended");
                Console.WriteLine("Wait 1 minutes befor exit");
                Thread.Sleep(60000);
                Environment.Exit(1);
            }
        }
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            ThreadLancer.StartCollecteur();
        }
    }
}

