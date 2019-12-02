using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

public class Map
{
    //Stage of expansion from 0 to 6 (0 pre war, 6 only capital left)
    public int stage;

    //Stage of expansion positions
    public Dictionary<int, List<Region>> warStagePositions = new Dictionary<int, List<Region>>()
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
    public int expansion;

    //Pre war expansion stage positions
    public Dictionary<int, Region> prewarStagePositions = new Dictionary<int, Region>()
    {
        { 1, new Region(new Point(850, 92), false)}
    };

}