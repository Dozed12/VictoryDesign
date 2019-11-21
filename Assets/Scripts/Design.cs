using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Importance
{
    NONE,
    LOW,
    MEDIUM,
    HIGH
}

//Design Institute
[Serializable]
public class DesignInstitute
{
    //Name
    public string name;

    //Designs
    public List<Design> designs;

    //Design types this can generate
    public Type[] types;

    //Naming conventions
    public string baseName;
    public string connector;
    public string specific;

    //Base Design Names
    public Dictionary<Type, string> baseNames;

    public DesignInstitute(Type[] types)
    {
        //Set random Naming Conventions
        baseName = Naming.BaseNameRules.GetRandom();
        connector = Naming.ConnectorNameRules.GetRandom();
        specific = Naming.SpecificNameRules.GetRandom();

        //Set base names
        baseNames = new Dictionary<Type, string>();
        System.Random random = new System.Random();
        for (int i = 0; i < types.Length; i++)
        {
            Fare.Xeger xeger = new Fare.Xeger(baseName, random);
            baseNames.Add(types[i], xeger.Generate());
        }

        //Set types
        this.types = types;
    }

    //Generate Name
    private string GenerateName(Type type)
    {
        //Random provider
        System.Random random = new System.Random();

        //Get Base Name
        string name = baseNames[type];

        //Setup Xeger
        Fare.Xeger xeger;

        //Connector (if connector is empty just ignore it, Xeger doesn't like empty string rule)
        if (connector != "")
        {
            xeger = new Fare.Xeger(connector, random);
            name += xeger.Generate();
        }

        //Specific
        xeger = new Fare.Xeger(specific, random);
        name += xeger.Generate();

        return name;
    }

    //Check if this Insitute can design specified type
    public bool CanDesign(Type type)
    {
        return Array.Exists(types, element => element == type);
    }

    //Generate Design
    public Design GenerateDesign(Type type)
    {
        //Generate Name
        string name = GenerateName(type);

        //Identify Type
        if (type == typeof(Rifle))
        {
            return new Rifle().Generate(name);
        }
        else if (type == typeof(SmallArm))
        {
            return new SmallArm().Generate(name);
        }
        else if (type == typeof(Uniform))
        {
            return new Uniform().Generate(name);
        }
        else if (type == typeof(Helmet))
        {
            return new Helmet().Generate(name);
        }
        else if (type == typeof(MachineGun))
        {
            return new MachineGun().Generate(name);
        }
        //Not a type of design
        else
        {
            return null;
        }
    }
}

//Generic Design
[Serializable]
public abstract class Design
{
    //Design Institute that developed this
    public DesignInstitute developer;

    //How long does the design last before new design is needed(months)
    public int redesignPeriod;

    //Base Value of Redesign Period(variates with type)
    public int redesignPeriodBase;

    //Variation of redesign period +-
    public static float REDESIGN_PERIOD_VARIATION;

    //Name of design
    public string name;

    //Date of design development
    public DateTime date;

    //Importance of design
    public Importance importance;

    //Generate Design Generic
    virtual public Design Generate(string name)
    {
        //Design Date
        date = Game.date;

        //Design Name
        this.name = name;

        //Redesign Period
        redesignPeriod = redesignPeriodBase + UnityEngine.Random.Range(-redesignPeriodBase * Mathf.RoundToInt(REDESIGN_PERIOD_VARIATION / 2),
                                                                        redesignPeriodBase * Mathf.RoundToInt(REDESIGN_PERIOD_VARIATION / 2));

        return this;
    }
}

//Characteristic of a design
[Serializable]
public class Characteristic
{
    //Name of characteristic
    public string name;

    //Importance of characteristic for design
    public Importance importance;

    //Value of characteristic -10 to 10
    public int trueValue;

    //Predicted value of characteristic -2 to 2
    public int predictedValue;

    //Constructor
    public Characteristic(string name, Importance importance)
    {
        this.name = name;
        this.importance = importance;
    }

