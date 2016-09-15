using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;



namespace wagonMovement
{

    class Program
    {

        public class volumeMovement
        {
            string wagonID;
            string Origin;
            string Via;
            string Destination;
            double weight;
            //bool hasBeenCounted;

        };

        public class wagonDetails
        {
            string wagonID;
            string origin;
            string plannedDestination;
            string destination;
            DateTime attachmentTime;
            DateTime detachmentTime;
            double netWeight;

            // constructor for all relevant fields
            wagonDetails(string[] line)
            {


            }

        };

        static void Main(string[] args)
        {

            string path = @"S:\Corporate Strategy\Market Analysis & Forecasts\Volume\Wagon movement analysis\";
            string file = "2015-16 FY Freight volumes test.txt";
            string filename = path + file;

            /* Structure of the volume data:
             * 0 - Wagon Class
             * 1 - Wagon Number
             * 2 - Train Number
             * 3 - Train Date
             * 4 - Comodity
             * 5 - Origin Code
             * 6 - Planned Destination Code
             * 7 - Actual Destination Code
             * 8 - Attachment Time
             * 9 - Detachment Time
             * 10 - Tare Weight
             * 11 - Gross Weight
             * 12 - Distance Travelled
             * 13 - Wagon Sequence
             * 14 - Record ID
             * 15 - Wagon Movement Count (smae as ID)
             */
            List<wagonDetails> wagon = new List<wagonDetails>();

            /* Read the text file */
            string[] lines = System.IO.File.ReadAllLines(filename);
            char[] delimiters = {','};

            double tareWeight = 0;
            double grossWeight = 0;
            DateTime attachmentTime = new DateTime(2000,1,1);
            DateTime dettachmentTime = new DateTime(2000, 1, 1);

            /* Populate the wagonDetails array */
            foreach (string line in lines)
            {
                string record = line;
                string[] fields = line.Split(delimiters);

                string wagonID = Regex.Unescape(fields[0]) + " " + Regex.Unescape(fields[1]);
                string origin = Regex.Unescape(fields[5]);
                string plannedDestination = Regex.Unescape(fields[6]);
                string destination = Regex.Unescape(fields[7]);
                DateTime.TryParse(fields[8], out attachmentTime);
                DateTime.TryParse(fields[9], out dettachmentTime);
                double.TryParse(fields[11], out tareWeight);
                double.TryParse(fields[10], out grossWeight);
                double netWeight = grossWeight - tareWeight;

                Console.WriteLine(Regex.Unescape(fields[0]));
            }

            
            /* Write back to excel file. */




        }

    }

}
