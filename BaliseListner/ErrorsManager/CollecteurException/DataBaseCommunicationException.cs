using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaliseListner.CollecteurException
{
    class DataBaseCommunicationException :Exception
    {
        public DataBaseCommunicationException(String cause)
            : base(cause)
        {
                
        }
        public DataBaseCommunicationException(String cause, Exception innerException)
            : base(cause, innerException)
        {

        }
        public DataBaseCommunicationException(Exception innerException)
            : base("Erreur dans la Communication avec la base de donnée.",innerException)
        {

        }
    }
}
