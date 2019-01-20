using Oracle.DataAccess.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
//using System.Data.OracleClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PdfWindowsService.DAL
{
    public class RequestMastGateway
    {
        string connectionString = ConfigurationManager.ConnectionStrings["ULTIMUS"].ConnectionString;
        public DataTable GetAllUnprocessedRequests()
        {
            DataTable dtab = new DataTable();
            string sql = "select * from SEBL_PI_REQUEST_MAST where process_flag=0";
            using (OracleConnection connection =
                new OracleConnection())
            {
                connection.ConnectionString =
                    connectionString;

                try
                {
                    connection.Open();
                    OracleCommand command = new OracleCommand(sql, connection);
                    command.CommandType = CommandType.Text;
                    OracleDataReader dr = command.ExecuteReader();

                    //dr.Read();
                    dtab.Load(dr);
                    return dtab;

                }
                catch (OracleException ex)
                {
                    throw ex;
                }
                finally
                {
                    connection.Close();
                }
            }
        }


        public DataTable GetAllPdf(string lc_no, string bill_ref_no)
        {
            DataTable dtab = new DataTable();
            string sql = "select * from tbl_pdf t where t.lc_no='"+lc_no+"' and t.bill_ref_no='"+bill_ref_no+"'";
            using (OracleConnection connection =
                new OracleConnection())
            {
                connection.ConnectionString =
                    connectionString;

                try
                {
                    connection.Open();
                    OracleCommand command = new OracleCommand(sql, connection);
                    command.CommandType = CommandType.Text;
                    OracleDataReader dr = command.ExecuteReader();

                    //dr.Read();
                    dtab.Load(dr);
                    return dtab;

                }
                catch (OracleException ex)
                {
                    throw ex;
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        public List<VirtualRequest> GetAllUnprocessedPdf1()
        {
            List<VirtualRequest> vrs = new List<VirtualRequest>();
            DataTable dtab = this.GetAllUnprocessedRequests();
            foreach (DataRow drRequest in dtab.Rows)
            {
                VirtualRequest vr = new VirtualRequest();

                DataTable dtabPdf = GetAllPdf(drRequest["lc_no"].ToString(), drRequest["bill_ref_no"].ToString());
                foreach (DataRow drpdf in dtabPdf.Rows)
                {
                    vr.REQUEST_ID = Convert.ToInt32(drRequest["REQUEST_ID"]);
                    vr.QUEUE_ID = drRequest["QUEUE_ID"]==DBNull.Value? (int?)null : Convert.ToInt32(drRequest["QUEUE_ID"]);
                    
                    vr.LC_NO = drpdf["lc_no"].ToString();
                    vr.BILL_REF_NO = drpdf["BILL_REF_NO"].ToString();
                    vr.NAVIGATION_LINK = drpdf["NAVIGATION_LINK"].ToString();
                    vrs.Add(vr);
                }
                
                

            }
            return vrs;
        }

        public List<VirtualRequest> GetAllUnprocessedPdf()
        {
            List<VirtualRequest> vrs = new List<VirtualRequest>();
            string sql = "select a.request_id,a.queue_id, a.lc_no, a.bill_ref_no, b.navigation_link from " +
                            "(select * from SEBL_PI_REQUEST_MAST where process_flag=0) a inner join (select * from tbl_pdf) b " +
                             "on a.lc_no=b.lc_no and a.bill_ref_no=b.bill_ref_no";
            using (OracleConnection connection =
                new OracleConnection())
            {
                connection.ConnectionString =
                    connectionString;

                try
                {
                    connection.Open();
                    OracleCommand command = new OracleCommand(sql, connection);
                    command.CommandType = CommandType.Text;
                    OracleDataReader dr = command.ExecuteReader();

                    //dr.Read();
                    while (dr.Read())
                    {
                        VirtualRequest vr = new VirtualRequest();
                        vr.REQUEST_ID = Convert.ToInt32(dr["REQUEST_ID"]);
                        vr.QUEUE_ID = Convert.ToInt32(dr["QUEUE_ID"]);
                        vr.LC_NO = dr["LC_NO"].ToString();
                        vr.BILL_REF_NO = dr["BILL_REF_NO"].ToString();
                        vr.NAVIGATION_LINK = dr["NAVIGATION_LINK"].ToString();
                        vrs.Add(vr);
                    }

                    return vrs;

                }
                catch (OracleException ex)
                {
                    throw ex;
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        public void SavePdf(string sourceLink, string destFolder, string fileName)
        {
            string destLink = destFolder + "\\" + fileName;
            try
            {
                File.Copy(sourceLink, destLink,true);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        //For Ftp 
        public void SaveFromFtp(string inputfilepath, string destFolder, string fileName)
        {
            string ftpfilepath = destFolder + "\\" + fileName;
            string ftphost = "127.0.0.1";
            //here correct hostname or IP of the ftp server to be given  

            string ftpfullpath = "ftp://" + ftphost + ftpfilepath;
            FtpWebRequest ftp = (FtpWebRequest)FtpWebRequest.Create(ftpfullpath);
            ftp.Credentials = new NetworkCredential("userid", "password");
            //userid and password for the ftp server to given  

            ftp.KeepAlive = true;
            ftp.UseBinary = true;
            ftp.Method = WebRequestMethods.Ftp.UploadFile;
            FileStream fs = File.OpenRead(inputfilepath);
            byte[] buffer = new byte[fs.Length];
            fs.Read(buffer, 0, buffer.Length);
            fs.Close();
            Stream ftpstream = ftp.GetRequestStream();
            ftpstream.Write(buffer, 0, buffer.Length);
            ftpstream.Close();
        }

        public void SavePdfHttp(string sourceLink, string destFolder, string fileName)
        {
            string destLink = destFolder + "\\" + fileName;
            
            using (WebClient client = new WebClient())
            {
                  client.DownloadFile(sourceLink,destLink);
                //client.UploadFile(sourceLink,destLink);
            }
        }
        


        public int InsertDocReg(VirtualRequest vr, string fileName, string folderLocation)
        {
            string fileNavigateUrl = folderLocation + @"\" + fileName;
            OracleDataAdapter adp = new OracleDataAdapter();
            using (OracleConnection connection = new OracleConnection())
            {

                connection.ConnectionString = connectionString;
                try
                {

                    connection.Open();
                    OracleCommand command = new OracleCommand();
                    command.Connection = connection;
                    command.CommandText = "pkg_pi.fsp_addup_doc_register";
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("pQUEUE_ID", OracleDbType.Int32,8).Value = null;
                    command.Parameters.Add("pREQUEST_ID", OracleDbType.Int32,8).Value = vr.REQUEST_ID;
                    command.Parameters.Add("pFILE_NM", OracleDbType.Varchar2,500).Value = fileName;
                    command.Parameters.Add("pFILE_NAVIGATE_URL", OracleDbType.Varchar2,500).Value = fileNavigateUrl;
                    command.Parameters.Add("pFOLDER_LOCATION", OracleDbType.Varchar2,500).Value = folderLocation;
                    command.Parameters.Add("pHO_UPLOAD_FLAG", OracleDbType.Int16,1).Value = 1;
                    command.Parameters.Add("pBR_UPLOAD_FLAG", OracleDbType.Int16,1).Value = 0;
                    command.Parameters.Add("pREMARKS", OracleDbType.Varchar2,500).Value = "System Generated";
                    command.Parameters.Add("pSYS_GEN_FLAG", OracleDbType.Char,1 ).Value = 'S';
                    command.Parameters.Add("pAUTH_STATUS_ID", OracleDbType.Varchar2,1).Value = "A";
                    command.Parameters.Add("pqueue_id_out", OracleDbType.Int32, 8).Direction = ParameterDirection.Output;
                    command.Parameters.Add("puser_id", OracleDbType.Varchar2,15).Value = "asif.ho";
                    command.Parameters.Add("perrorcode", OracleDbType.Int32, 5).Direction = ParameterDirection.Output;
                    command.Parameters.Add("perrormsg", OracleDbType.Varchar2, 2000).Direction = ParameterDirection.Output;

                    command.ExecuteNonQuery();
                    int queue_id_out = int.Parse(command.Parameters["pqueue_id_out"].Value.ToString());
                    command.Parameters.Clear();
                    return queue_id_out;
                }
                
                catch (OracleException ex)
                {
                    throw ex;
                }
                finally
                {
                    connection.Close();
                }
            }

        }

        public SEBL_PI_REQUEST_MAST GetRequestByID(int pREQUEST_ID)
        {
            string sql = "select * from SEBL_PI_REQUEST_MAST where REQUEST_ID="+pREQUEST_ID;
            using (OracleConnection connection =
                new OracleConnection())
            {
                connection.ConnectionString =
                    connectionString;

                try
                {
                    connection.Open();
                    OracleCommand command = new OracleCommand(sql, connection);
                    command.CommandType = CommandType.Text;
                    OracleDataReader dr = command.ExecuteReader();
                    
                    SEBL_PI_REQUEST_MAST pi_request = new SEBL_PI_REQUEST_MAST();

                    while (dr.Read())
                    {

                        pi_request.REQUEST_ID = Convert.ToInt32(dr["REQUEST_ID"]);
                        pi_request.BRANCH_ID = Convert.ToString(dr["BRANCH_ID"]);
                        pi_request.TRANS_DATE = dr["TRANS_DATE"] == DBNull.Value ? null : (DateTime?)dr["TRANS_DATE"];
                        pi_request.BATCH_NO = Convert.ToInt32(dr["BATCH_NO"]);
                        pi_request.LC_NO = dr["LC_NO"].ToString();
                        pi_request.BILL_NO = dr["BILL_NO"].ToString();
                        pi_request.BILL_REF_NO = dr["BILL_REF_NO"].ToString();
                        pi_request.BILL_REF_DATE = dr["BILL_REF_DATE"] == DBNull.Value ? null : (DateTime?)dr["BILL_REF_DATE"];
                        pi_request.LODG_ACCPT_DT = dr["LODG_ACCPT_DT"] == DBNull.Value ? null : (DateTime?)dr["LODG_ACCPT_DT"];
                        pi_request.ADVICE_NO = dr["ADVICE_NO"].ToString();
                        pi_request.CURRENCY_ID = dr["CURRENCY_ID"].ToString();
                        pi_request.CUSTOMER_ID = dr["CUSTOMER_ID"].ToString();
                        pi_request.BILL_AMT = dr["BILL_AMT"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["BILL_AMT"]);
                        pi_request.HOGL_AMT = dr["HOGL_AMT"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["HOGL_AMT"]);
                        pi_request.SWIFT_AMT = dr["SWIFT_AMT"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["SWIFT_AMT"]);
                        pi_request.REMBURSE_AMT = dr["REMBURSE_AMT"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["REMBURSE_AMT"]);
                        pi_request.DISCRIP_AMT = dr["DISCRIP_AMT"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["DISCRIP_AMT"]);
                        pi_request.VAT_AMT = dr["VAT_AMT"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["VAT_AMT"]);
                        pi_request.BILL_PREPARED_BY = dr["BILL_PREPARED_BY"].ToString();
                        pi_request.REMARKS = dr["REMARKS"].ToString();
                        pi_request.REQUEST_EVENT_ID = dr["REQUEST_EVENT_ID"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["REQUEST_EVENT_ID"]);
                        pi_request.REQUEST_STATUS_ID = dr["REQUEST_STATUS_ID"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["REQUEST_STATUS_ID"]);
                        pi_request.AUTH_STATUS_ID = dr["AUTH_STATUS_ID"].ToString();
                        pi_request.MAKE_BY = dr["MAKE_BY"].ToString();
                        pi_request.MAKE_DT = dr["MAKE_DT"] == DBNull.Value ? null : (DateTime?)dr["MAKE_DT"];
                        pi_request.AUTH_BY = dr["AUTH_BY"].ToString();
                        pi_request.AUTH_DT = dr["AUTH_DT"] == DBNull.Value ? null : (DateTime?)dr["AUTH_DT"];
                        pi_request.APPROVED_BY = dr["APPROVED_BY"].ToString();
                        pi_request.AUTH_DT = dr["AUTH_DT"] == DBNull.Value ? null : (DateTime?)dr["AUTH_DT"];
                        pi_request.QUEUE_ID = dr["QUEUE_ID"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["QUEUE_ID"]);
                        pi_request.PROCESS_FLAG = dr["PROCESS_FLAG"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["PROCESS_FLAG"]);

                    }
                    return pi_request;

                }
                catch (OracleException ex)
                {
                    throw ex;
                }
                finally
                {
                    connection.Close();
                }
            }
        }
        public void UpdateRequestMast(SEBL_PI_REQUEST_MAST pRequestMast, int pqueue_id_out)
        {
            using (OracleConnection connection = new OracleConnection())
            {
                //string sql = "update SEBL_PI_REQUEST_MAST set PROCESS_FLAG=param1 where REQUEST_ID=param2";
                string sql = "update SEBL_PI_REQUEST_MAST t set t.PROCESS_FLAG=1, t.queue_id="+pqueue_id_out+" where REQUEST_ID=" + pRequestMast.REQUEST_ID;
                connection.ConnectionString = connectionString;
                try
                {

                    connection.Open();
                    OracleCommand command = new OracleCommand(sql,connection);
                    command.CommandType=CommandType.Text;
                    //cmd.Parameters.AddWithValue("param1", 1);
                    command.ExecuteNonQuery();
                }

                catch (OracleException ex)
                {
                    throw ex;
                }
                finally
                {
                    connection.Close();
                }
            }
        }
    }
}
