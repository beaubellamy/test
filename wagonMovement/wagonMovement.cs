/*
 * Wagon Movement class
 * 
 */


//----------------------------------------
// Uncomment to turn logging on 
//#define LOGGING 
//---------------------------------------
//#define HOMEPATH
#define WORKPATH

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Office.Interop.Excel;

using TrainLibrary;

namespace wagonMovement
{

    class Program
    {

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

            /* Populate the wagon list with the data from the data file. */
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

            /* Combine the wagon movements based on planned destiantion and weights. */
            List<volumeMovement> volume = new List<volumeMovement>();
            volume = combineWagonMovements(wagon);
            
            /* Combine the volume movements that appear to be continuations of the same wagon. */
            volume = combineVolumeMovements(volume);

            /* Write the wagon details to excel. */
            writeWagonDataToExcel(wagon);
            writeVolumeDataToExcel(volume);

            tool.messageBox("Program Complete.");

        }


        //////////////////////////////////////////////////////////
        // CONSIDER PUTTING THESE FUNCTION IN TOOLS.

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
            bool intermodalTraffic = false;

            double tareWeight = 0;
            double grossWeight = 0;
            DateTime attachmentTime = new DateTime(2000, 1, 1);
            DateTime detachmentTime = new DateTime(2000, 1, 1);

            /* Create the list of wagon objects. */
            List<wagonDetails> wagon = new List<wagonDetails>();

            /* Validate the format of the first line of the file.
             * NOTE: This can not be header information.
             */
            bool validFormat = false;
            string[] fields = lines[0].Split(delimiters);
            validFormat = tool.validateFileFormat(fields);
            if (!validFormat)
            {
                /* The file format is invalid, return the empty wagon object. */
                throw new IOException("Data file has an invalid format.");
            }
            
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

                /* Wagon ID. */
                if (field0.Count() == 3 && field1.Count() == 1)
                {
                    intermodalTraffic = filterIntermodalTraffic(field0[1]);
                    wagonID = field0[1] + "-" + field1[0];
                }
                else
                {
                    intermodalTraffic = false;
                    tool.messageBox("Wagon ID configuration has not been accounted for.", "Unknown wagon ID configuration.");
                }

