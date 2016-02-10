using Microsoft.ProjectOxford.Face;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace uwp_sample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private string _subscriptionKey = "9e3535b705f44c01bdc5cc7280fd570f";
        private string _faceListName = "bb9";

        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void AddPictures(object sender, RoutedEventArgs e)
        {
            var faceServiceClient = new FaceServiceClient(_subscriptionKey);

            var picturesFolder = KnownFolders.PicturesLibrary;

            // Get the files in the current folder, sorted by date.
           var storageFiles = await picturesFolder.GetFilesAsync(CommonFileQuery.OrderByName);

            foreach (var item in storageFiles)
            {

            }
        }
    }
}
