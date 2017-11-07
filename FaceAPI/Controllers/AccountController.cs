using FaceAPI.Web.Helper;
using FaceAPI.Web.Models;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using FaceAPI.Web.Service;
using System.Web.Security;

namespace FaceAPI.Web.Controllers
{
    public class AccountController : BaseController
    {
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(AccountModel.LoginModel model)
        {
            if (model.Account == "admin" && model.Password == "accuvally")
            {
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
        [AllowAnonymous]
        public ActionResult Logout()
        {
            //清除所有的 session
            Session.RemoveAll();
            //清除Session中的資料
            Session.Abandon();
            //登出表單驗證
            FormsAuthentication.SignOut();
            //導至登入頁
            return RedirectToAction("Login");
        }

    }
}