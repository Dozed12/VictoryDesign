using UnityEngine;
using System.Collections.Generic;

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
                points.Add(new Point(temp.x, y1));

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
}