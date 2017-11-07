using FaceAPI.Web.Common.Cache;
using FaceAPI.Web.Common.Enum;
using FaceAPI.Web.Models;
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace FaceAPI.Web.Controllers
{
    [AllowAnonymous]
    public class AccountController : BaseController
    {
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(AccountModel.LoginModel model)
        {
            if ((model.Account == "admin" || model.Account == "felix") && model.Password == "accuvally")
            {
                if (model.Account == "admin" && model.Password == "accuvally")
                {

                    int login = RedisCache.GetCache<int>(CacheNameEnum.FaceDetection, "");
                    login++;
                    RedisCache.SetCache(CacheNameEnum.FaceDetection, "", login, new TimeSpan(365, 0, 0, 0));
                }


                FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1,
                    model.Account,
                    DateTime.UtcNow.AddHours(8),
                    DateTime.UtcNow.AddHours(8).AddDays(3),
                    true,
                    model.Account,
                    FormsAuthentication.FormsCookiePath);

                string encTicket = FormsAuthentication.Encrypt(ticket);
                var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket);

                cookie.HttpOnly = true;
                Response.Cookies.Add(cookie);
            }
            return RedirectToAction("Index", "FaceDetection");
        }

        [HttpGet]
        public ActionResult Logout()
        {
            Session.RemoveAll();
            Session.Abandon();
            FormsAuthentication.SignOut();

            return RedirectToAction("Login");
        }

    }
}