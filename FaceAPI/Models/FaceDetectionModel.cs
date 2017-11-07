using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Web;
using Microsoft.ProjectOxford.Face;

namespace FaceAPI.Web.Models
{
    public class FaceDetectionModel
    {
        #region Properties

        public string FileName { get; set; }

        public string FilePath { get; set; }

        public string Gender { get; set; }

        public string Age { get; set; }

        public string ImagePath { get; set; }

        public string PersonName { get; set; }

        public int Height { get; set; }

        public int Width { get; set; }

        public int Left { get; set; }

        public int Top { get; set; }

        //public string IsSmiling { get; set; }

        public string Emotion { get; set; }

        public string Hair { get; set; }

        public double? Confidence { get; set; }
        #endregion Properties


    }
}