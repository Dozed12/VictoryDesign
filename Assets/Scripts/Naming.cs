using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Fare(xeger) https://www.softwaredeveloper.blog/fare-generate-string-matching-regex-in-csharp
//Regex https://regex101.com/

public class Naming
{

    public static WeightedRandomBag<string> BaseNameRules = new WeightedRandomBag<string>();
    public static WeightedRandomBag<string> ConnectorNameRules = new WeightedRandomBag<string>();

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
    }

    //Generate Industry Name(generic for testing)
    public static string GenIndustry()
    {
        Fare.Xeger xeger = new Fare.Xeger(BaseNameRules.GetRandom(), new System.Random());
        return xeger.Generate();
    }

}
