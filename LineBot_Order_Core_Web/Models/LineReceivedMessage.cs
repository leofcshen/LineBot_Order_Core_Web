using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LineBot_Order_Core_Web.Models
{
    public class LineReceivedMessage
    {
        public List<Event> events;
        public LineReceivedMessage()
        {
            events = new List<Event>();
        }
    }
}
