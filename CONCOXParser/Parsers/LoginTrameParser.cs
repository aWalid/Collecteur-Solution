using CONCOXParser.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CONCOXParser.Parsers
{
    public class LoginTrameParser
    {
        public static LoginTrame ParseLoginTrame(string inputLoginTrame)
        {
            LoginTrame extractedTrame = new LoginTrame();
            extractedTrame.NISBalise = GetNisBalise(inputLoginTrame);
            extractedTrame.NumeroTrame = GetNumeroTrame(inputLoginTrame);
            SetOriginalTrameValue(inputLoginTrame, ref extractedTrame);
            return extractedTrame;
        }
        
        public static string GetNisBalise(string inputLoginTrame)
        {
            string nisBalise;

            nisBalise = inputLoginTrame.Substring(7, 16);
            return nisBalise;
        }

        public static int GetNumeroTrame(string inputLoginTrame)
        {
            int numTrame;
            string numTrameBrute = inputLoginTrame.Substring(24, 4);
            //Parse to decimal value
            numTrame = int.Parse(numTrameBrute, System.Globalization.NumberStyles.AllowHexSpecifier);
            //return the value
            return numTrame;
        }

        public static void SetOriginalTrameValue(string inputLoginTrame, ref LoginTrame  trame)
        {
            trame.OriginalTrameValue = inputLoginTrame;
        }
        
    }

}