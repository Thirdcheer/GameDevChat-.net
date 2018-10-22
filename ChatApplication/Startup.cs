using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Microsoft.AspNet.Identity;
using System.Web.Helpers;
using System.Security.Claims;

[assembly: OwinStartup(typeof(ChatApplication.Startup))]

namespace ChatApplication
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }


        
    }
}
