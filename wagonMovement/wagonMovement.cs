using System;
using System.IO;
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
            
            public string wagonID;
            public string origin;
            public string plannedDestination;
            public string destination;
            public DateTime attachmentTime;
            public DateTime detachmentTime;
            public double netWeight;
            
            /* Constructor for all relevant fields */
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

            // This will be redundant when the file is obtained from the dialogue box.
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
            
            // Create a wagon object list from the wagon movement text file.
            wagon = readWagonDataFile(filename);


            /*
             * wagon movement algorithms ...
             */


            // Write the wagon details to excel.
            writeToExcel(wagon);
            

        }
        
        /*
         * Read the wagon movement data from the text file.
         */
        public static List<wagonDetails> readWagonDataFile(string filename)
        {
        
            /* Read the all lines of the text file. */
            string[] lines = System.IO.File.ReadAllLines(filename);
            char[] delimiters = {','};

            double tareWeight = 0;
            double grossWeight = 0;
            DateTime attachmentTime = new DateTime(2000,1,1);
            DateTime detachmentTime = new DateTime(2000, 1, 1);

            /* Create the list of wagon objects. */
            List<wagonDetails> wagon = new List<wagonDetails>();

            /* Extract the wagon details from the data file. */
            foreach (string line in lines)
            {
                
                string wagonID = "";
                string origin = "";
                string plannedDestination = "";
                string destination = "";
                double netWeight = 0;

                /* Split the lines into the fields */
                string[] fields = line.Split(delimiters);
                char [] newDelimeters = {'\'','"'};

                /* Check the number of fields */
                if (fields.Count() != 16)
                {
                    Console.WriteLine("Incorrect number of fields detected in text file.");
                    Console.WriteLine("Press enter to close...");
                    Console.ReadLine();
                    // Return empty wagon list.
                    return wagon;       
                }

                /* Clean the wagon ID fields. */
                string[] field0 = Regex.Unescape(fields[0]).Split(newDelimeters);
                string[] field1 = Regex.Unescape(fields[1]).Split(newDelimeters);
                
                /*
                 * Extract the cleaned fields and populate the wagon detail arrays.
                 */
                /* Wagon ID. */
                if (field0.Count() == 3 && field1.Count() == 1)
                    wagonID = field0[1] + "-" + field1[0];
                else
                    Console.WriteLine("Wagon ID configuration has not been accounted for.");

                /* Wagon Origin. */
                string[] field = Regex.Unescape(fields[5]).Split(newDelimeters);
                if (field.Count() == 3)
                    origin = field[1];
                else
                    Console.WriteLine("Origin configuration has not been accounted for.");

                /* Wagon planned destination. */
                field = Regex.Unescape(fields[6]).Split(newDelimeters);
                if (field.Count() == 3)
                    plannedDestination = field[1];
                else
                    Console.WriteLine("plannedDestination configuration has not been accounted for.");

                /* Wagon destination. */
                field = Regex.Unescape(fields[7]).Split(newDelimeters);
                if (field.Count() == 3)
                    destination = field[1];
                else
                    Console.WriteLine("destination configuration has not been accounted for.");

                /* Remaining Wagon details. */
                DateTime.TryParse(fields[8], out attachmentTime);
                DateTime.TryParse(fields[9], out detachmentTime);
                double.TryParse(fields[10], out tareWeight);
                double.TryParse(fields[11], out grossWeight);
                netWeight = grossWeight - tareWeight;

                // TODO:
                // Clean the data - validate - function.
                // Check against known issues

                /* Construct the wagon object and add to the list. */
                wagonDetails data = new wagonDetails(wagonID, origin, plannedDestination, destination, attachmentTime, detachmentTime, netWeight);
                wagon.Add(data);

            }
            /* Return the completed wagon List. */
            return wagon;
        }

        /*
         * Write the wagon movement details to excel for analysis.
         */
        public static void writeToExcel(List<wagonDetails> wagon)
        {
            /* May need the PIA for Excel references.
             * https://msdn.microsoft.com/en-us/library/kh3965hw(v=vs.100).aspx
             */
            /* Create the microsfot excel references. */
            Microsoft.Office.Interop.Excel.Application excel;
            Microsoft.Office.Interop.Excel._Workbook workbook;
            Microsoft.Office.Interop.Excel._Worksheet worksheet;

            /*Start Excel and get Application object. */
            excel = new Microsoft.Office.Interop.Excel.Application();

            /* Get the reference to the new workbook. */
            workbook = (Microsoft.Office.Interop.Excel._Workbook)(excel.Workbooks.Add(""));
            worksheet = (Microsoft.Office.Interop.Excel._Worksheet)workbook.ActiveSheet;

            /* Create the header details. */
            string[] header = { "Wagon ID", "Origin", "Planned Destiantion", "Destination", "Attatchment Time", "Detatchment Time", "Net Weight" };
            worksheet.get_Range("A1", "G1").Value2 = header;

            /* Deconstruct the wagon details into excel columns. */
            string[,] ID = new string[wagon.Count, 1];
            string[,] Orig = new string[wagon.Count, 1];
            string[,] Planned = new string[wagon.Count, 1];
            string[,] Dest = new string[wagon.Count, 1];
            DateTime[,] attatch = new DateTime[wagon.Count, 1];
            DateTime[,] detatch = new DateTime[wagon.Count, 1];
            double[,] weight = new double[wagon.Count, 1];

            /* Loop through the wagon list to deconstruct the data. */
            for (int i = 0; i < wagon.Count; i++)
            {
                ID[i, 0] = wagon[i].wagonID;
                Orig[i, 0] = wagon[i].origin;
                Planned[i, 0] = wagon[i].plannedDestination;
                Dest[i, 0] = wagon[i].destination;
                attatch[i, 0] = wagon[i].attachmentTime;
                detatch[i, 0] = wagon[i].detachmentTime;
                weight[i, 0] = wagon[i].netWeight;

            }

            /* Write the wagon data to the excel file. */
            worksheet.get_Range("A2", "A" + wagon.Count).Value2 = ID;
            worksheet.get_Range("B2", "B" + wagon.Count).Value2 = Orig;
            worksheet.get_Range("C2", "C" + wagon.Count).Value2 = Planned;
            worksheet.get_Range("D2", "D" + wagon.Count).Value2 = Dest;
            worksheet.get_Range("E2", "E" + wagon.Count).Value2 = attatch;
            worksheet.get_Range("F2", "F" + wagon.Count).Value2 = detatch;
            worksheet.get_Range("G2", "G" + wagon.Count).Value2 = weight;
            
            string savePath = @"C:\Users\Beau\Documents\ARTC\Wagon Volumes";    // home path
            string saveFilename = savePath + @"\wagonDetails.xlsx";

            /* Check the file does not exist yet. */
            if (File.Exists(saveFilename))
                File.Delete(saveFilename);

            /* Save the excel file. */
            excel.UserControl = false;
            workbook.SaveAs(saveFilename, Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookDefault, Type.Missing, Type.Missing,
                false, false, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlNoChange,
                Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);

            workbook.Close();
            return;
        }

    }

}
