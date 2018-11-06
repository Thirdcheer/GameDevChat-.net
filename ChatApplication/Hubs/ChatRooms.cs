using ChatApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChatApplication.Hubs
{
    public class ChatRooms
    {
        static List<string> rooms;
        DataLayer dl = new DataLayer();
        string servername = "";
        public ChatRooms(string servername)
        {
            this.servername = servername;
            rooms = dl.getRoomsNames(servername);
        }
        
       public void refresh(string servername)
        {
            rooms = dl.getRoomsNames(servername);
        }

        public bool Exists(string name)
        {
            return rooms.Contains(name);
        }

        public List<string> GetAll()
        {
            return rooms;
        }
    }
}