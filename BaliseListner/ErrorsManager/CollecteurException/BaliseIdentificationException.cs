using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaliseListner.CollecteurException
{
    class BaliseIdentificationException :Exception
    {
        public BaliseIdentificationException(String cause)
            : base(cause)
        {
                
        }
        public BaliseIdentificationException(Exception innerException)
            : base("Erreur de recuperation des matricules du boitie",innerException)
        {

        }
        
        public BaliseIdentificationException(String cause,Exception innerException)
            : base("Erreur de recuperation des matricules du boitie cause :" + cause, innerException)
        {

        }
    }
}
