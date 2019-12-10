using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public static class Utils
{
    public static System.Random random = new System.Random();

    //Json dump to console
    public static void Dump(object obj)
    {
        Debug.Log(JsonUtility.ToJson(obj, true));
    }

    //Wrapper for Array Dump
    [Serializable]
    private class Wrapper<T>
    {
        public T[] Items;
    }

    //Json dump array to console
    public static void DumpArray<T>(T[] array)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        Debug.Log(JsonUtility.ToJson(wrapper, true));
    }

    //Get child by name
    public static GameObject GetChild(GameObject holder, string childName)
    {
        foreach (Transform child in holder.transform)
        {
            if (child.name == childName)
                return child.gameObject;
        }

        return null;
    }

    //Get child recursively by name
    public static GameObject GetChildRecursive(GameObject holder, string childName)
    {
        Transform[] children = holder.GetComponentsInChildren<Transform>();

        foreach (Transform child in children)
        {
            if (child.name == childName)
                return child.gameObject;
        }

        return null;
    }

    //Clear Children
    public static void ClearChildren(GameObject holder)
    {
        foreach (Transform child in holder.transform)
        {
            UnityEngine.Object.Destroy(child.gameObject);
        }
    }

    //Calculate Average of 2 values (Randomly floor or ceil)
    public static int IntRandomAverage(int a, int b)
    {
        int random = UnityEngine.Random.Range(0, 2);

        if (random == 1)
        {
            return Mathf.CeilToInt((a + b) / 2);
        }
        else
        {
            return Mathf.FloorToInt((a + b) / 2);
        }
    }

    //Sign Coloring (color green or red based on signal)
    public static string SignColoring(int val)
    {
        if(val >= 0)
            return "<color=green>" + val + "</color>";
        else
            return "<color=red>" + val + "</color>";
    }
}