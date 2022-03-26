using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CONCOXParser.Types
{
    public class eTimeTrame

    {
        public int NumeroProtocol;
        public string eTime;

        public eTimeTrame()
        {
            this.NumeroProtocol = 138;        // 8A e-Time check
            this.eTime = getTime();
        }

        public string getTime()
        {
            DateTime t = DateTime.UtcNow;
            return t.Year.ToString("0000").Substring(2) + t.Month.ToString("00") + t.Day.ToString("00") + t.Hour.ToString("00") + t.Minute.ToString("00") + t.Second.ToString("00");

        }
    }
}
