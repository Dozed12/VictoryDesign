using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Game
{
    //Date and Turn
    public static int turn;
    public static DateTime date;

    //Our Nation
    public static Nation us;

    //Enemy Nation
    public static Nation them;

    //Calculate Design Difference
    public static void DesignDifference(Design a, Design b)
    {
        int diff = 0;

        //Calculate characteristics difference
        for (int i = 0; i < a.characteristics.Count; i++)
        {
            diff += (b.characteristics[i].trueValue - a.characteristics[i].trueValue) * (int)a.characteristics[i].importance;
        }

        //Multiply by design importance
        diff *= (int)a.importance;
    }

    //Calculate Nation Difference
    public static void NationDifference()
    {
        
    }
}
