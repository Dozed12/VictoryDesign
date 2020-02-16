using UnityEngine;
using System.Collections.Generic;

public class PixelMatrix
{

    public Color[] pixels;
    public int width;
    public int height;

    public PixelMatrix(Texture2D texture)
    {
        this.pixels = texture.GetPixels();
        this.width = texture.width;
        this.height = texture.height;
    }

    public PixelMatrix(PixelMatrix c)
    {
        this.width = c.width;
        this.height = c.height;
        this.pixels = (Color[])c.pixels.Clone();
    }

    public PixelMatrix(int width, int height, Color clear)
    {
        //Initialize
        pixels = new Color[width * height];
        this.width = width;
        this.height = height;

        //Clear with color
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = clear;
        }
    }

    public Color GetPixel(int x, int y)
    {
        if (x < 0)
            x = 0;
        else if (x > width - 1)
            x = width - 1;

        if (y < 0)
            y = 0;
        else if (y > height - 1)
            y = height - 1;

        return pixels[x * width + y];
    }

    public Color GetPixelSafe(int x, int y)
    {
        return pixels[x * width + y];
    }

    public void SetPixel(int x, int y, Color cl)
    {
        //Check if outside
        if (x < 0 || x >= width || y < 0 || y >= height)
            return;

        pixels[x * width + y] = cl;
    }

    public void SetPixelSafe(int x, int y, Color cl)
    {
        pixels[x * width + y] = cl;
    }
}

public struct Point
{
    public int x;
    public int y;

    public Point(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}

public static class DrawingUtils
{
    //Copy Texture
    public static Texture2D TextureCopy(Texture2D original)
    {
        Texture2D copy = new Texture2D(original.width, original.height, original.format, false);
        Graphics.CopyTexture(original, copy);
        return copy;
    }

    //Paint NET coordinates to Unity
    public static Point PaintCoordinatesToUnity(Texture2D texture, int x, int y)
    {
        return new Point(x, texture.height - y);
    }
    public static Point PaintCoordinatesToUnity(Texture2D texture, Point point)
    {
        return new Point(point.x, texture.height - point.y);
    }

    //Scan-line flood fill
    public static Texture2D FloodFillLine(Texture2D bmp, Point point, Color replacementColor)
    {
        Point pt = point;

        Color targetColor = bmp.GetPixel(pt.x, pt.y);
        if (targetColor == replacementColor)
        {
            return bmp;
        }

        Stack<Point> pixels = new Stack<Point>();

        pixels.Push(pt);
        while (pixels.Count != 0)
        {
            Point temp = pixels.Pop();
            int y1 = temp.y;

            while (y1 >= 0 && bmp.GetPixel(temp.x, y1) == targetColor)
            {
                y1--;
            }
            y1++;
            bool spanLeft = false;
            bool spanRight = false;

            while (y1 < bmp.height && bmp.GetPixel(temp.x, y1) == targetColor)
            {
                bmp.SetPixel(temp.x, y1, replacementColor);

                Color clm1 = bmp.GetPixel(temp.x - 1, y1);
                Color clp1 = bmp.GetPixel(temp.x + 1, y1);

                if (!spanLeft && temp.x > 0 && clm1 == targetColor)
                {
                    pixels.Push(new Point(temp.x - 1, y1));
                    spanLeft = true;
                }
                else if (spanLeft && temp.x - 1 >= 0 && clm1 != targetColor)
                {
                    spanLeft = false;
                }
                if (!spanRight && temp.x < bmp.width - 1 && clp1 == targetColor)
                {
                    pixels.Push(new Point(temp.x + 1, y1));
                    spanRight = true;
                }
                else if (spanRight && temp.x < bmp.width - 1 && clp1 != targetColor)
                {
                    spanRight = false;
                }
                y1++;
            }

        }

        bmp.Apply();

        return bmp;

    }

    //Scan-line flood fill points
    public static List<Point> FloodFillLinePoints(Texture2D bmp, Point point)
    {
        List<Point> points = new List<Point>();

        Color replacementColor = Color.black;

        Point pt = point;

        Color targetColor = bmp.GetPixel(pt.x, pt.y);
        if (targetColor == replacementColor)
        {
            return points;
        }

        Stack<Point> pixels = new Stack<Point>();

        pixels.Push(pt);
        while (pixels.Count != 0)
        {
            Point temp = pixels.Pop();
            int y1 = temp.y;

            while (y1 >= 0 && bmp.GetPixel(temp.x, y1) == targetColor)
            {
                y1--;
            }
            y1++;
            bool spanLeft = false;
            bool spanRight = false;

            while (y1 < bmp.height && bmp.GetPixel(temp.x, y1) == targetColor)
            {
                bmp.SetPixel(temp.x, y1, replacementColor);
                points.Add(new Point(y1, temp.x));

                Color clm1 = bmp.GetPixel(temp.x - 1, y1);
                Color clp1 = bmp.GetPixel(temp.x + 1, y1);

                if (!spanLeft && temp.x > 0 && clm1 == targetColor)
                {
                    pixels.Push(new Point(temp.x - 1, y1));
                    spanLeft = true;
                }
                else if (spanLeft && temp.x - 1 >= 0 && clm1 != targetColor)
                {
                    spanLeft = false;
                }
                if (!spanRight && temp.x < bmp.width - 1 && clp1 == targetColor)
                {
                    pixels.Push(new Point(temp.x + 1, y1));
                    spanRight = true;
                }
                else if (spanRight && temp.x < bmp.width - 1 && clp1 != targetColor)
                {
                    spanRight = false;
                }
                y1++;
            }

        }

        return points;
    }

    //Combine Pixel Matrices
    public static PixelMatrix MultiCombine(List<PixelMatrix> list)
    {
        PixelMatrix result = new PixelMatrix(list[0].width, list[0].height, Color.clear);

        Color32 color = new Color32(86, 86, 86, 255);

        for (int i = 0; i < list[0].width; i++)
        {
            for (int j = 0; j < list[0].height; j++)
            {
                for (int l = 0; l < list.Count; l++)
                {
                    if (list[l].GetPixelSafe(j, i).a != 0)
                    {
                        result.SetPixelSafe(j, i, color);
                        break;
                    }
                }
            }
        }

        return result;
    }

}