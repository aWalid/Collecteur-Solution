using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModelBoitier
{
    public class TrameReal
    {
        private string nisBalise;
        public string NisBalise
        {
            get { return nisBalise; }
            set { nisBalise = value; }
        }

        private DateTime temps;
        public DateTime Temps
        {
          get { return temps; }
          set { temps = value; }
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
        private TrameReal t;
        public string Chauffeur
        {
          get { return chauffeur; }
          set { chauffeur = value; }
        }

        public TrameReal(string b, DateTime d, decimal lat, decimal lng, double v, Int16 t, string c, Int16 cap)
        {
            NisBalise = b;
            Temps = d;
            Latitude = lat;
            Longitude = lng;
            Vitesse = v;
            Temperature = t;
            Capteur = c;
            Direction = cap;
            Chauffeur = String.Empty;
        }

        public TrameReal(TrameReal t)
        {
            NisBalise = t.NisBalise;
            Temps = t.Temps;
            Latitude = t.Latitude;
            Longitude = t.Longitude;
            Vitesse = t.vitesse;
            Temperature = t.Temperature;
            Capteur = t.Capteur;
            Direction = t.Direction;
            Chauffeur = t.Chauffeur;
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

    }

}
