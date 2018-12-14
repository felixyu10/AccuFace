using FaceAPI.Web.Helper;
using FaceAPI.Web.Models;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
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
    public class UserController : BaseController
    {
        private static string directory = AppHelper.GetWebConfigSetting("UploadedDirectory");
        private FaceDetectionService service = new FaceDetectionService();
        private FaceModel faceModel = new FaceModel();
        private CreatePersonResult person = new CreatePersonResult();
        private Guid? personId = null;
        private Person[] persons = null;

        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> Create(string userName)
        {
            try
            {
                PersonGroup group = await faceModel.serviceClient.GetPersonGroupAsync(faceModel.personGroupId);
                persons = await faceModel.serviceClient.ListPersonsAsync(faceModel.personGroupId);
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

            HttpFileCollection fileRequest = System.Web.HttpContext.Current.Request.Files;
            if (fileRequest != null)
            {
                for (int i = 0; i < fileRequest.Count; i++)
                {
                    try
                    {
                        var file = fileRequest[i];
                        fileName = DateTime.UtcNow.AddHours(8).ToString("yyyyMMddHHmmss") + Path.GetExtension(file.FileName);
                        int size = file.ContentLength;
                        file.SaveAs(Path.Combine(Server.MapPath(directory), fileName));
                        string fullImgPath = Server.MapPath(directory) + '/' + fileName;
                        try
                        {
                            await GetDetectedFaces(fileName, userName);
                        }
                        catch (FaceAPIException ex)
                        {
                            string ErrorCode = ex.ErrorCode;
                        }

                        //User Binding原圖
                        using (Stream imgStream = System.IO.File.OpenRead(fullImgPath))
                        {
                            try
                            {
                                await faceModel.serviceClient.AddPersonFaceAsync(faceModel.personGroupId, personId.Value, imgStream);
                                await faceModel.serviceClient.TrainPersonGroupAsync(faceModel.personGroupId);
                                message = "新增成功！";
                                flag = true;
                            }
                            catch (FaceAPIException ex)
                            {
                                message = ex.ErrorMessage;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        message = "發生錯誤！";
                    }
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

        public async Task GetDetectedFaces(string uplImageName, string userName)
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
                                    //Group沒有任何User
                                    if (persons != null && persons.Any())
                                    {
                                        IdentifyResult[] identify = await faceModel.serviceClient.IdentifyAsync(faceModel.personGroupId, faceIds);

                                        //User存在
                                        if (identify[0].Candidates.Length > 0)
                                        {
                                            for (int j = 0; j < identify[0].Candidates.Length; j++)
                                            {
                                                personId = identify[0].Candidates[j].PersonId;

                                            }
                                        }
                                        //新增User
                                        else
                                        {
                                            person = await faceModel.serviceClient.CreatePersonAsync(faceModel.personGroupId, userName);
                                            personId = person.PersonId;
                                        }
                                    }
                                    //新增User
                                    else
                                    {

                                        person = await faceModel.serviceClient.CreatePersonAsync(faceModel.personGroupId, userName);
                                        personId = person.PersonId;
                                    }

                                    //User Binding裁切後的圖
                                    //if (personId != null)
                                    //{
                                    //    using (Stream imgStream = System.IO.File.OpenRead(cropImgFullPath))
                                    //    {
                                    //        await faceModel.serviceClient.AddPersonFaceAsync(faceModel.personGroupId, personId.Value, imgStream);
                                    //        await Task.Delay(500);
                                    //        await faceModel.serviceClient.TrainPersonGroupAsync(faceModel.personGroupId);
                                    //    }
                                    //}
                                }
                                catch (FaceAPIException ex)
                                {
                                    string ErrorCode = ex.ErrorCode;
                                    string ErrorMessage = ex.ErrorMessage;
                                    //do exception work
                                }
                                cropFace.Dispose();
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
                    //isComplete = true;
                }
            }
        }

    }
}