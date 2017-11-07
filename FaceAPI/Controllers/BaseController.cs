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

namespace FaceAPI.Web.Controllers
{
    [Authorize]
    public class BaseController : Controller
    {
       

    }
}