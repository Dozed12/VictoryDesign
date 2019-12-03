using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public struct Region
{
    public Point point;
    public bool occupied;

    public Region(Point point, bool occupied)
    {
        this.point = point;
        this.occupied = occupied;
    }
}

public static class Map
{
    //Stage of expansion from 0 to 6 (0 pre war, 6 only capital left)
    public static int stage;

    //Stage of expansion positions
    public static Dictionary<int, List<Region>> warStagePositions = new Dictionary<int, List<Region>>()
    {
        { 1, new List<Region>()
            {
                new Region( new Point(272, 333), false),
                new Region( new Point(366, 333), false),
                new Region( new Point(450, 387), false),
                new Region( new Point(533, 431), false),
                new Region( new Point(628, 400), false),
                new Region( new Point(692, 360), false),
                new Region( new Point(800, 351), false),
            }
        }
        //TODO Rest of regions
    };

    //Pre war expansion stage (before player war, 1 for each nation)
    public static int expansion;

    //Pre war expansion stage positions
    public static Dictionary<int, Region> prewarStagePositions = new Dictionary<int, Region>()
    {
        { 1, new Region(new Point(850, 92), false)}
    };

    //TODO Build map at current stage (an optimized version could just increment the paint in case it's enemy expansion[since rest of map will stay the same])
    public static Texture2D BuildMap(Texture2D map)
    {
        //Get Points
        List<Point> points = DrawingUtils.FloodFillLinePoints(DrawingUtils.TextureCopy(map), DrawingUtils.PaintCoordinatesToUnity(map, 700, 200));

        //Make copy of map
        Texture2D final = DrawingUtils.TextureCopy(map);

        //Draw diagonals
        int spacing = 13;
        int thickness = 3;
        Color32 enemyColor = new Color32(122,122,122,255);
        for (int i = 0; i < points.Count; i++)
        {
            if((points[i].x + points[i].y) % spacing < thickness)
            {
                final.SetPixel(points[i].x, points[i].y, enemyColor);
            }
        }

        //Apply to texture
        final.Apply();

        return final;
    }

    //TODO Progress Pre War Expansion (pre war stages are fixed order)

    //TODO Progress War Expansion (check if stage still has possible region to occupy if so randomly add it, else move to next stage)
}