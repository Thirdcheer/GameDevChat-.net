using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NuoDb.Data.Client;
using System.Configuration;
using System.Data;
namespace ChatApplication.Models
{
    public class DataLayer
    {
        NuoDbConnection con = new NuoDbConnection();
        public DataLayer()
        {
            con.ConnectionString = ConfigurationManager.ConnectionStrings["test"].ConnectionString;
        }
        public UserModel login(string email,string password)
        {
            UserModel user = new UserModel();
            DataTable dt = new DataTable();
            DataSet ds = new DataSet();
            string sql = "select * from login where username='" + email + "' and password='" + password + "'";
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