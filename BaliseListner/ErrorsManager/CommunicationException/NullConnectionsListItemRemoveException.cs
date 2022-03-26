using BaliseListner.CollecteurException;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaliseListner.DataAccess;

namespace BaliseListner.ErrorsManager
{

    /// <summary>
    /// class representing the exception for the remove item access (when null reference)
    /// </summary>
    public class NullConnectionsListItemRemoveException : Exception, ICollectorException
    {

        /// <summary>
        /// Login of simple exception (get the message and type from inner exception)
        /// </summary>
        public void LoginException()
        {
            DataBase.Logging(string.Format("Exception de Type: {0}", Properties.Resources.NullConnectionsListItemRemoveExceptionName), this.Message);
        }
        /// <summary>
        /// Login of simple exception (get the exception message from a given parameter)
        /// </summary>
        /// <param name="exceptionMessage">Custom Exception message to save (login)</param>
        public void LoginException(string exceptionMessage)
        {
            DataBase.Logging(string.Format("Exception de Type: {0}", Properties.Resources.NullConnectionsListItemRemoveExceptionName), exceptionMessage);
        }
    }
}
