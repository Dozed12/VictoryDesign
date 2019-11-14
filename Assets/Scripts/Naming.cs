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

    //Setup Naming Regex rules
    public static void SetupNaming()
    {
        BaseNameRules.AddEntry("[A-Z]", 100);
        BaseNameRules.AddEntry("[A-Z][A-Z]", 30);
        BaseNameRules.AddEntry("[A-Z][A-Z][A-Z]", 10);
        BaseNameRules.AddEntry("[A-Z]\\.", 10);
        BaseNameRules.AddEntry("[A-Z][a-z]{2,3}", 10);
        BaseNameRules.AddEntry("[A-Z][a-z]{2,3}\\.", 10);

        ConnectorNameRules.AddEntry("-", 100);
        ConnectorNameRules.AddEntry(" ", 100);
        ConnectorNameRules.AddEntry("", 100);

        SpecificNameRules.AddEntry("[A-Z]", 100);
        SpecificNameRules.AddEntry("^([0-9]|[1-9][0-9]|[1-9][0-9][0-9])$", 100);
    }

    public static string GenBaseName()
    {
        Fare.Xeger xeger = new Fare.Xeger(BaseNameRules.GetRandom(), new System.Random());
        return xeger.Generate();
    }

    public static string GenConnector()
    {
        Fare.Xeger xeger = new Fare.Xeger(ConnectorNameRules.GetRandom(), new System.Random());
        return xeger.Generate();
    }

    public static string GenSpecific()
    {
        Fare.Xeger xeger = new Fare.Xeger(SpecificNameRules.GetRandom(), new System.Random());
        return xeger.Generate();
    }

}
