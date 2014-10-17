using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Controls.Charting
{
    public class EuropeChart
    {

        #region Fields

        private const double Delta = 1;

        /// <summary>
        /// Path of the original image.
        /// </summary>
        private const string Path = @"C:\Users\Emil\Dropbox\Master Thesis\EuropeColorMapReduced.png";

        /// <summary>
        /// The original color of all other countries.
        /// </summary>
        private static readonly Color DefaultColor = ColorTranslator.FromHtml("#878787");

        /// <summary>
        /// The original country color map.
        /// </summary>
        private static readonly Dictionary<string, Color> CountryColorMap = new Dictionary<string, Color>
        {
            {"Sweden", ColorTranslator.FromHtml("#668000")},
            {"Slovenia", ColorTranslator.FromHtml("#6F971C")},
            {"Slovakia", ColorTranslator.FromHtml("#2B2200")},
            {"Serbia", ColorTranslator.FromHtml("#00FF66")},
            {"Romania", ColorTranslator.FromHtml("#FF2F7A")},
            {"Portugal", ColorTranslator.FromHtml("#FFD5D5")},
            {"Poland", ColorTranslator.FromHtml("#808000")},
            {"Norway", ColorTranslator.FromHtml("#88AA00")},
            {"Netherlands", ColorTranslator.FromHtml("#00FFFF")},
            {"Latvia", ColorTranslator.FromHtml("#008080")},
            {"Luxemborg", ColorTranslator.FromHtml("#24221C")},
            {"Lithuania", ColorTranslator.FromHtml("#FF0000")},
            {"Italy", ColorTranslator.FromHtml("#550000")},
            {"Ireland", ColorTranslator.FromHtml("#FF6600")},
            {"Hungary", ColorTranslator.FromHtml("#FFE6D5")},
            {"Croatia", ColorTranslator.FromHtml("#DE8787")},
            {"Greece", ColorTranslator.FromHtml("#00FFCC")},
            {"Great Britain", ColorTranslator.FromHtml("#FF00FF")},
            {"France", ColorTranslator.FromHtml("#800080")},
            {"Finland", ColorTranslator.FromHtml("#800000")},
            {"Estonia", ColorTranslator.FromHtml("#00FF00")},
            {"Spain", ColorTranslator.FromHtml("#2B0000")},
            {"Denmark", ColorTranslator.FromHtml("#000080")},
            {"Germany", ColorTranslator.FromHtml("#FFFF00")},
            {"Czech Republic", ColorTranslator.FromHtml("#D40000")},
            {"Switzerland", ColorTranslator.FromHtml("#C83737")},
            {"Bosnia", ColorTranslator.FromHtml("#00D4AA")},
            {"Bulgaria", ColorTranslator.FromHtml("#00AAD4")},
            {"Belgium", ColorTranslator.FromHtml("#0000FF")},
            {"Austria", ColorTranslator.FromHtml("#803300")},
            {"Cyprus", ColorTranslator.FromHtml("#004455")},
            {"Malta", ColorTranslator.FromHtml("#008000")},
        };

        #endregion

        /// <summary>
        /// Draws an image of europe with the countries colored as specified in the map supplied.
        /// </summary>
        /// <param name="countryColors"> colors to paint the countries </param>
        /// <param name="defaultColor"> default country color </param>
        /// <returns> a image of europe </returns>
        public static Bitmap DrawEurope(Dictionary<string, Color> countryColors, Color defaultColor)
        {
            var source = new Bitmap(Image.FromFile(Path));
            var target = Prepare(source);
            var colorMap = PrepareColorMap(countryColors, defaultColor);

            WriteColors(source, target, colorMap, 0);

            return target;
        }

        /// <summary>
        /// Draws an image of europe with the countries colored proportional to their value (from = 0, to = 1).
        /// </summary>
        /// <param name="countryValues"> country values </param>
        /// <param name="defaultColor"> default country color </param>
        /// <param name="from"> low value color </param>
        /// <param name="to"> high  value color </param>
        /// <returns> a image of europe </returns>
        public static Bitmap DrawEurope(Dictionary<string, double> countryValues, Color defaultColor, Color from, Color to)
        {
            var source = new Bitmap(Image.FromFile(Path));
            var padding = (source.Width + source.Height) / 100;
            var target = PrepareWithScaleBar(source, from, to, padding);
            var countryColors = CalculateColorMap(countryValues, from, to);
            var colorMap = PrepareColorMap(countryColors, defaultColor);

            WriteColors(source, target, colorMap, padding);

            return target;
        }

        /// <summary>
        /// Map from values to color using the from-to gradient (manual calculation).
        /// </summary>
        /// <param name="countryValues"> the data values </param>
        /// <param name="from"> the low value color </param>
        /// <param name="to"> the high value color </param>
        /// <returns> gradients to be painted </returns>
        private static Dictionary<string, Color> CalculateColorMap(Dictionary<string, double> countryValues, Color from, Color to)
        {
            var colorMap = new Dictionary<string, Color>();
            var min = countryValues.Values.Min();
            var max = countryValues.Values.Max();
            
            foreach (var key in countryValues.Keys)
            {
                // Normalise to 1.
                var value = (countryValues[key] - min)/(max - min);
                // Blend colors.
                byte r = (byte) ((to.R*value) + from.R*(1 - value));
                byte g = (byte) ((to.G*value) + from.G*(1 - value));
                byte b = (byte) ((to.B*value) + from.B*(1 - value));
                var color = Color.FromArgb(r, g, b);
                
                colorMap.Add(key, color);
            }

            return colorMap;
        }

        /// <summary>
        /// Prepare a "naked" bitmap.
        /// </summary>
        /// <param name="source"> the source bitmap </param>
        /// <returns> naked bitmap </returns>
        private static Bitmap Prepare(Bitmap source)
        {
            return new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);;
        }

        /// <summary>
        /// Prepare a bitmap with a scale bar.
        /// </summary>
        /// <param name="source"> the source bitmap </param>
        /// <returns> bitmap with scale bar </returns>
        private static Bitmap PrepareWithScaleBar(Bitmap source, Color from, Color to, int padding)
        {
            var scaleBar = (int)(0.04 * source.Width);
            var target = new Bitmap(source.Width + scaleBar + 3*padding, source.Height + 2*padding,
                PixelFormat.Format32bppArgb);
            var verticalFillRectangle = new Rectangle
            {
                Width = scaleBar,
                Height = source.Height,
                X = source.Width + 2*padding,
                Y = padding
            };
            var myVerticalGradient = new LinearGradientBrush(verticalFillRectangle, to, from, 90f);

            using (var graphics = Graphics.FromImage(target))
            {
                graphics.FillRectangle(myVerticalGradient, verticalFillRectangle);
            }

            return target;
        }

        /// <summary>
        /// Prepare the colormap; which color goes to which.
        /// </summary>
        /// <param name="countryColors"> what colors should the country have at the end </param>
        /// <param name="defaultColor"> what color should non listed countries have </param>
        /// <returns> from-to color mapping </returns>
        private static Dictionary<Color, Color> PrepareColorMap(Dictionary<string, Color> countryColors, Color defaultColor)
        {
            var colorMap = new Dictionary<Color, Color>();

            // Here we loop the different country colors.
            foreach (var key in CountryColorMap.Keys)
            {
                var original = CountryColorMap[key];
                var replacement = countryColors.ContainsKey(key) ? countryColors[key] : defaultColor;
                colorMap.Add(original, replacement);
            }
            // Add the default color (non EU grid countries).
            colorMap.Add(DefaultColor, DefaultColor);

            return colorMap;
        }

        // Copied (well, most of it) from here: http://stackoverflow.com/questions/17208254/how-to-change-pixel-color-of-an-image-in-c-net
        static unsafe void WriteColors(Bitmap source, Bitmap target, Dictionary<Color, Color> colorMap, int padding)
        {
            const int pixelSize = 4; // 32 bits per pixel

            BitmapData sourceData = null, targetData = null;

            try
            {
                sourceData = source.LockBits(
                  new Rectangle(0, 0, source.Width, source.Height),
                  ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                targetData = target.LockBits(
                  new Rectangle(padding, padding, source.Width, source.Height),
                  ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

                // Here we loop the different country colors.
                foreach (var key in colorMap.Keys)
                {
                    // Here we loop the image.
                    for (int y = 0; y < source.Height; ++y)
                    {
                        byte* sourceRow = (byte*)sourceData.Scan0 + (y * sourceData.Stride);
                        byte* targetRow = (byte*)targetData.Scan0 + (y * targetData.Stride);

                        for (int x = 0; x < source.Width; ++x)
                        {
                            byte b = sourceRow[x * pixelSize + 0];
                            byte g = sourceRow[x * pixelSize + 1];
                            byte r = sourceRow[x * pixelSize + 2];
                            byte a = sourceRow[x * pixelSize + 3];

                            if (key.R != r || key.G != g || key.B != b) continue;

                            var replacement = colorMap[key];

                            r = replacement.R;
                            g = replacement.G;
                            b = replacement.B;

                            targetRow[x * pixelSize + 0] = b;
                            targetRow[x * pixelSize + 1] = g;
                            targetRow[x * pixelSize + 2] = r;
                            targetRow[x * pixelSize + 3] = a;
                        }
                    }
                }

            }
            finally
            {
                if (sourceData != null)
                    source.UnlockBits(sourceData);

                if (targetData != null)
                    target.UnlockBits(targetData);

            }
        }

    }
}
