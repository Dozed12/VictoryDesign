using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Region
{
    public Point point;
    public Point[] pixels;
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
    //Matrix Image
    public static PixelMatrix matrixMap;

    //Stage of expansion from 0 to 6 (0 pre war, 6 only capital left)
    public static int warStage = 0;

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
    public static Region revengePosition = new Region(new Point(695, 200));

    //Setup Pixel Groups
    public static void SetupPixels(Texture2D map)
    {
        //Create Pixel Matrix for easier access
        PixelMatrix matrix = new PixelMatrix(map);

        //For each Unification Stage
        for (int i = 0; i < unificationPositions.Count; i++)
        {
            //Get Points
            unificationPositions[i].pixels = DrawingUtils.FloodFillLinePoints(matrix, DrawingUtils.PaintCoordinatesToUnity(map, unificationPositions[i].point)).ToArray();
        }

        //For each Alliance Stage
        for (int i = 0; i < allyPositions.Count; i++)
        {
            allyPositions[i].pixels = DrawingUtils.FloodFillLinePoints(matrix, DrawingUtils.PaintCoordinatesToUnity(map, allyPositions[i].point)).ToArray();
        }

        //Revenge Stage
        revengePosition.pixels = DrawingUtils.FloodFillLinePoints(matrix, DrawingUtils.PaintCoordinatesToUnity(map, revengePosition.point)).ToArray();

        //For each War stage
        for (int i = 0; i < warStagePositions.Count; i++)
        {
            //For each region of the stage
            for (int j = 0; j < warStagePositions[i].Count; j++)
            {
                warStagePositions[i][j].pixels = DrawingUtils.FloodFillLinePoints(matrix, DrawingUtils.PaintCoordinatesToUnity(map, warStagePositions[i][j].point)).ToArray();
            }
        }
    }

    //Build map at current stage (an optimized version could just increment the paint in case it's enemy expansion[since rest of map will stay the same])
    public static Texture2D BuildMap(Texture2D map)
    {
        //Settings
        int spacing = 13;
        int thicknessUs = 6;
        int thicknessOther = 10;
        Color32 enemyColor = new Color32(122, 122, 122, 255);
        Color32 enemyAllyColor = new Color32(160, 160, 160, 255);

        //Transform to Matrix
        PixelMatrix matrix = new PixelMatrix(matrixMap);

        //For each Unification Stage
        for (int i = 0; i < unificationPositions.Count; i++)
        {
            if (unificationPositions[i].unified)
            {
                //Get Points
                Point[] points = unificationPositions[i].pixels;

                //Draw diagonals
                for (int p = 0; p < points.Length; p++)
                {
                    matrix.SetPixelSafe(points[p].x, points[p].y, enemyColor);
                }
            }
        }

        //For each Alliance Stage
        for (int i = 0; i < allyPositions.Count; i++)
        {
            if (allyPositions[i].ally)
            {
                //Get Points
                Point[] points = allyPositions[i].pixels;

                //Draw diagonals
                for (int p = 0; p < points.Length; p++)
                {
                    matrix.SetPixelSafe(points[p].x, points[p].y, enemyAllyColor);
                }
            }
        }

        //Revenge Stage
        if (revengePosition.occupied)
        {
            //Get Points
            Point[] points = revengePosition.pixels;

            //Draw diagonals
            for (int p = 0; p < points.Length; p++)
            {
                if ((points[p].x + points[p].y) % spacing < thicknessOther)
                {
                    matrix.SetPixelSafe(points[p].x, points[p].y, enemyColor);
                }
            }
        }

        //For each War stage
        for (int i = 0; i < warStage; i++)
        {
            //Regions of this stage
            List<Region> regionsOfStage = warStagePositions[warStage - 1];

            //For each region of the stage
            for (int j = 0; j < regionsOfStage.Count; j++)
            {
                //If occupied
                if (regionsOfStage[j].occupied)
                {
                    //Get Points
                    Point[] points = regionsOfStage[j].pixels;

                    //Draw diagonals
                    for (int p = 0; p < points.Length; p++)
                    {
                        if ((points[p].x + points[p].y) % spacing < thicknessUs)
                        {
                            matrix.SetPixelSafe(points[p].x, points[p].y, enemyColor);
                        }
                    }
                }
            }
        }

        //Make copy of map
        Texture2D final = DrawingUtils.TextureCopy(map);

        //Apply matrix pixels
        final.SetPixels(matrix.pixels);

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
        unificationPositions[num].unified = true;
    }

    //Progress Unification
    public static void ProgressAllies(int num)
    {
        allyPositions[num].ally = true;
    }

    //Progress Revenge
    public static void ProgressRevenge(int num)
    {
        revengePosition.occupied = true;
    }
}