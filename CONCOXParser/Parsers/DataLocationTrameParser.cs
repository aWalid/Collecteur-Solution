using CONCOXParser.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CONCOXParser.Parsers
{
    public class DataLocationTrameParser
    {
        private const int DATA_TRAME_LENGHT = 68;
        private const int LONGCONVERSIONFACTOR = 1800000;
        private const int LATCONVERSIONFACTOR = 1800000;
        public static DataLocationTrame ParseDataLocationTrame(string inputLocationTrame)
        {
            DataLocationTrame trame = new DataLocationTrame();

            trame.Date = GetDate(inputLocationTrame);
            trame.Latitude = GetLatitude(inputLocationTrame);
            trame.Longitude = GetLongitude(inputLocationTrame);
            trame.North = GetLongitudeDirection(inputLocationTrame);
            trame.West = GetLatitudeDirection(inputLocationTrame);
            trame.ProtocolNumber = GetProtocolNumber(inputLocationTrame).ToString();
            trame.SatNumber = GetSatelliteNumber(inputLocationTrame);
            trame.Speed = GetSpeedNumber(inputLocationTrame);
            trame.cap = GetCap(inputLocationTrame);
            trame.Ignition = GetIgnition(inputLocationTrame);

            return trame;
        }

        // Ignition Implementation ToDo
        private static short GetCap(string inputTrame)
        {
            if (string.IsNullOrEmpty(inputTrame))
            {
                //Case of invalid trame format
                return 0;
            }
            else
            {
                //Check if the string provided represents a valid trame sent by the GT06N box
                if (CheckTrameFormat(inputTrame))
                {
                    string capBrute = GetCapBruteData(inputTrame);
                    short cap = ParseCapBruteValue(capBrute);
                    return cap;
                }
                else
                {
                    return 0;   //The data frame (trame) is not detected as a valid GT06N data frame }
                }
            }

        }


        private static short ParseCapBruteValue(string capBrute)
        {
            Int16 c;
            if (!Int16.TryParse(capBrute, out c))
            {
                c = 0;
            }
            return c;
        }


        private static string GetCapBruteData(string inputTrame)
        {
            Dictionary<string, string> parsedTrame;
            if (TryParseDataLocationTrame(inputTrame, out parsedTrame))
            {
                return parsedTrame["Cap"];
            }
            return null;
        }

        // Ignition Implementation ToDo
        private static short GetIgnition(string inputTrame)
        {
            if (string.IsNullOrEmpty(inputTrame))
            {
                //Case of invalid trame format
                return 0;
            }
            else
            {
                //Check if the string provided represents a valid trame sent by the GT06N box
                if (CheckTrameFormat(inputTrame))
                {
                    string ignitionBrute = GetIgnitionBruteData(inputTrame);
                    short ignition = ParseIgnitionBruteValue(ignitionBrute);
                    return ignition;
                }
                else
                {
                    return 0;   //The data frame (trame) is not detected as a valid GT06N data frame }
                }
            }
        }

        private static string GetIgnitionBruteData(string inputTrame)
        {
            Dictionary<string, string> parsedTrame;
            if (TryParseDataLocationTrame(inputTrame, out parsedTrame))
            {
                return parsedTrame["Ignition"];
            }
            return null;
        }

        private static short ParseIgnitionBruteValue(string ignitionBrute)
        {
            Int16 c;
            if (!Int16.TryParse(ignitionBrute, out c))
            {
                c = 0;
            }
            return c;
        }

        private static short ParseAlermBruteValue(string alermBrute)
        {
            Int16 c;
            if (!Int16.TryParse(alermBrute, out c))
            {
                c = 0;
            }
            return c;
        }
        // Speed Implementation 
        private static double GetSpeedNumber(string inputTrame)
        {
            if (string.IsNullOrEmpty(inputTrame))
            {
                //Case of invalid trame format
                return 0;
            }
            else
            {
                //Check if the string provided represents a valid trame sent by the GT06N box
                if (CheckTrameFormat(inputTrame))
                {
                    string SpeedBrute = GetSpeedBruteData(inputTrame);
                    double speed = ParseSpeedBruteValue(SpeedBrute);
                    return speed;
                }
                else
                {
                    return 0; //The data frame (trame) is not detected as a valid GT06N data frame }
                }
            }
        }

        private static double ParseSpeedBruteValue(string SpeedBrute)
        {
            if (string.IsNullOrEmpty(SpeedBrute))
            {
                return 0;
            }
            else
            {
                int intValueOfLong = int.Parse(SpeedBrute, System.Globalization.NumberStyles.AllowHexSpecifier);
                double longValue = (double)intValueOfLong;
                return longValue;
            }
        }


        private static decimal GetLongitude(string inputTrame)
        {
            if (string.IsNullOrEmpty(inputTrame))
            {
                //Case of invalid trame format
                return 0;
            }
            else
            {
                //Check if the string provided represents a valid trame sent by the GT06N box
                if (CheckTrameFormat(inputTrame))
                {
                    string LongitudeBrute = GetLongitudeBruteData(inputTrame);
                    Boolean west = GetLatitudeDirection(inputTrame);
                    decimal longitude = (west ? -1 * ParseLongitudeBruteValue(LongitudeBrute) : ParseLongitudeBruteValue(LongitudeBrute));
                    //Console.WriteLine("Longitude: {0} west: {1}", LongitudeBrute, west.ToString());
                    return longitude;

                }
                else
                {
                    return 0; //The data frame (trame) is not detected as a valid GT06N data frame }
                }
            }
        }
        private static decimal GetLatitude(string inputTrame)
        {
            if (string.IsNullOrEmpty(inputTrame))
            {
                //Case of invalid trame format
                return 0;
            }
            else
            {
                //Check if the string provided represents a valid trame sent by the GT06N box
                if (CheckTrameFormat(inputTrame))
                {
                    string LatitudeBrute = GetLatitudeBruteData(inputTrame);

                    Boolean north = GetLongitudeDirection(inputTrame);
                    decimal latitude = (north ? ParseLatitudeBruteValue(LatitudeBrute) : -1 * ParseLatitudeBruteValue(LatitudeBrute));
                    //Console.WriteLine("Latitude: {0} north: {1}", LatitudeBrute, north.ToString());
                    return latitude;

                }
                else
                {
                    return 0; //The data frame (trame) is not detected as a valid GT06N data frame }

                }
            }
        }
        private static int GetProtocolNumber(string inputTrame)
        {
            if (string.IsNullOrEmpty(inputTrame))
            {
                //Case of invalid trame format
                return 0;
            }
            else
            {
                //Check if the string provided represents a valid trame sent by the GT06N box
                if (CheckTrameFormat(inputTrame))
                {
                    string protocolNumberBrute = GetProtocolNumberStringValue(inputTrame);
                    int protocolNbr = ParseProtocoleNumberBruteValue(protocolNumberBrute);
                    return protocolNbr;
                }
                else
                {
                    return 0; //The data frame (trame) is not detected as a valid GT06N data frame }

                }
            }
        }
        private static DateTime? GetDate(string inputTrame)
        {
            if (string.IsNullOrEmpty(inputTrame))
            {
                //Case of invalid trame format
                return null;
            }
            else
            {
                //Check if the string provided represents a valid trame sent by the GT06N box
                if (CheckTrameFormat(inputTrame))
                {
                    string DateBrute = GetDateStringValue(inputTrame);
                    DateTime? date = ParseDateBruteValue(DateBrute);
                    return date;
                }
                else
                {
                    return null; //The data frame (trame) is not detected as a valid GT06N data frame }

                }
            }
        }
        private static string GetSatelliteNumber(string inputTrame)
        {
            if (string.IsNullOrEmpty(inputTrame))
            {
                //Case of invalid trame format
                return null;
            }
            else
            {
                //Check if the string provided represents a valid trame sent by the GT06N box
                if (CheckTrameFormat(inputTrame))
                {
                    string satNbr = GetSatNumberStringValue(inputTrame);
                    return satNbr;
                }
                else
                {
                    return null; //The data frame (trame) is not detected as a valid GT06N data frame }

                }
            }
        }
        private static DateTime? ParseDateBruteValue(string dateBrute)
        {
            if (string.IsNullOrEmpty(dateBrute))
            {
                return null;
            }
            else
            {

                int yearsPart = int.Parse(dateBrute.Substring(0, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
                int monthsPart = int.Parse(dateBrute.Substring(2, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
                int daysPart = int.Parse(dateBrute.Substring(4, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
                int hoursPart = int.Parse(dateBrute.Substring(6, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
                int minutesPart = int.Parse(dateBrute.Substring(8, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
                int secondsPart = int.Parse(dateBrute.Substring(10, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
                DateTime parsedDate = new DateTime(2000 + yearsPart, monthsPart, daysPart, hoursPart, minutesPart, secondsPart);
                return parsedDate;
            }
        }

        private static string GetDateStringValue(string inputTrame)
        {
            Dictionary<string, string> parsedTrame;
            if (TryParseDataLocationTrame(inputTrame, out parsedTrame))
            {
                return parsedTrame["Date"];
            }
            return null;
        }

        private static int ParseProtocoleNumberBruteValue(string protocolNumberBrute)
        {
            if (string.IsNullOrEmpty(protocolNumberBrute))
            {
                return 0;
            }
            else
            {
                return int.Parse(protocolNumberBrute);
            }
        }

        private static decimal ParseLatitudeBruteValue(string latitudeBrute)
        {
            if (string.IsNullOrEmpty(latitudeBrute))
            {
                return 0;
            }
            else
            {
                int intValueOfLong = int.Parse(latitudeBrute, System.Globalization.NumberStyles.AllowHexSpecifier);
                decimal longValue = (decimal)intValueOfLong / LATCONVERSIONFACTOR;
                return longValue;
            }
        }

        private static string GetLatitudeBruteData(string inputTrame)
        {
            Dictionary<string, string> parsedTrame;
            if (TryParseDataLocationTrame(inputTrame, out parsedTrame))
            {
                return parsedTrame["Latitude"];
            }
            return null;
        }

        private static Boolean GetLatitudeDirection(string inputTrame)
        {
            Dictionary<string, string> parsedTrame;
            if (TryParseDataLocationTrame(inputTrame, out parsedTrame))
            {
                return (parsedTrame["West"] == "1");
            }
            return false;
        }

        private static Boolean GetLongitudeDirection(string inputTrame)
        {
            Dictionary<string, string> parsedTrame;
            if (TryParseDataLocationTrame(inputTrame, out parsedTrame))
            {
                return (parsedTrame["North"] == "1");
            }
            return true;
        }
        /// <summary>
        /// Converts a longitude from hexa GT06N format to a decimal positioning format
        /// </summary>
        /// <param name="LongitudeBrute"> Hexa GT06N formated value</param>
        /// <returns>the decimal representation of the longitude </returns>
        private static decimal ParseLongitudeBruteValue(string LongitudeBrute)
        {
            if (string.IsNullOrEmpty(LongitudeBrute))
            {
                return 0;
            }
            else
            {
                int intValueOfLong = int.Parse(LongitudeBrute, System.Globalization.NumberStyles.AllowHexSpecifier);
                decimal longValue = (decimal)intValueOfLong / LONGCONVERSIONFACTOR;
                return longValue;
            }
        }
        /// <summary>
        /// Extracts a longitude value string which is contained within the brute trame 
        /// </summary>
        /// <param name="inputTrame">A trame in hexa format that corresponds to the GT06N protocol</param>
        /// <returns>the hexa part that represent the brute format of the longitude value</returns>
        private static string GetLongitudeBruteData(string inputTrame)
        {
            Dictionary<string, string> parsedTrame;
            if (TryParseDataLocationTrame(inputTrame, out parsedTrame))
            {
                return parsedTrame["Longitude"];
            }
            return null;
        }
        /// <summary>
        /// Extracts a speed value string which is contained within the brute trame 
        /// </summary>
        /// <param name="inputTrame">A trame in hexa format that corresponds to the GT06N protocol</param>
        /// <returns>the hexa part that represent the brute format of the longitude value</returns>
        private static string GetSpeedBruteData(string inputTrame)
        {
            Dictionary<string, string> parsedTrame;
            if (TryParseDataLocationTrame(inputTrame, out parsedTrame))
            {
                return parsedTrame["Speed"];
            }
            return null;
        }

        /// <summary>
        /// Verifies if the trame corresponds to the format of GT06N data frames
        /// </summary>
        /// <param name="inputTrame">The brute trame supposedly sent by a GT06N box </param>
        /// <returns>true if the format is correct, false otherwise</returns>
        private static bool CheckTrameFormat(string inputTrame)
        {
            if (string.IsNullOrEmpty(inputTrame))
            {
                return false;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(inputTrame))
                {
                    return false;
                }
                else
                {
                    ConcoxTrameType typeOfTrame = GetTrameFormat(inputTrame);
                    if (typeOfTrame != ConcoxTrameType.Unknown)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }
        /// <summary>
        /// Detects the type of the sent frame (Login, LocationData, Alarm, Status)
        /// </summary>
        /// <param name="inputTrame">The iput format</param>
        /// <returns>the type of the data frame</returns>
        private static ConcoxTrameType GetTrameFormat(string inputTrame)
        {
            if (string.IsNullOrWhiteSpace(inputTrame) || string.IsNullOrEmpty(inputTrame))
            {
                return ConcoxTrameType.Unknown;
            }
            else
            {
                Dictionary<string, string> ParsedTrame;
                if (TryParseDataLocationTrame(inputTrame, out ParsedTrame))
                {
                    return ConcoxTrameType.GT800Data;
                }
                else
                    //TODO: complete the rest of cases
                    return ConcoxTrameType.Unknown;
            }
        }

        private static bool TryParseDataLocationTrame(string inputTrame, out Dictionary<string, string> ParsedTrame)
        {
            if (string.IsNullOrEmpty(inputTrame) || inputTrame.Length < DATA_TRAME_LENGHT)
            {
                ParsedTrame = null;
                return false;
            }
            else
            {
                string protocolNumber = GetProtocolNumberStringValue(inputTrame);
                if (protocolNumber != "12" && protocolNumber != "22" && protocolNumber != "16")
                {
                    ParsedTrame = null;
                    return false;
                }
                else
                {
                    ParsedTrame = new Dictionary<string, string>();
                    if (GetTrameStartBit(inputTrame) == "7878")
                    {
                        ParsedTrame.Add("StartBit", "7878");
                    }
                    ParsedTrame.Add("TrameLength", GetTrameLengthStringValue(inputTrame));
                    ParsedTrame.Add("ProtocolNumber", GetProtocolNumberStringValue(inputTrame));
                    ParsedTrame.Add("Date", GetDateStringStringValue(inputTrame));
                    ParsedTrame.Add("SatelliteNumber", GetSatNumberStringValue(inputTrame));
                    ParsedTrame.Add("Latitude", GetLatStringValue(inputTrame));
                    ParsedTrame.Add("West", GetWestStringValue(inputTrame));
                    ParsedTrame.Add("Longitude", GetLongStringValue(inputTrame));
                    ParsedTrame.Add("North", GetNorthStringValue(inputTrame));
                    ParsedTrame.Add("Speed", GetSpeedStringValue(inputTrame));
                    ParsedTrame.Add("Cap", GetCapStringValue(inputTrame));
                    if (protocolNumber == "12")
                        ParsedTrame.Add("Ignition", GetWeTrackIgnitionStringValue(inputTrame));
                    else if (protocolNumber == "22")
                        ParsedTrame.Add("Ignition", GetIgnitionStringValue(inputTrame));

                    if (protocolNumber == "16")
                        ParsedTrame.Add("Alerm", GetWeTrackAlermStringValue(inputTrame));

                    return true;
                }
            }
        }

        private static string GetWeTrackAlermStringValue(string inputTrame)
        {
            try
            {
                return inputTrame.Substring(68, 2);
            }
            catch
            {
                return "";
            }
        }

        private static string GetWeTrackIgnitionStringValue(string inputTrame)
        {
            return "0" + Convert.ToString(Convert.ToInt32(inputTrame.Substring(40, 2), 16), 2).PadLeft(8, '0').Substring(0, 1);
        }

        private static string GetCapStringValue(string inputTrame)
        {
            string N = "000000" + Convert.ToString(Convert.ToInt32(inputTrame.Substring(40, 2), 16), 2).PadLeft(8, '0').Substring(6, 2);
            string M = Convert.ToString(Convert.ToInt32(inputTrame.Substring(42, 2), 16), 2).PadLeft(8, '0');
            return Convert.ToString(Convert.ToInt32(N + M, 2));
        }

        private static string GetIgnitionStringValue(string inputTrame)
        {
            return inputTrame.Substring(61, 1);
        }

        private static string GetSpeedStringValue(string inputTrame)
        {
            return inputTrame.Substring(38, 2);
        }

        private static string GetLongStringValue(string inputTrame)//recuperation latitude a corrigé
        {
            return inputTrame.Substring(30, 8);
        }

        private static string GetNorthStringValue(string inputTrame)
        {
            string N = Convert.ToString(Convert.ToInt32(inputTrame.Substring(40, 2), 16), 2).PadLeft(8, '0');
            //Console.WriteLine("Cap: {0}, North: {1}", N, N.Substring(5, 1));
            return N.Substring(5, 1);
        }

        private static string GetLatStringValue(string inputTrame)//recuperation longitude a corrigé
        {
            return inputTrame.Substring(22, 8);
        }

        private static string GetWestStringValue(string inputTrame)
        {
            string N = Convert.ToString(Convert.ToInt32(inputTrame.Substring(40, 2), 16), 2).PadLeft(8, '0');
            //Console.WriteLine("Cap: {0}, West: {1}", N, N.Substring(4, 1));
            return N.Substring(4, 1);
        }

        private static string GetSatNumberStringValue(string inputTrame)
        {
            return inputTrame.Substring(20, 2);
        }

        private static string GetDateStringStringValue(string inputTrame)
        {
            return inputTrame.Substring(8, 12);
        }

        private static string GetProtocolNumberStringValue(string inputTrame)
        {
            return inputTrame.Substring(6, 2);
        }

        private static string GetTrameLengthStringValue(string inputTrame)
        {
            return inputTrame.Substring(4, 2);
        }

        private static string GetTrameStartBit(string inputTrame)
        {
            //return the first 04 caracters of the input trame string
            return inputTrame.Substring(0, 4);
        }


        public static DataLocationTrame ParseWeTrackDataLocationTrame(string inputTrame)
        {
            DataLocationTrame trame = new DataLocationTrame();

            trame.Date = GetDate(inputTrame);
            trame.Latitude = GetLatitude(inputTrame);
            trame.Longitude = GetLongitude(inputTrame);
            trame.North = GetLongitudeDirection(inputTrame);
            trame.West = GetLatitudeDirection(inputTrame);
            trame.ProtocolNumber = GetProtocolNumber(inputTrame).ToString();
            trame.SatNumber = GetSatelliteNumber(inputTrame);
            trame.Speed = GetSpeedNumber(inputTrame);
            trame.cap = GetCap(inputTrame);
            trame.Ignition = GetWeTrackIgnition(inputTrame);
            trame.Alerm = GetWeTrackAlerm(inputTrame);
            return trame;
        }


        private static short GetWeTrackAlerm(string inputTrame)
        {
            if (string.IsNullOrEmpty(inputTrame))
            {
                //Case of invalid trame format
                return 0;
            }
            else
            {
                //Check if the string provided represents a valid trame sent by the GT06N box
                if (CheckTrameFormat(inputTrame))
                {
                    string alermBrute = GetWeTrackAlermBruteData(inputTrame);
                    short alerm = ParseAlermBruteValue(alermBrute);
                    return alerm;
                }
                else
                {
                    return 0;   //The data frame (trame) is not detected as a valid GT06N data frame }
                }
            }
        }

        private static short GetWeTrackIgnition(string inputTrame)
        {
            if (string.IsNullOrEmpty(inputTrame))
            {
                //Case of invalid trame format
                return 0;
            }
            else
            {
                //Check if the string provided represents a valid trame sent by the GT06N box
                if (CheckTrameFormat(inputTrame))
                {
                    string ignitionBrute = GetWeTrackIgnitionBruteData(inputTrame);
                    short ignition = ParseIgnitionBruteValue(ignitionBrute);
                    return ignition;
                }
                else
                {
                    return 0;   //The data frame (trame) is not detected as a valid GT06N data frame }
                }
            }
        }

        private static string GetWeTrackIgnitionBruteData(string inputTrame)
        {
            Dictionary<string, string> parsedTrame;
            if (TryParseDataLocationTrame(inputTrame, out parsedTrame))
            {
                return parsedTrame["Ignition"];
            }
            return null;
        }

        private static string GetWeTrackAlermBruteData(string inputTrame)
        {
            Dictionary<string, string> parsedTrame;
            if (TryParseDataLocationTrame(inputTrame, out parsedTrame))
            {
                return parsedTrame["Alerm"];
            }
            return null;
        }


    }

}
