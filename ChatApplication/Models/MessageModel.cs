using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChatApplication.Models
{
    public class MessageModel
    {
        public int id { get; set; }
        public int roomid { get; set; }
        public string message { get; set; }
        public DateTime msgdate { get; set; }
        public int senderid { get; set; }
    }
}