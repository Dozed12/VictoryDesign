using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;

//Fare(xeger) https://www.softwaredeveloper.blog/fare-generate-string-matching-regex-in-csharp
//Regex https://regex101.com/

public static class DesignNaming
{

    public static WeightedRandomBag<string> BaseNameRules = new WeightedRandomBag<string>();
    public static WeightedRandomBag<string> ConnectorNameRules = new WeightedRandomBag<string>();
    public static WeightedRandomBag<string> SpecificNameRules = new WeightedRandomBag<string>();

    public static System.Random random;

    //Setup Naming Regex rules
    public static void SetupNaming()
    {
        //Base Name
        BaseNameRules.AddEntry("[A-Z]", 10);
        BaseNameRules.AddEntry("[A-Z][A-Z]", 3);
        BaseNameRules.AddEntry("[A-Z][A-Z][A-Z]", 1);
        BaseNameRules.AddEntry("[A-Z][a-z]", 1);
        BaseNameRules.AddEntry("[A-Z][a-z][a-z]", 1);

        //Connectors
        ConnectorNameRules.AddEntry("-", 1);
        ConnectorNameRules.AddEntry(" ", 5);
        ConnectorNameRules.AddEntry("", 1);
        ConnectorNameRules.AddEntry(".", 1);

        //Specifics
        SpecificNameRules.AddEntry("[A-Z]", 1);
        SpecificNameRules.AddEntry("[1-9]", 5);
        SpecificNameRules.AddEntry("[1-9][0-9]", 3);
        SpecificNameRules.AddEntry("[1-9][0-9][0-9]", 1);

        //Set Random Number Generator
        random = new System.Random();
    }

    public static string GenBaseName()
    {
        Fare.Xeger xeger = new Fare.Xeger(BaseNameRules.GetRandom(), random);
        return xeger.Generate();
    }

    public static string GenConnector()
    {
        Fare.Xeger xeger = new Fare.Xeger(ConnectorNameRules.GetRandom(), random);
        return xeger.Generate();
    }

    public static string GenSpecific()
    {
        Fare.Xeger xeger = new Fare.Xeger(SpecificNameRules.GetRandom(), random);
        return xeger.Generate();
    }

}

public static class DesignerNaming
{
    //Locations
    public static string[] locations;

    //Load Location Names
    [Serializable]
    class LocationsCollection
    {
        public Location[] info;
    }
    [Serializable]
    class Location
    {
        public Fields fields;
    }
    [Serializable]
    class Fields
    {
        public string name;
    }
    public static void LoadLocations()
    {
        //Load File
        string path = Application.dataPath + "/StreamingAssets/Locations.json";
        string data = File.ReadAllText(path);

        //Deserialize
        LocationsCollection locationsRaw = JsonUtility.FromJson<LocationsCollection>(data);

        //Pass names to locations array
        locations = new string[locationsRaw.info.Length];
        for (int i = 0; i < locationsRaw.info.Length; i++)
        {
            locations[i] = locationsRaw.info[i].fields.name;
        }
    }

}