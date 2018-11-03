using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChatApplication.Models
{
    public class ServerModel
    {
        public int id { get; set; }
        public string name { get; set; }
        public DateTime created { get; set; }
        public string joinString{ get; set; }

        public ServerModel(string name)
        {
            this.name = name;
            created = DateTime.Now;
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            joinString = new string(Enumerable.Repeat(chars, 15)
              .Select(s => s[random.Next(s.Length)]).ToArray());
       
        }

        public ServerModel()
        {
        }
    }
}