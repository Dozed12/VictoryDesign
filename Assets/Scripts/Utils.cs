using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    //Json dump to console
    public static void Dump(object obj)
    {
        var output = JsonUtility.ToJson(obj, true);
        Debug.Log(output);
    }
}