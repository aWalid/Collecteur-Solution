using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaliseListner.Collection
{
   public static  class CollectionUtils
    {

       public static List<T> remove<T>(T obj, List<T> listObj)
       { 
           T objtoRemove=default(T);
           foreach(T objec in listObj){
               if (obj.Equals(objec))
                   objtoRemove = objec;
           }
           if (objtoRemove != null)
               listObj.Remove(objtoRemove);

           return listObj;
       }
    }
}
