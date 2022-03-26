using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMLSerializer.SerializeException
{
    public class SerializationXmlConfigExeception : Exception
    {
         public SerializationXmlConfigExeception(String cause):base(cause)
        {

        }
         public SerializationXmlConfigExeception(String cause, Exception innerException)
            : base(cause, innerException)
        {

        }
    }
}
