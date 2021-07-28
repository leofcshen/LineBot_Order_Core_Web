using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LineBot_Order_Core_Web.Models
{
    public class EventMessage
    {
        public string id { get; set; }
        public string type { get; set; }
        public string text { get; set; }
    }
}
