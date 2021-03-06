﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NuoDb.Data.Client;
using System.Configuration;
using System.Data;
using System.Data.Common;

namespace ChatApplication.Models
{
    public class DataLayer
    {
        NuoDbConnection con = new NuoDbConnection();
        string login_table = "users";
        string server_table = "servers";
        string server_user_table = "serveruser";
        string room_table = "rooms";
        string messages_table = "messages";
        string events_table = "events";

        public DataLayer()
        {
            con.ConnectionString = ConfigurationManager.ConnectionStrings["test"].ConnectionString;
        }

        public UserModel login(string email,string password)
        {
            UserModel user = new UserModel();
            DataTable dt = new DataTable();
            DataSet ds = new DataSet();
            string sql = "select * from " + login_table +  " where email='" + email + "' and password='" + password + "'";
            NuoDbDataAdapter da = new NuoDbDataAdapter(sql, con);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            foreach (DataRow row in dt.Rows)
            {
                user.id = Convert.ToInt32(row["id"].ToString());
                user.username = row["username"].ToString();
                user.password = row["password"].ToString();
            }
            return user;
        }

        public UserModel register(string username, string email, string password)
        {
            UserModel user = new UserModel();
            DataTable dt = new DataTable();
            DataSet ds = new DataSet();
            string sql = String.Format("select 1 from {0} where email='{1}'", login_table, email);
            NuoDbDataAdapter da = new NuoDbDataAdapter(sql, con);

            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];

            if(dt.Rows.Count == 0)
            {
                NuoDbCommand nuocmd = new NuoDbCommand(String.Format("insert {0} (username, email, password, userrole) values ('{1}', '{2}', '{3}', 'User')", login_table, username, email, password), con);
                con.Open();
                nuocmd.ExecuteNonQuery();
                con.Close();

                sql = String.Format("select {0} from {1} where email='{2}'", "id", login_table, email);
                da = new NuoDbDataAdapter(sql, con);
                ds.Reset();
                da.Fill(ds);
                dt = ds.Tables[0];
                foreach (DataRow row in dt.Rows)
                {
                    user.id = Convert.ToInt32(row["id"].ToString());
                }
                user.username = username;
       
                return user;
            }
            else
            {
                return null;
            }
        }

        public ServerModel addServer(string name, string username)
        {
            ServerModel server = new ServerModel(name);
            UserModel user = getUser(username);
            

            if (!serverExists(name))
            {
                DataTable dt = new DataTable();
                DataSet ds = new DataSet();
                NuoDbCommand nuocmd = new NuoDbCommand(String.Format("insert {0} (name, created, joinstring) values ('{1}', '{2}', '{3}')", server_table, server.name, server.created, server.joinString), con);
                con.Open();
                nuocmd.ExecuteNonQuery();
                con.Close();

                string sql = String.Format("select {0} from {1} where name='{2}'", "id", server_table, name);
                NuoDbDataAdapter da = new NuoDbDataAdapter(sql, con);
                ds.Reset();
                da.Fill(ds);
                dt = ds.Tables[0];
                foreach (DataRow row in dt.Rows)
                {
                    server.id = Convert.ToInt32(row["id"].ToString());
                }

                nuocmd = new NuoDbCommand(String.Format("insert {0} (serverid, userid) values ('{1}', '{2}')", server_user_table, server.id, user.id), con);
                con.Open();
                nuocmd.ExecuteNonQuery();
                nuocmd = new NuoDbCommand(String.Format("update {0} set userrole = 'Admin' where username = '{1}'", login_table, user.username), con);
                nuocmd.ExecuteNonQuery();
                con.Close();
                addRoom("general", server.name);
                return server;
            }
            else
            {
                return null;
            }

        }

