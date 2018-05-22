
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using TrainLibrary;
using IOLibrary;

namespace wagonMovement
{

    class Algorithm
    {
        /* Weight threshold for differences for the standard analysis to be considered the same. units in tonnes. */
        public const double weightThreshold = 0.05;
        /* Weight threshold for differences for the volume model analysis to be considered the same. units in tonnes. */
        public const double volumeModelWeightThreshold = 0.5;
        /* Default threshold for the time between dettachment and attachement in 
         * hours to be considered a continuation of the same movement. 
         * - Primarily used for the volume model analysis.
         */
        public const double attatchThreshold = 2;

        /// <summary>
        /// Entry point to create and show the form. 
        /// </summary>
        /// <param name="args">A list of arguments for the initialisation of the program.</param>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new WagonMovementForm());

        }

        /// <summary>
        /// Process the wagon movements into individual volume movements.
        /// </summary>
        /// <param name="filename">Filename of the wagon data.</param>
        public static void processWagonMovements(string filename, string destinationFolder, DateTime fromDate, DateTime toDate, bool volumeModel, bool combineIntermodalAndSteel)
        {
            /* Create the Wagon list. */
            List<wagonDetails> wagon = new List<wagonDetails>();

            /* Populate the wagon list with the data from the data file. */
            try
            {
                wagon = FileOperations.readWagonDataFile(filename, combineIntermodalAndSteel);
                /* Extract the data for the date range. */
                wagon = wagon.Where(w => w.trainDate >= fromDate).Where(w => w.trainDate < toDate).ToList();

                if (wagon.Count() == 0)
                {
                    Tools.messageBox("No wagons found. \nCheck the file and dates.");
                    return;
                }
            }
            catch (IOException exception)
            {
                if (wagon.Count() == 0)
                {
                    Tools.messageBox(exception.Message, "File format.");
                    return;
                }
            }

            /* Sort the wagon data to ensure all similar wagon IDs are consecutive. */
            wagon = wagon.OrderBy(w => w.wagonID).ThenBy(w => w.commodity).ThenBy(w => w.attachmentTime).ThenBy(w => w.netWeight).ToList();

            /* Combine the wagon movements based on planned destination and weights. */
            List<volumeMovement> volume = new List<volumeMovement>();
            
            if (volumeModel)
                volume = combineWagonMovementsAlternateMethod(wagon);
            else
                volume = combineWagonMovements(wagon);

            /* Get the location properties for each Geo code. */
            readGeoLocationCodes();
              
            /* Combine the volume movements that appear to be continuations of the same wagon. */
            if (volumeModel)
                volume = combineVolumeMovementsAlternateMethod(volume);
            else
                volume = combineVolumeMovements(volume);

            /* populate all the lcoation detaisle for each origin destination code. */
            populateLocations(volume);

            /* Write the wagon details to excel. */
            FileOperations.writeWagonData(wagon, destinationFolder);
            //FileOperations.writeVolumeData(volume, destinationFolder);
            FileOperations.writeVolumeDataByCommodity(volume, destinationFolder);


        }

