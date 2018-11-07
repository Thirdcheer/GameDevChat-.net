using System;
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
        public DataLayer()
        {
            con.ConnectionString = ConfigurationManager.ConnectionStrings["test"].ConnectionString;
        }

        public UserModel login(string username,string password)
        {
            UserModel user = new UserModel();
            DataTable dt = new DataTable();
            DataSet ds = new DataSet();
            string sql = "select * from " + login_table +  " where username='" + username + "' and password='" + password + "'";
            NuoDbDataAdapter da = new NuoDbDataAdapter(sql, con);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            foreach (DataRow row in dt.Rows)
            {
                user.userid = Convert.ToInt32(row["id"].ToString());
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
                    user.userid = Convert.ToInt32(row["id"].ToString());
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
            DataTable dt = new DataTable();
            DataSet ds = new DataSet();
            string sql = String.Format("select 1 from {0} where name='{1}'", server_table, name);
            NuoDbDataAdapter da = new NuoDbDataAdapter(sql, con);

            UserModel user = getUser(username);

            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];

            if (dt.Rows.Count == 0)
            {
                NuoDbCommand nuocmd = new NuoDbCommand(String.Format("insert {0} (name, created, joinstring) values ('{1}', '{2}', '{3}')", server_table, server.name, server.created, server.joinString), con);
                con.Open();
                nuocmd.ExecuteNonQuery();
                con.Close();

                sql = String.Format("select {0} from {1} where name='{2}'", "id", server_table, name);
                da = new NuoDbDataAdapter(sql, con);
                ds.Reset();
                da.Fill(ds);
                dt = ds.Tables[0];
                foreach (DataRow row in dt.Rows)
                {
                    server.id = Convert.ToInt32(row["id"].ToString());
                }

                nuocmd = new NuoDbCommand(String.Format("insert {0} (serverid, userid) values ('{1}', '{2}')", server_user_table, server.id, user.userid), con);
                con.Open();
                nuocmd.ExecuteNonQuery();
                nuocmd = new NuoDbCommand(String.Format("update {0} set userrole = 'Admin' where username = '{1}'", login_table, user.username), con);
                nuocmd.ExecuteNonQuery();
                con.Close();
                addRoom("general", server.name, 20);
                return server;
            }
            else
            {
                return null;
            }

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
                user.userid = Convert.ToInt32(row["id"]);
                user.username = row["username"].ToString();
            }

            return user;
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

                NuoDbCommand nuocmd = new NuoDbCommand(String.Format("insert {0} (serverid, userid) values ('{1}', '{2}')", server_user_table, server.id, user.userid), con);
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

       public RoomModel addRoom(string name, string serverName, int capacity)
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

            foreach (DataRow row in dt.Rows)
            {
                roomNames.Add(row["name"].ToString());
            }

            if (roomNames.Contains(name))
            {
                return null;
            }
            else
            {
                room.name = name;
                room.serverId = server.id;
                NuoDbCommand nuocmd = new NuoDbCommand(String.Format("insert {0} (name, capacity, serverid) values ('{1}', '{2}', '{3}')", room_table, name, capacity, server.id), con);
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
                user.userid = Convert.ToInt32(row["id"].ToString());
                user.username = row["username"].ToString();
                user.password = row["password"].ToString();
                userlist.Add(user);
            }
            return userlist;
        }

    }
}