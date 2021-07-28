using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LineBot_Order_Core_Web.Models
{
    public class Event
    {
        public string type { get; set; }
        public Source source { get; set; }
        public EventMessage message { get; set; }
        public string replyToken { get; set; }
        public Event()
        {
            source = new Source();
            message = new EventMessage();
        }
    }
}
