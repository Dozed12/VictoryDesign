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
    public static int DesignDifference(Design a, Design b)
    {
        int diff = 0;

        //Calculate characteristics difference
        for (int i = 0; i < a.characteristics.Count; i++)
        {
            diff += (a.characteristics[i].trueValue - b.characteristics[i].trueValue) * (int)a.characteristics[i].importance;
        }

        //Multiply by design importance
        diff *= (int)a.importance;

        return diff;
    }

    //Calculate Nation Difference
    public static int NationDifference()
    {
        int diff = 0;

        //Calculate difference in each design
        foreach (KeyValuePair<string, Design> item in us.designs)
        {
            diff += DesignDifference(us.designs[item.Key], them.designs[item.Key]);
        }

        return diff;
    }

    [Serializable]
    public class CharacteristicAnalysis
    {
        public string name;
        public int ourValue;
        public int theirValue;
        public int diff;
        public int importance;
        public int diffImportance;
    }

    [Serializable]
    public class DesignAnalysis
    {
        public string name;
        public int diff;
        public int importance;
        public int diffImportance;
        public List<CharacteristicAnalysis> characteristics;
    }

    //Deep Difference Analysis
    public static DesignAnalysis[] DeepDifferenceAnalysis()
    {
        //New Analysis
        List<DesignAnalysis> analysis = new List<DesignAnalysis>();

        //Each design
        foreach (KeyValuePair<string, Design> item in us.designs)
        {
            //New Design Analysis
            DesignAnalysis designAnalysis = new DesignAnalysis();
            designAnalysis.characteristics = new List<CharacteristicAnalysis>();
            designAnalysis.name = item.Key;

            //Each Characteristic
            for (int i = 0; i < item.Value.characteristics.Count; i++)
            {
                CharacteristicAnalysis characteristicAnalysis = new CharacteristicAnalysis();

                //Characteristic Analysis
                characteristicAnalysis.name = item.Value.characteristics[i].name;
                characteristicAnalysis.ourValue = us.designs[item.Key].characteristics[i].trueValue;
                characteristicAnalysis.theirValue = them.designs[item.Key].characteristics[i].trueValue;
                characteristicAnalysis.diff = us.designs[item.Key].characteristics[i].trueValue - them.designs[item.Key].characteristics[i].trueValue;
                characteristicAnalysis.importance = (int)us.designs[item.Key].characteristics[i].importance;
                characteristicAnalysis.diffImportance = (us.designs[item.Key].characteristics[i].trueValue - them.designs[item.Key].characteristics[i].trueValue) * characteristicAnalysis.importance;

                //Add to Design Analysis base diff
                designAnalysis.diff += characteristicAnalysis.diffImportance;

                designAnalysis.characteristics.Add(characteristicAnalysis);
            }

            //Get importance
            designAnalysis.importance = (int)us.designs[item.Key].importance;

            //Calculate design diff importance
            designAnalysis.diffImportance = designAnalysis.diff * (int)us.designs[item.Key].importance;

            analysis.Add(designAnalysis);
        }

        return analysis.ToArray();
    }
}
