using System;
using System.Windows;
using System.Windows.Media;
using System.Threading;
using System.Drawing;
using System.Windows.Media.Imaging;

namespace HelloRS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Get instance of SenseManager 
            PXCMSession session = PXCMSession.CreateInstance();

            // Get RS version 
            PXCMSession.ImplVersion version = session.QueryVersion();
            textBox1.Text = version.major.ToString() + "." + version.minor.ToString();

            // setup Pipeline 
            PXCMSenseManager sm = session.CreateSenseManager();

            // Get streams ready 
            sm.EnableStream(PXCMCapture.StreamType.STREAM_TYPE_COLOR, 640, 480);
            sm.EnableStream(PXCMCapture.StreamType.STREAM_TYPE_DEPTH, 640, 480);

            // Init Pipeline 
            sm.Init();

            // Get samples
            pxcmStatus status = sm.AcquireFrame(true); // Synchronous capturing 
            PXCMCapture.Sample sample = sm.QuerySample();

            // Convert samples to image 
            PXCMImage image = sample.color;
            PXCMImage dimage = sample.depth;

            PXCMImage.ImageData data;
            image.AcquireAccess(PXCMImage.Access.ACCESS_READ, PXCMImage.PixelFormat.PIXEL_FORMAT_RGB32, out data);
            WriteableBitmap wbm = data.ToWritableBitmap(0,
                                  image.info.width,
                                  image.info.height,
                                  96.0, 96.0);

            PXCMImage.ImageData data2;
            dimage.AcquireAccess(PXCMImage.Access.ACCESS_READ, PXCMImage.PixelFormat.PIXEL_FORMAT_DEPTH_RAW, out data2);
            WriteableBitmap wbm2 = data2.ToWritableBitmap(
                                   0,
                                   dimage.info.width,
                                   dimage.info.height,
                                   96.0, 96.0);

            // Display image 
            imageRGB.Source = wbm;
            imageDepth.Source = wbm2;


            // Clean up 
            image.ReleaseAccess(data);
            image.ReleaseAccess(data2);
            sm.ReleaseFrame();


            sm.Close();
            session.Dispose();
        }
    }
}
