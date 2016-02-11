using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Search;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace uwp_sample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private string _subscriptionKey = "9e3535b705f44c01bdc5cc7280fd570f";
        private string _faceListNameRoot = "bb10";
        private string _storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=bb9oxford;AccountKey=HXBFbF6CbLPQ6ZqGKk+ipeRV0aX64QcLcS8y/w3XRtUHbix53eh79Q0yu+vgGj2KWXUmR3vXsVpyOctWh0L7gA==;BlobEndpoint=https://bb9oxford.blob.core.windows.net/;TableEndpoint=https://bb9oxford.table.core.windows.net/;QueueEndpoint=https://bb9oxford.queue.core.windows.net/;FileEndpoint=https://bb9oxford.file.core.windows.net/";
        private string _containerName = "studio";
        private ObservableCollection<FaceListData> _faceLists = new ObservableCollection<FaceListData>();

        public MainPage()
        {
            this.InitializeComponent();

            FaceLists.ItemsSource = _faceLists;
        }

        private async void AddFaces(object sender, RoutedEventArgs e)
        {
            var storageAccount = CloudStorageAccount.Parse(_storageConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();

            var container = blobClient.GetContainerReference(_containerName);
            await container.SetPermissionsAsync(new BlobContainerPermissions
            {
                PublicAccess = BlobContainerPublicAccessType.Blob
            });

            var detectedFaces = 0;
            var currentFaceListId = "";
            var faceServiceClient = new FaceServiceClient(_subscriptionKey);

            foreach (var blob in await ListBlobsAsync(container))
            {
                Debug.WriteLine(blob.Uri);
                try
                {
                    var faces = await faceServiceClient.DetectAsync(blob.Uri.ToString(), true, true, null);

                    foreach (var face in faces)
                    {
                        if (detectedFaces++ == 0)
                        {
                            currentFaceListId = await CreateFaceListAsync(faceServiceClient);
                            Debug.WriteLine(currentFaceListId);
                        }

                        try
                        {
                            var faceData = new FaceData
                            {
                                BlobUrl = blob.Uri.ToString(),
                                FaceRectangle = face.FaceRectangle
                            };

                            var faceDataJS = JsonConvert.SerializeObject(faceData);

                            var faceResult = await faceServiceClient.AddFaceToFaceListAsync(currentFaceListId, blob.Uri.ToString(), faceDataJS, face.FaceRectangle);
                            Debug.WriteLine(faceResult.PersistedFaceId);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }

                        if (detectedFaces >= 1000)
                        {
                            detectedFaces = 0;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        private async Task<string> CreateFaceListAsync(FaceServiceClient faceServiceClient)
        {
            var faceListId = _faceListNameRoot + Guid.NewGuid();
            await faceServiceClient.CreateFaceListAsync(faceListId, faceListId, null);
            return faceListId;
        }

        public async Task<List<IListBlobItem>> ListBlobsAsync(CloudBlobContainer container)
        {
            BlobContinuationToken continuationToken = null;
            var results = new List<IListBlobItem>();

            do
            {
                var response = await container.ListBlobsSegmentedAsync(continuationToken);
                continuationToken = response.ContinuationToken;
                results.AddRange(response.Results);
            }
            while (continuationToken != null);

            return results;
        }

        private async void RefreshFaceLists(object sender, RoutedEventArgs e)
        {
            var faceServiceClient = new FaceServiceClient(_subscriptionKey);
            var facelists = await faceServiceClient.ListFaceListsAsync();
            foreach (var facelist in facelists)
            {
                if (facelist.FaceListId.StartsWith(_faceListNameRoot))
                {
                    var faceListData = new FaceListData { FaceListId = facelist.FaceListId };
                    var fl = await faceServiceClient.GetFaceListAsync(facelist.FaceListId);
                    faceListData.PersistedFaces = fl.PersistedFaces.Count();
                    _faceLists.Add(faceListData);
                }
            }
        }

        private async void ClearFaceLists(object sender, RoutedEventArgs e)
        {
            var faceServiceClient = new FaceServiceClient(_subscriptionKey);
            var faceLists = await faceServiceClient.ListFaceListsAsync();
            foreach (var faceList in faceLists)
            {
                if (faceList.FaceListId.StartsWith(_faceListNameRoot))
                {
                    await faceServiceClient.DeleteFaceListAsync(faceList.FaceListId);
                }
            }
            _faceLists.Clear();
        }

        private async void ImagePicker_Click(object sender, RoutedEventArgs e)
        {
            var fileOpenPicker = new FileOpenPicker();
            fileOpenPicker.FileTypeFilter.Add(".jpg");
            fileOpenPicker.FileTypeFilter.Add(".jpeg");
            fileOpenPicker.FileTypeFilter.Add(".png");
            fileOpenPicker.ViewMode = PickerViewMode.Thumbnail;
            fileOpenPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

            var storageFile = await fileOpenPicker.PickSingleFileAsync();

            FaceButtons.Children.Clear();

            using (var stream = await storageFile.OpenAsync(FileAccessMode.Read))
            {
                var image = new BitmapImage();
                image.SetSource(stream);
                SelectedImage.Source = image;

                stream.Seek(0);

                var faceServiceClient = new FaceServiceClient(_subscriptionKey);
                var faces = await faceServiceClient.DetectAsync(stream.AsStream(), true, true, new FaceAttributeType[] { FaceAttributeType.Gender, FaceAttributeType.Age, FaceAttributeType.FacialHair, FaceAttributeType.Smile });

                foreach (var face in faces)
                {
                    var scale = SelectedImage.ActualHeight / image.PixelHeight;
                    var faceButton = new Button();
                    faceButton.Width = face.FaceRectangle.Width * scale;
                    faceButton.Height = face.FaceRectangle.Height * scale;
                    faceButton.Margin = new Thickness(face.FaceRectangle.Left * scale + 25, face.FaceRectangle.Top * scale + 25, 0, 0);
                    faceButton.Tag = face.FaceId;
                    faceButton.Click += FaceBox_Click;
                    FaceButtons.Children.Add(faceButton);
                }
            }
        }

        private async void FaceBox_Click(object sender, RoutedEventArgs e)
        {
            var faceButton = (Button)sender;
            var faceServiceClient = new FaceServiceClient(_subscriptionKey);
            var faceLists = await faceServiceClient.ListFaceListsAsync();
            var faceList = faceLists.First(fl => fl.Name.StartsWith(_faceListNameRoot));
            var fl2 = await faceServiceClient.GetFaceListAsync(faceList.FaceListId);
            var similarFaces = await faceServiceClient.FindSimilarAsync(new Guid(faceButton.Tag.ToString()), faceList.FaceListId);

            SimilarFaces.ItemsSource = from pf in fl2.PersistedFaces
                                       join sf in similarFaces
                                       on pf.PersistedFaceId equals sf.PersistedFaceId
                                       orderby sf.Confidence descending
                                       select new SimilarFaceResult(pf.UserData)
                                       {
                                           Confidence = (int)(sf.Confidence * 100)
                                       };

        }
    }
}
