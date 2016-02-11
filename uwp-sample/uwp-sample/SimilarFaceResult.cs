using Microsoft.ProjectOxford.Face.Contract;
using Newtonsoft.Json;
using System.IO;
using System.Net;

namespace uwp_sample
{
    internal class SimilarFaceResult
    {
        public SimilarFaceResult(string faceDataJS)
        {
            var faceData = JsonConvert.DeserializeObject<FaceData>(faceDataJS);
            BlobUrl = faceData.BlobUrl;
            FaceRectangle = faceData.FaceRectangle;
        }

        public string BlobUrl { get; set; }

        public FaceRectangle FaceRectangle { get; set; }

        public int Confidence { get; set; }
    }
}