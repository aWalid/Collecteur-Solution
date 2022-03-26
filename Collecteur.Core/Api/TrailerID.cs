using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Collecteur.Core.Api
{
    public  class TrailerID
    {
        private DateTime dateTime;

        public DateTime Datetime
        {
            get { return dateTime; }
            set { dateTime = value; }
        }

        private string balise;
        public string Balise
        {
            get { return balise; }
            set { balise = value; }
        }
                      
        private string idTrailer;
        public string IDTrailer
        {
            get { return idTrailer; }
            set { idTrailer = value; }
        }

        private string lastID;
        public string LastID
        {
            get { return lastID; }
            set { lastID = value; }
        }
        
        public TrailerID(string boitier, DateTime date, string trailerID, string lastid )
        {
            this.Balise = boitier;
            this.IDTrailer = trailerID;
            this.Datetime = date;
            this.LastID = lastid == string.Empty ? "0" : lastid;
        }

        public TrailerID(TrailerID id)
        {
            this.Balise = id.Balise;
            this.IDTrailer = id.IDTrailer;
            this.Datetime = id.Datetime;
            this.LastID = id.LastID == null ? "0" : id.LastID;
        }

        public Byte isNewID()
        {
            if (LastID == "0")
                return 2;
            else if (!LastID.Equals(IDTrailer, StringComparison.OrdinalIgnoreCase))
                return 1;
            else
                return 0;
        }
    }
}