        public bool serverExists(string servername)
        {
            ServerModel server = new ServerModel(servername);
            DataTable dt = new DataTable();
            DataSet ds = new DataSet();
            string sql = String.Format("select 1 from {0} where name='{1}'", server_table, servername);
            NuoDbDataAdapter da = new NuoDbDataAdapter(sql, con);
            

            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];

            if (dt.Rows.Count == 0)
                return false;
            else
                return true;
        }

        public UserModel getUser(string name)
        {
            UserModel user = new UserModel();

            DataTable dt = new DataTable();
            DataSet ds = new DataSet();
            string sql = String.Format("select * from {0} where username='{1}'", login_table, name);
            NuoDbDataAdapter da = new NuoDbDataAdapter(sql, con);

            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];

            foreach(DataRow row in dt.Rows)
            {
                user.id = Convert.ToInt32(row["id"]);
                user.username = row["username"].ToString();
                user.role = row["userrole"].ToString();
            }

            return user;
        }

        public UserModel getUserByEmail(string email)
        {

            UserModel user = new UserModel();

            DataTable dt = new DataTable();
            DataSet ds = new DataSet();
            string sql = String.Format("select * from {0} where email='{1}'", login_table, email);
            NuoDbDataAdapter da = new NuoDbDataAdapter(sql, con);

            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];

            foreach (DataRow row in dt.Rows)
            {
                user.id = Convert.ToInt32(row["id"]);
                user.username = row["username"].ToString();
                user.role = row["userrole"].ToString();
            }

            return user;
        }

        public UserModel getUser(int id)
        {
            UserModel user = new UserModel();

            DataTable dt = new DataTable();
            DataSet ds = new DataSet();
            string sql = String.Format("select * from {0} where id='{1}'", login_table, id);
            NuoDbDataAdapter da = new NuoDbDataAdapter(sql, con);

            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];

            foreach (DataRow row in dt.Rows)
            {
                user.id = Convert.ToInt32(row["id"]);
                user.username = row["username"].ToString();
                user.role = row["userrole"].ToString();
            }

            return user;
        }

        public List<string> checkEmail()
        {
            UserModel user = new UserModel();

            DataTable dt = new DataTable();
            DataSet ds = new DataSet();
            string sql = String.Format("select email from {0}", login_table);
            NuoDbDataAdapter da = new NuoDbDataAdapter(sql, con);

            List<string> emails = new List<string>();

            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];

            foreach (DataRow row in dt.Rows)
            {
                emails.Add(row["email"].ToString());
            }

            return emails;
        }

        public ServerModel getUserServer(string username)
        {
            UserModel user = getUser(username);
            DataTable dt = new DataTable();
            DataSet ds = new DataSet();
            string sql = String.Format("select serverid from {0} where userid={1}", server_user_table, user.id);
            NuoDbDataAdapter da = new NuoDbDataAdapter(sql, con);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];

            int serverid = -1;

            foreach (DataRow row in dt.Rows)
            {
                serverid = Convert.ToInt32(row["serverid"]);
            }

            if(serverid == -1)
            {
                return null;
            }

            dt = new DataTable();
            ds = new DataSet();
            sql = String.Format("select * from {0} where id='{1}'", server_table, serverid);
            da = new NuoDbDataAdapter(sql, con);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            ServerModel server = new ServerModel();

            foreach (DataRow row in dt.Rows)
            {
                server.id = Convert.ToInt32(row["id"]);
                server.name = row["name"].ToString();
            }
            return server;
        }

        public ServerModel getServer(string name)
        {
            DataTable dt = new DataTable();
            DataSet ds = new DataSet();
            string sql = String.Format("select * from {0} where name='{1}'", server_table, name);
            NuoDbDataAdapter da = new NuoDbDataAdapter(sql, con);

            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];

            ServerModel server = new ServerModel();

            foreach (DataRow row in dt.Rows)
            {
                server.id = Convert.ToInt32(row["id"]);
                server.name = row["name"].ToString();
                server.joinString = row["joinstring"].ToString();
            }

            return server;
        }

        public string getServerName(string user)
        {
            
            DataTable dt = new DataTable();
            DataSet ds = new DataSet();
            string sql = String.Format("select name from {0} join {1} on {0}.id = {1}.serverid join {2} on {1}.userid = {2}.id where users.username = '{3}'", server_table, server_user_table, login_table, user);

            NuoDbDataAdapter da = new NuoDbDataAdapter(sql, con);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];

            string serverName = "";
            foreach(DataRow  row in dt.Rows)
            {
                serverName = row["name"].ToString();
            }

            return serverName;
        }

        public List<EventModel> getEvents(string username)
        {
            DataTable dt = new DataTable();
            DataSet ds = new DataSet();
            ServerModel server = getUserServer(username);
            List<EventModel> eventsList = new List<EventModel>();
            string sql = String.Format("select id, subject, description, startdate, enddate, color, fullday, serverid from {0} where serverid = {1}", events_table, server.id);
            NuoDbDataAdapter da = new NuoDbDataAdapter(sql, con);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            foreach (DataRow row in dt.Rows)
            {
                EventModel ev = new EventModel();
                ev.id = Convert.ToInt32(row["id"].ToString());
                ev.subject = row["subject"].ToString();
                ev.description = row["description"].ToString();
                ev.startdate = Convert.ToDateTime(row["startdate"]);
                ev.enddate = Convert.ToDateTime(row["enddate"]);
                ev.color = row["color"].ToString();
                ev.fullday = Convert.ToBoolean(row["fullday"]);
                ev.serverid = Convert.ToInt32(row["serverid"]);
                eventsList.Add(ev);
            }

            return eventsList;
        }

        public EventModel getEvent(int id)
        {
            DataTable dt = new DataTable();
            DataSet ds = new DataSet();
            string sql = String.Format("select * from {0} where {0}.id = '{1}'", events_table, id);

            NuoDbDataAdapter da = new NuoDbDataAdapter(sql, con);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];

            if (dt.Rows.Count == 0)
            {
                return null;
            }
            else
            {
                EventModel ev = new EventModel();
                foreach (DataRow row in dt.Rows)
                {
                    ev.id = Convert.ToInt32(row["id"].ToString());
                    ev.subject = row["subject"].ToString();
                    ev.description = row["description"].ToString();
                    ev.startdate = Convert.ToDateTime(row["startdate"]);
                    ev.enddate = Convert.ToDateTime(row["enddate"]);
                    ev.color = row["color"].ToString();
                    ev.fullday = Convert.ToBoolean(row["fullday"]);
                    ev.serverid = Convert.ToInt32(row["serverid"]);
                }
                return ev;
            }
        }

        public void updateEvent(EventModel ev)
        {
            NuoDbCommand nuocmd = new NuoDbCommand(String.Format("update {0} set subject = '{2}', startdate = '{3}', enddate='{4}', description='{5}', fullday='{6}', color='{7}', serverid='{8}' where {0}.id = {1}", events_table, ev.id, ev.subject, ev.startdate, ev.enddate, ev.description, ev.fullday, ev.color, ev.serverid), con);
            con.Open();
            nuocmd.ExecuteNonQuery();
            con.Close();
        }

        public void addEvent(EventModel ev)
        {
            NuoDbCommand nuocmd = new NuoDbCommand(String.Format("insert {0} (subject, startdate, enddate, description, fullday, color, serverid) values ('{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')", events_table, ev.subject, ev.startdate, ev.enddate, ev.description, ev.fullday, ev.color, ev.serverid), con);
            con.Open();
            nuocmd.ExecuteNonQuery();
            con.Close();
        }

        public void deleteEvent(int eventID)
        {
            NuoDbCommand nuocmd = new NuoDbCommand(String.Format("delete from {0} where {0}.id = {1}", events_table, eventID), con);
            con.Open();
            nuocmd.ExecuteNonQuery();
            con.Close();
        }


        public ServerModel joinServer(string joinstring, string username)
        {
            ServerModel server = new ServerModel();
            DataTable dt = new DataTable(); 
            DataSet ds = new DataSet();
            string sql = String.Format("select * from {0} where joinstring='{1}'", server_table, joinstring);
            NuoDbDataAdapter da = new NuoDbDataAdapter(sql, con);
            UserModel user = getUser(username);
           
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];

            if (dt.Rows.Count == 0)
            {
                return null;
            }
            else
            {
                foreach (DataRow row in dt.Rows)
                {
                    server.id = Convert.ToInt32(row["id"]);
                    server.name = row["name"].ToString();
                }

                NuoDbCommand nuocmd = new NuoDbCommand(String.Format("insert {0} (serverid, userid) values ('{1}', '{2}')", server_user_table, server.id, user.id), con);
                con.Open();
                nuocmd.ExecuteNonQuery();
                con.Close();

                return server;
            }
        }

        public List<string> getRoomsNames(string serverName)
        {
            ServerModel server = getServer(serverName);
            UserModel user = new UserModel();
            DataTable dt = new DataTable();
            DataSet ds = new DataSet();

            string sql = String.Format("select name from {0} where serverid='{1}'", room_table, server.id);
            NuoDbDataAdapter da = new NuoDbDataAdapter(sql, con);
            List<string> roomNames = new List<string>();
            RoomModel room = new RoomModel();
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];

            foreach (DataRow row in dt.Rows)
            {
                roomNames.Add(row["name"].ToString() + "-" + serverName);
            }

            return roomNames;
        }

        public RoomModel getRoom(string roomname, string servername)
        {
            ServerModel server = getServer(servername);
            DataTable dt = new DataTable();
            DataSet ds = new DataSet();

            string sql = String.Format("select * from {0} where serverid='{1}' and name='{2}'", room_table, server.id, roomname);
            NuoDbDataAdapter da = new NuoDbDataAdapter(sql, con);
            RoomModel room = new RoomModel();
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];

            foreach (DataRow row in dt.Rows)
            {
                room.name = row["name"].ToString() + "-" + servername;
                room.id = Convert.ToInt32(row["id"]);
                room.serverId = Convert.ToInt32(row["serverid"]);
            }

            return room;
        }

        public RoomModel addRoom(string name, string serverName)
       {
            ServerModel server = getServer(serverName);
            UserModel user = new UserModel();
            DataTable dt = new DataTable();
            DataSet ds = new DataSet();

            string sql = String.Format("select name from {0} where serverid='{1}'", room_table, server.id);
            NuoDbDataAdapter da = new NuoDbDataAdapter(sql, con);
            List<string> roomNames = getRoomsNames(serverName);
            RoomModel room = new RoomModel();
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];

            /*foreach (DataRow row in dt.Rows)
            {
                roomNames.Add(row["name"].ToString().Replace("-"+serverName, ""));
            }*/

            if (roomNames.Contains(name + "-" + serverName))
            {
                return null;
            }
            else
            {
                room.name = name;
                room.serverId = server.id;
                NuoDbCommand nuocmd = new NuoDbCommand(String.Format("insert {0} (name, serverid) values ('{1}', '{2}')", room_table, name, server.id), con);
                con.Open();
                nuocmd.ExecuteNonQuery();
                con.Close();

                return room;
            }
        }

        public List<UserModel> getusers(int id)
        {
            DataTable dt = new DataTable();
            DataSet ds = new DataSet();
            List<UserModel> userlist = new List<UserModel>();
            string sql = "select * from login where id<>"+id;
            NuoDbDataAdapter da = new NuoDbDataAdapter(sql, con);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            foreach (DataRow row in dt.Rows)
            {
                UserModel user = new UserModel();
                user.id = Convert.ToInt32(row["id"].ToString());
                user.username = row["username"].ToString();
                user.password = row["password"].ToString();
                user.role = row["userrole"].ToString();
                userlist.Add(user);
            }
            return userlist;
        }
        

        public void saveMessage(string message, string roomname, string sender)
        {
            UserModel user = getUser(sender);
            ServerModel server = getUserServer(sender);
            RoomModel room = getRoom(roomname.Replace(server.name, ""), server.name);
            DataTable dt = new DataTable();
            DataSet ds = new DataSet();
            NuoDbCommand nuocmd = new NuoDbCommand(String.Format("insert {0} (roomid, message, msgdate, senderid) values ('{1}', '{2}', '{3}', '{4}')", messages_table, room.id, message, DateTime.Now, user.id ), con);
            con.Open();
            nuocmd.ExecuteNonQuery();
            con.Close();
        }

        public List<MessageModel> getMessages(string roomname, string servername)
        {
            DataTable dt = new DataTable();
            DataSet ds = new DataSet();
            ServerModel server = getServer(servername);
            RoomModel room = getRoom(roomname, server.name);
            List<MessageModel> messagesList = new List<MessageModel>();
            string sql = String.Format("select * from {0} where {0}.roomid = {1}",  messages_table, room.id);
            NuoDbDataAdapter da = new NuoDbDataAdapter(sql, con);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            foreach (DataRow row in dt.Rows)
            {
                MessageModel message = new MessageModel();
                message.id = Convert.ToInt32(row["id"].ToString());
                message.roomid = Convert.ToInt32(row["roomid"]);
                message.message = row["message"].ToString();
                message.msgdate = Convert.ToDateTime(row["msgdate"]);
                message.senderid = Convert.ToInt32(row["senderid"]);
                messagesList.Add(message);
            }

            //List<MessageModel> lista = messagesList.OrderBy(o => o.msgdate).ToList();
            //messagesList.Reverse();


            return messagesList ;
        }

        public int getMessagesLastRow(string roomname, string servername)
        {
            DataTable dt = new DataTable();
            DataSet ds = new DataSet();
            ServerModel server = getServer(servername);
            RoomModel room = getRoom(roomname, server.name);
            List<MessageModel> messagesList = new List<MessageModel>();
            string sql = String.Format("select * from {0} where {0}.roomid = {1} order by {0}.id desc limit 1", messages_table, room.id);
            NuoDbDataAdapter da = new NuoDbDataAdapter(sql, con);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            int count = 0;
            foreach (DataRow row in dt.Rows)
            {
                count = Convert.ToInt32(row["id"]);
            }

            return count;
        }

        public List<MessageModel> getMessagesAboveId(string roomname, string servername, int id, int count)
        {
            DataTable dt = new DataTable();
            DataSet ds = new DataSet();
            ServerModel server = getServer(servername);
            RoomModel room = getRoom(roomname, server.name);
            List<MessageModel> messagesList = new List<MessageModel>();
            if(id == 0)
            {
                id = getMessagesLastRow(roomname, servername) + 1;
            }

            string sql = String.Format("select * from {0} where {0}.roomid = {1} and {0}.id < {2} order by {0}.id desc limit {3}", messages_table, room.id, id, count);
            NuoDbDataAdapter da = new NuoDbDataAdapter(sql, con);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            foreach (DataRow row in dt.Rows)
            {
                MessageModel message = new MessageModel();
                message.id = Convert.ToInt32(row["id"].ToString());
                message.roomid = Convert.ToInt32(row["roomid"]);
                message.message = row["message"].ToString();
                message.msgdate = Convert.ToDateTime(row["msgdate"]);
                message.senderid = Convert.ToInt32(row["senderid"]);
                messagesList.Add(message);
            }

            //List<MessageModel> lista = messagesList.OrderBy(o => o.msgdate).ToList();
           // messagesList.Reverse();


            return messagesList;

        }


    }
}