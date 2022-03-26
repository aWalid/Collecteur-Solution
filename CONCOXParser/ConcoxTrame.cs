using CONCOXParser.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CONCOXParser
{
    public class ConcoxTrame
    {
        public ConcoxTrameType Type;
        public StatusTrame StatusTrame;
        public LoginTrame LoginTrame;
        public AlarmTrame AlarmTrame;
        public GPRStatusTrame GPRStatusTrame;
        public DataLocationTrame DataLocationTrame;
        public eTimeTrame eTimeCheckTrame;
        public OtherTrame OtherTrame;
    }
}
