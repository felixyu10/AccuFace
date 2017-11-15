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
using FaceAPI.Web.Common.Helper;

namespace FaceAPI.Web.Controllers
{
    public class FaceDetectionController : BaseController
    {
        private static string directory = AppHelper.GetWebConfigSetting("UploadedDirectory");
        private static string uplImageName = string.Empty;
        private int maxImageSize = 450;
        private ObservableCollection<FaceDetectionModel> detectedFaces = new ObservableCollection<FaceDetectionModel>();
        private ObservableCollection<FaceDetectionModel> resultCollection = new ObservableCollection<FaceDetectionModel>();
        private FaceDetectionService service = new FaceDetectionService();
        private FaceModel faceModel = new FaceModel();

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Webcam()
        {
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> UploadPhoto()
        {
            try
            {
                PersonGroup group = await faceModel.serviceClient.GetPersonGroupAsync(faceModel.personGroupId);
            }
            catch (FaceAPIException ex)
            {
                if (ex.ErrorCode == "PersonGroupNotFound")
                {
                    //加入人員群組
                    await faceModel.serviceClient.CreatePersonGroupAsync(faceModel.personGroupId, "Accupass");
                }
            }
            string message = string.Empty;
            string fileName = string.Empty;
            bool flag = false;
            //Requested File Collection
            HttpFileCollection fileRequest = System.Web.HttpContext.Current.Request.Files;
            if (fileRequest != null)
            {
                HttpPostedFileBase file = Request.Files[0];
                fileName = DateTime.UtcNow.AddHours(8).ToString("yyyyMMddHHmmss") + Path.GetExtension(file.FileName);
                int size = file.ContentLength;

                try
                {
                    file.SaveAs(Path.Combine(Server.MapPath(directory), fileName));
                    message = "上傳成功！";
                    uplImageName = fileName;
                    flag = true;
                }
                catch (Exception e)
                {
                    message = e.Message;
                }
            }
            return new JsonResult
            {
                Data = new
                {
                    Message = message,
                    UplImageName = fileName,
                    Status = flag
                }
            };
        }

        [HttpGet]
        public async Task<dynamic> GetDetectedFaces()
        {

            resultCollection.Clear();
            detectedFaces.Clear();

            string detectedResultsInText = "辨識中...";
            string fullImgPath = Server.MapPath(directory) + '/' + uplImageName;
            string queryFaceImageUrl = directory + '/' + uplImageName;


            if (!string.IsNullOrWhiteSpace(uplImageName))
            {
                try
                {
                    // Call detection REST API
                    using (FileStream fStream = System.IO.File.OpenRead(fullImgPath))
                    {
                        // User picked one image
                        var imageInfo = UIHelper.GetImageInfoForRendering(fullImgPath);
                        Face[] faces = await faceModel.serviceClient.DetectAsync(fStream,
                            true,
                            true,
                            new FaceAttributeType[]
                            {
                                FaceAttributeType.Gender,
                                FaceAttributeType.Age,
                                FaceAttributeType.Emotion,
                                FaceAttributeType.Hair
                            });


                        detectedResultsInText = string.Format("有{0}個人臉", faces.Length);
                        Bitmap cropFace = null;

                        if (faces != null)
                        {
                            List<string> items = new List<string>();
                            int index = 0;
                            foreach (Face face in faces)
                            {
                                index++;

                                //Create & Save crop Images
                                string cropImgFileName = string.Format("{0}_Crop{1}.jpg", uplImageName, index);
                                string cropImgPath = directory + '/' + cropImgFileName;
                                string cropImgFullPath = Server.MapPath(directory) + '/' + cropImgFileName;

                                cropFace = service.CropBitmap(
                                                (Bitmap)Image.FromFile(fullImgPath),
                                                face.FaceRectangle.Left,
                                                face.FaceRectangle.Top,
                                                face.FaceRectangle.Width,
                                                face.FaceRectangle.Height);
                                cropFace.Save(cropImgFullPath, ImageFormat.Jpeg);

                                Guid[] faceIds = new Guid[] { face.FaceId };
                                string personName = "未知";
                                double? confidence = null;
                                try
                                {
                                    IdentifyResult[] identify = await faceModel.serviceClient.IdentifyAsync(faceModel.personGroupId, faceIds);


                                    if (identify[0].Candidates.Length > 0)
                                    {
                                        for (int j = 0; j < identify[0].Candidates.Length; j++)
                                        {
                                            confidence = identify[0].Candidates[j].Confidence;
                                            if (confidence > 0.7)
                                            {
                                                Guid personId = identify[0].Candidates[j].PersonId;
                                                Person person = await faceModel.serviceClient.GetPersonAsync(faceModel.personGroupId, personId);
                                                personName = person.Name;

                                                //User Binding原圖
                                                if (faces.Count() == 1)
                                                {
                                                    using (Stream img = System.IO.File.OpenRead(fullImgPath))
                                                    {
                                                        await faceModel.serviceClient.AddPersonFaceAsync(faceModel.personGroupId, person.PersonId, img);
                                                        await faceModel.serviceClient.TrainPersonGroupAsync(faceModel.personGroupId);
                                                    }
                                                }
                                                //User Binding裁切圖
                                                else
                                                {
                                                    using (Stream img = System.IO.File.OpenRead(cropImgFullPath))
                                                    {
                                                        await faceModel.serviceClient.AddPersonFaceAsync(faceModel.personGroupId, person.PersonId, img);
                                                        await faceModel.serviceClient.TrainPersonGroupAsync(faceModel.personGroupId);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    cropFace.Dispose();

                                    bool hair = face.FaceAttributes.Hair.HairColor.Any();

                                    detectedFaces.Add(new FaceDetectionModel()
                                    {
                                        PersonName = personName,
                                        ImagePath = fullImgPath,
                                        FileName = cropImgFileName,
                                        FilePath = cropImgPath,
                                        Left = face.FaceRectangle.Left,
                                        Top = face.FaceRectangle.Top,
                                        Width = face.FaceRectangle.Width,
                                        Height = face.FaceRectangle.Height,
                                        Gender = face.FaceAttributes.Gender == "male" ? "男" : "女",
                                        Age = string.Format("{0:#}歲", face.FaceAttributes.Age),
                                        //IsSmiling = face.FaceAttributes.Smile > 0.0 ? "有" : "沒有",
                                        Emotion = face.FaceAttributes.Emotion.ToRankedList().OrderByDescending(o => o.Value).FirstOrDefault().Key,
                                        Hair = hair? face.FaceAttributes.Hair.HairColor.OrderByDescending(o => o.Confidence).FirstOrDefault().Color.ToString() : "None",
                                        Confidence = confidence
                                    });
                                }
                                catch (FaceAPIException ex)
                                {
                                    string ErrorCode = ex.ErrorCode;
                                    string ErrorMessage = ex.ErrorMessage;
                                    //do exception work
                                }

                            }
                        }

                        // Convert detection result into UI binding object for rendering
                        var rectFaces = UIHelper.CalculateFaceRectangleForRendering(faces, maxImageSize, imageInfo);
                        foreach (var face in rectFaces)
                        {
                            resultCollection.Add(face);
                        }
                    }
                }
                catch (FaceAPIException ex)
                {
                    string ErrorCode = ex.ErrorCode;
                    string ErrorMessage = ex.ErrorMessage;
                    //do exception work
                }
            }
            return new JsonResult
            {
                Data = new
                {
                    QueryFaceImage = queryFaceImageUrl,
                    MaxImageSize = maxImageSize,
                    FaceInfo = detectedFaces,
                    FaceRectangles = resultCollection,
                    DetectedResults = detectedResultsInText
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        [HttpPost]
        public JsonResult Capture()
        {
            bool flag = true;
            string fullImgPath = "";
            try
            {
                string imgUrl = System.Web.HttpContext.Current.Request["imgUrl"];
                imgUrl = imgUrl.Substring("data:image/jpeg;base64,".Length);
                byte[] buffer = Convert.FromBase64String(imgUrl);
                uplImageName = string.Format("{0}.jpg", DateTime.UtcNow.AddHours(8).ToString("yyyyMMddHHmmss"));
                fullImgPath = string.Format("~/Uploaded/{0}", uplImageName);
                System.IO.File.WriteAllBytes(Server.MapPath(fullImgPath), buffer);

            }
            catch
            {
                flag = false;
            }
            return new JsonResult
            {
                Data = new
                {
                    UplImageName = uplImageName,
                    Status = flag
                }
            };
        }

    }
}