using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Web.Mvc;
using ChatApplication.Models;

namespace ChatApplication.Hubs
{
    public class ChatHub : Hub
    {

        static ConcurrentDictionary<string, string> dic = new ConcurrentDictionary<string, string>();
        ChatRooms rooms;
        DataLayer dl = new DataLayer();

        public void getRooms(string servername)
        {
            rooms = new ChatRooms(servername);
        }
        
        public async Task Join(string roomName)
        {
            await Groups.Add(Context.ConnectionId, roomName);
            //Clients.Group(roomName).addChatMessage(Context.User.Identity.Name + " joined.");
            //Clients.Group(roomName).roomenter(Context.User.Identity.Name, roomName);
        }

        public Task Leave(string roomName)
        {
           // Clients.Group(roomName).roomleft(Context.User.Identity.Name, roomName);
            return Groups.Remove(Context.ConnectionId, roomName);
        }

        public string processMessage(string message)
        {
            return message.Replace("'", "''");
        }
        public void Send(string name, string message, string roomname)
        {
            Trace.WriteLine("From: " + name + " message: " + message + " in " + roomname);
            dl.saveMessage(processMessage(message), roomname, name);
            Clients.Group(roomname).broadcastMessage(name, message, roomname, String.Format("{0:g}", DateTime.Now), false);
        }

        public void SendToSpecific(string name, string message, string to)
        {
            Clients.Caller.broadcastMessage(name, message);
            Clients.Client(dic[to]).broadcastMessage(name, message, DateTime.Now, false);
        }

        public void Sendimage(string name, string imagelink, string roomname)
        {
            dl.saveMessage("imagesrc: " + processMessage(imagelink), roomname, name);
            Clients.Group(roomname).broadcastImage(name, imagelink, roomname, String.Format("{0:g}", DateTime.Now), false);
        }

        public void Sendfile(string name, string filesrc, string roomname)
        {
            dl.saveMessage("filesrc: " + processMessage(filesrc), roomname, name);
            Clients.Group(roomname).broadcastFile(name, filesrc, roomname, String.Format("{0:g}", DateTime.Now), false);
        }

        public void Notify(string name, string id, string roomname)
        {
            var exists = dic.FirstOrDefault(x => x.Key == name);
            if (exists.Key == null)
            {
                dic.TryAdd(name, id);
                foreach (KeyValuePair<String, String> entry in dic)
                {
                    Clients.Caller.online(entry.Key);
                }
                Clients.Group(roomname).enters(name, roomname);
            }
        }
        

        public override Task OnDisconnected(bool stopCalled)
        {
            var name = dic.FirstOrDefault(x => x.Value == Context.ConnectionId.ToString());
            string s;
            dic.TryRemove(name.Key, out s);
            Trace.WriteLine(Context.ConnectionId + " - disconnected");
            Clients.All.disconnect(name.Key);
            return base.OnDisconnected(stopCalled);
        }

        public void Load(string roomname, string servername, string connID)
        {
            List<MessageModel> messages = dl.getMessages(roomname, servername);

            foreach(MessageModel message in messages)
            {
                if (message.message.Contains("imagesrc:"))
                {
                    Clients.Client(connID).broadcastImage(dl.getUser(message.senderid).username, message.message.Replace("imagesrc: ", ""), roomname, String.Format("{0:g}", message.msgdate), true);

                }
                else if (message.message.Contains("filesrc:"))
                {
                    Clients.Client(connID).broadcastFile(dl.getUser(message.senderid).username, message.message.Replace("filesrc: ", ""), roomname, String.Format("{0:g}", message.msgdate), true);

                }
                else
                {
                    Clients.Client(connID).broadcastMessage(dl.getUser(message.senderid).username, message.message, roomname, String.Format("{0:g}", message.msgdate), true);
                }
            }
            Clients.Client(connID).scrollDown();
        }


        public int Load(string roomname, string servername, string connID, int from, int count)
        {
            List<MessageModel> messages = dl.getMessagesAboveId(roomname, servername, from, count);
            int lastid = 0;

            foreach (MessageModel message in messages)
            {
                if (message.message.Contains("imagesrc:"))
                {
                    Clients.Client(connID).broadcastImage(dl.getUser(message.senderid).username, message.message.Replace("imagesrc: ", ""), roomname, String.Format("{0:g}", message.msgdate), true);

                }
                else if (message.message.Contains("filesrc:"))
                {
                    Clients.Client(connID).broadcastFile(dl.getUser(message.senderid).username, message.message.Replace("filesrc: ", ""), roomname, String.Format("{0:g}", message.msgdate), true);

                }
                else
                {
                    Clients.Client(connID).broadcastMessage(dl.getUser(message.senderid).username, message.message, roomname, String.Format("{0:g}", message.msgdate), true);
                }

                lastid = message.id;

            }
            if(from == 0)
            {
                Clients.Client(connID).scrollDown();
            }
            
            return lastid;
        }

        
    }
}