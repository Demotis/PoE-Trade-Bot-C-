using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using TradeBotSharedLib.Models;

namespace TradeBotSharedLib.Services
{
    class OpenCV_Service
    {
        public OpenCV_Service()
        {

        }

        public static Position FindObject(Bitmap sourceBitmap, Bitmap templateBitmap, double threshold = 0.95)
        {
            Position res = new Position();

            using (Image<Bgr, byte> source = sourceBitmap.ToImage<Bgr, byte>()) // Image B
            using (Image<Bgr, byte> template = templateBitmap.ToImage<Bgr, byte>()) // Image A
            using (Image<Gray, float> result = source.MatchTemplate(template, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed))
            {
                double[] minValues, maxValues;
                Point[] minLocations, maxLocations;

                result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);

                if (maxValues[0] > threshold)
                {
                    res = new Position
                    {
                        Left = maxLocations[0].X,
                        Top = maxLocations[0].Y,
                        Width = template.Size.Width,
                        Height = template.Size.Height
                    };
                }
                if (Convert.ToBoolean(ConfigManager.Instance.ApplicationConfig["SaveTestImages"]))
                {
                    string guid = Guid.NewGuid().ToString();
                    result.Save(ConfigManager.Instance.ApplicationConfig["TestImagePath"] + guid + ".png");
                }
            }

            return res;
        }


        public static Position FindObject(Bitmap source_img, string path_template, double threshold = 0.95)
        {
            return FindObject(source_img, new Bitmap(path_template), threshold);
        }

        public static List<Position> FindObjects(Bitmap source_img, string path_template, double threshold = 0.95)
        {
            List<Position> res_pos = new List<Position>();

            using (Image<Bgr, byte> source = source_img.ToImage<Bgr, byte>()) // Image B
            using (Image<Bgr, byte> template = new Image<Bgr, byte>(path_template)) // Image A
            using (Image<Gray, float> result = source.MatchTemplate(template, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed))
            {
                while (true)
                {
                    double[] minValues, maxValues;
                    Point[] minLocations, maxLocations;

                    result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);

                    if (maxValues[0] > threshold)
                    {
                        // This is a match. Do something with it, for example draw a rectangle around it.
                        Rectangle match = new Rectangle(maxLocations[0], template.Size);
                        result.Draw(match, new Gray(), 3);

                        res_pos.Add(new Position
                        {
                            Left = maxLocations[0].X,
                            Top = maxLocations[0].Y,
                            Width = template.Size.Width,
                            Height = template.Size.Height
                        });
                    }
                    else
                        break;
                }
                if (Convert.ToBoolean(ConfigManager.Instance.ApplicationConfig["SaveTestImages"]))
                {
                    string guid = Guid.NewGuid().ToString();
                    result.Save(ConfigManager.Instance.ApplicationConfig["TestImagePath"] + guid + ".png");
                }
            }

            return res_pos;
        }














        internal static List<Position> FindCurrencies(Bitmap source_img, string path_template, double threshold = 0.95)
        {
            List<Position> res_pos = new List<Position>();

            Image<Bgr, byte> source = source_img.ToImage<Bgr, byte>(); // Image B
            Image<Bgr, byte> template = new Image<Bgr, byte>(path_template); // Image A

            template = template.Resize(33, 33, Emgu.CV.CvEnum.Inter.Cubic);
            template.ROI = new Rectangle(0, 11, 33, 24);

            using (Image<Gray, float> result = source.MatchTemplate(template, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed))
            {
                while (true)
                {
                    double[] minValues, maxValues;
                    Point[] minLocations, maxLocations;

                    result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);

                    // You can try different values of the threshold. I guess somewhere between 0.75 and 0.95 would be good.

                    if (maxValues[0] > threshold)
                    {
                        // This is a match. Do something with it, for example draw a rectangle around it.
                        Rectangle match = new Rectangle(maxLocations[0], template.Size);
                        result.Draw(match, new Gray(), 3);

                        res_pos.Add(new Position
                        {
                            Left = maxLocations[0].X,
                            Top = maxLocations[0].Y,
                            Width = template.Size.Width,
                            Height = template.Size.Height
                        });
                    }
                    else
                        break;
                }

                if (Convert.ToBoolean(ConfigManager.Instance.ApplicationConfig["SaveTestImages"]))
                {
                    string guid = Guid.NewGuid().ToString();
                    result.Save(ConfigManager.Instance.ApplicationConfig["TestImagePath"] + guid + ".png");
                }
            }

            return res_pos;
        }
    }
}
