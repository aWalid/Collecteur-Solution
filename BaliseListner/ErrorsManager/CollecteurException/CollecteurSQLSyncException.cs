using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaliseListner.CollecteurException
{
    public class CollecteurSQLSyncException : Exception
    {
        public CollecteurSQLSyncException(String cause)
            : base(cause)
        {
                
        }
        public CollecteurSQLSyncException(String cause, Exception innerException)
            : base(cause, innerException)
        {

        }
        public CollecteurSQLSyncException(Exception innerException)
            : base("Erreur dans la Synchronisation des balise avec la base de donnée.",innerException)
        {

        }
    }
}
