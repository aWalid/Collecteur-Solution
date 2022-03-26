using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CONCOXParser.Types
{
    public class OtherTrame

    {
        public int NumeroProtocol;

        public OtherTrame(string protocol)
        {
            this.NumeroProtocol = int.Parse(protocol, System.Globalization.NumberStyles.AllowHexSpecifier);
        }
    }
}
