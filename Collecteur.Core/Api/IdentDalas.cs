using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Collecteur.Core.Api
{
   public  class IdentDalas
    {
        private DateTime dateTime;

        private Balise balise;
        public DateTime Datetime
        {
            get { return dateTime; }
            set { dateTime = value; }
        }
        public String Balise
        {
            get { return balise.Nisbalise; }
            set { balise.Nisbalise = value; }
        }
       private String trameValue;
        public String TrameValue
        {
            get { return trameValue; }
            set { trameValue = value; }
        }
        private String dalasKey;
        public String DalasKey
        {
            get { return dalasKey; }
            set { dalasKey = value; }
        }
        public IdentDalas(Balise Balise, String trame)
        {
            this.balise = Balise;
            this.trameValue = trame;
        }

        public IdentDalas(Balise Balise, DateTime date,String dalasKey)
        {
            this.balise = Balise;
            this.dalasKey = dalasKey;
            this.dateTime = date;
        }


    }
}
