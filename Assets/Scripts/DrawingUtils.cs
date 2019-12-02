using UnityEngine;
using System.Collections.Generic;

public static class DrawingUtils
{

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

    public static void FloodFill(Texture2D input, Texture2D output, Color sourceColor, Color targetColor, float tollerance, int x, int y)
    {
        var q = new Queue<Point>(input.width * input.height);
        q.Enqueue(new Point(x, y));
        int iterations = 0;

        var width = input.width;
        var height = input.height;
        while (q.Count > 0)
        {
            var point = q.Dequeue();
            var x1 = point.x;
            var y1 = point.y;
            if (q.Count > width * height)
            {
                throw new System.Exception("The algorithm is probably looping. Queue size: " + q.Count);
            }

            if (output.GetPixel(x1, y1) == targetColor)
            {
                continue;
            }

            output.SetPixel(x1, y1, targetColor);

            var newPoint = new Point(x1 + 1, y1);
            if (CheckValidity(input, input.width, input.height, newPoint, sourceColor, tollerance))
                q.Enqueue(newPoint);

            newPoint = new Point(x1 - 1, y1);
            if (CheckValidity(input, input.width, input.height, newPoint, sourceColor, tollerance))
                q.Enqueue(newPoint);

            newPoint = new Point(x1, y1 + 1);
            if (CheckValidity(input, input.width, input.height, newPoint, sourceColor, tollerance))
                q.Enqueue(newPoint);

            newPoint = new Point(x1, y1 - 1);
            if (CheckValidity(input, input.width, input.height, newPoint, sourceColor, tollerance))
                q.Enqueue(newPoint);

            iterations++;
        }
    }

    public static List<Point> FloodFillFetch(Texture2D input, Texture2D output, Color sourceColor, Color targetColor, float tollerance, int x, int y)
    {
        List<Point> points = new List<Point>();

        var q = new Queue<Point>(input.width * input.height);
        q.Enqueue(new Point(x, y));
        int iterations = 0;

        var width = input.width;
        var height = input.height;
        while (q.Count > 0)
        {
            var point = q.Dequeue();
            var x1 = point.x;
            var y1 = point.y;
            if (q.Count > width * height)
            {
                throw new System.Exception("The algorithm is probably looping. Queue size: " + q.Count);
            }

            if (points.Contains(new Point(x1, y1)))
            {
                continue;
            }

            points.Add(new Point(x1,y1));

            var newPoint = new Point(x1 + 1, y1);
            if (CheckValidity(input, input.width, input.height, newPoint, sourceColor, tollerance))
                q.Enqueue(newPoint);

            newPoint = new Point(x1 - 1, y1);
            if (CheckValidity(input, input.width, input.height, newPoint, sourceColor, tollerance))
                q.Enqueue(newPoint);

            newPoint = new Point(x1, y1 + 1);
            if (CheckValidity(input, input.width, input.height, newPoint, sourceColor, tollerance))
                q.Enqueue(newPoint);

            newPoint = new Point(x1, y1 - 1);
            if (CheckValidity(input, input.width, input.height, newPoint, sourceColor, tollerance))
                q.Enqueue(newPoint);

            iterations++;
        }

        return points;
    }

    static bool CheckValidity(Texture2D texture, int width, int height, Point p, Color sourceColor, float tollerance)
    {
        if (p.x < 0 || p.x >= width)
        {
            return false;
        }
        if (p.y < 0 || p.y >= height)
        {
            return false;
        }

        var color = texture.GetPixel(p.x, p.y);

        var distance = Mathf.Abs(color.r - sourceColor.r) + Mathf.Abs(color.g - sourceColor.g) + Mathf.Abs(color.b - sourceColor.b);
        return distance <= tollerance;
    }
}