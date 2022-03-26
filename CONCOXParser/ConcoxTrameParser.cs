using CONCOXParser.Parsers;
using CONCOXParser.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CONCOXParser
{
    public class ConcoxTrameParser
    {
        private const int DATA_TRAME_LENGHT = 68;
        private const int LONGCONVERSIONFACTOR = 1800000;
        private const int LATCONVERSIONFACTOR = 1800000;
        
        public ConcoxTrame ParseTrame(string inputTrame)
        {
            if(string.IsNullOrEmpty(inputTrame))
            {
                return null;
            }
            else
            {
                //Detect the type of Trame
                ConcoxTrameType typeOfTrame = GetTrameFormat(inputTrame);
                ConcoxTrame extractedTrame = new ConcoxTrame();
                if(typeOfTrame == ConcoxTrameType.Unknown)
                {
                    extractedTrame = ParseOtherTrame(inputTrame);
                    return extractedTrame;
                }
                else
                {
                    switch(typeOfTrame)
                    {
                        case ConcoxTrameType.Login:
                            {
                                extractedTrame = ParseLoginTrame(inputTrame);
                                return extractedTrame;
                            }
                        case ConcoxTrameType.Alarm:
                            {
                                extractedTrame = ParseAlarmTrame(inputTrame);
                                return extractedTrame;
                            }
                        case ConcoxTrameType.WeTrackDataAlarm:
                            {
                                extractedTrame = ParseWeTrackLocationData(inputTrame);
                                return extractedTrame;
                            }
                        case ConcoxTrameType.GT800Data:
                            {
                                extractedTrame = ParseLocationDataTrame(inputTrame);
                                return extractedTrame;
                            }
                        case ConcoxTrameType.WeTrackData:
                            {
                                extractedTrame = ParseWeTrackLocationData(inputTrame);
                                return extractedTrame;
                            }
                        case ConcoxTrameType.Status:
                            {
                                extractedTrame = ParseStatusTrame(inputTrame);
                                return extractedTrame;
                            }
                        case ConcoxTrameType.GPRStatus:
                            {
                                extractedTrame = ParseGPRStatusTrame(inputTrame);
                                return extractedTrame;
                            }
                        case ConcoxTrameType.eTime:
                            {
                                extractedTrame = ParseTimeTrame(inputTrame);
                                return extractedTrame;
                            }
                        default:
                            {
                                extractedTrame = ParseOtherTrame(inputTrame);
                                return extractedTrame;
                            }
                    }
                }
            }
        }

 

        private ConcoxTrame ParseWeTrackLocationData(string inputTrame)
        {
            ConcoxTrame trame = new ConcoxTrame();
            DataLocationTrame dataLocationTrame = DataLocationTrameParser.ParseWeTrackDataLocationTrame(inputTrame);
            trame.DataLocationTrame = dataLocationTrame;
            trame.Type = ConcoxTrameType.WeTrackData;
            return trame;
        }

        private ConcoxTrame ParseTimeTrame(string inputTrame)
        {
            ConcoxTrame trame = new ConcoxTrame();
            eTimeTrame eTimeTrame = new eTimeTrame();
            trame.eTimeCheckTrame = eTimeTrame;
            trame.Type = ConcoxTrameType.eTime;
            return trame;
        }

        private ConcoxTrame ParseStatusTrame(string inputTrame)
        {
            ConcoxTrame trame = new ConcoxTrame();
            StatusTrame statusTrame = StatusTrameParser.ParseStatusTrame(inputTrame);
            trame.StatusTrame = statusTrame;
            trame.Type = ConcoxTrameType.Status;
            return trame;
        }

        private ConcoxTrame ParseLocationDataTrame(string inputTrame)
        {
            ConcoxTrame trame = new ConcoxTrame();
            DataLocationTrame dataLocationTrame = DataLocationTrameParser.ParseDataLocationTrame(inputTrame);
            trame.DataLocationTrame = dataLocationTrame;
            trame.Type = ConcoxTrameType.GT800Data;
            return trame;
        }

        private ConcoxTrame ParseAlarmTrame(string inputTrame)
        {
            ConcoxTrame trame = new ConcoxTrame();
            AlarmTrame alarmTrame = AlarmTrameParser.ParseAlarmTrame(inputTrame);
            trame.AlarmTrame = alarmTrame;
            trame.Type = ConcoxTrameType.Alarm;
            return trame;
        }

 
        private ConcoxTrame ParseLoginTrame(string inputTrame)
        {
            ConcoxTrame trame = new ConcoxTrame();
            LoginTrame loginTrame = LoginTrameParser.ParseLoginTrame(inputTrame);
            trame.LoginTrame = loginTrame;
            trame.Type = ConcoxTrameType.Login;
            return trame;
        }
        private ConcoxTrame ParseGPRStatusTrame(string inputTrame)
        {
            ConcoxTrame trame = new ConcoxTrame();
            GPRStatusTrame gPRStatusTrame = GPRStatusTrameParser.ParseGPRStatusTrame(inputTrame);
            trame.GPRStatusTrame = gPRStatusTrame;
            trame.Type = ConcoxTrameType.GPRStatus;
            return trame;
        }
        private ConcoxTrame ParseOtherTrame(string inputTrame)
        {
            ConcoxTrame trame = new ConcoxTrame();
            OtherTrame otherTrame = new OtherTrame(GetProtocolNumberStringValue(inputTrame));
            trame.OtherTrame = otherTrame;
            trame.Type = ConcoxTrameType.Unknown;
            return trame;
        }
        public decimal GetLongitude(string inputTrame)
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
                    decimal longitude = ParseLongitudeBruteValue(LongitudeBrute);
                    return longitude;
                }
                else
                {
                    return 0; //The data frame (trame) is not detected as a valid GT06N data frame }

                }
            }
        }
        public decimal GetLatitude(string inputTrame)
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
                    string LongitudeBrute = GetLatitudeBruteData(inputTrame);
                    decimal longitude = ParseLatitudeBruteValue(LongitudeBrute);
                    return longitude;
                }
                else
                {
                    return 0; //The data frame (trame) is not detected as a valid GT06N data frame }

                }
            }
        }
        public int GetProtocolNumber(string inputTrame)
        {
            if (string.IsNullOrEmpty(inputTrame))
            {
                //Case of invalid trame format
                return 0;
            }
            else
            {
                //Check if the string provided represents a valid trame sent by the GT06N box
                //if (CheckTrameFormat(inputTrame))
                string protocolNumberBrute = GetProtocolNumberStringValue(inputTrame);
                int protocolNbr = ParseProtocoleNumberBruteValue(protocolNumberBrute);
                return protocolNbr;
            }
        }
        public DateTime? GetDate(string inputTrame)
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
        public string GetSatelliteNumber(string inputTrame)
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
        private DateTime? ParseDateBruteValue(string dateBrute)
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
                DateTime parsedDate = new DateTime(yearsPart, monthsPart, daysPart, hoursPart, minutesPart,secondsPart);
                parsedDate.AddYears(2000);
                return parsedDate;
            }
        }

        private string GetDateStringValue(string inputTrame)
        {
            Dictionary<string, string> parsedTrame = new Dictionary<string, string>();
            if (TryParseDataLocationTrame(inputTrame, out parsedTrame))
            {
                return parsedTrame["Date"];
            }
            return null;
        }

        private int ParseProtocoleNumberBruteValue(string protocolNumberBrute)
        {
            if(string.IsNullOrEmpty(protocolNumberBrute))
            {
                return 0;
            }
            else
            {
                return int.Parse(protocolNumberBrute, System.Globalization.NumberStyles.AllowHexSpecifier);
            }
        }

        private decimal ParseLatitudeBruteValue(string latitudeBrute)
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

        private string GetLatitudeBruteData(string inputTrame)
        {
            Dictionary<string, string> parsedTrame = new Dictionary<string, string>();
            if (TryParseDataLocationTrame(inputTrame, out parsedTrame))
            {
                return parsedTrame["Latitude"];
            }
            return null;
        }
        /// <summary>
        /// Converts a longitude from hexa GT06N format to a decimal positioning format
        /// </summary>
        /// <param name="LongitudeBrute"> Hexa GT06N formated value</param>
        /// <returns>the decimal representation of the longitude </returns>
        private decimal ParseLongitudeBruteValue(string LongitudeBrute)
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
        private string GetLongitudeBruteData(string inputTrame)
        {
            Dictionary<string, string> parsedTrame = new Dictionary<string,string>();
            if (TryParseDataLocationTrame(inputTrame, out parsedTrame))
            {
                return parsedTrame["Longitude"];
            }
            return null;
        }
        /// <summary>
        /// Verifies if the trame corresponds to the format of GT06N data frames
        /// </summary>
        /// <param name="inputTrame">The brute trame supposedly sent by a GT06N box </param>
        /// <returns>true if the format is correct, false otherwise</returns>
        private bool CheckTrameFormat(string inputTrame)
        {
            if(string.IsNullOrEmpty(inputTrame))
            {
                return false;
            }
            else
            {
                if(string.IsNullOrWhiteSpace(inputTrame))
                {
                    return false;
                }
                else
                {
                    ConcoxTrameType typeOfTrame = GetTrameFormat(inputTrame);
                    if(typeOfTrame!=ConcoxTrameType.Unknown)
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
        private ConcoxTrameType GetTrameFormat(string inputTrame)
        {
            if (string.IsNullOrWhiteSpace(inputTrame) || string.IsNullOrEmpty(inputTrame))
            {
                return ConcoxTrameType.Unknown;
            }
            else
            {
                int protocol = GetProtocolNumber(inputTrame);
                switch (protocol)
                {
                    case 1:
                        {
                            return ConcoxTrameType.Login;
                        }
                    case 18:
                        {
                            return ConcoxTrameType.WeTrackData;
                        }
                    case 19:
                        {
                            return ConcoxTrameType.Status;
                        }
                    case 22:
                        {
                            return ConcoxTrameType.WeTrackDataAlarm;
                        }
                    case 34:
                        {
                            return ConcoxTrameType.GT800Data;
                        }
                    case 38:
                        {
                            return ConcoxTrameType.Alarm;
                        }
                    case 138:
                        {
                            return ConcoxTrameType.eTime;
                        }
                    case 139:
                        {
                            return ConcoxTrameType.GPRStatus;
                        }
                    default:
                        {
                            return ConcoxTrameType.Unknown;
                        }
                }
            }
        }

        private bool TryParseDataLocationTrame(string inputTrame, out Dictionary<string, string> ParsedTrame)
        {
            if(string.IsNullOrEmpty(inputTrame))
            {
                ParsedTrame = null;
                return false;
            }
            else
            {
                string protocolNumber = GetProtocolNumberStringValue(inputTrame);
                if (protocolNumber != "12" && protocolNumber != "22")
                {
                    ParsedTrame = null;
                    return false;
                }
                else
                {
                    ParsedTrame = new Dictionary<string, string>();
                    if(GetTrameStartBit(inputTrame) == "7878")
                    {
                        ParsedTrame.Add("StartBit", "7878");
                    }
                    ParsedTrame.Add("TrameLength", GetTrameLengthStringValue(inputTrame));
                    ParsedTrame.Add("ProtocolNumber", GetProtocolNumberStringValue(inputTrame));
                    ParsedTrame.Add("Date", GetDateStringStringValue(inputTrame));
                    ParsedTrame.Add("SatelliteNumber", GetSatNumberStringValue(inputTrame));
                    ParsedTrame.Add("Latitude", GetLatStringValue(inputTrame));
                    ParsedTrame.Add("Longitude", GetLongStringValue(inputTrame));
                    return true;
                }
            }
        }

        private string GetLongStringValue(string inputTrame)
        {
            return inputTrame.Substring(30, 8);
        }

        private string GetLatStringValue(string inputTrame)
        {
            return inputTrame.Substring(22, 8);
        }

        private string GetSatNumberStringValue(string inputTrame)
        {
            return inputTrame.Substring(20, 2);
        }

        private string GetDateStringStringValue(string inputTrame)
        {
            return inputTrame.Substring(8, 12);
        }

        private string GetProtocolNumberStringValue(string inputTrame)
        {
            return inputTrame.Substring(6, 2);
        }

        private string GetTrameLengthStringValue(string inputTrame)
        {
            return inputTrame.Substring(4, 2);
        }

        private string GetTrameStartBit(string inputTrame)
        {
            //return the first 04 caracters of the input trame string
            return inputTrame.Substring(0, 4);
        }
    }
}
