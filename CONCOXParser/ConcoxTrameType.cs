using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CONCOXParser
{
    public enum ConcoxTrameType
    {
        Login = 1,          // 01 Login 
        WeTrackData = 18,   // 12 Position WeTrack 
        Status = 19,        // 13 Maintien
        GT800Data = 34,     // 22 Position GT800    
        Alarm = 38,         // 26 Alarm
        WeTrackDataAlarm = 22,  // 16 WeTrack Data Alarm    
        eTime = 138,        // 8A e-Time check
        GPRStatus = 139,    // 8B Redemarage du boitier
        Unknown = 0      
    }
}
