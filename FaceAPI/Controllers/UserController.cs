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
    public class UserController : Controller
    {
        private static string directory = ConfigurationManager.AppSettings["UploadedDirectory"];
        private FaceDetectionService service = new FaceDetectionService();
        private FaceModel faceModel = new FaceModel();
        private bool isComplete = false;
        private Guid? personId = null;
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> UploadPhoto(string userName)
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
            string actualFileName = string.Empty;
            bool flag = true;

            HttpFileCollection fileRequested = System.Web.HttpContext.Current.Request.Files;
            if (fileRequested != null)
            {
                HttpPostedFileBase file = Request.Files[0];
                actualFileName = file.FileName;
                fileName = DateTime.UtcNow.AddHours(8).ToString("yyyyMMddHHmmss") + Path.GetExtension(file.FileName);
                int size = file.ContentLength;
                CreatePersonResult person = new CreatePersonResult();
                try
                {
                    file.SaveAs(Path.Combine(Server.MapPath(directory), fileName));
                    string fullImgPath = Server.MapPath(directory) + '/' + fileName;
                    try
                    {
                        await GetDetectedFaces(fileName);

                        while (true)
                        {
                            if (isComplete)
                            {
                                break;
                            }
                        }

                        if (personId == null)
                        {
                            person = await faceModel.serviceClient.CreatePersonAsync(faceModel.personGroupId, userName);
                            personId = person.PersonId;
                        }
                    }
                    catch (FaceAPIException ex)
                    {
                        string ErrorCode = ex.ErrorCode;
                    }
                    using (Stream img = System.IO.File.OpenRead(fullImgPath))
                    {
                        try
                        {
                            await faceModel.serviceClient.AddPersonFaceAsync(faceModel.personGroupId, personId.Value, img);
                            await Task.Delay(500);
                            await faceModel.serviceClient.TrainPersonGroupAsync(faceModel.personGroupId);

                            message = "新增成功！";

                        }
                        catch (FaceAPIException ex)
                        {
                            message = ex.ErrorMessage;
                            flag = false;
                        }
                    }

                }
                catch (Exception)
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

        [HttpGet]
        public async Task GetDetectedFaces(string uplImageName)
        {
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
                            true);
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

                                fullImgPath = cropImgFullPath;

                                Guid[] faceIds = new Guid[] { face.FaceId };

                                try
                                {
                                    IdentifyResult[] identify = await faceModel.serviceClient.IdentifyAsync(faceModel.personGroupId, faceIds);


                                    if (identify[0].Candidates.Length > 0)
                                    {
                                        for (int j = 0; j < identify[0].Candidates.Length; j++)
                                        {
                                            personId = identify[0].Candidates[j].PersonId;

                                        }
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

                            }
                        }


                    }
                }
                catch (FaceAPIException ex)
                {
                    string ErrorCode = ex.ErrorCode;
                    string ErrorMessage = ex.ErrorMessage;
                    //do exception work
                }
                finally
                {
                    isComplete = true;
                }
            }
        }
    }
}