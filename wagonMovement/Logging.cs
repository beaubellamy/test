/*
 * This class provides logging capability to any function in the 
 * wagonMovement namespace to write a string to the log file.
 */

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace wagonMovement
{
    /// <summary>
    /// Log Class to provide logging capability to the classes within the wagonMovement namespace.
    /// </summary>
    public class Logging
    {
        string logFilename = "";
        StreamWriter log = null;

        /// <summary>
        /// Logging Constructor.
        /// The path and logFile are instantiated 
        /// </summary>
        public Logging()
        {
            //string path = @"S:\Corporate Strategy\Market Analysis & Forecasts\Volume\Wagon movement analysis\Wagon Movement Source\";
            string path = @"C:\Users\Beau\Documents\ARTC\Wagon Volumes\";      // Home location
            logFilename = path + "log_" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".csv";
            openFileForWriting();
        }

        /// <summary>
        /// Logging Constructor.
        /// </summary>
        /// <param name="filename">The logging filename</param>
        public Logging(string filename)
        {
            this.logFilename = filename;
            openFileForWriting();
        }

        /// <summary>
        /// Open the logging file ready for writing to.
        /// </summary>
        /// <returns>True if the file was opened succesfully.</returns>
        private bool openFileForWriting()
        {
            try
            {
                FileStream fs = new FileStream(logFilename, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                log = new StreamWriter(fs);
                return true;
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("Could not Open log file: " + this.logFilename + ".", "Check spelling and folder location exists.",
                                       System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Exclamation);
                log = null;
                return false;
            }

        }

        /// <summary>
        /// Write a line to the log file.
        /// </summary>
        /// <param name="line">line to write to file.</param>
        public bool WriteLine(string line)
        {
            if (log == null)
                openFileForWriting();

            try
            {
                log.WriteLine(DateTime.Now.ToLongTimeString() + ", " + DateTime.Now.ToShortDateString() + ", " + line);
                log.Flush();
                return true;
            }
            catch// (Exception e)
            {
                System.Windows.Forms.MessageBox.Show("Could not Write to log file: " + this.logFilename + ".", "Error logging to file",
                                       System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Exclamation);
                return false;
            }

        }

    }

}