
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using XMLSerializer;

namespace Collecteur.Core.Api
{

    [Serializable]
    public class Trajet : Serialise, ICloneable
    {
        public Trajet(TrameReal trame)
        {
            nisBalise = trame.Balise.Nisbalise;
            distance = 0;
            Vmax = trame.Vitesse;
            dateDebut = trame.Temps;
            duree = 0;
            latitudeDebut = trame.Latitude;
            longitudeDebut = trame.Longitude;
            oldTrame = trame;
        }
        public Trajet()
        {

        }
        public Trajet(Trajet trajet)
        {
            nisBalise = trajet.NisBalise;
            distance = trajet.Distance;
            Vmax = trajet.VMax;
            dateDebut = trajet.DateDebut;
            duree = trajet.Duree;
            latitudeDebut = trajet.LatitudeDebut;
            longitudeDebut = trajet.LongitudeDebut;
            latitudeFin = trajet.LatitudeFin;
            longitudeFin = trajet.LongitudeFin;
            oldTrame = trajet.OLDTrame;
        }
        public void initializeTrajet(TrameReal trame)
        {
            nisBalise = trame.Balise.Nisbalise;
            distance = 0;
            Vmax = trame.Vitesse;
            dateDebut = trame.Temps;
            duree = 0;
            latitudeDebut = trame.Latitude;
            longitudeDebut = trame.Longitude;
            oldTrame = trame;
        }
        private string nisBalise;
        public string NisBalise
        {
            get { return nisBalise; }
            set { nisBalise = value; }
        }
        private double distance;
        public double Distance
        {
            get { return distance; }

        }
        private TrameReal oldTrame;
        public TrameReal OLDTrame
        {
            get { return oldTrame; }
            set { oldTrame = value; }
        }
        private double Vmax;
        public double VMax
        {
            get { return Vmax; }
            set { Vmax = value; }
        }
        private DateTime dateDebut;
        public DateTime DateDebut
        {
            get { return dateDebut; }
            set { dateDebut = value; }
        }
        private int duree;
        public int Duree
        {
            get { return duree; }

        }
        private Decimal latitudeDebut;
        public Decimal LatitudeDebut
        {
            get { return latitudeDebut; }
            set { latitudeDebut = value; }
        }
        private Decimal latitudeFin;
        public Decimal LatitudeFin
        {
            get { return latitudeFin; }
            set { latitudeFin = value; }
        }

        private Decimal longitudeDebut;
        public Decimal LongitudeDebut
        {
            get { return longitudeDebut; }
            set { longitudeDebut = value; }
        }
        private Decimal longitudeFin;
        public Decimal LongitudeFin
        {
            get { return longitudeFin; }
            set { longitudeFin = value; }
        }
        public static double EarthDistanceCalc(double Lat1, double Long1, double Lat2, double Long2)
        {
            double dDistance = Double.MinValue;
            double dLat1InRad = Lat1 * (Math.PI / 180.0);
            double dLong1InRad = Long1 * (Math.PI / 180.0);
            double dLat2InRad = Lat2 * (Math.PI / 180.0);
            double dLong2InRad = Long2 * (Math.PI / 180.0);

            double dLongitude = dLong2InRad - dLong1InRad;
            double dLatitude = dLat2InRad - dLat1InRad;

            // Intermediate result a.
            double a = Math.Pow(Math.Sin(dLatitude / 2.0), 2.0) +
            Math.Cos(dLat1InRad) * Math.Cos(dLat2InRad) *
            Math.Pow(Math.Sin(dLongitude / 2.0), 2.0);

            // Intermediate result c (great circle distance in Radians).
            double c = 2.0 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1.0 - a));

            // Distance.
            // const Double kEarthRadiusMiles = 3956.0;
            const Double kEarthRadiusKms = 6376.5;
            dDistance = kEarthRadiusKms * c;

            // resultat en m&#232;tres
            return dDistance * 1000;
        }
        public bool UpdateTrajet(TrameReal trame, ref TrameReal added)
        {
            if ((trame.Temps - oldTrame.Temps).TotalMinutes <= 0)
                return false;

            if ((trame.Temps - oldTrame.Temps).TotalMinutes >= 5)
            {
                // log à supprimer
                Logging("fin trajet");
                return true;
            }
            double distancetemp = EarthDistanceCalc((double)oldTrame.Latitude, (double)oldTrame.Longitude, (double)trame.Latitude, (double)trame.Longitude);
            if (distancetemp < 10 && trame.Vitesse < 10)
            {
                if (duree == 0)
                {
                    dateDebut = trame.Temps;
                }
                return false;
            }

            distance += distancetemp;
            Vmax = trame.Vitesse > Vmax ? trame.Vitesse : Vmax;

            latitudeFin = trame.Latitude;
            longitudeFin = trame.Longitude;
            // Log à supprimer
            Logging("update Trajet distance ajouté : " + distancetemp);
            if (duree == 0 && (trame.Temps - oldTrame.Temps).TotalMinutes >= 2)
            {
                added = new TrameReal(oldTrame);
                added.Temps = trame.Temps.AddMinutes(-1);
                added.TempsReception = new DateTime(1970, 1, 1, 0, 0, 0);
                dateDebut = added.Temps;
                duree = 60;
            }
            else
            {
                duree = (int)(trame.Temps - dateDebut).TotalSeconds;
            }

            oldTrame = trame;

            return false;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        private void Logging(String message)
        {

            String path = "Log\\Trajets\\" + DateTime.Now.ToString("dd-MM-yyyy");
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                using (StreamWriter sw = File.AppendText(path + "\\Balises-" + nisBalise + ".log"))
                {
                    sw.WriteLine(DateTime.Now + ": " + message);
                    sw.WriteLine("Temps Debut : " + this.dateDebut);
                    sw.WriteLine("duree : " + this.duree);
                    sw.WriteLine("distance : " + this.distance);
                    sw.Close();

                }
            }
            catch (Exception)
            {

            }

        }
    }
}
