using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrafficDetector.Main;
using static TrafficDetector.YOLO.YoloSharpCore;

namespace TrafficDetector.Services
{
    class ImageProcessService
    {
        public static BackgroundWorker imgProcessRoutine = new BackgroundWorker();
        public static bool IsImgProcessOn = false;
        public static void ImgProcessSetup()
        {
            imgProcessRoutine.DoWork += new DoWorkEventHandler(imgProcessRoutine_doWork);
            imgProcessRoutine.ProgressChanged += new ProgressChangedEventHandler(imgProcessRoutine_ProgressChanged);
            imgProcessRoutine.RunWorkerCompleted += new RunWorkerCompletedEventHandler(imgProcessRoutine_WorkerCompleted);
            imgProcessRoutine.WorkerReportsProgress = true;
            imgProcessRoutine.WorkerSupportsCancellation = true;
        }
        public static void StartImgProcessRoutine()
        {
            if (!imgProcessRoutine.IsBusy)
            {
                IsImgProcessOn = true;
                imgProcessRoutine.RunWorkerAsync();
            }
        }

        public static void StopImgProcessRoutine()
        {
            IsImgProcessOn = false;
            if (imgProcessRoutine.IsBusy) imgProcessRoutine.CancelAsync();
        }

        static Stopwatch s = new Stopwatch();
        static string processTimeStr = "";
        private static void imgProcessRoutine_doWork(object sender, DoWorkEventArgs e)
        {
            while (!imgProcessRoutine.CancellationPending)
            {
                if (GV.imgOriginal == null) continue;

                s = Stopwatch.StartNew();
                mYolo.Detect(GV.imgOriginal.ToBitmap(), out mYolo.FoundObjData);
                s.Stop();
                processTimeStr = $"Yolo process time: {s.ElapsedMilliseconds}ms, FPS: {1000f / s.ElapsedMilliseconds: 0.0}";
                /// Fill static list of data for displaying
                if (mYolo.FoundObjData.Count > 0)
                {
                    int num = mYolo.FoundObjData.Count;
                    PreviewService.listData = new List<YoloSharp.Data>();
                    try
                    {
                        foreach (var p in mYolo.FoundObjData)
                        {
                            PreviewService.listData.Add(p);
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
                else
                {
                    PreviewService.listData = new List<YoloSharp.Data>();
                }
                imgProcessRoutine.ReportProgress(0);
            }
        }

        private static void imgProcessRoutine_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            MainWindow.mMainWindow.TB_yolo_info.Text = processTimeStr;
        }

        private static void imgProcessRoutine_WorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }
    }
}
