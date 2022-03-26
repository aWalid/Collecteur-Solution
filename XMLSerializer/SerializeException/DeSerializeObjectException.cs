using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMLSerializer.SerializeException
{
  public  class DeSerializeObjectException : Exception
    {
        public DeSerializeObjectException(String cause):base(cause)
        {

        }
        public DeSerializeObjectException(String cause,Exception innerException)
            : base(cause, innerException)
        {

        }
    }
}
