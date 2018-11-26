using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChatApplication.Models
{
    public class RoomModel
    {
        public int id { get; set; }
        public string name { get; set; }
        public int serverId { get; set; }
    }
}