using System.Drawing;
using Tesseract;

class Clustering
{
    private const int WhiteThreshold = 125;

    public static Cluster GetClusterBounds(string imagePath)
    {
        Bitmap image = new Bitmap(imagePath);
        int width = image.Width;
        int height = image.Height;

        int left = 0, right = width, bottom = height, top = 0;

        bool changes = true;
        while (changes)
        {
            changes = false;
            int centerH = (right + left) / 2, centerV = (bottom + top) / 2;
            try
            {
                changes = false;
                if ((left < right - 1) && !IsWhite(image.GetPixel(left + 1, centerV)))
                {
                    left++;
                    changes = true;
                }
                if ((right > left + 1) && !IsWhite(image.GetPixel(right - 1, centerV)))
                {
                    right--;
                    changes = true;
                }
                if ((top < bottom - 1) && !IsWhite(image.GetPixel(centerH, top + 1)))
                {
                    top++;
                    changes = true;
                }
                if ((bottom > top + 1) && !IsWhite(image.GetPixel(centerH, bottom - 1)))
                {
                    bottom--;
                    changes = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during clustering: {ex.Message}");
            }
        }

        return new Cluster(top, right, bottom, left);
    }

    private static bool IsWhite(Color pixel)
    {
        return (pixel.R + pixel.G + pixel.B) / 3 >= WhiteThreshold;
    }
}

public class Cluster
{
    public int top;
    public int right;
    public int bottom;
    public int left;

    public Cluster(int top, int right, int bottom, int left)
    {
        this.top = top;
        this.right = right;
        this.bottom = bottom;
        this.left = left;
    }

    public Rect ToRect()
    {
        return Rect.FromCoords(left, top, right, bottom);
    }
}