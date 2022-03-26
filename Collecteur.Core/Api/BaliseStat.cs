using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Collecteur.Core.Api
{
    public class BaliseStat
    {
        private Balise Balise;
        public Boolean Connected;
        public DateTime dateTime;
        public String NiSBalise
        {
            get { return Balise.Nisbalise; }

        }
        public BaliseStat(Balise balise, Boolean stat, DateTime dateTime)
        {
            this.Balise = balise;
            this.Connected = stat;
            this.dateTime = dateTime;
        }

    }
}
