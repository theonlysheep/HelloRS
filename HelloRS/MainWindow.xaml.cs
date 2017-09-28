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
    /// 

   

    public partial class MainWindow : Window
    {
        PXCMSession session;
        PXCMSenseManager.Handler handler;

        public MainWindow()
        {
            InitializeComponent();

            // Setupt Eventhandler 
            handler = new PXCMSenseManager.Handler();
            
            // Get instance of SenseManager
            //
            session = PXCMSession.CreateInstance();

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            session.Dispose();

        }
        
        pxcmStatus OnNewSample(int mid, PXCMCapture.Sample sample)
        {
            PXCMImage image = sample.color;
            PXCMImage.ImageData data;
            image.AcquireAccess(PXCMImage.Access.ACCESS_READ, PXCMImage.PixelFormat.PIXEL_FORMAT_RGB32, out data);
            WriteableBitmap wbm = data.ToWritableBitmap(0,
                                      image.info.width,
                                     image.info.height,
                                           96.0, 96.0);
            wbm.Freeze();
            imageRGB.Dispatcher.Invoke(
                 new Action(() => imageRGB.Source = wbm));
            GC.Collect();
            image.ReleaseAccess(data);
            return pxcmStatus.PXCM_STATUS_NO_ERROR;
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            btnStart.IsEnabled = true;
            btnStop.IsEnabled = false;        
            session.Dispose();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            btnStart.IsEnabled = false;
            btnStop.IsEnabled = true;

            // Get RS version 
            PXCMSession.ImplVersion version = session.QueryVersion();
            textBox1.Text = version.major.ToString() + "." + version.minor.ToString();

            // setup Pipeline 
            PXCMSenseManager sm = session.CreateSenseManager();

            // Get streams ready 
            sm.EnableStream(PXCMCapture.StreamType.STREAM_TYPE_COLOR, 640, 480);

            // Create Eventhandler             
            handler.onNewSample = OnNewSample;

            // Init Pipeline 
            sm.Init(handler);
            sm.StreamFrames(false);



            // Get samples
            pxcmStatus status = sm.AcquireFrame(true); // Synchronous capturing 
            PXCMCapture.Sample sample = sm.QuerySample();


            
            sm.ReleaseFrame();


            sm.Close();
            

            
        }
    }
}
