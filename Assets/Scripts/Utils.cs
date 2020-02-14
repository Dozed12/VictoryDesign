using System;
using System.Linq;
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
        holder.transform.DetachChildren();
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

    //Sign Coloring (color green or maroon based on signal)
    public static string SignColoring(int val)
    {
        if (val >= 0)
            return "<color=green>" + val + "</color>";
        else
            return "<color=maroon>" + val + "</color>";
    }

    //Random Generation with Average (Terrible implementation but works)
    public static List<int> RandomAverage(int size, int avg, int a, int b)
    {
        while (true)
        {
            List<int> result = new List<int>();
            int total = 0;
            for (int i = 0; i < size; i++)
            {
                result.Add(UnityEngine.Random.Range(a, b + 1));
                total += result[i];
            }
            total = Mathf.FloorToInt(total / size);

            if (total == avg)
                return result;
        }
    }

    //Random Generation with Max (Terrible implementation but works)
    public static List<int> RandomMax(int size, int max, int a, int b)
    {
        while (true)
        {
            //Generate Values
            List<int> result = new List<int>();
            for (int i = 0; i < size; i++)
            {
                result.Add(UnityEngine.Random.Range(a, b + 1));
            }

            //Count Values
            Dictionary<int, int> count = new Dictionary<int, int>();
            for (int i = 0; i < size; i++)
            {
                if (count.ContainsKey(result[i]))
                    count[result[i]]++;
                else
                    count[result[i]] = 0;
            }

            //Check if over max
            bool valid = true;
            foreach (var item in count)
            {
                if (item.Value >= max)
                    valid = false;
            }

            //Return values if valid
            if (valid)
                return result;
        }
    }

    //Random Generation with Sum (Terrible implementation but works)
    public static List<int> RandomSum(int size, int sum, int a, int b)
    {
        while (true)
        {
            //Generate Values
            List<int> result = new List<int>();
            for (int i = 0; i < size; i++)
            {
                result.Add(UnityEngine.Random.Range(a, b + 1));
            }

            //Check sum
            if(result.Sum() == sum)
                return result;
        }
    }
}