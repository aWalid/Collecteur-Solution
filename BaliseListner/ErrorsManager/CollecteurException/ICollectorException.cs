using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaliseListner.CollecteurException
{
    public interface ICollectorException
    {
        //TODO Add other cases
        //TODO Add comments  
        void LoginException();
        void LoginException(string exceptionMessage);

    }
}
