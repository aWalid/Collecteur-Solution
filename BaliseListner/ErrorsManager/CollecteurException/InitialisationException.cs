using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaliseListner.CollecteurException
{
    public  class InitialisationException : System.Exception
    {
        public InitialisationException(String cause)
            : base(cause)
        {
                
        }

    }
}
