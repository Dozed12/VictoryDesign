using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
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

[Serializable]
public class Map
{
    //Stage of expansion from 0 to 5
    public int warStage = 0;

    //Stage of expansion positions
    public List<List<Region>> warStagePositions = new List<List<Region>>()
    {
         new List<Region>()
            {
                new Region( new Point(272, 333)),
                new Region( new Point(366, 333)),
                new Region( new Point(450, 387)),
                new Region( new Point(533, 431)),
                new Region( new Point(628, 400)),
                new Region( new Point(692, 360)),
                new Region( new Point(800, 351)),
            }
        ,
        new List<Region>()
            {
                new Region( new Point(200, 400)),
                new Region( new Point(305, 415)),
                new Region( new Point(400, 450)),
                new Region( new Point(500, 500)),
                new Region( new Point(585, 495)),
                new Region( new Point(664, 460)),
                new Region( new Point(760, 438)),
                new Region( new Point(900, 415)),
            }
        ,
        new List<Region>()
            {
                new Region( new Point(108, 440)),
                new Region( new Point(158, 488)),
                new Region( new Point(224, 480)),
                new Region( new Point(335, 504)),
                new Region( new Point(451, 560)),
                new Region( new Point(550, 550)),
                new Region( new Point(630, 540)),
                new Region( new Point(737, 511)),
                new Region( new Point(837, 505)),
                new Region( new Point(938, 477)),
                new Region( new Point(1000, 430)),
            }
        ,
        new List<Region>()
            {
                new Region( new Point(23, 513)),
                new Region( new Point(108, 506)),
                new Region( new Point(209, 545)),
                new Region( new Point(320, 585)),
                new Region( new Point(400, 630)),
                new Region( new Point(510, 630)),
                new Region( new Point(610, 630)),
                new Region( new Point(711, 600)),
                new Region( new Point(808, 600)),
                new Region( new Point(917, 571)),
                new Region( new Point(997, 521)),
            }
        ,
        new List<Region>()
            {
                new Region( new Point(150, 641)),
                new Region( new Point(241, 621)),
                new Region( new Point(279, 679)),
                new Region( new Point(450, 700)),
                new Region( new Point(577, 676)),
                new Region( new Point(736, 674)),
                new Region( new Point(873, 658)),
                new Region( new Point(1023, 600)),
            }
        ,
        new List<Region>()
            {
                new Region( new Point(645, 737)),
            }
    };

    //Stage of unification positions
    public List<Region> unificationPositions = new List<Region>()
    {
        new Region( new Point(850, 92)),
        new Region( new Point(449, 138))
    };

    //Stage of ally positions
    public List<Region> allyPositions = new List<Region>()
    {
        new Region( new Point(269, 30)),
        new Region( new Point(398, 270)),
        new Region( new Point(247, 192))
    };

    //Stage of revenge position
    public Region revengePosition = new Region(new Point(695, 200));

    //Setup Pixel Groups
    public void SetupPixels(Texture2D map)
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
    public Texture2D BuildMap(Texture2D map)
    {
        //Settings
        int spacing = 13;
        int thicknessUs = 6;
        int thicknessOther = 10;
        Color32 enemyColor = new Color32(122, 122, 122, 255);
        Color32 enemyAllyColor = new Color32(160, 160, 160, 255);

        //Transform to Matrix
        PixelMatrix matrix = new PixelMatrix(map);

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
        for (int i = 0; i < warStagePositions.Count; i++)
        {
            //Regions of this stage
            List<Region> regionsOfStage = warStagePositions[i];

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

    //Lose War Progress
    public void LoseWar(int num)
    {
        //Regions of this stage
        List<Region> regionsOfStage = warStagePositions[warStage];

        //Get unoccupied regions
        List<Region> unoccupied = new List<Region>();
        for (int j = 0; j < regionsOfStage.Count; j++)
        {
            if (!regionsOfStage[j].occupied)
                unoccupied.Add(regionsOfStage[j]);
        }

        //If unoccupied was only last one then progress stage
        if (unoccupied.Count == 1)
            warStage++;

        //Occupy random of unoccupied
        unoccupied[UnityEngine.Random.Range(0, unoccupied.Count)].occupied = true;

        //If more progress repeat process
        if (num - 1 > 0)
            LoseWar(num - 1);
    }

    //Win War Progress
    public void WinWar(int num)
    {
        //Regions of this stage
        List<Region> regionsOfStage = warStagePositions[warStage];

        //Get unoccupied regions
        List<Region> occupied = new List<Region>();
        for (int j = 0; j < regionsOfStage.Count; j++)
        {
            if (regionsOfStage[j].occupied)
                occupied.Add(regionsOfStage[j]);
        }

        //If unoccupied was only last one then progress stage
        if (occupied.Count == 1)
            warStage--;

        //Occupy random of unoccupied
        occupied[UnityEngine.Random.Range(0, occupied.Count)].occupied = false;

        //If more progress repeat process
        if (num - 1 > 0)
            WinWar(num - 1);
    }

    //Progress Unification
    public void ProgressUnification(int num)
    {
        unificationPositions[num].unified = true;
    }

    //Progress Unification
    public void ProgressAllies(int num)
    {
        allyPositions[num].ally = true;
    }

    //Progress Revenge
    public void ProgressRevenge(int num)
    {
        revengePosition.occupied = true;
    }
}