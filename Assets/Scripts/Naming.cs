using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Fare(xeger) https://www.softwaredeveloper.blog/fare-generate-string-matching-regex-in-csharp
//Regex https://regex101.com/

public class Naming
{

    public static WeightedRandomBag<string> BaseNameRules = new WeightedRandomBag<string>();
    public static WeightedRandomBag<string> ConnectorNameRules = new WeightedRandomBag<string>();
    public static WeightedRandomBag<string> SpecificNameRules = new WeightedRandomBag<string>();

    public static System.Random random;

    //Setup Naming Regex rules
    public static void SetupNaming()
    {
        //Base Name
        BaseNameRules.AddEntry("[A-Z]", 100);
        BaseNameRules.AddEntry("[A-Z][A-Z]", 30);
        BaseNameRules.AddEntry("[A-Z][A-Z][A-Z]", 10);
        BaseNameRules.AddEntry("[A-Z][a-z]", 10);
        BaseNameRules.AddEntry("[A-Z][a-z][a-z]", 10);

        //Connectors
        ConnectorNameRules.AddEntry("-", 200);
        ConnectorNameRules.AddEntry(" ", 300);
        ConnectorNameRules.AddEntry("", 100);

        //Specifics
        SpecificNameRules.AddEntry("[A-Z]", 20);
        SpecificNameRules.AddEntry("^([0-9]|[1-9][0-9]|[1-9][0-9][0-9])$", 100);

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
