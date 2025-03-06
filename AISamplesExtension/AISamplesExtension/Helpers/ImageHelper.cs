using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace AISamplesExtension.Helpers
{
    public static class ImageHelper
    {
        public static string SaveBitmapAsPng(Bitmap bitmap, string filePath)
        {
            string fullPath = Path.ChangeExtension(filePath, ".png");
            bitmap.Save(fullPath, ImageFormat.Png);
            return fullPath;
        }

        public static string SaveBitmapAsJpeg(Bitmap bitmap, string filePath)
        {
            string fullPath = Path.ChangeExtension(filePath, ".jpg");
            bitmap.Save(fullPath, ImageFormat.Jpeg);
            return fullPath;
        }
    }
}