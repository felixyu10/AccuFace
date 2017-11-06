using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Web;
using Microsoft.ProjectOxford.Face;

namespace FaceAPI.Web.Models
{
    public class FaceModel
    {
        public string personGroupId = ConfigurationManager.AppSettings["PersonGroupId"];
        private static string serviceKey = ConfigurationManager.AppSettings["FaceserviceKey"];
        private static string serviceEndPoint = ConfigurationManager.AppSettings["FaceServiceEndPoint"];
        public IFaceServiceClient serviceClient = new FaceServiceClient(serviceKey, serviceEndPoint);
    }
}