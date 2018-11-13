using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ChatApplication.Models;
using System.Web.UI.WebControls;
using System.Web.Security;
using System.IO;

namespace ChatApplication.Controllers
{
    public class HomeController : Controller
    {
        DataLayer dl = new DataLayer();

        [HttpPost]
        public ActionResult Index(FormCollection fc) 
        {
            ViewBag.Username = System.Web.HttpContext.Current.User.Identity.Name;
            ViewBag.Servername = dl.getServerName(ViewBag.Username);

            string name = fc["roomname"].ToString();
            int capacity = Convert.ToInt32(fc["capacity"]);
            RoomModel room = dl.addRoom(name, ViewBag.Servername, capacity);
            if (room != null)
            {
                ViewData["status"] = 1;
                ViewData["msg"] = "room added...";
                return RedirectToAction("Index");
            }
            else
            {
                ViewData["status"] = 2;
                ViewData["msg"] = "cannot add room";
                return View();
            }

        }

        [HttpPost]
        public ActionResult UploadFile(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
                try
                {
                    string path = Path.Combine(Server.MapPath("~/Uploaded"),
                                               Path.GetFileName(file.FileName));
                    file.SaveAs(path);
                    ViewBag.Message = "File uploaded successfully";
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "ERROR:" + ex.Message.ToString();
                }
            else
            {
                ViewBag.Message = "You have not specified a file.";
            }
            return RedirectToAction("Index");
        }

        [Authorize]
        public ActionResult Index()
        {
            ViewBag.Username = System.Web.HttpContext.Current.User.Identity.Name;
            ViewBag.Servername = dl.getServerName(ViewBag.Username);
            ViewBag.Rooms = dl.getRoomsNames(ViewBag.Servername);
            return View();  
        }
       
        public ActionResult serverjoin()
        {
            return View();
        }

        public ActionResult login()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }

        public ActionResult register()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public ActionResult login(FormCollection fc)
        {
            string username = fc["username"].ToString();
            string password = fc["password"].ToString();
            UserModel user = dl.login(username, password);

            if (user.userid > 0)
            {
                ViewData["status"] = 1;
                ViewData["msg"] = "login Successful...";
                ViewBag.Username = user.username;
                FormsAuthentication.SetAuthCookie(username, false);
                Session["username"] = user.username;
                Session["userid"] = user.userid.ToString();
                return RedirectToAction("Index");
            }
            else
            {

                ViewData["status"] = 2;
                ViewData["msg"] = "invalid Email or Password...";
                return View();
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult register(FormCollection fc)
        {
            string email = fc["email"].ToString();
            string username = fc["username"].ToString();
            string password = fc["password"].ToString();

            UserModel user = dl.register(username, email, password);
            
            if (user != null)
            {
                ViewData["status"] = 1;
                ViewData["msg"] = "user registered";
                Session["username"] = user.username;
                Session["userid"] = user.userid.ToString();
                FormsAuthentication.SetAuthCookie(username, false);
                return RedirectToAction("serverjoin", "Home");
            }
            else
            {
                ViewData["status"] = 2;
                ViewData["msg"] = "User with that email already exists";
                return View();
            }

        }

        [Authorize]
        [HttpPost]
        public ActionResult serverjoin(FormCollection fc)
        {
            if (fc.AllKeys.Contains("name"))
            {
                ServerModel server = dl.addServer(fc["name"], User.Identity.Name);
                if(server != null)
                {
                    ViewData["status"] = 1;
                    ViewData["msg"] = "server added";
                    return RedirectToAction("Index", "Home");
                }
            }
            else if (fc.AllKeys.Contains("joinstring"))
            {
                ServerModel server = dl.joinServer(fc["joinstring"], User.Identity.Name);
                ViewData["status"] = 1;
                ViewData["msg"] = "joined to server " + server.name;
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [Authorize]
        [HttpPost]
        public JsonResult friendlist()
        {
            int id = Convert.ToInt32(Session["userid"].ToString());
            List<UserModel> users = dl.getusers(id);
            List<ListItem> userlist = new List<ListItem>();
            foreach (var item in users)
            {
                userlist.Add(new ListItem
                {
                    Value = item.username.ToString(),
                    Text = item.username.ToString()

                });
            }
            return Json(userlist);
        }
    }
}