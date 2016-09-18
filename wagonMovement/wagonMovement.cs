using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

Microsoft.Office.Interop.Excel.Application oXL;
Microsoft.Office.Interop.Excel._Workbook oWB;
Microsoft.Office.Interop.Excel._Worksheet oSheet;
Microsoft.Office.Interop.Excel.Range oRng;
object misvalue = System.Reflection.Missing.Value;


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
            public wagonDetails(wagonDetails wagon)
            {
                this.wagonID = wagon.wagonID;
                this.origin = wagon.origin;
                this.plannedDestination = wagon.plannedDestination;
                this.destination = wagon.destination;
                this.attachmentTime = wagon.attachmentTime;
                this.detachmentTime = wagon.detachmentTime;
                this.netWeight = wagon.netWeight;
            }
            
            public wagonDetails(string wagonID, string origin, string plannedDestination, string destination, double netWeight)
            {
                this.wagonID = wagonID;
                this.origin = origin;
                this.plannedDestination = plannedDestination;
                this.destination = destination;
                this.attachmentTime = new DateTime(2000, 1, 1);
                this.detachmentTime = new DateTime(2000, 1, 1);
                this.netWeight = netWeight;
            }

            public wagonDetails(string wagonID, string origin, string plannedDestination, string destination, DateTime attachmentTime, DateTime detachmentTime, double netWeight)
            {
                this.wagonID = wagonID;
                this.origin = origin;
                this.plannedDestination = plannedDestination;
                this.destination = destination;
                this.attachmentTime = attachmentTime;
                this.detachmentTime = detachmentTime;
                this.netWeight = netWeight;
            }
            
        }
        

        static void Main(string[] args)
        {

            // Obtain file from dialogue.

            //string path = @"S:\Corporate Strategy\Market Analysis & Forecasts\Volume\Wagon movement analysis\";
            //string path = @"C:\Users\bbel1\Documents";
            string path = @"C:\Users\Beau\Documents\ARTC\Wagon Volumes";      // Home file location.
            string file = @"\2015-16 FY Freight volumes test.txt";
            string filename = path + file;

            // This will be redundant when the file is obtained from the dialogu box.
            if (!File.Exists(filename))
            {
                Console.WriteLine(filename+" does not exist. Check spelling and location.");
                return;
            }

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
            //wagonDetails wagonData = new wagonDetails();

            
            Microsoft.Office.Interop.Excel.Application oXL;
            //Microsoft.Office.Interop.Excel._Workbook oWB;
            //Microsoft.Office.Interop.Excel._Worksheet oSheet;
            //Microsoft.Office.Interop.Excel.Range oRng;
            //object misvalue = System.Reflection.Missing.Value;


            /* Read the text file */
            string[] lines = System.IO.File.ReadAllLines(filename);
            char[] delimiters = {','};

            double tareWeight = 0;
            double grossWeight = 0;
            DateTime attachmentTime = new DateTime(2000,1,1);
            DateTime detachmentTime = new DateTime(2000, 1, 1);

            /* Populate the wagonDetails array */
            foreach (string line in lines)
            {
                
                //string record = line;
                string[] fields = line.Split(delimiters);
                char [] newDelimeters = {'\'','"'};

                string[] field0 = Regex.Unescape(fields[0]).Split(newDelimeters);
                string[] field1 = Regex.Unescape(fields[1]).Split(newDelimeters);
                
                string wagonID = "";
                string origin = "";
                string plannedDestination = "";
                string destination = "";
                double netWeight = 0;

                if (field0.Count() == 3 && field1.Count() == 1)
                    wagonID = field0[1] + "-" + field1[0];
                else
                    Console.WriteLine("Wagon ID configuration has not been accounted for.");

                string[] field = Regex.Unescape(fields[5]).Split(newDelimeters);

                if (field.Count() == 3)
                    origin = field[1];
                else
                    Console.WriteLine("Origin configuration has not been accounted for.");

                field = Regex.Unescape(fields[6]).Split(newDelimeters);

                if (field.Count() == 3)
                    plannedDestination = field[1];
                else
                    Console.WriteLine("plannedDestination configuration has not been accounted for.");

                field = Regex.Unescape(fields[7]).Split(newDelimeters);

                if (field.Count() == 3)
                    destination = field[1];
                else
                    Console.WriteLine("destination configuration has not been accounted for.");

                DateTime.TryParse(fields[8], out attachmentTime);
                DateTime.TryParse(fields[9], out detachmentTime);
                double.TryParse(fields[10], out tareWeight);
                double.TryParse(fields[11], out grossWeight);
                netWeight = grossWeight - tareWeight;

                wagonDetails data = new wagonDetails(wagonID, origin, plannedDestination, destination, attachmentTime, detachmentTime, netWeight);
                wagon.Add(data);

            }

            
            /* Write back to excel file. */




        }

    }

}
