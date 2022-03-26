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
     [Serializable]
    public abstract class Serialise 
    {
        protected Serialise()
        {
          
        }


        public  void saveXML(string path)
        {
            try { 
            StreamWriter ecrivain = new StreamWriter(path);
            
            XmlSerializer serializer = new XmlSerializer(this.GetType());
            serializer.Serialize(ecrivain, this);
            ecrivain.Close();
            }catch(Exception e){
                throw new SerializationXmlConfigExeception(e.Message, e);
            }
        }

        public void saveBinary(string path)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream flux = null;
            try
            {
                flux = new FileStream(path, FileMode.Create, FileAccess.Write);
                formatter.Serialize(flux, this);
                flux.Flush();
            }
            catch (Exception e)
            {
                throw new SerializationBinaryExeception(e.Message, e);
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
