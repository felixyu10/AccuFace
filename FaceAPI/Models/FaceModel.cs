using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Web;
using Microsoft.ProjectOxford.Face;
using FaceAPI.Web.Common.Helper;

namespace FaceAPI.Web.Models
{
    public class FaceModel
    {
        public string personGroupId = AppHelper.GetWebConfigSetting("PersonGroupId");
        private static string serviceKey = AppHelper.GetWebConfigSetting("FaceserviceKey");
        private static string serviceEndPoint = AppHelper.GetWebConfigSetting("FaceServiceEndPoint");
        public IFaceServiceClient serviceClient = new FaceServiceClient(serviceKey, serviceEndPoint);
    }
}