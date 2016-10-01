/*
 * Wagon Movement class
 * 
 */


//----------------------------------------
// Uncomment to turn logging on 
//#define LOGGING 
//---------------------------------------
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace wagonMovement
{

    class Program
    {

        /// <summary>
        /// A class to hold the journey characterstics of the volume.
        /// </summary>
        public class volumeMovement
        {
            public string wagonID;
            public string Origin;
            public string Via;
            public string Destination;
            public double weight;
            public bool hasBeenCounted;
           
            /// <summary>
            /// Default volumeMovement constructor
            /// </summary>
            /// <param name="volume">Wagon details object.</param>
            public volumeMovement(wagonDetails volume)
            {
                this.wagonID = volume.wagonID;
                this.Origin = volume.origin;
                this.Via = volume.plannedDestination;
                this.Destination = volume.destination;
                this.weight = volume.netWeight;
                this.hasBeenCounted = false;

            }

            /// <summary>
            /// VolumeMovement constructor.
            /// </summary>
            /// <param name="wagonID">Wagon class and number.</param>
            /// <param name="Origin">Wagon origin.</param>
            /// <param name="plannedDestination">Wagon planned destination.</param>
            /// <param name="Destination">Wagon actual destination.</param>
            /// <param name="weight">Net weight carried by the wagon.</param>
            public volumeMovement(string wagonID, string Origin, string plannedDestination, string Destination, double weight)
            {
                this.wagonID = wagonID;
                this.Origin = Origin;
                this.Via = plannedDestination;
                this.Destination = Destination;
                this.weight = weight;
                this.hasBeenCounted = false;

            }

            /// <summary>
            /// VolumeMovement constructor.
            /// </summary>
            /// <param name="wagonID">Wagon class and number.</param>
            /// <param name="Origin">Wagon origin.</param>
            /// <param name="plannedDestination">Wagon planned destination.</param>
            /// <param name="Destination">Wagon actual destination.</param>
            /// <param name="weight">Net weight carried by the wagon.</param>
            /// <param name="hasBeenCounted">Flag indicating if the volume has been counted in the final volume movement list.</param>
            public volumeMovement(string wagonID, string Origin, string plannedDestination, string Destination, double weight, bool hasBeenCounted)
            {
                this.wagonID = wagonID;
                this.Origin = Origin;
                this.Via = plannedDestination;
                this.Destination = Destination;
                this.weight = weight;
                this.hasBeenCounted = hasBeenCounted;

            }

           
        };

        /// <summary>
        /// A class to hold the wagon Characteristics for each wagon journey.
        /// </summary>
        public class wagonDetails
        {

            public string wagonID;
            public string origin;
            public string plannedDestination;
            public string destination;
            public DateTime attachmentTime;
            public DateTime detachmentTime;
            public double netWeight;

            /// <summary>
            /// Default Wagon Constrcutor.
            /// </summary>
            /// <param name="wagon">Wagon structure containing the origin, destination and volume carries among other properties.</param>
            public wagonDetails(wagonDetails wagon)
            {
                this.wagonID = wagon.wagonID;
                this.origin = wagon.origin;
                this.plannedDestination = wagon.plannedDestination;
                this.destination = wagon.destination;
                this.attachmentTime = wagon.attachmentTime;
                this.detachmentTime = wagon.detachmentTime;
                this.netWeight = wagon.netWeight;

                /* Fix the known issues with the location codes. */
                fixIssues(this.plannedDestination, this.destination);
            }

            /// <summary>
            /// Wagon Constructor.
            /// </summary>
            /// <param name="wagonID">The Wagon class ID.</param>
            /// <param name="origin">The Origin location code of the wagon.</param>
            /// <param name="plannedDestination">The planned destination code of the wagon.</param>
            /// <param name="destination">The destination code of the wagon.</param>
            /// <param name="netWeight">The net weight carried to the destination by the wagon.</param>
            public wagonDetails(string wagonID, string origin, string plannedDestination, string destination, double netWeight)
            {
                this.wagonID = wagonID;
                this.origin = origin;
                this.plannedDestination = plannedDestination;
                this.destination = destination;
                this.attachmentTime = new DateTime(2000, 1, 1);
                this.detachmentTime = new DateTime(2000, 1, 1);
                this.netWeight = netWeight;

                /* Fix the known issues with the location codes. */
                fixIssues(this.plannedDestination, this.destination);
            }

            /// <summary>
            /// Wagon Constructor.
            /// </summary>
            /// <param name="wagonID">The Wagon class ID.</param>
            /// <param name="origin">The Origin location code of the wagon.</param>
            /// <param name="plannedDestination">The planned destination code of the wagon.</param>
            /// <param name="destination">The destination code of the wagon.</param>
            /// <param name="attachmentTime">The time the wagon was attached to the Train.</param>
            /// <param name="detachmentTime">The time the wagon was detached from the Train.</param>
            /// <param name="netWeight">The net weight carried to the destination by the wagon.</param>
            public wagonDetails(string wagonID, string origin, string plannedDestination, string destination, DateTime attachmentTime, DateTime detachmentTime, double netWeight)
            {
                this.wagonID = wagonID;
                this.origin = origin;
                this.plannedDestination = plannedDestination;
                this.destination = destination;
                this.attachmentTime = attachmentTime;
                this.detachmentTime = detachmentTime;
                this.netWeight = netWeight;

                /* Fix the known issues with the location codes. */
                fixIssues(this.plannedDestination, this.destination);
            }

            /// <summary>
            /// Fix the known issues with the location codes.
            /// </summary>
            /// <param name="plannedDestination">The planned destination code of the wagon.</param>
            /// <param name="destination">The destination code of teh wagon.</param>
            private void fixIssues(string plannedDestination, string destination)
            {
                /* Issue 1:
                 * The location code 'LAV' does not exist. It is assumed that this refers to SCT-Laverton 
                 * as the next origin location is 'SCT' (SCT-Laverton). 
                 */
                if (plannedDestination.Equals("LAV"))
                    this.plannedDestination = "SCT";
                
                /* Issue 2:
                 * When the location code 'CNM' appears in the destination, the next origin location is 'PGM'.
                 * These locations are approximtly 20 km apart, with no indication of how the wagon was 
                 * transported between these locations. Therefore, it is assumed that the two locations 
                 * are the same. The 'PGM' location has been chosen to be the reference location. 
                 */
                if (destination.Equals("CNM"))
                    this.destination = "PGM";

            }
        }

        /// <summary>
        /// Instantiation of the Log Class to provide logging capability.
        /// </summary>
#if (LOGGING)
        public static Logging log = new Logging();
#endif
        /* Create a tools Object. */
        public static Tools tool = new Tools();

        [STAThread]
        static void Main(string[] args)
        {

#if (LOGGING)
                log.WriteLine("Begin:");
#endif

            /* Use a browser to select the desired data file. */
            string filename = null;
            try
            {
                filename = tool.selectDataFile();
                if (filename == null)
                    return;
            }
            catch
            {
                return;
            }

            /* Create the Wagon list. */
            List<wagonDetails> wagon = new List<wagonDetails>();

            /* Populte the wagon list with the data from the data file. */
            try
            {
                wagon = readWagonDataFile(filename);
            }
            catch (IOException exception)
            {
                if (wagon.Count() == 0)
                {
                    tool.messageBox(exception.Message, "File format.");
                    return;
                }
            }

            List<volumeMovement> volume = new List<volumeMovement>();
            volume = algorithm(wagon);
            /*
             * wagon movement algorithms ...
             */


            /* Write the wagon details to excel. */
            writeToExcel(wagon);


        }


        //////////////////////////////////////////////////////////
        // CONSIDER PUTTING THESE FUNCTION IS TOOLS.

        /// <summary>
        /// Read the wagon data file.
        /// The file is assumed to be in a specific format
        /// column  Feild
        ///  0      Wagon Class
        ///  1      Wagon Number
        ///  2      Train Number
        ///  3      Train Date
        ///  4      Comodity
        ///  5      Origin Code
        ///  6      Planned Destination Code
        ///  7      Actual Destination Code
        ///  8      Attachment Time
        ///  9      Detachment Time
        ///  10     Tare Weight
        ///  11     Gross Weight
        ///  12     Distance Travelled
        ///  13     Wagon Sequence
        ///  14     Record ID
        ///  15     Wagon Movement Count (same as ID)
        /// </summary>
        /// <param name="filename">The wagon data file.</param>
        /// <returns>The list of wagon objects.</returns>
        public static List<wagonDetails> readWagonDataFile(string filename)
        {
           
            /* Read the all lines of the text file. */
            string[] lines = System.IO.File.ReadAllLines(filename);
            char[] delimiters = { ',' };

            double tareWeight = 0;
            double grossWeight = 0;
            DateTime attachmentTime = new DateTime(2000, 1, 1);
            DateTime detachmentTime = new DateTime(2000, 1, 1);

            /* Create the list of wagon objects. */
            List<wagonDetails> wagon = new List<wagonDetails>();

            /* Validate the format of the first line of the file. */
            // NOTE: This can not be header information.
            Boolean validFormat = false;
            string[] fields = lines[0].Split(delimiters);
            validFormat = tool.validateFileFormat(fields);
            if (!validFormat)
            {
                /* The file format is invalid, return the empty wagon object. */
                throw new IOException("Data file has an invalid format.");
            }

            Boolean filter = true;

            /* Extract the wagon details from the data file. */
            foreach (string line in lines)
            {
                string wagonID = "";
                string origin = "";
                string plannedDestination = "";
                string destination = "";
                double netWeight = 0;

                /* Split the lines into the fields */
                fields = line.Split(delimiters);
                char[] newDelimeters = { '\'', '"' };

                /* Clean the wagon ID fields. */
                string[] field0 = Regex.Unescape(fields[0]).Split(newDelimeters);
                string[] field1 = Regex.Unescape(fields[1]).Split(newDelimeters);

                /* Extract the cleaned fields and populate the wagon detail arrays. */
                filter = filterData(field0[1]);

                /* Wagon ID. */
                if (field0.Count() == 3 && field1.Count() == 1)
                {
                    filter = filterData(field0[1]);
                    wagonID = field0[1] + "-" + field1[0];
                }
                else
                {
                    filter = false;
                    tool.messageBox("Wagon ID configuration has not been accounted for.", "Unknown wagon ID configuration.");
                }

                if (filter)
                {
                    /* Wagon Origin. */
                    string[] field = Regex.Unescape(fields[5]).Split(newDelimeters);
                    if (field.Count() == 3)
                        origin = field[1];
                    else
                        tool.messageBox("Origin location code in unknown: " + origin, "Unknown location code.");

                    /* Wagon planned destination. */
                    field = Regex.Unescape(fields[6]).Split(newDelimeters);
                    if (field.Count() == 3)
                        plannedDestination = field[1];
                    else
                        tool.messageBox("Consigned Destination location code in unknown: " + origin, "Unknown location code.");

                    /* Wagon destination. */
                    field = Regex.Unescape(fields[7]).Split(newDelimeters);
                    if (field.Count() == 3)
                        destination = field[1];
                    else
                        tool.messageBox("Destination location code in unknown: " + origin, "Unknown location code.");

                    /* Remaining Wagon details. */
                    DateTime.TryParse(fields[8], out attachmentTime);
                    DateTime.TryParse(fields[9], out detachmentTime);
                    double.TryParse(fields[10], out tareWeight);
                    double.TryParse(fields[11], out grossWeight);
                    netWeight = grossWeight - tareWeight;

                    /* Construct the wagon object and add to the list. */
                    wagonDetails data = new wagonDetails(wagonID, origin, plannedDestination, destination, attachmentTime, detachmentTime, netWeight);
                    wagon.Add(data);
                }
            }

            /* Return the completed wagon List. */
            return wagon;
        }

        /// <summary>
        /// Filter the wagon data so that only intermodal traffic is kept.
        /// </summary>
        /// <param name="wagonClass">The wagon class.</param>
        /// <returns>True if the wagon class is 4 characters long with no numbers.</returns>
        public static Boolean filterData(string wagonClass)
        {
            /* Wagon class must be a string of 4 characters long.
             * Any other indicates a locomotive or wagons that aren't freight related. */
            if (wagonClass.Count() == 4)
            {
                if (Regex.IsMatch(wagonClass, @"\d"))
                    return false;
                else 
                    return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Write the wagon details to an excel file for later analysis.
        /// </summary>
        /// <param name="wagon">The wagon object containing the origin, destinaiton and net weight</param>
        public static void writeToExcel(List<wagonDetails> wagon)
        {

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
            string[] header = { "Wagon ID", "Origin", "Planned Destiantion", "Destination", 
                                  "Attatchment Time", "Detatchment Time", "Net Weight" };
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

            //string savePath = @"S:\Corporate Strategy\Market Analysis & Forecasts\Volume\Wagon movement analysis";
            string savePath = @"C:\Users\Beau\Documents\ARTC\Wagon Volumes";    // home path
            string saveFilename = savePath + @"\wagonDetails_" + DateTime.Now.ToString("yyyyMMdd") + ".xlsx";

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

        //////////////////////////////////////////////////////////

        public static List<volumeMovement> algorithm(List<wagonDetails> wagon)
        {
            List<volumeMovement> volume = new List<volumeMovement>();
            int searchIdx = 0;
            //int volumeIdx = 0;

            for (int recordIdx = 0; recordIdx < wagon.Count(); recordIdx++)
            {
                volumeMovement item = new volumeMovement(wagon[recordIdx]);

                /* Wagon reached the planned destination without any intermediate stops. */
                if (wagon[recordIdx].plannedDestination.Equals(wagon[recordIdx].destination))
                {
                    volume.Add(item);
                }
                else
                {
                    /* The wagon was detatched at an intermediate location before continueing on to the planned destination. */

                    /* Find the recordId, where the wagon reaches its planned destination. */
                    searchIdx = recordIdx + 1;
                    /*wagonDetails currentWagonMovement = new wagonDetails(wagon[recordIdx]);
                    wagonDetails searchWagonMovement = new wagonDetails(wagon[searchIdx]);
                    wagonDetails previousSearchWagonMovement = new wagonDetails(wagon[searchIdx-1]);
                    */
                    while (!wagon[searchIdx].plannedDestination.Equals(wagon[searchIdx].destination))
                    {
                        if (wagon[searchIdx-1].wagonID.Equals(wagon[searchIdx].wagonID) &&
                           !wagon[recordIdx].plannedDestination.Equals(wagon[searchIdx].plannedDestination) &&
                            wagon[recordIdx].attachmentTime < wagon[recordIdx-1].attachmentTime)
                        {
                            /* Correct the apparent mismatch in locations where the time stamps seems to 
                             * indicate that the wagon has been attatched to two trains simultaneously. */
                            
                            /* Replace the origin location. */
                            //item.Origin = wagon[recordIdx].origin;
                            //volume.RemoveAt(volume.Count()-1);
                            //volume.Add(item);
                            volume.Last().Origin = wagon[recordIdx].origin;
                            searchIdx++;
                            recordIdx++;

                        }
                        else if (wagon[recordIdx].plannedDestination.Equals(wagon[searchIdx].plannedDestination) &&
                                !wagon[searchIdx].plannedDestination.Equals(wagon[searchIdx].destination) )
                        {
                            /* The wagon movement is continueing to the planned destiantion. */
                            if (searchIdx == wagon.Count() - 1)
                                break;

                            searchIdx++;
                        }
                        else
                        {
                            searchIdx--;
                            break;
                        }

                    } // while loop

                    /* The wagon has reached its planned destination. */
                    //volumeMovement volumeItem = new volumeMovement(wagon[recordIdx].wagonID, wagon[recordIdx].origin, "", wagon[recordIdx].destination, wagon[recordIdx].netWeight);
                    item = new volumeMovement(wagon[recordIdx].wagonID, wagon[recordIdx].origin, "", wagon[recordIdx].destination, wagon[recordIdx].netWeight);
                    volume.Add(item);


                    if (!wagon[recordIdx].plannedDestination.Equals(wagon[searchIdx].plannedDestination))
                    { 
                        /* The wagon has been detatched before it reached the planned destination. */
                        searchIdx = recordIdx;
                    }

                    /* Loop through the wagon records to determine the wagon weight and final destination. */
                    for (int index = recordIdx; index < searchIdx; index++)
                    { 
                        /* Check the wagons being compared are the same wagon. */
                        if (wagon[recordIdx].wagonID.Equals(wagon[index].wagonID))
                        {

                            if (index <= recordIdx + 1)
                            {
                                if(wagon[recordIdx].netWeight == wagon[index].netWeight)
                                {
                                    /* The weight has not changed. */
                                    volume.Last().Destination = wagon[index].destination;
                                }
                                else if (wagon[recordIdx].netWeight < wagon[index].netWeight)
                                {
                                    /* Weight has been added at the intermediate destination. */
                                    volume.Last().Destination = wagon[index].destination;
                
                                    item = new volumeMovement(wagon[recordIdx].wagonID, wagon[index].origin, "", wagon[index].destination, wagon[index].netWeight- wagon[recordIdx].netWeight);
                                    volume.Add(item);
                           
                                }
                                else
                                {
                                    /* Weight has been removed at the intermediate destination. */
                                    volume.Last().Destination = wagon[index].destination;
                                    volume.Last().weight = wagon[index].netWeight;

                                    item = new volumeMovement(wagon[recordIdx].wagonID, wagon[recordIdx].origin, "", wagon[recordIdx].destination, wagon[recordIdx].netWeight - wagon[index].netWeight);
                                    volume.Add(item);
                                }
                            }
                            else
                            {
                                /* Multiple intermediate locations have been found. */
                                if (wagon[recordIdx].netWeight == wagon[index].netWeight)
                                {
                                    /* The weights remained the same. */
                                    volume.Last().Destination = wagon[index].destination;
                                }
                                else if (wagon[recordIdx].netWeight < wagon[index].netWeight)
                                {
                                    /* Weight has been added at an intermediate locations. */
                                    volume[volume.Count() - 1].Destination = wagon[index].destination;
                                    volume.Last().Destination = wagon[index].destination;
                                }
                                else
                                {
                                    /* Weight has been removed at an intermediate locations. */
                                    volume.Last().Destination = wagon[index].destination;
                                }
                            }
                        }
                        else
                        {
                            searchIdx--;
                        }
                    }
                    /* Reset the record Index to the current location. */
                    recordIdx = searchIdx;
                }
                /* Find the wagon movements that are continuations of the volumes. */

                if (volume.Count() > 1)
                {
                    /* It is assumed that any volume movement that is a continuation does not have a change in weight. */
                    if (volume.Last().wagonID.Equals(volume[volume.Count()-1].wagonID) &&
                        volume.Last().Origin.Equals(volume[volume.Count()-1].Destination) &&
                        volume.Last().weight.Equals(volume[volume.Count()-1].weight))
                    {
                        /* Update the previous volume destination. */
                        int previousIdx = volume.Count() - 2;   // maybe -1
                        volume[previousIdx].Via = volume[previousIdx].Destination;
                        volume[previousIdx].Destination = volume.Last().Destination;
                    }
                }
    

                /* Record the number of origin-destination pairs. */

                /* Write the volume dat to file for analysis. */
            }

            return volume;
        }


    } // end of program class

} // end of namespace
