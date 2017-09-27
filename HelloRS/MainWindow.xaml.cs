using System;
using System.Windows;
using System.Windows.Media;
using System.Threading;
using System.Drawing;

namespace HelloRS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Thread processingThread;
        private PXCMSenseManager senseManager;
        private PXCMHandModule hand;
        private PXCMHandConfiguration handConfig;
        private PXCMHandData handData;
        private PXCMHandData.GestureData gestureData;
       

        public MainWindow()
        {
            InitializeComponent();
           
            // Instantiate and initialize the SenseManager
            senseManager = PXCMSenseManager.CreateInstance();
            senseManager.EnableStream(PXCMCapture.StreamType.STREAM_TYPE_COLOR, 640, 480, 30);
            senseManager.EnableStream(PXCMCapture.StreamType.STREAM_TYPE_DEPTH, 640, 480, 30);
            senseManager.EnableHand();
            senseManager.Init();

            /*
            // Configure the Hand Module
            hand = senseManager.QueryHand();
            handConfig = hand.CreateActiveConfiguration();
            handConfig.EnableGesture("wave");
            handConfig.EnableAllAlerts();
            handConfig.ApplyChanges();
            */

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            processingThread.Abort();
            if (handData != null) handData.Dispose();
            handConfig.Dispose();
            senseManager.Dispose();
        }

        private void ProcessingThread()
        {
            // Start AcquireFrame/ReleaseFrame loop
            while (senseManager.AcquireFrame(true) >= pxcmStatus.PXCM_STATUS_NO_ERROR)
            {
                PXCMCapture.Sample sample = senseManager.QuerySample();
                Bitmap colorBitmap;
                PXCMImage.ImageData colorData;

                Bitmap depthBitmap;
                PXCMImage.ImageData depthData;

                // Get color image data
                sample.color.AcquireAccess(PXCMImage.Access.ACCESS_READ, PXCMImage.PixelFormat.PIXEL_FORMAT_RGB24, out colorData);
                colorBitmap = colorData.ToBitmap(0, sample.color.info.width, sample.color.info.height);

                sample.depth.AcquireAccess(PXCMImage.Access.ACCESS_READ, PXCMImage.PixelFormat.PIXEL_FORMAT_DEPTH, out depthData);
                depthBitmap = depthData.ToBitmap(0, sample.depth.info.width, sample.depth.info.height);

                /*
                // Retrieve gesture data
                hand = senseManager.QueryHand();

                if (hand != null)
                {
                    // Retrieve the most recent processed data
                    handData = hand.CreateOutput();
                    handData.Update();
                    handWaving = handData.IsGestureFired("wave", out gestureData);
                }
                */

                // Update the user interface
                UpdateUI(colorBitmap, depthBitmap);

                // Release the frame
                if (handData != null) handData.Dispose();
                colorBitmap.Dispose();
                depthBitmap.Dispose();
                sample.color.ReleaseAccess(colorData);
                sample.depth.ReleaseAccess(depthData);
                senseManager.ReleaseFrame();
            }
        }

        private void UpdateUI(Bitmap colorBitmap, Bitmap depthBitmap)
        {
            this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate ()
            {
                if (colorBitmap != null)
                {
                    // Mirror the color stream Image control
                    imageRGB.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
                    ScaleTransform mainTransform = new ScaleTransform();
                    mainTransform.ScaleX = -1;
                    mainTransform.ScaleY = 1;
                    imageRGB.RenderTransform = mainTransform;

                    // Display the color stream
                    imageRGB.Source = ConvertBitmap.BitmapToBitmapSource(colorBitmap);
                }

                if (depthBitmap != null)
                {
                    // Mirror the color stream Image control
                    imageDepth.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
                    ScaleTransform mainTransform = new ScaleTransform();
                    mainTransform.ScaleX = -1;
                    mainTransform.ScaleY = 1;
                    imageDepth.RenderTransform = mainTransform;

                    // Display the color stream
                    imageDepth.Source = ConvertBitmap.BitmapToBitmapSource(depthBitmap);
                }
            }));
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            // Start the worker thread
            processingThread = new Thread(new ThreadStart(ProcessingThread));
            processingThread.Start();

        }
    }
}
