using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModel
{

    public class SmsConfiguration
    {
        public string ApiUrl { get; set; }
        public string ApiKey { get; set; }
        public string SenderNumber { get; set; }
    }

    public class SmsRequest
    {
        public string ReceiverNumber { get; set; }
        public string Message { get; set; }
        public string ApiKey { get; set; }
        public string SenderNumber { get; set; }
    }
}
