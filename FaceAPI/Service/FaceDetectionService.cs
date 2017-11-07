using System;
using System.Configuration;
using System.Drawing;
using System.IO;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using FaceAPI.Web.Common.Helper;

namespace FaceAPI.Web.Service
{
    public class FaceDetectionService
    {
        private static string personGroupId = AppHelper.GetWebConfigSetting("PersonGroupId");
        private static string serviceKey = AppHelper.GetWebConfigSetting("FaceserviceKey");
        private static string serviceEndPoint = AppHelper.GetWebConfigSetting("FaceServiceEndPoint");
        private IFaceServiceClient faceServiceClient = new FaceServiceClient(serviceKey, serviceEndPoint);

        public Bitmap CropBitmap(Bitmap bitmap, int cropX, int cropY, int cropWidth, int cropHeight)
        {
            Rectangle rect = new Rectangle(cropX, cropY, cropWidth, cropHeight);
            Bitmap cropped = bitmap.Clone(rect, bitmap.PixelFormat);
            return cropped;
        }

        /// <summary>
        /// 加入人員群組
        /// </summary>
        public async void AddPersonGroup()
        {
            await faceServiceClient.CreatePersonGroupAsync(personGroupId, "Accupass");
        }

        /// <summary>
        /// 針對人員群組進行訓練
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void PersonGroupTrain()
        {
            await faceServiceClient.TrainPersonGroupAsync(personGroupId);
        }

        /// <summary>
        /// 刪除人員群組
        /// </summary>
        public async void DeletePersonGroup()
        {
            await faceServiceClient.DeletePersonGroupAsync(personGroupId);
        }

        /// <summary>
        /// 加入人員
        /// </summary>

        public async void AddPerson()
        {
            await faceServiceClient.CreatePersonAsync(personGroupId, "felix");
        }

        /// <summary>
        /// 加入人臉
        /// </summary>
        public async void AddFace(string fullImgPath)
        {
            byte[] data = File.ReadAllBytes(fullImgPath);
            MemoryStream stream = new MemoryStream(data);
            AddPersistedFaceResult result = await faceServiceClient.AddPersonFaceAsync(personGroupId, Guid.Parse("3C3F4783-F2B7-4FB6-96D4-01D53ECB608B"), stream);

        }
    }
}