                if (intermodalTraffic)
                {
                    /* Validate the location codes */

                    /* Wagon Origin. */
                    string[] field = Regex.Unescape(fields[5]).Split(newDelimeters);
                    if (field.Count() == 3)
                        origin = field[1];
                    else
                        tool.messageBox("Origin location code is unknown: " + origin, "Unknown location code.");

                    /* Wagon planned destination. */
                    field = Regex.Unescape(fields[6]).Split(newDelimeters);
                    if (field.Count() == 3)
                        plannedDestination = field[1];
                    else
                        tool.messageBox("Consigned Destination location code in unknown: " + origin + " - " + field, "Unknown location code.");

                    /* Wagon destination. */
                    field = Regex.Unescape(fields[7]).Split(newDelimeters);
                    if (field.Count() == 3)
                        destination = field[1];
                    else
                    {   /* If the destination field is empty, assume the wagon reaches the planned destination. */
                        if (field[0].Equals(""))
                            destination = plannedDestination;
                        else
                            tool.messageBox("Destination location code is unknown: " + origin + " - " + field, "Unknown location code.");
                    }

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
        public static bool filterIntermodalTraffic(string wagonClass)
        {
            /* Wagon class must be a string of 4 characters long.
             * Any other indicates a locomotive or wagons that aren't freight related. */
            if (wagonClass.Count() == 4)
            {
                /* There are no numbers in the wagon class ID of intermodal traffic. */
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
        /// <param name="wagon">The list of wagon objects containing the origin, destinaiton and net weight</param>
        public static void writeWagonDataToExcel(List<wagonDetails> wagon)
        {
            /* Maximum number of rows in an excel worksheet is 1,048,576 (round down to a nice number) */
            int maxExcelRows = 1048500;
            
            /* Create the microsfot excel references. */
            Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
            Workbook workbook;
            Worksheet worksheet;

            /* Get the reference to the new workbook. */
            workbook = (Workbook)(excel.Workbooks.Add(""));

            /* Create the header details. */
            string[] headerString = { "Wagon ID", "Origin", "Planned Destiantion", "Destination", 
                                  "Attatchment Time", "Detatchment Time", "Net Weight" };
                        
            /* Get the page size of the excel worksheet. */
            int header = 2;
            int excelPageSize = wagon.Count()-1;
            int excelPages = 1;

            if (wagon.Count() > maxExcelRows)
            {
                excelPageSize = 1000000;
                excelPages = (int)Math.Round((double)wagon.Count() / excelPageSize + 0.5);
            }

            /* Deconstruct the wagon details into excel columns. */
            string[,] ID = new string[excelPageSize, 1];
            string[,] Orig = new string[excelPageSize, 1];
            string[,] Planned = new string[excelPageSize, 1];
            string[,] Dest = new string[excelPageSize, 1];
            DateTime[,] attatch = new DateTime[excelPageSize, 1];
            DateTime[,] detatch = new DateTime[excelPageSize, 1];
            double[,] weight = new double[excelPageSize, 1];

            
            /* Loop through the excel pages. */
            for (int excelPage = 0; excelPage < excelPages; excelPage++)
            {
                /* Set the active worksheet. */
                worksheet = (Worksheet)workbook.Sheets[excelPage + 1];
                workbook.Sheets[excelPage + 1].Activate();
                worksheet.get_Range("A1", "G1").Value2 = headerString;

                /* Loop through the data for each excel page. */
                for (int j = 0; j < excelPageSize; j++)
                {
                    /* Check we dont try to read more data than there really is. */
                    int checkIdx = j + excelPage * excelPageSize;
                    if (checkIdx < wagon.Count())
                    {
                        ID[j, 0] = wagon[checkIdx].wagonID;
                        Orig[j, 0] = wagon[checkIdx].origin;
                        Planned[j, 0] = wagon[checkIdx].plannedDestination;
                        Dest[j, 0] = wagon[checkIdx].destination;
                        attatch[j, 0] = wagon[checkIdx].attachmentTime;
                        detatch[j, 0] = wagon[checkIdx].detachmentTime;
                        weight[j, 0] = wagon[checkIdx].netWeight;
                    }
                    else
                    {
                        /* The end of the data has been reached. Populate the remaining elements. */
                        ID[j, 0] = "";
                        Orig[j, 0] = "";
                        Planned[j, 0] = "";
                        Dest[j, 0] = "";
                        attatch[j, 0] = DateTime.MinValue;
                        detatch[j, 0] = DateTime.MinValue;
                        weight[j, 0] = 0;
                    }
                }

                /* Write the data to the active excel workseet. */
                worksheet.get_Range("A" + header, "A" + (header + excelPageSize)).Value2 = ID;
                worksheet.get_Range("B" + header, "B" + (header + excelPageSize)).Value2 = Orig;
                worksheet.get_Range("C" + header, "C" + (header + excelPageSize)).Value2 = Planned;
                worksheet.get_Range("D" + header, "D" + (header + excelPageSize)).Value2 = Dest;
                worksheet.get_Range("E" + header, "E" + (header + excelPageSize)).Value2 = attatch;
                worksheet.get_Range("F" + header, "F" + (header + excelPageSize)).Value2 = detatch;
                worksheet.get_Range("G" + header, "G" + (header + excelPageSize)).Value2 = weight;

            }

#if (HOMEPATH)
            string savePath = @"C:\Users\Beau\Documents\ARTC\Wagon Volumes";
#else
            string savePath = @"S:\Corporate Strategy\Market Analysis & Forecasts\Volume\Wagon movement analysis";
#endif

            string saveFilename = savePath + @"\wagonDetails_" + DateTime.Now.ToString("yyyyMMdd") + ".xlsx";

            /* Check the file does not exist yet. */
            if (File.Exists(saveFilename))
                File.Delete(saveFilename);

            /* Save the excel file. */
            excel.UserControl = false;
            workbook.SaveAs(saveFilename, XlFileFormat.xlWorkbookDefault, Type.Missing, Type.Missing,
                false, false, XlSaveAsAccessMode.xlNoChange,
                Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);

            workbook.Close();

            return;
        }
        
        /// <summary>
        /// Write the volume data to an excel file for analysis.
        /// </summary>
        /// <param name="volume">The list of volume objects containing the final origin destination details.</param>
        public static void writeVolumeDataToExcel(List<volumeMovement> volume)
        {
            /* Maximum number of rows in an excel worksheet is 1,048,576 (round down to a nice number) */
            int maxExcelRows = 1048500;
            
            /* Create the microsfot excel references. */
            Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
            Workbook workbook;
            Worksheet worksheet;

            /* Get the reference to the new workbook. */
            workbook = (Workbook)(excel.Workbooks.Add(""));

            /* Create the header details. */
            string[] headerString = { "Wagon ID", "Origin", "Via", "Destination", "Weight" };

            /* Get the page size of the excel worksheet. */
            int header = 2;
            int excelPageSize = volume.Count()-1;
            int excelPages = 1;

            if (volume.Count() > maxExcelRows)
            {
                excelPageSize = 1000000;
                excelPages = (int)Math.Round((double)volume.Count() / excelPageSize + 0.5);
            }
            
            /* Deconstruct the volume details into excel columns. */
            string[,] ID = new string[excelPageSize, 1];
            string[,] Orig = new string[excelPageSize, 1];
            string[,] Via = new string[excelPageSize, 1];
            string[,] Dest = new string[excelPageSize, 1];
            double[,] weight = new double[excelPageSize, 1];

            /* Loop through the excel pages. */
            for (int excelPage = 0; excelPage < excelPages; excelPage++)
            {
                /* Set the active worksheet. */
                worksheet = (Worksheet)workbook.Sheets[excelPage + 1];
                workbook.Sheets[excelPage + 1].Activate();
                worksheet.get_Range("A1", "E1").Value2 = headerString;

                /* Loop through the data for each excel page. */
                for (int j = 0; j < excelPageSize; j++)
                {
                    /* Check we dont try to read more data than there really is. */
                    int checkIdx = j + excelPage * excelPageSize;
                    if (checkIdx < volume.Count())
                    {
                        ID[j, 0] = volume[checkIdx].wagonID;
                        Orig[j, 0] = volume[checkIdx].Origin;
                        Via[j, 0] = volume[checkIdx].Via;
                        Dest[j, 0] = volume[checkIdx].Destination;
                        weight[j, 0] = volume[checkIdx].weight;
                    }
                    else
                    {
                        /* The end of the data has been reached. Populate the remaining elements. */
                        ID[j, 0] = "";
                        Orig[j, 0] = "";
                        Via[j, 0] = "";
                        Dest[j, 0] = "";
                        weight[j, 0] = 0;
                    }
                }

                /* Write the data to the active excel workseet. */
                worksheet.get_Range("A" + header, "A" + (header + excelPageSize)).Value2 = ID;
                worksheet.get_Range("B" + header, "B" + (header + excelPageSize)).Value2 = Orig;
                worksheet.get_Range("C" + header, "C" + (header + excelPageSize)).Value2 = Via;
                worksheet.get_Range("D" + header, "D" + (header + excelPageSize)).Value2 = Dest;
                worksheet.get_Range("E" + header, "E" + (header + excelPageSize)).Value2 = weight;

            }

#if (HOMEPATH)
            string savePath = @"C:\Users\Beau\Documents\ARTC\Wagon Volumes";
#else
            string savePath = @"S:\Corporate Strategy\Market Analysis & Forecasts\Volume\Wagon movement analysis";
#endif

            string saveFilename = savePath + @"\volumeDetails_" + DateTime.Now.ToString("yyyyMMdd") + ".xlsx";

            /* Check the file does not exist yet. */
            if (File.Exists(saveFilename))
                File.Delete(saveFilename);

            /* Save the excel file. */
            excel.UserControl = false;
            workbook.SaveAs(saveFilename, XlFileFormat.xlWorkbookDefault, Type.Missing, Type.Missing,
                false, false, XlSaveAsAccessMode.xlNoChange,
                Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);

            workbook.Close();

            return;
        }

        /// <summary>
        /// Join wagon movements based on wagon ID, planned destination and weight.
        /// </summary>
        /// <param name="wagon">A list of wagon movements.</param>
        /// <returns>A list of volume movements</returns>
        public static List<volumeMovement> combineWagonMovements(List<wagonDetails> wagon)
        {
            /* Initialise teh volume list */
            List<volumeMovement> volume = new List<volumeMovement>();
            int searchIdx = 0;
            
            /* search through all wagon movements */
            for (int recordIdx = 0; recordIdx < wagon.Count(); recordIdx++)
            {
                /* Create a new volume movement */
                volumeMovement item = new volumeMovement(wagon[recordIdx]);

                /* Wagon reached the planned destination without any intermediate stops. */
                if (wagon[recordIdx].plannedDestination.Equals(wagon[recordIdx].destination))
                {
                    volume.Add(item);
                }
                else
                {

                    /* The wagon was detatched at an intermediate location before continueing 
                     * on to the planned destination. 
                     */

                    /* Find the recordId, where the wagon reaches its planned destination. */
                    if ((recordIdx + 1) < wagon.Count())
                        searchIdx = recordIdx + 1;
                    
                    while (!wagon[searchIdx].plannedDestination.Equals(wagon[searchIdx].destination))
                    {
                        /* The volume has not reached the planned destination. */

                        if (wagon[searchIdx - 1].wagonID.Equals(wagon[searchIdx].wagonID) &&
                           !wagon[recordIdx].plannedDestination.Equals(wagon[searchIdx].plannedDestination) &&
                            wagon[recordIdx].attachmentTime < wagon[recordIdx - 1].attachmentTime)
                        {
                            /* Correct the apparent mismatch in locations where the time stamps seems to 
                             * indicate that the wagon has been attatched to two trains simultaneously. 
                             */

                            /* Replace the origin location. */
                            volume.Last().Origin = wagon[recordIdx].origin;
                            volume.Last().attachmentTime = wagon[recordIdx].attachmentTime;
                            searchIdx++;
                            recordIdx++;

                        }
                        else if (wagon[recordIdx].plannedDestination.Equals(wagon[searchIdx].plannedDestination) &&
                                !wagon[searchIdx].plannedDestination.Equals(wagon[searchIdx].destination))
                        {
                            /* The wagon movement is continueing to the planned destination. */
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
                    item = new volumeMovement(wagon[recordIdx].wagonID, wagon[recordIdx].origin, "", wagon[recordIdx].destination, wagon[recordIdx].netWeight, wagon[recordIdx].attachmentTime, wagon[recordIdx].detachmentTime);
                    volume.Add(item);


                    if (!wagon[recordIdx].plannedDestination.Equals(wagon[searchIdx].plannedDestination))
                    {
                        /* The wagon has been detatched before it reached the planned destination. */
                        searchIdx = recordIdx;
                    }

                    /* Loop through the wagon records to determine the wagon weight and final destination. */
                    for (int index = recordIdx; index <= searchIdx; index++)
                    {
                        /* Check the wagons being compared are the same wagon. */
                        if (wagon[recordIdx].wagonID.Equals(wagon[index].wagonID))
                        {

                            if (index <= recordIdx + 1)
                            {
                                if (wagon[recordIdx].netWeight == wagon[index].netWeight)
                                {
                                    /* The weight has not changed. */
                                    volume.Last().Destination = wagon[searchIdx].destination;   // was [index]
                                    volume.Last().detachmentTime = wagon[searchIdx].detachmentTime;
                                }
                                else if (wagon[recordIdx].netWeight < wagon[index].netWeight)
                                {
                                    /* Weight has been added at the intermediate destination. */
                                    volume.Last().Destination = wagon[searchIdx].destination;   // was [index]
                                    volume.Last().detachmentTime = wagon[searchIdx].detachmentTime;

                                    item = new volumeMovement(wagon[recordIdx].wagonID, wagon[index].origin, "", wagon[searchIdx].destination, wagon[index].netWeight - wagon[recordIdx].netWeight, wagon[index].attachmentTime, wagon[searchIdx].detachmentTime);
                                    volume.Add(item);

                                }
                                else
                                {
                                    /* Weight has been removed at the intermediate destination. */
                                    volume.Last().Destination = wagon[index].destination;   // was [index]
                                    volume.Last().detachmentTime = wagon[index].detachmentTime;
                                    volume.Last().weight = wagon[index].netWeight;

                                    item = new volumeMovement(wagon[recordIdx].wagonID, wagon[recordIdx].origin, "", wagon[recordIdx].destination, wagon[recordIdx].netWeight - wagon[index].netWeight, wagon[recordIdx].attachmentTime, wagon[recordIdx].detachmentTime);
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
                                    volume.Last().detachmentTime = wagon[index].detachmentTime;
                                }
                                else if (wagon[recordIdx].netWeight < wagon[index].netWeight)
                                {
                                    /* Weight has been added at an intermediate locations. */
                                    volume[volume.Count() - 1].Destination = wagon[index].destination;
                                    volume[volume.Count() - 1].detachmentTime = wagon[index].detachmentTime;
                                    volume.Last().Destination = wagon[index].destination;
                                }
                                else
                                {
                                    /* Weight has been removed at an intermediate locations. */
                                    volume[volume.Count() - 1].Destination = wagon[index].destination;
                                    volume[volume.Count() - 1].detachmentTime = wagon[index].detachmentTime;
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

                /* Check for time stamps that are wrong. */
                // weight is the same, attach times are within 2 min, and attatch - detatch of next item is within 2 min.
                double timeDifference = 1440.0;
                double travelTime = 1440.0;
                if (recordIdx != wagon.Count() - 1)
                {
                    timeDifference = (wagon[recordIdx + 1].attachmentTime - wagon[recordIdx].attachmentTime).TotalMinutes;
                    travelTime = (wagon[recordIdx + 1].detachmentTime - wagon[recordIdx + 1].attachmentTime).TotalMinutes;

                    if (wagon[recordIdx + 1].wagonID.Equals(wagon[recordIdx].wagonID) &&
                        wagon[recordIdx + 1].netWeight == wagon[recordIdx].netWeight &&
                        timeDifference < 2.0 &&
                        travelTime < 2.0)
                    {
                        recordIdx++;
                    }
                }
                
                /* Find the wagon movements that are continuations of the volumes. */
                if (volume.Count() > 1)
                {
                    /* It is assumed that any volume movement that is a continuation does not have a change in weight. */
                    if (volume.Last().wagonID.Equals(volume[volume.Count() - 2].wagonID) &&
                        volume.Last().Origin.Equals(volume[volume.Count() - 2].Destination) &&
                        !volume.Last().Destination.Equals(volume[volume.Count() - 2].Origin) &&
                        volume.Last().weight == volume[volume.Count() - 2].weight &&
                        volume.Last().weight > 0)
                    {
                        /* Update the previous volume destination. */
                        int previousIdx = volume.Count() - 2;   // maybe -1
                        volume[previousIdx].Via = volume[previousIdx].Destination;
                        volume[previousIdx].Destination = volume.Last().Destination;
                    }
                }

                            }

            return volume;
        }

        /// <summary>
        /// Combine apparent continuations of the volume movements.
        /// </summary>
        /// <param name="volume">Initial list of volume movements</param>
        /// <returns>List of volume moevements</returns>
        public static List<volumeMovement> combineVolumeMovements(List<volumeMovement> volume)
        {
            /* Create a new list of volumes that will be returned. */
            List<volumeMovement> newVolume = new List<volumeMovement>();

            /* Initialise the indecies and counters. */
            int volumeIdx = 0;
            int current = volumeIdx;
            int next = current + 1;
            
            /* Initialise the wagon strings. */
            string wagonID = "";
            string Origin = "";
            string Via = "";
            string Destination = "";
            DateTime attachmentTime = new DateTime(2000, 1, 1);
            DateTime detachmentTime = new DateTime(2000, 1, 1);

            double weight = 0;

            /* loop through the existing volumes to see if any need to be combined. */
            for (volumeIdx = 0; volumeIdx < volume.Count() - 1; volumeIdx++)
            {
                current = volumeIdx;
                next = current + 1;

                /* Locate the last volume movement that is equal */
                while (volume[current].weight == volume[next].weight &&
                       volume[current].weight > 0)
                {
                    next++;
                    if (next == volume.Count())
                    {
                        break;
                    }
                }
                next--;
                
                /* This corrects for combining two wagon movements that are just return journeys with the same weight. */
                if ((volume[next].attachmentTime - volume[current].detachmentTime).TotalMinutes > 4800)
                {
                    /* If the last wagon movement in the journey is at least 2 or more movements later, then they should still be combined */
                    if (next - current > 1)
                        next++;

                    /* The next wagon movement should remain a seperate movement. */
                    if (next != current)
                        next--;
                }

                /* Populate the volume strings with the required details. */
                wagonID = volume[current].wagonID;
                Origin = volume[current].Origin;
                if (volume[current].Via.Equals(volume[next].Via))
                {
                    Via = "";
                }
                else
                {
                    Via = volume[next].Origin;
                }
                Destination = volume[next].Destination;
                weight = volume[current].weight;
                attachmentTime = volume[current].attachmentTime;
                detachmentTime = volume[current].attachmentTime;

                /* Create the volume itme and add to the list. */
                volumeMovement item = new volumeMovement(wagonID, Origin, Via, Destination, weight, attachmentTime, detachmentTime);
                newVolume.Add(item);
                
                volumeIdx = next;


            }
            
            return newVolume;
            
        }
        

    } // end of program class

} // end of namespace
