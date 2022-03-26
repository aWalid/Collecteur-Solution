using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT06NParser
{
    public enum TrameType
    {
        Data = 18,      // 12 position 
        Login = 1,      // 01 Login
        Status = 19,    // 13 maintien
        Alarm = 22,     // 16 alarm
        GPRStatus=139,  // 8B Redemarage du boitier
        Unknown=0
        
    }
}
