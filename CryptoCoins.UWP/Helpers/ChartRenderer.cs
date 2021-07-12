using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;

namespace CryptoCoins.UWP.Helpers
{
    public static class ChartRenderer
    {
        private const double Epsilon = 2.2204460492503131e-9;

        private static int DefaultTickCount => 8;

        internal static double NormalizeStep(double initialStep)
        {
            var magnitute = Math.Floor(Math.Log10(initialStep));
            var magnitutePower = Math.Pow(10d, magnitute);

            // Calculate most significant digit of the new step size
            double magnitudeDigit = (int) (initialStep / magnitutePower + .5);

            if (magnitudeDigit > 5)
            {
                magnitudeDigit = 10;
            }
            else if (magnitudeDigit > 2)
            {
                magnitudeDigit = 5;
            }
            else if (magnitudeDigit > 1)
            {
                magnitudeDigit = 2;
            }

            return magnitudeDigit * magnitutePower;
        }

        private static double CalculateAutoStep(double min, double max)
        {
            var step = (max - min) / (DefaultTickCount - 1);
            return NormalizeStep(step);
        }

        public static bool IsZero(double value)
        {
            return Math.Abs(value) < 10.0 * Epsilon;
        }

        public static double RoundMaxToMajorStep(double max, double step)
        {
            double mod;

            mod = max % step;
            if (!IsZero(mod))
            {
                if (mod > 0)
                {
                    max += step - mod;
                }
                else if (mod < 0)
                {
                    max += step + mod;
                }
            }
            return max;
        }

        public static void RenderData(CanvasDrawingSession drawingSession, float width, float height, float offsetX, float offsetY, Color strokeColor, Color fillColor, float thickness,
            List<double> data)
        {
            using (var stroke = new CanvasPathBuilder(drawingSession))
            using (var fill = new CanvasPathBuilder(drawingSession))
            {
                var min = data.Min();
                var max = data.Max();
                var stepY = CalculateAutoStep(min, max);
                min -= stepY;
                max += stepY;
                max = RoundMaxToMajorStep(max, stepY);
                var stepX = width / (data.Count - 1);

                fill.BeginFigure(0f, height + offsetY);
                var p = (data[0] - min) / (max - min);
                var point = new Vector2(offsetX, (float) (1 - p) * height + offsetY);
                fill.AddLine(point);
                stroke.BeginFigure(point);
                offsetX += stepX;
                for (var i = 1; i < data.Count; i++, offsetX += stepX)
                {
                    var value = data[i];
                    p = (value - min) / (max - min);
                    point = new Vector2(offsetX, (float) (1 - p) * height + offsetY);
                    fill.AddLine(point);
                    stroke.AddLine(point);
                }
                fill.AddLine(offsetX, height + offsetY);
                fill.EndFigure(CanvasFigureLoop.Closed);
                stroke.EndFigure(CanvasFigureLoop.Open);
                drawingSession.FillGeometry(CanvasGeometry.CreatePath(fill), fillColor);
                drawingSession.DrawGeometry(CanvasGeometry.CreatePath(stroke), strokeColor, thickness);
            }
        }
    }
}
