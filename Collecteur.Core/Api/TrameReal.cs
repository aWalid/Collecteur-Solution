
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using XMLSerializer;

namespace Collecteur.Core.Api
{
    [Serializable]
    public class TrameReal : Serialise //, IEquatable<TrameReal>, IComparable<TrameReal>
    {

        public TrameReal()
        {

        }
        [XmlIgnore]
        public Mission Mission
        {
            get;
            set;
        }

        private OptionsAux options;


        private DateTime temps;
        public DateTime Temps
        {
            get { return temps; }
            set { temps = value; }
        }
        private DateTime tempsReception;
        public DateTime TempsReception
        {
            get { return tempsReception; }
            set { tempsReception = value; }
        }
        [XmlIgnore]
        public OptionsAux Options
        {
            get { return options; }
            set { options = value; }
        }

        private Decimal latitude;
        public Decimal Latitude
        {
            get { return latitude; }
            set { latitude = value; }
        }

        private Decimal longitude;
        public Decimal Longitude
        {
            get { return longitude; }
            set { longitude = value; }
        }

        private Balise balise;
        public Balise Balise
        {
            get { return balise; }
            set { balise = value; }
        }

        public String NisBalise
        {
            get { return Balise.Nisbalise; }
        }

        private double vitesse;
        public double Vitesse
        {
            get { return vitesse; }
            set { vitesse = value; }
        }

        private Int16 temperature;
        public Int16 Temperature
        {
            get { return temperature; }
            set { temperature = value; }
        }

        private string capteur;
        public string Capteur
        {
            get { return capteur; }
            set { capteur = value; }
        }

        private Int16 direction;
        public Int16 Direction
        {
            get { return direction; }
            set { direction = value; }
        }

        private string chauffeur;

        public string Chauffeur
        {
            get { return chauffeur; }
            set { chauffeur = value; }
        }

        private string trailerID = string.Empty;

        public string TrailerID
        {
            get { return trailerID; }
            set { trailerID = value; }
        }


        public TrameReal(Balise balise, DateTime d, decimal lat, decimal lng, double v, Int16 t, string c, Int16 cap, OptionsAux opts, DateTime receptionDateTime)
        {
            Balise = balise;
            Temps = d;
            Latitude = lat;
            Longitude = lng;
            Vitesse = v;
            Temperature = t;
            Capteur = c;
            Direction = cap;
            Chauffeur = String.Empty;
            TrailerID = "";
            options = opts;
            this.tempsReception = receptionDateTime;
        }

        public TrameReal(Balise balise, DateTime d, decimal lat, decimal lng, double v, Int16 t, string c, Int16 cap, string ID, OptionsAux opts, DateTime receptionDateTime)
        {
            Balise = balise;
            Temps = d;
            Latitude = lat;
            Longitude = lng;
            Vitesse = v;
            Temperature = t;
            Capteur = c;
            Direction = cap;
            Chauffeur = String.Empty;
            TrailerID = ID;
            options = opts;
            this.tempsReception = receptionDateTime;
        }

        public TrameReal(TrameReal t)
        {
            Balise = t.Balise;
            Temps = t.Temps;
            Latitude = t.Latitude;
            Longitude = t.Longitude;
            Vitesse = t.vitesse;
            Temperature = t.Temperature;
            Capteur = t.Capteur;
            Direction = t.Direction;
            Chauffeur = t.Chauffeur;
            TrailerID = t.TrailerID;
            this.tempsReception = t.TempsReception;
        }


        //public TrameReal(string boitier, string TempsReel, decimal LatitudeReel, decimal LongitudeReel, decimal vitesseReel, short Temperature1, string Capteur1, short directionReel)
        //{
        //    // TODO: Complete member initialization
        //    NisBalise = boitier;
        //    Temps = TempsReel;
        //    Latitude = LatitudeReel;
        //    Longitude = LongitudeReel;
        //    Vitesse = vitesseReel;
        //    Temperature = Temperature1;
        //    Capteur = Capteur1;
        //    Direction = directionReel;
        //}
        public override string ToString()
        {

            XmlSerializer serializer = new XmlSerializer(this.GetType());

            MemoryStream ms = new MemoryStream();
            XmlWriterSettings ws = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = " ",
                OmitXmlDeclaration = true,
                Encoding = new UTF8Encoding(false),
            };

            XmlWriter w = XmlWriter.Create(ms, ws);
            serializer.Serialize(w, this);
            w.Flush();
            return Encoding.UTF8.GetString(ms.ToArray());

        }

    }

}


