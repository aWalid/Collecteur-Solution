using Collecteur.Core.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaliseListner.ThreadListener
{
    public class ConnectionManager<T> : List<T>
    {
      /// <summary>
      /// search connection for balise if exist 
      /// </summary>
      /// <param name="balise">balise to search</param>
      /// <returns>true if connection is found, false if not</returns>
      public bool findConnectionByNISBalise(Balise balise)
      {
        int i = 0;
        foreach (T con in this)
        {
            if( balise.Nisbalise.Equals(con.ToString()))
            i++;
              
        }

        return i>=1;
      }

       
    }
}