    //Generate values
    public void Generate()
    {
        /* Predicted values and True value bounds
        -2      -10     -5
        -1      -10     0
        0       -5      5
        1       0       10
        2       5       10
        */
        
        //Predicted value from -2 to 2
        predictedValue = UnityEngine.Random.Range(-2, 2 + 1);

        //Calculate bounds from predicted
        int trueLeftBound = 0;
        int trueRightBound = 0;

        switch (predictedValue)
        {
            case -2:
                trueLeftBound = -10;
                trueRightBound = -5;
                break;
            case -1:
                trueLeftBound = -10;
                trueRightBound = 0;
                break;
            case 0:
                trueLeftBound = -5;
                trueRightBound = 5;
                break;
            case 1:
                trueLeftBound = 0;
                trueRightBound = 10;
                break;
            case 2:
                trueLeftBound = 5;
                trueRightBound = 10;
                break;
        }

        //Randomize true value from bounds
        trueValue = UnityEngine.Random.Range(trueLeftBound, trueRightBound + 1);
    }
}

//Specific Designs

[Serializable]
public class Rifle : Design
{
    //Characteristics
    public Characteristic accuracy = new Characteristic("Accuracy", Importance.HIGH);
    public Characteristic power = new Characteristic("Power", Importance.MEDIUM);
    public Characteristic portability = new Characteristic("Portability", Importance.LOW);

    override public Design Generate(string name)
    {
        //Base redesign period
        redesignPeriodBase = 24;

        //Call Generic
        base.Generate(name);

        //Generate characteristics values
        accuracy.Generate();
        power.Generate();
        portability.Generate();

        //Design importance
        importance = Importance.HIGH;

        return this;
    }
}

[Serializable]
public class SmallArm : Design
{
    //Characteristics
    public Characteristic accuracy = new Characteristic("Accuracy", Importance.HIGH);
    public Characteristic power = new Characteristic("Power", Importance.MEDIUM);
    public Characteristic portability = new Characteristic("Portability", Importance.LOW);

    override public Design Generate(string name)
    {
        //Base redesign period
        redesignPeriodBase = 24;

        //Call Generic
        base.Generate(name);

        //Generate characteristics values
        accuracy.Generate();
        power.Generate();
        portability.Generate();

        //Design importance
        importance = Importance.MEDIUM;

        return this;
    }
}

[Serializable]
public class Uniform : Design
{
    //Characteristics
    public Characteristic weatherResistance = new Characteristic("Weather Resistance", Importance.MEDIUM);
    public Characteristic camouflage = new Characteristic("Camouflage", Importance.MEDIUM);
    public Characteristic comfort = new Characteristic("Comfort", Importance.LOW);

    override public Design Generate(string name)
    {
        //Base redesign period
        redesignPeriodBase = 36;

        //Call Generic
        base.Generate(name);

        //Generate characteristics values
        weatherResistance.Generate();
        camouflage.Generate();
        comfort.Generate();

        //Design importance
        importance = Importance.LOW;

        return this;
    }
}

[Serializable]
public class Helmet : Design
{
    //Characteristics
    public Characteristic protection = new Characteristic("Protection", Importance.HIGH);
    public Characteristic comfort = new Characteristic("Comfort", Importance.LOW);

    override public Design Generate(string name)
    {
        //Base redesign period
        redesignPeriodBase = 36;

        //Call Generic
        base.Generate(name);

        //Generate characteristics values
        protection.Generate();
        comfort.Generate();

        //Design importance
        importance = Importance.LOW;

        return this;
    }
}

[Serializable]
public class MachineGun : Design
{
    //Characteristics
    public Characteristic rof = new Characteristic("Rate of Fire", Importance.HIGH);
    public Characteristic power = new Characteristic("Power", Importance.MEDIUM);
    public Characteristic portability = new Characteristic("Portability", Importance.LOW);

    override public Design Generate(string name)
    {
        //Base redesign period
        redesignPeriodBase = 36;

        //Call Generic
        base.Generate(name);

        //Generate characteristics values
        rof.Generate();
        power.Generate();
        portability.Generate();

        //Design importance
        importance = Importance.LOW;

        return this;
    }
}