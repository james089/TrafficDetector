using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrafficDetector.Main
{
    class GV
    {
        public static System.Media.SoundPlayer Sound_wrong = new System.Media.SoundPlayer(
            Environment.CurrentDirectory + @"\Resources\stop_sign_detected.wav");

        public static int imgWidth = 0, imgHeight = 0;
        public static double _zoomFactor = 0;

        public static Image<Bgr, byte> imgOriginal;
        public static Image<Bgr, byte> imgOriginal_pure;
        public static Image<Bgr, byte> imgProcessed;

        // ML
        public static string[] ML_Folders = new string[20];
        public enum MLFolders
        {
            /// <summary>
            /// @"\YOLO\model", files to be used in detection
            /// </summary>
            [Description(@"\YOLO\model")]
            ML_YOLO_model,
        }
    }

    public static class Func
    {
        public static string GetDescription<T>(this T e) where T : IConvertible
        {
            string description = null;

            if (e is Enum)
            {
                Type type = e.GetType();
                Array values = System.Enum.GetValues(type);

                foreach (int val in values)
                {
                    if (val == e.ToInt32(CultureInfo.InvariantCulture))
                    {
                        var memInfo = type.GetMember(type.GetEnumName(val));
                        var descriptionAttributes = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                        if (descriptionAttributes.Length > 0)
                        {
                            // we're only getting the first description we find
                            // others will be ignored
                            description = ((DescriptionAttribute)descriptionAttributes[0]).Description;
                        }

                        break;
                    }
                }
            }

            return description;
        }
    }
}
