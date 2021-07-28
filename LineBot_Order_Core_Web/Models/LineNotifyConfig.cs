using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LineBot_Order_Core_Web.Models
{
    public class LineNotifyConfig
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string CallbackUrl { get; set; }
        public string AuthorizeUrl { get; set; }
        public string TokenUrl { get; set; }
        public string State { get; set; }
        public string SuccessUrl { get; set; }
        public string NotifyUrl { get; set; }
    }
}
