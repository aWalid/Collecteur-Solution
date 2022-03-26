using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collecteur.Core.Api
{
    public class Mission
    {
        private DateTime start;

        private DateTime end;

        public String Nivehicule
        {
            get;
            set;
        }
        public int NParc
        {
            get;
            set;
        }
        public String NomMis
        {
            get;
            set;
        }
        bool cloturee
        {
            get;
            set;
        }             

        public DateTime Start
        {
            get { return start; }

        }
        public String balise { get; set; }
       
        public DateTime End
        {
            get { return end; }
            
        }

        public Mission(String balise,DateTime start, DateTime end,String nivehicule,int nparc,String nomMis, bool cloturee)
        {
            this.balise = balise;
            this.start = start;
            this.end = end;
            this.Nivehicule = nivehicule;
            this.NParc = nparc;
            this.NomMis = nomMis;
            this.cloturee = cloturee;
            
        }

      
         public bool Equals(Mission m )
        {
            if(m == null)
                return false;

            return this.balise == m.balise && this.start== m.start;
        }
        

    }
}
