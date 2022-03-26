using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Collecteur.Core.Api
{
    [Serializable]
    public class BaliseInfo 
    {
         public Firmware firmware { get; set; }

         public char[] trameSeparator { get; set; }

         public String OK { get; set; }

         public String KO { get; set; }
         public BaliseInfo(Firmware firmware)
         {
             this.firmware = firmware;
             this.trameSeparator = new char[2] {'\r','\n'};
             this.KO = "#\r\n";
             this.OK = "!\r\n";
           
         }
         public BaliseInfo()
         {
             this.firmware = Firmware.UNKNOWN;
             this.trameSeparator = new char[2] { '\r', '\n' };
         }

         public Regex getRegixForMatriculeBalise()
         {
             switch (firmware)
             {
                 case Firmware.VEGEO3: return new Regex(@"([\d]+) ([\w]+)$");
                 case Firmware.VEGEO5: return new Regex(@"([\d]+) ([\w]+)$");
                 case Firmware.VEGEO6: return new Regex(@"([\d]+) ([\w]+)$");
                 case Firmware.ATRACK: return new Regex(@"([\d]+) ([\w]+)$");
                 
                 default: return null;

             }
           
         }

    }

    [Serializable]
     public enum Firmware
     {
         VEGEO3,
         VEGEO5,
         VEGEO6,
         ATRACK,
         UNKNOWN
     }
}