        /// <summary>
        /// Join wagon movements based on wagon ID, planned destination and weight.
        /// </summary>
        /// <param name="wagon">A list of wagon movements.</param>
        /// <returns>A list of volume movements</returns>
        private static List<volumeMovement> combineWagonMovements(List<wagonDetails> wagon)
        {
            /* Default threshold for the time between dettachment and attachement in 
             * hours to be considered a continuation of the same movement. 
             */
            double attachThreshold = 2;

            /* Initialise the volume list */
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
                             * 
                             * NOTE: removed this code when it appeared to replace some destinations 
                             * that were not expected. However, the code is required to correct for the 
                             * attatchemnt time.
                             */

                            /* Replace the origin location. */
                            volume.Last().Origin[0] = wagon[recordIdx].origin;
                            volume.Last().attachmentTime = wagon[recordIdx].attachmentTime;
                            searchIdx++;
                            recordIdx++;

                        }
                        else if (wagon[searchIdx - 1].wagonID.Equals(wagon[searchIdx].wagonID) &&
                                wagon[recordIdx].plannedDestination.Equals(wagon[searchIdx].plannedDestination) &&
                                !wagon[searchIdx].plannedDestination.Equals(wagon[searchIdx].destination) &&
                                wagon[searchIdx].attachmentTime < wagon[searchIdx-1].detachmentTime.AddHours(attachThreshold))
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
                    item = new volumeMovement(wagon[recordIdx].TrainID, wagon[recordIdx].trainOperator, wagon[recordIdx].commodity, wagon[recordIdx].wagonID, 
                        wagon[recordIdx].origin, "", wagon[recordIdx].destination, wagon[recordIdx].netWeight, wagon[recordIdx].grossWeight, 
                        wagon[recordIdx].attachmentTime, wagon[recordIdx].detachmentTime);
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
                                    volume.Last().Destination[0] = wagon[index].destination;
                                    volume.Last().detachmentTime = wagon[index].detachmentTime;
                                    
                                }
                                else if (wagon[recordIdx].netWeight < wagon[index].netWeight)
                                {
                                    /* Weight has been added at the intermediate destination. */
                                    volume.Last().Destination[0] = wagon[searchIdx].destination;
                                    volume.Last().detachmentTime = wagon[searchIdx].detachmentTime;
                                                                        
                                    item = new volumeMovement(wagon[recordIdx].TrainID, wagon[recordIdx].trainOperator, wagon[recordIdx].commodity, wagon[recordIdx].wagonID,
                                        wagon[index].origin, "", wagon[searchIdx].destination, wagon[index].netWeight - wagon[recordIdx].netWeight, wagon[index].grossWeight - wagon[recordIdx].grossWeight, 
                                        wagon[index].attachmentTime, wagon[searchIdx].detachmentTime);
                                    volume.Add(item);
                                                                        
                                }
                                else
                                {
                                    /* Weight has been removed at the intermediate destination. */
                                    volume.Last().Destination[0] = wagon[index].destination;
                                    volume.Last().detachmentTime = wagon[index].detachmentTime;
                                    volume.Last().netWeight = wagon[index].netWeight;
                                                                        
                                    item = new volumeMovement(wagon[recordIdx].TrainID, wagon[recordIdx].trainOperator, wagon[recordIdx].commodity, wagon[recordIdx].wagonID,
                                        wagon[recordIdx].origin, "", wagon[recordIdx].destination, wagon[recordIdx].netWeight - wagon[index].netWeight, wagon[recordIdx].grossWeight - wagon[index].grossWeight,
                                        wagon[recordIdx].attachmentTime, wagon[recordIdx].detachmentTime);
                                    volume.Add(item);
                                                                        
                                }
                            }
                            else
                            {
                                /* Multiple intermediate locations have been found. */
                                if (wagon[recordIdx].netWeight == wagon[index].netWeight)
                                {
                                    /* The weights remained the same. */
                                    volume.Last().Destination[0] = wagon[index].destination;
                                    volume.Last().detachmentTime = wagon[index].detachmentTime;
                                                                        
                                }
                                else if (wagon[recordIdx].netWeight < wagon[index].netWeight)
                                {
                                    /* Weight has been added at an intermediate locations. */
                                    volume[volume.Count() - 1].Destination[0] = wagon[index].destination;
                                    volume[volume.Count() - 1].detachmentTime = wagon[index].detachmentTime;
                                    volume.Last().Destination[0] = wagon[index].destination;
                                                                        
                                }
                                else
                                {
                                    /* Weight has been removed at an intermediate locations. */
                                    volume[volume.Count() - 1].Destination[0] = wagon[index].destination;
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
                    if (volume.Last().wagonID.Equals(volume[volume.Count() - 1].wagonID) &&
                        volume.Last().Origin.Equals(volume[volume.Count() - 1].Destination) &&
                        !volume.Last().Destination.Equals(volume[volume.Count() - 1].Origin) &&
                        volume.Last().netWeight == volume[volume.Count() - 1].netWeight &&
                        volume.Last().netWeight > 0)
                    {
                        /* Update the previous volume destination. */
                        int previousIdx = volume.Count() - 1;   // maybe -1
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
        /// <returns>List of volume movements</returns>
        private static List<volumeMovement> combineVolumeMovements(List<volumeMovement> volume)
        {
            /* Create a new list of volumes that will be returned. */
            List<volumeMovement> newVolume = new List<volumeMovement>();

            /* Initialise the indecies and counters. */
            int volumeIdx = 0;
            int current = volumeIdx;
            int next = current + 1;

            /* Initialise the wagon strings. */
            string trainID = "";
            string wagonID = "";
            trainOperator trainOperator = trainOperator.Unknown;
            trainCommodity commodity = trainCommodity.Unknown;

            string Origin = "";
            List<string> originLocation = new List<string>();
            string Via = "";
            List<string> viaLocation = new List<string>();
            string Destination = "";
            List<string> destinationLocation = new List<string>();

            DateTime attachmentTime = new DateTime(2000, 1, 1);
            DateTime detachmentTime = new DateTime(2000, 1, 1);

            double netWeight = 0;
            double grossWeight = 0;
            
            /* loop through the existing volumes to see if any need to be combined. */
            for (volumeIdx = 0; volumeIdx < volume.Count() - 1; volumeIdx++)
            {
               
                current = volumeIdx;
                next = current + 1;

                /* Locate the last volume movement that is equal */
                while (Math.Abs(volume[current].netWeight - volume[next].netWeight) < weightThreshold &&
                       volume[current].netWeight > 0)
                {
                    next++;
                    if (next == volume.Count())
                    {
                        break;
                    }
                }
                next--;

                /* This corrects for combining two wagon movements that are just return journeys
                 * with the same weight. 
                 */
                if (volume[current].Origin[0].Equals(volume[next].Destination[0]))
                {
                    /* If the last wagon movement in the journey is at least 2 or more movements 
                     * later, then they should still be combined 
                     */
                    if (next - current > 1)
                        next++;

                    /* The next wagon movement should remain a seperate movement. */
                    if (next != current)
                        next--;
                }

                /* Populate the volume properties with the required details. */
                wagonID = volume[current].wagonID;
                trainID = volume[current].trainID;
                trainOperator = volume[current].trainOperator;
                commodity = volume[current].commodity;

                Origin = volume[current].Origin[0];
                if (volume[current].Via.Equals(volume[next].Via))
                {
                    Via = "";
                }
                else
                {
                    Via = volume[next].Origin[0];
                }
                Destination = volume[next].Destination[0];
                netWeight = volume[current].netWeight;
                grossWeight = volume[current].grossWeight;
                attachmentTime = volume[current].attachmentTime;
                detachmentTime = volume[current].detachmentTime;

                /* Populate initial location codes. */
                originLocation = new List<string> { Origin, "Unknown Region", "Unknown State", "Unknown Area" };
                viaLocation = new List<string> { Via, "Unknown Region", "Unknown State", "Unknown Area" };
                destinationLocation = new List<string> { Destination, "Unknown Region", "Unknown State", "Unknown Area" };
                                
                /* Create the volume itme and add to the list. */
                volumeMovement item = new volumeMovement(trainID, trainOperator, commodity, wagonID, originLocation, viaLocation, destinationLocation, 
                    netWeight, grossWeight, attachmentTime, detachmentTime);
                newVolume.Add(item);

                volumeIdx = next;


            }

            return newVolume;

        }

        /// <summary>
        /// Combine the wagon movements so that the total volume for each leg of the journey 
        /// is provided. This will primarily be used to process the wagon data for the Volume Model.
        /// Note: This will indicate a significantly higher total volume for full journies.
        /// </summary>
        /// <param name="wagon">A list of wagon movements.</param>
        /// <returns>A list of volume movements</returns>
        private static List<volumeMovement> combineWagonMovementsAlternateMethod(List<wagonDetails> wagon)
        {
            /* Initialise the volume list */
            List<volumeMovement> volume = new List<volumeMovement>();
            int searchIdx = 0;
                        
            /* Search through all wagon movements */
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

                    while (getTrainType(wagon[recordIdx].TrainID).Equals(getTrainType(wagon[searchIdx].TrainID)) &&
                        wagon[searchIdx - 1].detachmentTime.AddHours(attatchThreshold) > wagon[searchIdx].attachmentTime &&
                        !wagon[searchIdx].plannedDestination.Equals(wagon[searchIdx].destination))
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
                            volume.Last().Origin[0] = wagon[recordIdx].origin;
                            volume.Last().attachmentTime = wagon[recordIdx].attachmentTime;
                            searchIdx++;
                            recordIdx++;

                        }
                        else if (wagon[searchIdx - 1].wagonID.Equals(wagon[searchIdx].wagonID) &&
                                wagon[recordIdx].plannedDestination.Equals(wagon[searchIdx].plannedDestination) &&
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
                    item = new volumeMovement(wagon[recordIdx].TrainID, wagon[recordIdx].trainOperator, wagon[recordIdx].commodity, wagon[recordIdx].wagonID,
                        wagon[recordIdx].origin, wagon[recordIdx].plannedDestination, wagon[recordIdx].destination, wagon[recordIdx].netWeight, wagon[recordIdx].grossWeight,
                        wagon[recordIdx].attachmentTime, wagon[recordIdx].detachmentTime);
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

                        if (getTrainType(wagon[recordIdx].TrainID).Equals(getTrainType(wagon[index].TrainID)) &&
                            wagon[recordIdx].wagonID.Equals(wagon[index].wagonID))
                        {

                            if (index <= recordIdx + 1)
                            {
                                if (Math.Abs(wagon[recordIdx].netWeight - wagon[index].netWeight) < volumeModelWeightThreshold)
                                {
                                    /* The weight is within a set threshold and is considered to have remained the same. */
                                    volume.Last().Destination[0] = wagon[index].destination;
                                    volume.Last().detachmentTime = wagon[index].detachmentTime;
                                    
                                }
                                else if (wagon[recordIdx].netWeight < wagon[index].netWeight)
                                {
                                    /* Weight has been added at the intermediate destination. */
                                    volume.Last().Destination[0] = wagon[index].destination;
                                    volume.Last().detachmentTime = wagon[index].detachmentTime;
                                    
                                    item = new volumeMovement(wagon[recordIdx].TrainID, wagon[recordIdx].trainOperator, wagon[recordIdx].commodity, wagon[recordIdx].wagonID,
                                        wagon[index].origin, wagon[index].plannedDestination, wagon[searchIdx].destination, wagon[index].netWeight - wagon[recordIdx].netWeight, wagon[index].grossWeight - wagon[recordIdx].grossWeight,
                                        wagon[index].attachmentTime, wagon[searchIdx].detachmentTime);
                                    volume.Add(item);
                                    
                                }
                                else
                                {
                                    /* Weight has been removed at the intermediate destination. */
                                    volume.Last().Destination[0] = wagon[index].destination;
                                    volume.Last().detachmentTime = wagon[index].detachmentTime;
                                    volume.Last().netWeight = wagon[index].netWeight;
                                    
                                    item = new volumeMovement(wagon[recordIdx].TrainID, wagon[recordIdx].trainOperator, wagon[recordIdx].commodity, wagon[recordIdx].wagonID,
                                        wagon[recordIdx].origin, wagon[index].plannedDestination, wagon[recordIdx].destination, wagon[recordIdx].netWeight - wagon[index].netWeight, wagon[recordIdx].grossWeight - wagon[index].grossWeight,
                                        wagon[recordIdx].attachmentTime, wagon[recordIdx].detachmentTime);
                                    volume.Add(item);
                                    
                                }
                            }
                            else
                            {
                                /* Multiple intermediate locations have been found. */
                                if (Math.Abs(wagon[recordIdx].netWeight - wagon[index].netWeight) < volumeModelWeightThreshold)
                                {
                                    /* The weight is within a set threshold and is considered to have remained the same. */
                                    volume.Last().Destination[0] = wagon[index].destination;
                                    volume.Last().OriginDestination = volume.Last().Origin[0] + "-" + volume.Last().Destination[0];
                                    volume.Last().detachmentTime = wagon[index].detachmentTime;
                                    
                                }
                                else if (wagon[recordIdx].netWeight < wagon[index].netWeight)
                                {
                                    /* Weight has been added at an intermediate locations. */
                                    volume[volume.Count() - 1].Destination[0] = wagon[index].destination;
                                    volume[volume.Count() - 1].OriginDestination = volume[volume.Count() - 1].Origin[0] + "-" + volume.Last().Destination[0];
                                    volume[volume.Count() - 1].detachmentTime = wagon[index].detachmentTime;
                                    volume.Last().Destination[0] = wagon[index].destination;
                                    volume.Last().OriginDestination = volume.Last().Origin[0] + "-" + volume.Last().Destination[0];
                                    
                                }
                                else
                                {
                                    /* Weight has been removed at an intermediate locations. */
                                    volume[volume.Count() - 1].Destination[0] = wagon[index].destination;
                                    volume[volume.Count() - 1].OriginDestination = volume[volume.Count() - 1].Origin[0] + "-" + volume.Last().Destination[0];
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

             

            }

            return volume;
        }

        /// <summary>
        /// Relabel the Origin-Destination label as the origin - destination of the total 
        /// volume while retaining each movement involved.
        /// </summary>
        /// <param name="volume">A list of volume movements</param>
        /// <returns>A list of volume movements</returns>
        private static List<volumeMovement> combineVolumeMovementsAlternateMethod(List<volumeMovement> volume)
        {
            /* Track the origin of the volume. */
            string Origin = "";
            
            for (int index = 0; index < volume.Count(); index++)
            {
                /* Flags indicating location or opertar need to be updated. */
                bool locationChange = false;
                bool operatorChange = false;
                                
                if (volume[index].netWeight > 0)
                {
                    int searchIdx = index+1;

                    /* Determine the final destination of the volume and track the first origin location. */
                    while(volume[searchIdx].wagonID.Equals(volume[index].wagonID) &&
                        volume[index].netWeight > 0 &&
                        volume[searchIdx].netWeight == volume[index].netWeight &&
                        volume[searchIdx-1].Destination[0].Equals(volume[searchIdx].Origin[0]))
                    {
                        locationChange = true;
                        Origin = volume[index].Origin[0];

                        /* Interail will sometimes transport SCT wagons. Convert these operators to SCT. */
                        if (volume[searchIdx].trainOperator.Equals(trainOperator.SCT))
                            operatorChange = true;

                        searchIdx++;
                    }

                    if (locationChange)
                    {
                        for (int i = index; i < searchIdx; i++)
                        {
                            /* re-mapp the actual origin destination of the volume for each movement that is involved. */
                            volume[i].OriginDestination = Origin + "-" + volume[searchIdx - 1].Destination[0];
                            /* This maintains the physicsal origin destination of each movement while still labeling 
                             * each movement with the full journey origin destination pair. 
                             */

                            /* Convert Interail operator to SCT if required. */
                            if (volume[i].trainOperator.Equals(trainOperator.Interail) && operatorChange)
                                volume[i].trainOperator = trainOperator.SCT;
                        }
                        index = searchIdx - 1;
                    }

                }
                
            }
                   
            return volume;
        }
                
        /// <summary>
        /// Read the excel file that has mapped the ARTC location codes 
        /// to location names and ABS statistical regions. These values 
        /// are then stored in a location dictionary that can be accessed 
        /// through out the program.
        /// </summary>
        public static void readGeoLocationCodes()
        {
            /* Check the file is not open */
            string filename = FileOperations.geoLocationFile;
            Tools.isFileOpen(filename);

            /* Read the lines */
            string[] lines = System.IO.File.ReadAllLines(filename);
            char[] delimeters = { ',','\t' };
            string[] fields = null;

            /* Seperate the fields. */
            int headerCount = 0;
                        
            foreach (string line in lines)
            {
                /* Seperate each record into each field */
                fields = line.Split(delimeters);

                /* ignore the first two lines as header information. */
                if (headerCount >= 2)
                    if (!FileOperations.locationDictionary.ContainsKey(fields[0]))
                        FileOperations.locationDictionary.Add(fields[0], new List<string> { fields[3], fields[4], fields[5], fields[15] });

                /*
                 * 3:  Location
                 * 4:  SA4 Region
                 * 5:  State
                 * 15: Location Area
                 */

                headerCount++;
            }
            
        }

        /// <summary>
        /// Map the location codes to the location name, states and regions. This function must be called 
        /// after readGeoLocationCodes() to ensure the dictionary elements have been populated.
        /// 
        /// This function is only required when its neccessary to write the volume to file before 
        /// combining movements.
        /// </summary>
        /// <param name="volume">A list of volume objects that require location mapping</param>
        /// <returns>The resuling list of volume objects with the location codes mapped to location names, states and regions.</returns>
        public static List<volumeMovement> populateLocations(List<volumeMovement> volume)
        {
            List<string> dictionary = new List<string>();

            foreach (volumeMovement item in volume)
            {
                /* Map the Origin location code. */
                if (FileOperations.locationDictionary.TryGetValue(item.Origin[0], out dictionary))
                    item.Origin = new List<string> { dictionary[0], dictionary[1], dictionary[2], dictionary[3] };
                else
                    item.Origin = new List<string> { item.Origin[0], "Unknown Region", "Unknown State", "Unknown Area" };

                /* Map the Intermediate location code. */
                if (item.Via[0] != "")
                {
                    if (FileOperations.locationDictionary.TryGetValue(item.Via[0], out dictionary))
                        item.Via = new List<string> { dictionary[0], dictionary[1], dictionary[2], dictionary[3] };
                    else
                        item.Via = new List<string> { item.Via[0], "Unknown Region", "Unknown State", "Unknown Area" };
                }
                else
                {
                    item.Via = new List<string> { "", "", "", "" };
                }

                /* Map the Destination location code. */
                if (FileOperations.locationDictionary.TryGetValue(item.Destination[0], out dictionary))
                    item.Destination = new List<string> { dictionary[0], dictionary[1], dictionary[2], dictionary[3] };
                else
                    item.Destination = new List<string> { item.Destination[0], "Unknown Region", "Unknown State", "Unknown Area" };

                /* Map the Origin-Destination description */
                string origin = item.OriginDestination.Substring(0, 3);
                string destination = item.OriginDestination.Substring(4, 3);

                if (FileOperations.locationDictionary.TryGetValue(origin, out dictionary))
                    origin = dictionary[3];
                else
                    origin = item.Destination[0];

                if (FileOperations.locationDictionary.TryGetValue(destination, out dictionary))
                    destination = dictionary[3];
                else
                    destination = item.Destination[0];

                /* Replace the loation code with the location area. */
                item.OriginDestination = origin + " - " + destination;

            }

            return volume;
        }

        /// <summary>
        /// Helper function to display the initial wagon movements.
        /// </summary>
        /// <param name="wagon"></param>
        public static void displayWagonMovement(wagonDetails wagon)
        {
            Console.WriteLine("{0}  {1}  {2}  {3}  {4}  {5}  {6}  {7}  {8}  {9}", 
                wagon.TrainID, wagon.trainOperator, wagon.commodity,wagon.wagonID, wagon.origin, wagon.plannedDestination, 
                wagon.destination, wagon.attachmentTime, wagon.detachmentTime, wagon.netWeight);

        }

        /// <summary>
        /// Helper function to display the volume movements.
        /// </summary>
        /// <param name="volume"></param>
        public static void displayVolumeMovement(volumeMovement volume)
        {
            Console.WriteLine("{0}  {1}  {2}  {3}  {4}  {5}  {6}  {7}  {8}  {9}",
                volume.trainID, volume.trainOperator, volume.commodity, volume.wagonID, volume.Origin[0], volume.Via[0],
                volume.Destination[0], volume.attachmentTime, volume.detachmentTime, volume.netWeight);

        }

        /// <summary>
        /// Extract the train type from the train ID. Its assumed that the 2nd and 3rd characters 
        /// are indicative of the train type, ie Melb-Bris (MB), Mel-Perth (MP), Syd-Mel (SM).
        /// </summary>
        /// <param name="trainID">The string representing the train ID.</param>
        /// <returns>The 2 character string representing the train type.</returns>
        public static string getTrainType(string trainID)
        {
            /* Assume train type is given by the 2nd and 3rd characters in the train ID. */
            if (trainID.Count() >= 3)
                return trainID.Substring(1, 2);
            else
                return trainID;
        
        }

    } // end of program class

} // end of namespace
