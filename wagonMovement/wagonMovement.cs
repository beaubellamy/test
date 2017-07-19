
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

using TrainLibrary;
using IOLibrary;

namespace wagonMovement
{

    class Algorithm
    {
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
        public static void processWagonMovements(string filename, string destinationFolder)
        {
            /* Create the Wagon list. */
            List<wagonDetails> wagon = new List<wagonDetails>();

            /* Populate the wagon list with the data from the data file. */
            try
            {
                wagon = FileOperations.readWagonDataFile(filename);
            }
            catch (IOException exception)
            {
                if (wagon.Count() == 0)
                {
                    Tools.messageBox(exception.Message, "File format.");
                    return;
                }
            }

            /* Combine the wagon movements based on planned destination and weights. */
            List<volumeMovement> volume = new List<volumeMovement>();
            volume = combineWagonMovements(wagon);

            /* Combine the volume movements that appear to be continuations of the same wagon. */
            volume = combineVolumeMovements(volume);

            /* Write the wagon details to excel. */
            FileOperations.writeWagonData(wagon, destinationFolder);
            FileOperations.writeVolumeData(volume, destinationFolder);

            
        }

        /// <summary>
        /// Join wagon movements based on wagon ID, planned destination and weight.
        /// </summary>
        /// <param name="wagon">A list of wagon movements.</param>
        /// <returns>A list of volume movements</returns>
        private static List<volumeMovement> combineWagonMovements(List<wagonDetails> wagon)
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
