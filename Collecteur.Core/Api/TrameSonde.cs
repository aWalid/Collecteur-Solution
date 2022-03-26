
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
    public class TrameSonde : Serialise //, IEquatable<TrameReal>, IComparable<TrameReal>
    {





        public TrameSonde()
        {

        }


        private DateTime temps;
        public DateTime Temps
        {
            get { return temps; }
            set { temps = value; }
        }
  
        private string nisbalise;
        public String NisBalise
        {
            get { return nisbalise; }
            set { nisbalise = value; }
        }

        private Int32? volume1;
        public Int32? Volume1
        {
            get { return volume1; }
            set { volume1 = value; }
        }
        private Int16? temperature1;
        public Int16? Temperature1
        {
            get { return temperature1; }
            set { temperature1 = value; }
        }
        private Int32? volume2;
        public Int32? Volume2
        {
            get { return volume2; }
            set { volume2 = value; }
        }
        private Int16? temperature2;
        public Int16? Temperature2
        {
            get { return temperature2; }
            set { temperature2 = value; }
        }
        private Int32? volume3;
        public Int32? Volume3
        {
            get { return volume3; }
            set { volume3 = value; }
        }
        private Int16? temperature3;
        public Int16? Temperature3
        {
            get { return temperature3; }
            set { temperature3 = value; }
        }
        private Int32? volume4;
        public Int32? Volume4
        {
            get { return volume4; }
            set { volume4 = value; }
        }
        private Int16? temperature4;
        public Int16? Temperature4
        {
            get { return temperature4; }
            set { temperature4 = value; }
        }

        private Int32 witesse;
        public Int32 Witesse
        {
            get { return witesse; }
            set { witesse = value; }
        }

        private byte moteur;
        public byte Moteur
        {
            get { return moteur; }
            set { moteur = value; }
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


        public TrameSonde(
            string nisbalise,
            DateTime temps,
            Int16? temperature1,
            Int16? volume1,
            Int16? temperature2,
            Int16? volume2,
            Int16? temperature,
            Int16? volume3,
            Int16? temperature4,
            Int16? volume4,
            Int32 witesse,
            byte moteur,
            decimal longitude,
            decimal latitude
            )
        {
            Temps = temps;
            NisBalise = nisbalise;
            Temperature1 = temperature1;
            Volume1 = volume1;
            Temperature2 = temperature2;
            Volume2 = volume2;
            Temperature3 = temperature3;
            Volume3 = volume3;
            Temperature4 = temperature4;
            Volume4 = volume4;

            Witesse = witesse;


            Moteur = moteur;

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


