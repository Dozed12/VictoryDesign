using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
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
}