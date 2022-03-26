using GT06NParser.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT06NParser
{
    public class GT06NTrame
    {
        public TrameType Type;
        public StatusTrame StatusTrame;
        public LoginTrame LoginTrame;
        public AlarmTrame AlarmTrame;
        public GPRStatusTrame GPRStatusTrame;
        public DataLocationTrame DataLocationTrame;
        public OtherTrame OtherTrame;
    }
}
