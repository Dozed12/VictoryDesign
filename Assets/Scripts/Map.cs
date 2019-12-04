using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Region
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
    //Pre war expansion stage (before player war, 1 for each nation)
    public static int expansionStage;

    //Pre war expansion stage positions
    public static List<Region> prewarStagePositions = new List<Region>()
    {
        { new Region(new Point(850, 92), false)}
    };

    //Stage of expansion from 0 to 6 (0 pre war, 6 only capital left)
    public static int warStage;

    //Stage of expansion positions
    public static List<List<Region>> warStagePositions = new List<List<Region>>()
    {
        { new List<Region>()
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

    //Build map at current stage (an optimized version could just increment the paint in case it's enemy expansion[since rest of map will stay the same])
    public static Texture2D BuildMap(Texture2D map)
    {
        //Make copy of map
        Texture2D final = DrawingUtils.TextureCopy(map);

        //Settings
        int spacing = 13;
        int thickness = 3;
        Color32 enemyColor = new Color32(122, 122, 122, 255);

        //For each stage
        for (int i = 0; i < warStage; i++)
        {
            //Regions of this stage
            List<Region> regionsOfStage = warStagePositions[warStage - 1];

            //For each region of the stage
            for (int j = 0; j < regionsOfStage.Count; j++)
            {
                //If stage is occupied
                if (regionsOfStage[j].occupied)
                {
                    //Get Points
                    List<Point> points = DrawingUtils.FloodFillLinePoints(DrawingUtils.TextureCopy(map), DrawingUtils.PaintCoordinatesToUnity(map, regionsOfStage[j].point));

                    //Draw diagonals
                    for (int p = 0; p < points.Count; p++)
                    {
                        if ((points[p].x + points[p].y) % spacing < thickness)
                        {
                            final.SetPixel(points[p].x, points[p].y, enemyColor);
                        }
                    }
                }
            }
        }

        //Apply to texture
        final.Apply();

        return final;
    }

    //TODO Progress Pre War Expansion (pre war stages are fixed order)

    //Progress War Expansion (check if stage still has possible region to occupy if so randomly add it, else move to next stage)
    public static void ProgressWar(int num)
    {
        //Regions of this stage
        List<Region> regionsOfStage = warStagePositions[warStage - 1];

        //Get unoccupied regions
        List<Region> unoccupied = new List<Region>();
        for (int j = 0; j < regionsOfStage.Count; j++)
        {
            if (!regionsOfStage[j].occupied)
                unoccupied.Add(regionsOfStage[j]);
        }

        //Occupy random of unoccupied
        unoccupied[UnityEngine.Random.Range(0, unoccupied.Count)].occupied = true;

        //If unoccupied was only last one then progress stage
        if (unoccupied.Count == 1)
            warStage++;

        //If more progress repeat process
        if (num - 1 > 0)
            ProgressWar(num - 1);
    }
}