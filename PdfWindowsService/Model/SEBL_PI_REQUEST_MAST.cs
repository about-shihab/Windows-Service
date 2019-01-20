using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfWindowsService
{
    public class SEBL_PI_REQUEST_MAST
    {
        public int REQUEST_ID { get; set; }

        public string BRANCH_ID { get; set; }
        public DateTime? TRANS_DATE { get; set; }
        public int? BATCH_NO { get; set; }
        public string LC_NO { get; set; }
        public string BILL_NO { get; set; }
        public string BILL_REF_NO { get; set; }
        public DateTime? BILL_REF_DATE { get; set; }
        public DateTime? LODG_ACCPT_DT { get; set; }
        public string ADVICE_NO { get; set; }
        public string CURRENCY_ID { get; set; }
        public string CUSTOMER_ID { get; set; }
        public int? BILL_AMT { get; set; }
        public int? HOGL_AMT { get; set; }
        public int? SWIFT_AMT { get; set; }
        public int? REMBURSE_AMT { get; set; }
        public int? DISCRIP_AMT { get; set; }
        public int? VAT_AMT { get; set; }
        public string BILL_PREPARED_BY { get; set; }
        public string REMARKS { get; set; }
        public int? REQUEST_EVENT_ID { get; set; }
        public int? REQUEST_STATUS_ID { get; set; }
        public string AUTH_STATUS_ID { get; set; }
        public string MAKE_BY { get; set; }
        public DateTime? MAKE_DT { get; set; }
        public string AUTH_BY { get; set; }
        public DateTime? AUTH_DT { get; set; }
        public string APPROVED_BY { get; set; }
        public DateTime? APPROVED_DT { get; set; }
        public int? QUEUE_ID { get; set; }
        public int? PROCESS_FLAG { get; set; }

    }
}
