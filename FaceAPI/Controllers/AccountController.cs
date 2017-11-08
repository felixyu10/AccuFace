using FaceAPI.Web.Common.Cache;
using FaceAPI.Web.Common.Enum;
using FaceAPI.Web.Models;
using System;
using System.Collections.Generic;
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
            Dictionary<string, string> accountDic = new Dictionary<string, string>();
            accountDic.Add("admin", "accuvally");
            accountDic.Add("felix", "accuvally");
            accountDic.Add("microsoft", "poiuzxcv");

            if (accountDic.ContainsKey(model.Account) && accountDic[model.Account] == model.Password)
            {
                if (model.Account == "admin" || model.Account == "microsoft")
                {
                    int login = RedisCache.GetCache<int>(CacheNameEnum.FaceDetection, "");
                    login++;
                    RedisCache.SetCache(CacheNameEnum.FaceDetection, model.Account, login, new TimeSpan(100, 0, 0, 0));
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