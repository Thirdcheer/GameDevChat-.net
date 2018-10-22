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
        string login_table = "login";
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
                NuoDbCommand nuocmd = new NuoDbCommand(String.Format("insert {0} (username, email, password) values ('{1}', '{2}', '{3}')", login_table, username, email, password), con);
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