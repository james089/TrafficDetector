using Emgu.CV;
using Emgu.CV.Structure;
using mUserControl_BSC_dll.Animations;
using mUserControl_BSC_dll.UserControls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TrafficDetector.Main;
using TrafficDetector.YOLO;
using Utilities_BSC_dll_x64;
using YoloSharp;
using static TrafficDetector.YOLO.YoloSharpCore;

namespace TrafficDetector.Services
{
    class PreviewService
    {
        static string videoFPS = "";
        static bool isStopSignDetected = false;
        static bool isBlinking = false;
        const int DELAY = 15;
        static int notStopSignCount = 0;

        public enum previewFPS
        {
            LOW = 5,
            MEDIUM = 10,
            HIGH = 30
        }

        public static BackgroundWorker previewRoutine = new BackgroundWorker();
        public static bool IsCapturing = false;

        public static void previewSetup()
        {
            previewRoutine.DoWork += new DoWorkEventHandler(previewRoutine_doWork);
            previewRoutine.ProgressChanged += new ProgressChangedEventHandler(previewRoutine_ProgressChanged);
            previewRoutine.RunWorkerCompleted += new RunWorkerCompletedEventHandler(previewRoutine_WorkerCompleted);
            previewRoutine.WorkerReportsProgress = true;
            previewRoutine.WorkerSupportsCancellation = true;
        }

        public static void startPreview(previewFPS previewFPS)
        {
            IsCapturing = true;
            if (!previewRoutine.IsBusy)
            {
                previewRoutine.RunWorkerAsync(previewFPS);
                ImageProcessService.StartImgProcessRoutine();
            }
        }

        public static void StopPreview()
        {
            IsCapturing = false;
            previewRoutine.CancelAsync();
            ImageProcessService.StopImgProcessRoutine();
        }


        private static void previewRoutine_WorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsCapturing = false;
            mNotification.Show("Live view stopped");
        }

        private static void previewRoutine_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (GV.imgOriginal == null) return;
            GUIUpdates();
        }

        public static void GUIUpdates()
        {
            // Normal
            MainWindow.mMainWindow.ibOriginal.Source = ImgConverter.ToBitmapSource(GV.imgProcessed);
            MainWindow.mMainWindow.TB_preveiw_info.Text = videoFPS;

            if (isStopSignDetected && !isBlinking)
            {
                GV.RobotVoice.Volume = 100;         // 0...100
                GV.RobotVoice.Rate = 0;             // -10...10
                GV.RobotVoice.SpeakAsync("Stop Sign Detected");
                isBlinking = true;
                MainWindow.mMainWindow.Panel_stop.Visibility = System.Windows.Visibility.Visible;
                AniShape.blink(MainWindow.mMainWindow.Panel_stop, true);
            }
            else if (notStopSignCount >= DELAY && isBlinking)
            {
                notStopSignCount = 0;
                isBlinking = false;
                MainWindow.mMainWindow.Panel_stop.Visibility = System.Windows.Visibility.Collapsed;
                AniShape.blink(MainWindow.mMainWindow.Panel_stop, false);
            }
        }

        static Stopwatch fps = new Stopwatch();
        public static List<Data> listData = new List<Data>();
        private static void previewRoutine_doWork(object sender, DoWorkEventArgs e)
        {
            IsCapturing = true;
            previewFPS FPS = (previewFPS)e.Argument;
            Capture webcam;
            try
            {
                webcam = new Capture();
            }
            catch (Exception ex)
            {
                return;
            }

            while (!previewRoutine.CancellationPending)
            {
                fps = Stopwatch.StartNew();
                //==== Display Original image==========
                try
                {
                    GV.imgOriginal = webcam.QueryFrame();
                }
                catch (Exception ex)
                {
                }

                if (GV.imgOriginal != null)
                {
                    GV.imgOriginal_pure = GV.imgOriginal.Copy();
                    processedImageDisplaying();
                }


                Thread.Sleep(1000 / (int)FPS);
                fps.Stop();
                videoFPS = $"Video FPS:{1000f / fps.ElapsedMilliseconds: 0.0}";

                previewRoutine.ReportProgress(0);
            }
        }

        public static void processedImageDisplaying()
        {
            GV.imgProcessed = GV.imgOriginal.Copy();
            if (ImageProcessService.IsImgProcessOn)
            {
                isStopSignDetected = false;
                foreach (var p in listData)
                {
                    float scale = GV.imgOriginal.Width / 1000f;
                    Rectangle rect = new Rectangle() { X = p.X, Y = p.Y, Width = p.Width, Height = p.Height };
                    Color c = ImageLoader.ConvertHsvToRgb(p.Id * 1.0f / mYolo._yolo.ClassNames.Length, 1, 0.8f);

                    GV.imgProcessed = drawBorder(GV.imgProcessed, rect, c, 3f * scale);
                    GV.imgProcessed = drawString(
                        $"{p.Name} ({p.Confidence * 100:00.0}%)",
                        GV.imgProcessed,
                        rect.Location,
                        Brushes.White,
                        15f * scale);
                    if (p.Name.Contains("stopSign"))
                        isStopSignDetected = true;
                }
                if (!isStopSignDetected)
                    notStopSignCount++;
            }
            else
            {
                GV.imgProcessed = new Image<Bgr, byte>(mYolo.Detect(GV.imgProcessed.ToBitmap()));
            }
        }

        private static Image<Bgr, byte> drawBorder(Image<Bgr, byte> image, Rectangle d, Color color, float size)
        {
            Bitmap bitmap = image.ToBitmap();
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                Pen pen = new Pen(color, size);
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                g.DrawRectangle(pen, d.X, d.Y, d.Width, d.Height);

                pen.Dispose();

                return new Image<Bgr, byte>(bitmap);
            }
        }

        private static Image<Bgr, byte> drawString(string content, Image<Bgr, byte> image, Point topLeft, Brush brush, double size)
        {
            Bitmap bitmap = image.ToBitmap();
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                Font font = new Font(FontFamily.GenericSerif, (float)size, System.Drawing.FontStyle.Bold);

                g.DrawString(content, font, brush, new PointF(topLeft.X, topLeft.Y));

                font.Dispose();
                return new Image<Bgr, byte>(bitmap);
            }
        }
    }
}
