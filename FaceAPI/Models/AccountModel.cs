using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace FaceAPI.Web.Models
{
    public class AccountModel
    {
        public class LoginModel
        {
            [DisplayName("帳號")]
            [Required(ErrorMessage = "必填欄位")]
            public string Account{ get; set; }

            [DisplayName("密碼")]
            [Required(ErrorMessage = "必填欄位")]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            public string ReturnUrl { get; set; }
        }
    }
}