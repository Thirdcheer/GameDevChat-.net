using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace ChatApplication.Hubs
{
    public class ChatHub : Hub
    {

        static ConcurrentDictionary<string, string> dic = new ConcurrentDictionary<string, string>();
        ChatRooms rooms;

        public void getRooms(string servername)
        {
            rooms = new ChatRooms(servername);

        }
        
        public async Task Join(string roomName)
        {
            await Groups.Add(Context.ConnectionId, roomName);
            Clients.Group(roomName).addChatMessage(Context.User.Identity.Name + " joined.");
        }

        public Task LeaveRoom(string roomName)
        {
            return Groups.Remove(Context.ConnectionId, roomName);
        }

        public void Send(string name, string message, string roomname)
        {
            Trace.WriteLine("From: " + name + " message: " + message + " in " + roomname);
            Clients.Group(roomname).broadcastMessage(name, message, roomname);
        }

        public void SendToSpecific(string name, string message, string to)
        {
            Clients.Caller.broadcastMessage(name, message);
            Clients.Client(dic[to]).broadcastMessage(name, message);
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
                Clients.Group(roomname).enters(name);
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
    }
}