using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TrafficDetector.Main;
using TrafficDetector.Services;
using static TrafficDetector.Main.GV;
using static TrafficDetector.YOLO.YoloSharpCore;
using System.Speech.Synthesis;

namespace TrafficDetector
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow mMainWindow = null;
        public MainWindow()
        {
            mMainWindow = this;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Maximized;
            this.MainPageArea.Width = SystemParameters.PrimaryScreenWidth;
            this.MainPageArea.Height = SystemParameters.PrimaryScreenHeight;

            Panel_stop.Visibility = Visibility.Collapsed;
            
            PreviewService.previewSetup();
            ImageProcessService.ImgProcessSetup();

            /// Load Yolo Model
            GV.ML_Folders[(int)MLFolders.ML_YOLO_model] = Environment.CurrentDirectory + MLFolders.ML_YOLO_model.GetDescription();
            if (!Directory.Exists(GV.ML_Folders[(int)MLFolders.ML_YOLO_model]))
            {
                Directory.CreateDirectory(GV.ML_Folders[(int)MLFolders.ML_YOLO_model]);
            }

            mYolo.LoadModel(GV.ML_Folders[(int)MLFolders.ML_YOLO_model]);

            PreviewService.startPreview(PreviewService.previewFPS.HIGH);
        }

        private void Btn_minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Btn_close_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
