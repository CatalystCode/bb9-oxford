using Microsoft.ProjectOxford.Face.Contract;

namespace uwp_sample
{
    public class FaceData
    {
        public string BlobUrl { get; set; }

        public FaceRectangle FaceRectangle { get; set; }
    }
}