using OpenCvSharp;

class ImageProprocessor
{
    private const float resizeFactor = 4f;
    private const int blurKernelSize = 13;
    private const int thresholdValue = 200;

    public static void PreprocessImage(string imagePath)
    {
        var output = Path.Combine(Directory.GetCurrentDirectory(), "output");
        var gray = Cv2.ImRead(imagePath, ImreadModes.Grayscale);

        Rotate(gray);

        Cv2.Resize(gray, gray, new Size(gray.Width * resizeFactor, gray.Height * resizeFactor));
        Cv2.GaussianBlur(gray, gray, new Size(blurKernelSize, blurKernelSize), blurKernelSize, blurKernelSize);
        Cv2.Threshold(gray, gray, thresholdValue, 255, ThresholdTypes.Binary);
        Deskew(gray);

        Mat image = new();
        Cv2.CvtColor(gray, image, ColorConversionCodes.GRAY2BGR);
        Cv2.ImWrite(Path.Combine(output, Path.GetFileNameWithoutExtension(imagePath) + ".preprocessed.png"), image);
    }

    private static void Rotate(Mat src)
    {
        if (src.Width > src.Height)
        {
            Cv2.Rotate(src, src, RotateFlags.Rotate90Clockwise);
        }
    }

    private static void Deskew(Mat src)
    {
        Cv2.FindContours(src, out Point[][] contours, out _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
        var largest = contours.OrderByDescending(c => Cv2.ContourArea(c)).First();
        RotatedRect rect = Cv2.MinAreaRect(largest);
        float angle = rect.Angle;

        while (angle > 45) { angle -= 90; }
        while (angle < -45) { angle += 90; }

        Point2f center = new Point2f(src.Width / 2, src.Height / 2);
        Mat rotMat = Cv2.GetRotationMatrix2D(center, -angle, 1.0);
        Cv2.WarpAffine(src, src, rotMat, src.Size());
    }
}