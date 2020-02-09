using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Region
{
    public Point point;
    public bool unified = false;
    public bool ally = false;
    public bool occupied = false;   

    public Region(Point point)
    {
        this.point = point;
    }
}

public static class Map
{
    //Stage of expansion from 0 to 6 (0 pre war, 6 only capital left)
    public static int warStage;

    //Stage of expansion positions
    public static List<List<Region>> warStagePositions = new List<List<Region>>()
    {
        { new List<Region>()
            {
                new Region( new Point(272, 333)),
                new Region( new Point(366, 333)),
                new Region( new Point(450, 387)),
                new Region( new Point(533, 431)),
                new Region( new Point(628, 400)),
                new Region( new Point(692, 360)),
                new Region( new Point(800, 351)),
            }
        }
        //TODO Rest of regions
    };

    //Stage of unification positions
    public static List<Region> unificationPositions = new List<Region>()
    {
        new Region( new Point(850, 92)),
        new Region( new Point(449, 138))
    };

    //Stage of ally positions
    public static List<Region> allyPositions = new List<Region>()
    {
        new Region( new Point(269, 30)),
        new Region( new Point(398, 270)),
        new Region( new Point(247, 192))
    };

    //Stage of revenge position
    public static Region revengePosition = new Region( new Point(695, 200));

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

    //Progress Unification
    public static void ProgressUnification(int num)
    {
        unificationPositions[num].occupied = true;
    }
}