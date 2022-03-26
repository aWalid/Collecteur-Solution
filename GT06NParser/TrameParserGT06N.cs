using GT06NParser.Parsers;
using GT06NParser.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT06NParser
{
    public class TrameParserGT06N
    {
        private const int DATA_TRAME_LENGHT = 72;
        private const int LONGCONVERSIONFACTOR = 1800000;
        private const int LATCONVERSIONFACTOR = 1800000;
        

        public GT06NTrame ParseTrame(string inputTrame)
        {
            if(string.IsNullOrEmpty(inputTrame))
            {
                return null;
            }
            else
            {
                //Detect the type of Trame
                TrameType typeOfTrame = GetTrameFormat(inputTrame);
                GT06NTrame extractedTrame = new GT06NTrame();
                if(typeOfTrame == TrameType.Unknown)
                {
                    extractedTrame = ParseOtherTrame(inputTrame);
                    return extractedTrame;
                   
                }
                else
                {
                    
                    switch(typeOfTrame)
                    {
                        case TrameType.Login:
                            {
                                extractedTrame = ParseLoginTrame(inputTrame);
                                return extractedTrame;
                            }
                        case TrameType.Alarm:
                            {
                                extractedTrame = ParseAlarmTrame(inputTrame);
                                return extractedTrame;
                            }
                        case TrameType.Data:
                            {
                                extractedTrame = ParseLocationDataTrame(inputTrame);
                                return extractedTrame;
                            }
                        case TrameType.Status:
                            {
                                extractedTrame = ParseStatusTrame(inputTrame);
                                return extractedTrame;
                            }
                        case TrameType.GPRStatus:
                            {
                                extractedTrame = ParseGPRStatusTrame(inputTrame);
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

        private GT06NTrame ParseStatusTrame(string inputTrame)
        {
            GT06NTrame trame = new GT06NTrame();
            StatusTrame statusTrame = StatusTrameParser.ParseStatusTrame(inputTrame);
            trame.StatusTrame = statusTrame;
            trame.Type = TrameType.Status;
            return trame;
        }

        private GT06NTrame ParseLocationDataTrame(string inputTrame)
        {
            GT06NTrame trame = new GT06NTrame();
            DataLocationTrame dataLocationTrame = DataLocationTrameParser.ParseDataLocationTrame(inputTrame);
            trame.DataLocationTrame = dataLocationTrame;
            trame.Type = TrameType.Data;
            return trame;
        }

        private GT06NTrame ParseAlarmTrame(string inputTrame)
        {
            GT06NTrame trame = new GT06NTrame();
            AlarmTrame alarmTrame = AlarmTrameParser.ParseAlarmTrame(inputTrame);
            trame.AlarmTrame = alarmTrame;
            trame.Type = TrameType.Alarm;
            return trame;
        }

        private GT06NTrame ParseLoginTrame(string inputTrame)
        {
            GT06NTrame trame = new GT06NTrame();
            LoginTrame loginTrame = LoginTrameParser.ParseLoginTrame(inputTrame);
            trame.LoginTrame = loginTrame;
            trame.Type = TrameType.Login;
            return trame;
        }
        private GT06NTrame ParseGPRStatusTrame(string inputTrame)
        {
            GT06NTrame trame = new GT06NTrame();
            GPRStatusTrame gPRStatusTrame = GPRStatusTrameParser.ParseGPRStatusTrame(inputTrame);
            trame.GPRStatusTrame = gPRStatusTrame;
            trame.Type = TrameType.GPRStatus;
            return trame;
        }
        private GT06NTrame ParseOtherTrame(string inputTrame)
        {
            GT06NTrame trame = new GT06NTrame();
            OtherTrame otherTrame = new OtherTrame(GetProtocolNumberStringValue(inputTrame));
            trame.OtherTrame = otherTrame;
            trame.Type = TrameType.Unknown;
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
                //{
                    string protocolNumberBrute = GetProtocolNumberStringValue(inputTrame);
                    int protocolNbr = ParseProtocoleNumberBruteValue(protocolNumberBrute);
                    return protocolNbr;
                //}
                //else
                //{
                //    return 0; //The data frame (trame) is not detected as a valid GT06N data frame }

                //}
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
                    TrameType typeOfTrame = GetTrameFormat(inputTrame);
                    if(typeOfTrame!=TrameType.Unknown)
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
        private TrameType GetTrameFormat(string inputTrame)
        {
            if (string.IsNullOrWhiteSpace(inputTrame) || string.IsNullOrEmpty(inputTrame))
            {
                return TrameType.Unknown;
            }
            else
            {
                int protocol = GetProtocolNumber(inputTrame);
                switch (protocol)
                {
                    case 1:
                        {
                            return TrameType.Login;
                        }
                    case 18:
                        {
                            return TrameType.Data;
                        }
                    case 19:
                        {
                            return TrameType.Status;
                        }
                    case 22:
                        {
                            return TrameType.Alarm;
                        }
                    case 139:
                        {
                            return TrameType.GPRStatus;
                        }
                    default:
                        {
                            return TrameType.Unknown;
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
                if (inputTrame.Length != DATA_TRAME_LENGHT)
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
