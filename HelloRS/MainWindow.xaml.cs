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


        private void Button_Click(object sender,
                             RoutedEventArgs e)
        {
            PXCMSession session =
                             PXCMSession.CreateInstance();
            PXCMSession.ImplVersion version =
                                   session.QueryVersion();
            textBox1.Text = version.major.ToString() + "."
                               + version.minor.ToString();


            PXCMSenseManager sm =
                             session.CreateSenseManager();
            sm.EnableStream(
                 PXCMCapture.StreamType.STREAM_TYPE_COLOR,
                 0, 0);
            sm.EnableStream(
                 PXCMCapture.StreamType.STREAM_TYPE_DEPTH,
                 0, 0);
            sm.Init();

            pxcmStatus status = sm.AcquireFrame(true);
            PXCMCapture.Sample sample = sm.QuerySample();

            PXCMImage image = sample.color;
            PXCMImage dimage = sample.depth;

            PXCMImage.ImageData data;
            image.AcquireAccess(
               PXCMImage.Access.ACCESS_READ,
               PXCMImage.PixelFormat.PIXEL_FORMAT_RGB32,
               out data);
            WriteableBitmap wbm = data.ToWritableBitmap(0,
                                  image.info.width,
                                  image.info.height,
                                  96.0, 96.0);

            PXCMImage.ImageData data2;
            dimage.AcquireAccess(
              PXCMImage.Access.ACCESS_READ,
              PXCMImage.PixelFormat.PIXEL_FORMAT_DEPTH_RAW,
              out data2);
            WriteableBitmap wbm2 = data2.ToWritableBitmap(
                                   0,
                                   dimage.info.width,
                                   dimage.info.height,
                                   96.0, 96.0);
            imageRGB.Source = wbm;
            imageDepth.Source = wbm2;
            image.ReleaseAccess(data);
            image.ReleaseAccess(data2);
            sm.ReleaseFrame();
            sm.Close();
            session.Dispose();
        }
    }
}
