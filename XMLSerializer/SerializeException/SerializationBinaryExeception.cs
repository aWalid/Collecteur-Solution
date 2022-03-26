using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMLSerializer.SerializeException
{
    public class SerializationBinaryExeception : Exception
    {
         public SerializationBinaryExeception(String cause):base(cause)
        {

        }
         public SerializationBinaryExeception(String cause, Exception innerException)
            : base(cause, innerException)
        {

        }
    }
}
