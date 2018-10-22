using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ChatApplication.Models;
using System.Web.UI.WebControls;
using System.Web.Security;

namespace ChatApplication.Controllers
{
    public class HomeController : Controller
    {
        DataLayer dl = new DataLayer();

        [Authorize]
        public ActionResult Index()
        {
            ViewBag.Username = System.Web.HttpContext.Current.User.Identity.Name;
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
                return RedirectToAction("Index");
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