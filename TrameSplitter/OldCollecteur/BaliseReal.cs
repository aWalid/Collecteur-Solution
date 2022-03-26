using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ModelBoitier
{
    class BaliseReal
    {
        private string nsBalise;
        public string NISBalise
        {
            get { return nsBalise; }
            set { nsBalise = value; }
        }

        private TrameReal gprsTrame;
        public TrameReal GPRSTrame
        {
            get { return gprsTrame; }
            set { gprsTrame = value; }
        }

        public BaliseReal(string balise, TrameReal t)
        {
            this.NISBalise = balise;
            this.GPRSTrame = new TrameReal(t);
        }

    }
}
