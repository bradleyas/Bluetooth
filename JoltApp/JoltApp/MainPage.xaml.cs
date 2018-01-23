using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Windows.UI.Popups;

using Windows.Media.Capture;
using System.Threading.Tasks;
using Windows.System.Display;
using Windows.Graphics.Display;
using Windows.ApplicationModel;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace JoltApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        MediaCapture mediaCapture;
        bool isPreviewing = false;
        DisplayRequest displayRequest = new DisplayRequest();

        public MainPage()
        {
            this.InitializeComponent();

            Application.Current.Suspending += Application_Suspending;
        }

        private async void Application_Suspending(object sender, SuspendingEventArgs e)
        {
            if(Frame.CurrentSourcePageType == typeof(MainPage))
            {
                var deferral = e.SuspendingOperation.GetDeferral();
                await StopPreviewAsync();
                deferral.Complete();
            }
        }

        private async void coolButton_Click(object sender, RoutedEventArgs e)
        {
            if (isPreviewing)
            {
                coolButton.Content = "Start Preview";
                await StopPreviewAsync();
            }
            else
            {
                coolButton.Content = "Stop Preview";
                await StartPreviewAsync();
            }
        }

        private async Task StopPreviewAsync()
        {
            if(mediaCapture != null)
            {
                if (isPreviewing)
                {
                    await mediaCapture.StopPreviewAsync();
                    isPreviewing = false;
                }

                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    WebcamPreview.Source = null;
                    if (displayRequest != null)
                    {
                        displayRequest.RequestRelease();
                    }
                    mediaCapture.Dispose();
                    mediaCapture = null;
                });
            }
        }

        private async Task StartPreviewAsync()
        {
            try
            {
                mediaCapture = new MediaCapture();
                await mediaCapture.InitializeAsync();
                displayRequest.RequestActive();
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;
            } catch (UnauthorizedAccessException)
            {
                new MessageDialog("The app was denied access to the camera");
                return;
            }

            try
            {
                WebcamPreview.Source = mediaCapture;
                await mediaCapture.StartPreviewAsync();
                isPreviewing = true;
            } catch (System.IO.FileLoadException)
            {
                mediaCapture.CaptureDeviceExclusiveControlStatusChanged += _mediaCapture_CaptureDeviceExclusiveControlStatusChanged;

            }
        }

        private async void _mediaCapture_CaptureDeviceExclusiveControlStatusChanged(MediaCapture sender, MediaCaptureDeviceExclusiveControlStatusChangedEventArgs args)
        {
            if(args.Status == MediaCaptureDeviceExclusiveControlStatus.SharedReadOnlyAvailable)
            {
                new MessageDialog("The app was denied access to the camera");
            }
            else if(args.Status == MediaCaptureDeviceExclusiveControlStatus.ExclusiveControlAvailable && !isPreviewing)
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    await StartPreviewAsync();
                });
            }
        }

    }
}
