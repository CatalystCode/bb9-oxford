using Microsoft.ProjectOxford.Face.Contract;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using System.Net.Http;

namespace uwp_sample
{
    public class SimilarFaceResult : INotifyPropertyChanged
    {
        public SimilarFaceResult(string faceDataJS)
        {
            var faceData = JsonConvert.DeserializeObject<FaceData>(faceDataJS);
            BlobUrl = faceData.BlobUrl;
            FaceRectangle = faceData.FaceRectangle;

            ProcessImageAsync();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private async Task ProcessImageAsync()
        {
            var bitmapImage = new BitmapImage();

            try
            {
                using (var client = new HttpClient())
                {
                    using (var stream = await client.GetStreamAsync(new Uri(BlobUrl)))
                    {
                        var memoryStream = new MemoryStream();
                        await stream.CopyToAsync(memoryStream);
                        memoryStream.Position = 0;
                        bitmapImage.SetSource(memoryStream.AsRandomAccessStream());
                    }
                }
            }
            catch (Exception ex)
            { }
            //InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream();
            //var httpClient = new HttpClient();
            //var webReq = (HttpWebRequest)WebRequest.Create(BlobUrl);
            //var httpClient.re

            //using (WebResponse response = await webReq.GetResponseAsync())
            //{
            //    using (Stream responseStream = response.GetResponseStream())
            //    {
            //        try
            //        {
            //            responseStream.CopyTo(stream);
            //        }
            //        catch (Exception ex)
            //        { }
            //    }
            //}

            //var image = new BitmapImage();
            //image.SetSource(stream);
            //Image = image;

            Image = bitmapImage;

            var scale = 350.0 / Image.PixelWidth;
            FaceBoxWidth = FaceRectangle.Width * scale;
            FaceBoxHeight = FaceRectangle.Height * scale;
            FaceBoxMargin = new Thickness(FaceRectangle.Left * scale, FaceRectangle.Top * scale, 0, 0);

            OnPropertyChanged("Image");
            OnPropertyChanged("FaceBoxWidth");
            OnPropertyChanged("FaceBoxHeight");
            OnPropertyChanged("FaceBoxMargin");
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                var args = new PropertyChangedEventArgs(propertyName);
                PropertyChanged(this, args);
            }
        }

        public BitmapImage Image { get; set; }

        public double FaceBoxWidth { get; set; }

        public double FaceBoxHeight { get; set; }

        public Thickness FaceBoxMargin { get; set; }

        public string BlobUrl { get; set; }

        public FaceRectangle FaceRectangle { get; set; }

        public int Confidence { get; set; }
    }
}