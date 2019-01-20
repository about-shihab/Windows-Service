using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Configuration;
using Oracle.DataAccess.Client;
using PdfWindowsService.DAL;
using System.IO;
using System.Transactions;

namespace PdfWindowsService
{
    public partial class Service_Pdf : ServiceBase
    {
        RequestMastGateway requestMastGateway = new RequestMastGateway();

        public Service_Pdf()
        {
            InitializeComponent();
        }
        public void OnDebug()
        {
            OnStart(null);
        }
        protected override void OnStart(string[] args)
        {
            this.WriteToFile("\n -------------------------------------------------------------------------------\n");
            this.WriteToFile("Pdf copy Service started {0}");
            //ProcessPdfRequest();
            this.ScheduleService();
        }

        protected override void OnStop()
        {
            this.WriteToFile("Pdf copy Service stopped {0}\n");
            this.Schedular.Dispose();
        }


        private Timer Schedular;

        public void ScheduleService()
        {
            try
            {
                Schedular = new Timer(new TimerCallback(SchedularCallback));
                string mode = ConfigurationManager.AppSettings["Mode"].ToUpper();
                this.WriteToFile("Service Mode: " + mode + " {0}");
                


                //Set the Default Time.
                DateTime scheduledTime = DateTime.MinValue;

                if (mode.ToUpper() == "INTERVAL")
                {
                    //Get the Interval in Minutes from AppSettings.
                    int intervalMinutes = Convert.ToInt32(ConfigurationManager.AppSettings["IntervalMinutes"]);
                    this.WriteToFile("Service Interval time " + intervalMinutes + " minutes");

                    //Set the Scheduled Time by adding the Interval to Current Time.
                    scheduledTime = DateTime.Now.AddMinutes(intervalMinutes);
                    if (DateTime.Now > scheduledTime)
                    {
                        //If Scheduled Time is passed set Schedule for the next Interval.
                        scheduledTime = scheduledTime.AddMinutes(intervalMinutes);
                    }
                }

                TimeSpan timeSpan = scheduledTime.Subtract(DateTime.Now);
                string schedule = string.Format("{0} day(s) {1} hour(s) {2} minute(s) {3} seconds(s)", timeSpan.Days, timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);

                this.WriteToFile("Scheduled Service wil run at " + DateTime.Now.AddMinutes(Convert.ToInt32(ConfigurationManager.AppSettings["IntervalMinutes"]))+" -- Time remaining : "+schedule);

                //Get the difference in Minutes between the Scheduled and Current Time.
                int dueTime = Convert.ToInt32(timeSpan.TotalMilliseconds);

                //Change the Timer's Due Time.
                Schedular.Change(dueTime, Timeout.Infinite);
            }
            catch (Exception ex)
            {
                WriteToFile("Pdf Copy Service Error on: {0} " + ex.Message + ex.StackTrace);

                //Stop the Windows Service.
                using (System.ServiceProcess.ServiceController serviceController = new System.ServiceProcess.ServiceController("SimpleService"))
                {
                    serviceController.Stop();
                }
            }
        }


        private void SchedularCallback(object e)
        {

            try
            {
                ProcessPdfRequest();

                this.ScheduleService();
            }
            catch (Exception ex)
            {
                WriteToFile("Pdf Copy Service Error on: {0} " + ex.Message + ex.StackTrace);

                //Stop the Windows Service.
                using (System.ServiceProcess.ServiceController serviceController = new System.ServiceProcess.ServiceController("Pdf Copy Log Service"))
                {
                    serviceController.Stop();
                }
            }
        }

        public void ProcessPdfRequest()
        {
            try
            {
                List<VirtualRequest> vrequests = new List<VirtualRequest>();
                vrequests = requestMastGateway.GetAllUnprocessedPdf1();
                string destFolder = ConfigurationManager.AppSettings["DestinationFolder"].ToString();
                if (!Directory.Exists(destFolder))
                    Directory.CreateDirectory(destFolder);
                foreach (VirtualRequest vr in vrequests)
                {
                    string fileName =@vr.LC_NO + "@" + vr.BILL_REF_NO + ".pdf";
                    string navLink = vr.NAVIGATION_LINK;
                    //string navLink = @"http://www.un.org/Depts/Cartographic/map/profile/banglade.pdf";


                    if (navLink.Contains("http://") || navLink.Contains("https://"))
                    {
                        requestMastGateway.SavePdfHttp(navLink, destFolder, fileName);
                    }
                    else
                    {
                        requestMastGateway.SavePdf(navLink, destFolder, fileName);
                    }


                    //InsertFileNameandLocation in sebl_pi_doc
                    int queue_id_out = requestMastGateway.InsertDocReg(vr, fileName, destFolder);
                    

                    //updateProcessFlagByRequestID
                    SEBL_PI_REQUEST_MAST pi_request_mast = requestMastGateway.GetRequestByID(vr.REQUEST_ID);
                    requestMastGateway.UpdateRequestMast(pi_request_mast, queue_id_out);
                    WriteToFile(fileName + " is inserted with queue Id " + queue_id_out +" and Copied at " + DateTime.Now);

                }
                this.WriteToFile("\n -------------------------------------------------------------------------------\n");

            }
            catch (Exception ex)
            {

                throw ex;
            }
                
         }
            
        

        private void WriteToFile(string text)
        {
            string folderPath = ConfigurationManager.AppSettings["DestinationFolder"]+"\\Service Log";
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string path = folderPath + "\\ServiceLog.txt";
            using (StreamWriter writer = new StreamWriter(path, true))
            {
                writer.WriteLine(string.Format(text, DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt")));
                writer.Close();
            }
        }


        
    }
}
