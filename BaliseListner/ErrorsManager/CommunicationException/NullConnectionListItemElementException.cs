using BaliseListner.CollecteurException;
using BaliseListner.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BaliseListner.ErrorsManager
{
    /// <summary>
    /// class representing the exception for the connections elements access (when null reference)
    /// </summary>
    public class NullConnectionListItemElementException:  Exception,ICollectorException
    {
        /// <summary>
        /// Login of simple exception (get the message and type from inner exception)
        /// </summary>
        public void LoginException()
        {
            DataBase.Logging(string.Format("Exception de Type: {0}", Properties.Resources.NullConnectionListItemElementExceptionName), this.Message);
        }
        /// <summary>
        /// Login of simple exception (get the exception message from a given parameter)
        /// </summary>
        /// <param name="exceptionMessage">Custom Exception message to save (login)</param>
        public void LoginException(string exceptionMessage)
        {
            DataBase.Logging(string.Format("Exception de Type: {0}", Properties.Resources.NullConnectionListItemElementExceptionName), exceptionMessage);
        }

    }
}
