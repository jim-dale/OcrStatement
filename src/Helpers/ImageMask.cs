
namespace OcrStatement
{
    using System.Drawing;
    using Tesseract;

    public class ImageMask
    {
        const float CmsPerInch = 2.54f;

        // Abusing the RectangleF struct to hold the offsets, measured in centimetres,
        // from sides of an image. For example Mask of (1, 1, 1, 1) would create a
        // rectangle 1 cm inside the image boundaries
        public static RectangleF Mask { get; set; } = new RectangleF(6.00f, 0, 0, 0);

        public static Rect CalculateMaskRectangle(Bitmap bitmap)
        {
            float hr = GetBitmapResolution(bitmap.HorizontalResolution);
            float vr = GetBitmapResolution(bitmap.VerticalResolution);

            int x1 = (int)(0 + PixelsFromCentimetres(Mask.Left, hr));
            int y1 = (int)(0 + PixelsFromCentimetres(Mask.Top, vr));
            int x2 = (int)(bitmap.Width - PixelsFromCentimetres(Mask.Width, hr));
            int y2 = (int)(bitmap.Height - PixelsFromCentimetres(Mask.Height, vr));

            return Rect.FromCoords(x1, y1, x2, y2);
        }

        private static float GetBitmapResolution(float resolution)
        {
            return (resolution == 0.0f) ? 300.0f : resolution;
        }

        private static int PixelsFromCentimetres(float cms, float resolution)
        {
            int result = (cms == 0) ? 0 : (int)(cms / CmsPerInch * resolution);
            return result;
        }
    }
}
