using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
             * Wagon Class
             * Wagon Number
             * Train Number
             * Train Date
             * Comodity
             * Origin Code
             * Planned Destination Code
             * Actual Destination Code
             * Attachment Time
             * Detachment Time
             * Tare Weight
             * Gross Weight
             * Distance Travelled
             * Wagon Sequence
             * Record ID
             * Wagon Movement Count (smae as ID)
             */
            List<wagonDetails> wagon = new List<wagonDetails>();

            /* Read the text file */
            string[] lines = System.IO.File.ReadAllLines(filename);


            /* Populate the wagonDetails array */
            foreach (string lines in line)
            {
                string wagonID = lines[0] + " " + lines[1];
            }


            /* Write back to excel file. */




        }

    }

}
