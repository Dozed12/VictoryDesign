using System;
using System.Collections;
using System.Collections.Generic;
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
            if(child.name == childName)
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
            if(child.name == childName)
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
}