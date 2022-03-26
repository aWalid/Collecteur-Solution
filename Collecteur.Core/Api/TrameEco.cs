
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
    public class TrameEco : Serialise //, IEquatable<TrameReal>, IComparable<TrameReal>
    {

        public TrameEco()
        {

        }


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


        private string nisbalise;
        public String NisBalise
        {
            get { return nisbalise; }
            set { nisbalise = value; }
        }

        private Int32 vitesse;
        public Int32 Vitesse
        {
            get { return vitesse; }
            set { vitesse = value; }
        }



        private byte moteur;
        public byte Moteur
        {
            get { return moteur; }
            set { moteur = value; }
        }

        private Int16 direction;
        public Int16 Direction
        {
            get { return direction; }
            set { direction = value; }
        }


        private Int16 rpm;
        public Int16 RPM
        {
            get { return rpm; }
            set { rpm = value; }
        }



        private Int16 tcoolant;
        public Int16 TCoolant
        {
            get { return tcoolant; }
            set { tcoolant = value; }
        }

        private Int16? gx;
        public Int16? Gx
        {
            get { return gx; }
            set { gx = value; }
        }

        private Int16? gy;
        public Int16? Gy
        {
            get { return gy; }
            set { gy = value; }
        }

        private Int16? gz;
        public Int16? Gz
        {
            get { return gz; }
            set { gz = value; }
        }

        private Int16? ccarb;
        public Int16? CCarb
        {
            get { return ccarb; }
            set { ccarb = value; }
        }
        public TrameEco(DateTime temps, Int16 direction, Int32 vitesse, string nisbalise, byte moteur, Int16 rpm, Int16 temperature, Int16? gx, Int16? gy, Int16? gz , Int16? ccarb)
        {
            Temps = temps;
            Vitesse = vitesse;
            Direction = direction;
            NisBalise = nisbalise;
            Moteur = moteur;
            RPM = rpm;
            TCoolant = temperature;
            Gx = gx;
            Gy = gy;
            Gz = gz;
            CCarb = ccarb;
        }

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


