using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMLSerializer;

namespace Collecteur.Core.Api
{
    [Serializable]
    public class Trame : Serialise
    {
        String trameValue;

        private DateTime dateTime;

        private DateTime receptionDateTime;

        public DateTime ReceptionDateTime
        {
            get { return receptionDateTime; }

        }
        public Balise balise { get; set; }
        public String NisBalise
        {
            get { return balise.Nisbalise; }

        }
        public DateTime Datetime
        {
            get { return dateTime; }
            set { dateTime = value; }
        }

        public String TrameValue
        {
            get { return trameValue; }
            set { trameValue = value; }
        }
        public Trame(Balise Balise, String trame, DateTime receptionDateTime)
        {
            this.balise = Balise;
            this.trameValue = trame;
            this.receptionDateTime = receptionDateTime;
        }
        public Trame()
        {
           
        }
        public String toSQL(){
            String insertRequest = "INSERT INTO [dbo].[T_Depot] ([trameBrute],[NISBalise],[gpsDate]) VALUES ";
            bool first = true;
                foreach (String unitTrame in 
                    this.trameValue.Split(this.balise.baliseInfo.trameSeparator, 
                    System.StringSplitOptions.RemoveEmptyEntries))
                {
                    if (unitTrame.Trim() == "#")
                        continue;
                    insertRequest += (first) ? ("") : ",";
                    first = false;
                    insertRequest += "('"+unitTrame+"','" + this.balise.Nisbalise + "', GETDATE() )";
                }
                return insertRequest;
        }
    }
}
