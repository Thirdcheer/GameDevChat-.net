using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChatApplication.Models
{
    public class EventModel
    {
        public int id { get; set; }
        public string subject { get; set; }
        public string description { get; set; }
        public DateTime startdate { get; set; }
        public DateTime enddate { get; set; }
        public string color { get; set; }
        public bool fullday { get; set; }
        public int serverid { get; set; }
    }
}