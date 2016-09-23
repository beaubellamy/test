/*
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace wagonMovement
{
    class Tools
    {

        // Constructor
        public Tools()
        {

        }

        /// <summary>
        /// Display a message box with details about an error or information about some properties.
        /// </summary>
        /// <param name="message">Message to display.</param>
        public void messageBox(string message)
        {
            System.Windows.Forms.MessageBox.Show(message, "Information",
                System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Exclamation);
        }

        /// <summary>
        /// Display a message box with details about an error or information about some properties.
        /// </summary>
        /// <param name="message">Message to display.</param>
        /// <param name="caption">Caption for the message box.</param>
        public void messageBox(string message, string caption)
        {
            System.Windows.Forms.MessageBox.Show(message, caption,
                System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Exclamation);
        }

        /// <summary>
        /// Validate the format of the data file.
        /// </summary>
        /// <param name="fields">All data fields for validation.</param>
        /// <returns>True if all fields are valid.</returns>
        public Boolean validateFileFormat(string[] fields)
        {
            
            Boolean validWagonNumber = false;
            Boolean validTrainNumber = false;
            Boolean validTrainDate = false;
            Boolean validComodity = false;
            Boolean validOriginCode = false;
            Boolean validPlannedDestinationCode = false;
            Boolean validActualDestinationCode = false;
            Boolean validAttachmentTime = false;
            Boolean validDetachmentTime = false;
            Boolean validTareWeight = false;
            Boolean validGrossWeight = false;
            Boolean validDistanceTravelled = false;
            Boolean validWagonSequence = false;
            Boolean validRecordID = false;
            Boolean validWagonMovementCount = false;

            /* Check the number of fields */
            if (fields.Count() != 16)
            {
                this.messageBox("Incorrect number of fields detected in text file.", "Invalid number of fields.");
                return false;
            }
            else
            {
                /* Clean the Fields from escape characters. */
                char[] newDelimeters = { '\'', '"' };
                string[] seperatedFields;
                for (int i = 0; i < fields.Count(); i++)
                {
                    seperatedFields = Regex.Unescape(fields[i]).Split(newDelimeters);
                    if (seperatedFields.Count() == 3)
                        fields[i] = seperatedFields[1];
                }

                /* Check each field and validate. */
                double result = 0;
                DateTime dateResult = DateTime.MinValue;

                // Wagon Class
                /* Wagon class can be a 4 character string or upto 4 numbers. */

                // Wagon number
                result = 0;
                if (double.TryParse(fields[1], out result))
                    validWagonNumber = true;
                else
                    validWagonNumber = false;

                // Train ID
                result = 0;
                if (double.TryParse(fields[2], out result))
                    validTrainNumber = false;
                else
                    validTrainNumber = true;

                // Train Date
                dateResult = DateTime.MinValue;
                if (DateTime.TryParse(fields[3], out dateResult))
                {
                    if (dateResult == DateTime.MinValue)
                        validTrainDate = false;
                    else
                        validTrainDate = true;
                }
                else
                {
                    validTrainDate = false;
                }

                // Commodity
                result = 0;
                if (double.TryParse(fields[4], out result))
                    validComodity = false;
                else
                    validComodity = true;

                // Origin
                result = 0;
                if (double.TryParse(fields[5], out result))
                    validOriginCode = false;
                else
                    if (fields[5].Count() == 3)
                        validOriginCode = true;
                    else
                        validOriginCode = false;

                // Planned Destination
                result = 0;
                if (double.TryParse(fields[6], out result))
                    validPlannedDestinationCode = false;
                else
                    if (fields[6].Count() == 3)
                        validPlannedDestinationCode = true;
                    else
                        validPlannedDestinationCode = false;

                // Destination
                result = 0;
                if (double.TryParse(fields[7], out result))
                    validActualDestinationCode = false;
                else
                    if (fields[7].Count() == 3)
                        validActualDestinationCode = true;
                    else
                        validActualDestinationCode = false;

                // Attatchment time
                dateResult = DateTime.MinValue;
                if (DateTime.TryParse(fields[8], out dateResult))
                {
                    if (dateResult == DateTime.MinValue)
                        validAttachmentTime = false;
                    else
                        validAttachmentTime = true;
                }
                else
                {
                    validAttachmentTime = false;
                }

                // Detatchment time
                dateResult = DateTime.MinValue;
                if (DateTime.TryParse(fields[9], out dateResult))
                {
                    if (dateResult == DateTime.MinValue)
                        validDetachmentTime = false;
                    else
                        validDetachmentTime = true;
                }
                else
                {
                    validDetachmentTime = false;
                }

                // Tare Weight
                result = 0;
                if (double.TryParse(fields[10], out result))
                    validTareWeight = true;
                else
                    validTareWeight = false;

                // Gross Weight
                result = 0;
                if (double.TryParse(fields[11], out result))
                    validGrossWeight = true;
                else
                    validGrossWeight = false;

                // Distance Travelled
                result = 0;
                if (double.TryParse(fields[12], out result))
                    validDistanceTravelled = true;
                else
                    validDistanceTravelled = false;

                // Wagon Sequence
                result = 0;
                if (double.TryParse(fields[13], out result))
                    validWagonSequence = true;
                else
                    validWagonSequence = false;

                // Record ID
                result = 0;
                if (double.TryParse(fields[14], out result))
                    validRecordID = true;
                else
                    validRecordID = false;

                // Wagon Movement Count (same as ID)
                result = 0;
                if (double.TryParse(fields[15], out result))
                    validWagonMovementCount = true;
                else
                    validWagonMovementCount = false;

            }

            /* If all the fields are validated, the file format is valid.  */
            if (validWagonNumber && validTrainNumber && validTrainDate && validComodity &&
                validOriginCode && validPlannedDestinationCode && validActualDestinationCode &&
                validAttachmentTime && validDetachmentTime && validTareWeight && validGrossWeight &&
                validDistanceTravelled && validWagonSequence && validRecordID && validWagonMovementCount)
                return true;
            else
                return false;

        }


        /// <summary>
        /// Function opens a dialog box to browse and select the data file.
        /// </summary>
        /// <returns>The full filename of the data file.</returns>
        public string selectDataFile()
        {

            /* Declare the filename to return. */
            string filename = null;

            /* Create a fileDialog browser to select the data file. */
            OpenFileDialog fileSelectBrowser = new OpenFileDialog();
            /* Set the browser properties. */
            fileSelectBrowser.Title = "Select Volume data file";
            fileSelectBrowser.InitialDirectory =
                @"C:\Users\Beau\Documents\ARTC\Wagon Volumes";
//                @"S:\Corporate Strategy\Market Analysis & Forecasts\Volume\Wagon movement analysis";
            fileSelectBrowser.Filter = "Text Files|*.txt";
            // "All EXCEL FILES (*.xlsx*)|*.xlsx*|All files (*.*)|*.*";
            fileSelectBrowser.FilterIndex = 2;
            fileSelectBrowser.RestoreDirectory = true;
            try
            {
                /* Open the broser and select a file. */
                if (fileSelectBrowser.ShowDialog() == DialogResult.OK)
                {
                    filename = fileSelectBrowser.FileName;
                }
                else
                    return filename;

            }
            catch
            {
                /* If there was a problem with the file, show an error  */
                this.messageBox("Could not Open data file: ", "Failed to open data file.");
                throw;
            }
            return filename;
        }

    }


}
