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
    public class FaceDetectionController : BaseController
    {
        private static string directory = ConfigurationManager.AppSettings["UploadedDirectory"];
        //+ '/' + DateTime.UtcNow.AddHours(8).ToString("yyyyMMdd");
        private static string uplImageName = string.Empty;
        private ObservableCollection<FaceDetectionModel> detectedFaces = new ObservableCollection<FaceDetectionModel>();
        private ObservableCollection<FaceDetectionModel> resultCollection = new ObservableCollection<FaceDetectionModel>();
        private int maxImageSize = 450;
        private FaceDetectionService service = new FaceDetectionService();
        private FaceModel faceModel = new FaceModel();

        public ActionResult Index()
        {
            return View();
        }

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
                    message = "上傳失敗！";
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

        [HttpPost]
        public async Task<dynamic> GetDetectedFaces()
        {
            try {
                PersonGroup group = await faceModel.serviceClient.GetPersonGroupAsync(faceModel.personGroupId);
            } catch (FaceAPIException ex) {
                if (ex.ErrorCode == "PersonGroupNotFound") {
                    //加入人員群組
                    await faceModel.serviceClient.CreatePersonGroupAsync(faceModel.personGroupId, "Accupass");
                }
            }
            string message = string.Empty;
            string fileName = string.Empty;
            //Requested File Collection
            HttpFileCollection fileRequest = System.Web.HttpContext.Current.Request.Files;
            if (fileRequest != null) {
                HttpPostedFileBase file = Request.Files[0];
                fileName = DateTime.UtcNow.AddHours(8).ToString("yyyyMMddHHmmss") + Path.GetExtension(file.FileName);
                int size = file.ContentLength;

                try {
                    file.SaveAs(Path.Combine(Server.MapPath(directory), fileName));
                    message = "上傳成功！";
                    uplImageName = fileName;
                } catch (Exception e) {
                    message = "上傳失敗！";
                }
            }


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
                                FaceAttributeType.Smile
                            });


                        detectedResultsInText = string.Format("偵測到{0}個人臉", faces.Length);
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
                                try
                                {
                                    IdentifyResult[] identify = await faceModel.serviceClient.IdentifyAsync(faceModel.personGroupId, faceIds);


                                    if (identify[0].Candidates.Length > 0)
                                    {
                                        for (int j = 0; j < identify[0].Candidates.Length; j++)
                                        {
                                            Guid personId = identify[0].Candidates[j].PersonId;
                                            Person person = await faceModel.serviceClient.GetPersonAsync(faceModel.personGroupId, personId);
                                            personName = person.Name;

                                            using (Stream img = System.IO.File.OpenRead(cropImgFullPath))
                                            {
                                                try
                                                {
                                                    await faceModel.serviceClient.AddPersonFaceAsync(faceModel.personGroupId, person.PersonId, img);
                                                    await Task.Delay(500);
                                                    await faceModel.serviceClient.TrainPersonGroupAsync(faceModel.personGroupId);
                                                }
                                                catch (FaceAPIException ex)
                                                {

                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        personName = "未知";
                                    }

                                }
                                catch (FaceAPIException ex)
                                {
                                    string ErrorCode = ex.ErrorCode;
                                    string ErrorMessage = ex.ErrorMessage;
                                    //do exception work
                                }


                                if (cropFace != null)
                                {
                                    cropFace.Dispose();
                                }

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
                                    IsSmiling = face.FaceAttributes.Smile > 0.0 ? "有" : "沒有",
                                    Emotion = face.FaceAttributes.Emotion.ToRankedList().OrderByDescending(o => o.Value).FirstOrDefault().Key
                                });

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