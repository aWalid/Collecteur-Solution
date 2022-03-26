using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CONCOXParser.Types
{
    public class DataLocationTrame
    {
        public decimal Longitude;
        public decimal Latitude;
        public Boolean North;
        public Boolean West;
        public Int16 cap;
        public Int16 Ignition;
        public string ProtocolNumber;
        public DateTime? Date;
        public string SatNumber;
        public double Speed;
        public int Alerm; // wetrack alerm 
    }
}
