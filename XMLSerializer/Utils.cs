using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using XMLSerializer.SerializeException;

namespace XMLSerializer
{
    public static class Utils
    {
        public static T loadXMLtoObject<T>(String path)
        {
            if (path == null || path.Length == 0)
                throw new DeSerializeObjectException("Exception dans la deserialisation d'object pour les reason suivants");
           
            StreamReader flux =null ;
            try {
                    flux = new StreamReader(path);
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    T temp =(T) serializer.Deserialize(flux);  
                    return temp;
            }
            catch (Exception e)
            {
                throw new DeSerializeObjectException("Exception dans la deserialisation d'object pour les reason suivants",e);
            }
            finally
            {
                if (flux != null)
                {
                     flux.Close();
                }
            }
           
          
          
        }
        public static T loadBinarytoObject<T>(String path)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream flux = null;
            if (path == null || path.Length== 0)
                throw   new DeSerializeObjectException("Exception dans la deserialisation d'object pour les reason suivants");
           
            try
            {
                flux = new FileStream(path, FileMode.Open, FileAccess.Read);

                return (T)formatter.Deserialize(flux);
            }
            catch(Exception e)
            {
                throw new DeSerializeObjectException("Exception dans la deserialisation d'object pour les reason suivants", e);
           
            }
            finally
            {
                if (flux != null)
                {
                    flux.Close();
                }
            }

        }

    }
}
