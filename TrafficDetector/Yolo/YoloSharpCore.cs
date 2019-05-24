using TrafficDetector.Services;
using mUserControl_BSC_dll.UserControls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using YoloSharp;

namespace TrafficDetector.YOLO
{
    class YoloSharpCore
    {
        public static YoloSharpCore mYolo = new YoloSharpCore();
        public bool IsYoloReady;
        public List<Data> FoundObjData = new List<Data>();
        
        /// Local
        /// 
        Bitmap _bitmap = null;
        float _aspectRatio;

        Brush _brush = new SolidBrush(Color.FromArgb(128, 40, 40, 0));

        public Yolo _yolo;

        public bool LoadModel(string modelPath)
        {
            ModelPath model = new ModelPath(modelPath);
            _aspectRatio = model.FixedAspectRatio;
            if (model.Found)
            {
                _yolo = new Yolo(model.ConfigPath, model.WeightsPath, model.NamesPath);
                IsYoloReady = true;
            }
            else
            {
                MessageBox.Show("Missing Required model(.cfg, .weight, .name) files", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

                IsYoloReady = false;
            }
            return IsYoloReady;
        }

        public bool Detect(Bitmap bitmap, out List<Data> foundObjCenters)
        {
            foundObjCenters = new List<Data>();
            try
            {
                var result = _yolo.Detect(bitmap, 0.5f);
                foreach (var data in result)
                {
                    Data d = data;
                    foundObjCenters.Add(d);
                }
                return result.Count() > 0;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public Bitmap Detect(Bitmap bitmap)
        {
            try
            {
                if (_bitmap != null)
                {
                    _bitmap.Dispose();
                }
                using (Bitmap tmp = bitmap)
                {
                    _bitmap = ImageLoader.AddBorder(tmp, _aspectRatio);
                }
                
                
                //Stopwatch watch = new Stopwatch();
                //watch.Start();
                var result = _yolo.Detect(_bitmap, 0.5f);
                //watch.Stop();

                // Draw result
                using (Graphics g = Graphics.FromImage(_bitmap))
                {
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    float scale = _bitmap.Width / 1000f;
                    foreach (var data in result)
                    {
                        Data d = data;

                        //Color c = ImageLoader.ConvertHsvToRgb(d.Id * 1.0f / _yolo.ClassNames.Length, 1, 0.8f);
                        Color c = Color.LightSalmon;
                        Pen pen = new Pen(c, 3f * scale);
                        pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                        Font font = new Font(FontFamily.GenericSerif, 15f * scale, System.Drawing.FontStyle.Regular);

                        //g.FillRectangle(_brush, d.X, d.Y, d.Width, 35f * scale);
                        g.DrawRectangle(pen, d.X, d.Y, d.Width, d.Height);
                        string status =  $"{d.Name} ({d.Confidence * 100:00.0}%)";
                        //string status = $"{d.Name}";
                        g.DrawString(status, font, Brushes.White, new PointF(d.X, d.Y + 3f * scale));

                        pen.Dispose();
                        font.Dispose();
                    }
                }
                return _bitmap;
            }
            catch (Exception ex)
            {
                //AppendMessage(ex.Message);
            }
            return bitmap;
        }

    }
}
