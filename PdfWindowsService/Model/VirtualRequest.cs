using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfWindowsService.DAL
{
    public class VirtualRequest
    {
        public int REQUEST_ID { get; set; }
        public int? QUEUE_ID { get; set; }
        public string LC_NO { get; set; }

        public string BILL_REF_NO { get; set; }
        public string NAVIGATION_LINK { get; set; }
    }
}
