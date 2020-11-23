using ImageMagick;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Text;

namespace MarineBot.Helpers
{
    internal static class ImageHelper
    {
        public static Stream GenerateGif(string baseDir, Bitmap avatar1, Bitmap avatar2)
        {
            Console.WriteLine("started gif job.");
            JToken framesData;
            using (StreamReader r = new StreamReader(baseDir + "/data.json"))
            {
                string json = r.ReadToEnd();
                framesData = JObject.Parse(json);
            }

            var collection = new MagickImageCollection();

            var mask = new Bitmap(baseDir + "/mask.jpg");
            var maskedAvatar1 = DoApplyMask(avatar1, mask);
            var maskedAvatar2 = DoApplyMask(avatar2, mask);
            mask.Dispose();

            var frames = Convert.ToInt32(framesData["frames"]);

            for (int j = 0; j < frames; j++)
            {
                var baseImage = new Bitmap($"{baseDir}/frame_{j}.jpg");
                var destImage = new Bitmap(baseImage.Width, baseImage.Height);
                var graphics = Graphics.FromImage(destImage);

                graphics.DrawImage(baseImage, 0, 0, baseImage.Width, baseImage.Height);

                if ((bool)framesData["avatar1"][j]["show"])
                    graphics.DrawImage(maskedAvatar1, Convert.ToInt32(framesData["avatar1"][j]["origin"]["x"]), Convert.ToInt32(framesData["avatar1"][j]["origin"]["y"]),
                                            Convert.ToInt32(framesData["avatar1"][j]["size"]["width"]), Convert.ToInt32(framesData["avatar1"][j]["size"]["height"]));
                if ((bool)framesData["avatar2"][j]["show"])
                    graphics.DrawImage(maskedAvatar2, Convert.ToInt32(framesData["avatar2"][j]["origin"]["x"]), Convert.ToInt32(framesData["avatar2"][j]["origin"]["y"]),
                                            Convert.ToInt32(framesData["avatar2"][j]["size"]["width"]), Convert.ToInt32(framesData["avatar2"][j]["size"]["height"]));

                MemoryStream memoryStream = new MemoryStream();
                destImage.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                memoryStream.Seek(0, SeekOrigin.Begin);

                collection.Add(new MagickImage(memoryStream));
                collection[collection.Count - 1].AnimationDelay = Convert.ToInt32(framesData["delayRate"]);

                baseImage.Dispose();
                destImage.Dispose();
                graphics.Dispose();
            }

            var settings = new QuantizeSettings();
            settings.Colors = 256;
            collection.Quantize(settings);
            collection.Optimize();

            MemoryStream outputStream = new MemoryStream();
            collection.Write(outputStream, MagickFormat.Gif);
            outputStream.Seek(0, SeekOrigin.Begin);
            Console.WriteLine("ended gif job.");

            return outputStream;
        }

        public static Bitmap LoadImage(string url)
        {
            using (WebClient wc = new WebClient())
            {
                using (Stream s = wc.OpenRead(url))
                {
                    return new Bitmap(s);
                }
            }
        }

        //https://stackoverflow.com/questions/21500040/applying-a-mask-to-a-bitmap-and-generating-a-composite-image-at-runtime
        private static Bitmap DoApplyMask(Bitmap input, Bitmap mask)
        {
            Bitmap output = new Bitmap(input.Width, input.Height, PixelFormat.Format32bppArgb);
            output.MakeTransparent();
            var rect = new Rectangle(0, 0, input.Width, input.Height);

            var bitsMask = mask.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var bitsInput = input.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var bitsOutput = output.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            unsafe
            {
                for (int y = 0; y < input.Height; y++)
                {
                    byte* ptrMask = (byte*)bitsMask.Scan0 + y * bitsMask.Stride;
                    byte* ptrInput = (byte*)bitsInput.Scan0 + y * bitsInput.Stride;
                    byte* ptrOutput = (byte*)bitsOutput.Scan0 + y * bitsOutput.Stride;
                    for (int x = 0; x < input.Width; x++)
                    {
                        //I think this is right - if the blue channel is 0 than all of them are (monochrome mask) which makes the mask black
                        if (ptrMask[4 * x] == 0)
                        {
                            ptrOutput[4 * x] = ptrInput[4 * x]; // blue
                            ptrOutput[4 * x + 1] = ptrInput[4 * x + 1]; // green
                            ptrOutput[4 * x + 2] = ptrInput[4 * x + 2]; // red

                            //Ensure opaque
                            ptrOutput[4 * x + 3] = 255;
                        }
                        else
                        {
                            ptrOutput[4 * x] = 0; // blue
                            ptrOutput[4 * x + 1] = 0; // green
                            ptrOutput[4 * x + 2] = 0; // red

                            //Ensure Transparent
                            ptrOutput[4 * x + 3] = 0; // alpha
                        }
                    }
                }

            }
            mask.UnlockBits(bitsMask);
            input.UnlockBits(bitsInput);
            output.UnlockBits(bitsOutput);

            return output;
        }
    }
}
