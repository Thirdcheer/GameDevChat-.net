﻿using System;
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
        public ActionResult Index(string roomname) 
        {
            ViewBag.Username = System.Web.HttpContext.Current.User.Identity.Name;
            ViewBag.Servername = dl.getServerName(ViewBag.Username);
            ViewBag.Userrole = dl.getUser(ViewBag.Username).role;
            ViewBag.Rooms = dl.getRoomsNames(ViewBag.Servername);

            var status = true;
            
            RoomModel room = dl.addRoom(roomname, ViewBag.Servername);
            if (room != null)
            {
                ViewData["status"] = 1;
                ViewData["msg"] = "room added...";
               
            }
            else
            {
                ViewData["status"] = 2;
                ViewData["msg"] = "cannot add room";
                status = false;
            }
            return RedirectToAction("Index");

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
            if (dl.getUserServer(User.Identity.Name) == null)
            {
                return RedirectToAction("serverjoin");
            }

            ViewBag.Username = System.Web.HttpContext.Current.User.Identity.Name;
            ViewBag.Servername = dl.getServerName(ViewBag.Username);
            ViewBag.Userrole = dl.getUser(System.Web.HttpContext.Current.User.Identity.Name).role;
            if(ViewBag.Userrole == "Admin")
            {
                ViewBag.joinstring = dl.getServer(ViewBag.Servername).joinString;
            }
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
            string email = fc["email"].ToString();
            string password = fc["password"].ToString();
            UserModel user = dl.login(email, password);

            if (user.id > 0)
            {
                ViewData["status"] = 1;
                ViewData["msg"] = "login Successful...";
                ViewBag.Username = user.username;
                FormsAuthentication.SetAuthCookie(dl.getUserByEmail(email).username, false);
                Session["username"] = user.username;
                Session["userid"] = user.id.ToString();
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
                Session["userid"] = user.id.ToString();
                FormsAuthentication.SetAuthCookie(username, false);
                ViewBag.NewUser = true;
                return RedirectToAction("serverjoin", "Home");
            }
            else
            {
                ViewData["status"] = 2;
                ViewData["msg"] = "User with that email already exists";
                ViewBag.NewUser = false;
                return View();
            }

        }

        public JsonResult GetEvents()
        {
                var events = dl.getEvents(System.Web.HttpContext.Current.User.Identity.Name);
                return new JsonResult { Data = events, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            
        }

        [HttpPost]
        public JsonResult emailExists(string email)
        {
            List<string> emails = dl.checkEmail();
            bool exists = false;

            if (emails.Contains(email)){
                exists = true;
            }

            return new JsonResult { Data = exists, JsonRequestBehavior = JsonRequestBehavior.AllowGet};
        }

        [HttpPost]
        public JsonResult serverExists(string servername)
        {
            bool exists = dl.serverExists(servername);
            return new JsonResult { Data = exists, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }


        [HttpPost]
        public JsonResult SaveEvent(EventModel e)
                {
                    var status = false;
                        if (e.id > 0)
                        {
                            //Update the event
                            var v =  dl.getEvent(e.id);
                            if (v != null)
                            {
                                v.subject = e.subject;
                                v.startdate = e.startdate;
                                v.enddate = e.enddate;
                                v.description = e.description;
                                v.fullday = e.fullday;
                                v.color = e.color;
                                v.serverid = dl.getUserServer(System.Web.HttpContext.Current.User.Identity.Name).id;
                                dl.updateEvent(v);
                            }
                        }
                        else
                        {
                e.serverid = dl.getUserServer(System.Web.HttpContext.Current.User.Identity.Name).id;
                dl.addEvent(e);
                        }
                        status = true;
                    
                    return new JsonResult { Data = new { status = status } };
                }

        [HttpPost]
        public JsonResult DeleteEvent(int eventID)
        {
            var status = false;
                var v = dl.getEvent(eventID);
                if (v != null)
                {
                    dl.deleteEvent(eventID);
                    status = true;
                }
            
            return new JsonResult { Data = new { status = status } };
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
                if(server == null)
                {
                    ViewData["msg"] = "Server with that joinsstring does not exists";
                    return View();
                }
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