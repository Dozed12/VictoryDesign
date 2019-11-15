using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Design Institute
public class DesignInstitute
{
    //Name
    public string name;

    //Designs
    public List<Design> designs;

    //TODO Naming convention

    //Generate Design
    public Design GenerateDesign(Type type)
    {
        //Identify Type
        if (type == typeof(Rifle))
        {
            return new Rifle().Generate();
        }
        else if (type == typeof(SmallArm))
        {
            return new SmallArm().Generate();
        }
        //Not a type of design
        else
        {
            return null;
        }
    }
}

//Generic Design
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
    public int importance;

    //Generate Design Generic
    virtual public Design Generate()
    {
        //Design Date
        date = Nation.date;

        //TODO Design Name
        name = "TEST";

        //Redesign Period
        redesignPeriod = redesignPeriodBase + UnityEngine.Random.Range(-redesignPeriodBase * Mathf.RoundToInt(REDESIGN_PERIOD_VARIATION / 2),
                                                                        redesignPeriodBase * Mathf.RoundToInt(REDESIGN_PERIOD_VARIATION / 2));

        return this;
    }
}

//Characteristic of a design
public class Characteristic
{
    //Name of characteristic
    public string name;

    //Importance of characteristic for design
    public int importance;

    //Value of characteristic -10 to 10
    public int trueValue;

    //Predicted value of characteristic -2 to 2
    public int predictedValue;

    //Constructor
    public Characteristic(string name, int importance)
    {
        this.name = name;
        this.importance = importance;
    }

    //Generate values
    public void Generate()
    {
        trueValue = UnityEngine.Random.Range(-10, 10 + 1);
        predictedValue = UnityEngine.Random.Range(-2, 2 + 1);
    }
}

//Specific Designs

public class Rifle : Design
{
    //Characteristics TODO set names and importance
    public Characteristic accuracy = new Characteristic("Accuracy", 1);
    public Characteristic power;
    public Characteristic portability;

    override public Design Generate()
    {
        //Base redesign period
        redesignPeriodBase = 24;

        //Call Generic
        base.Generate();

        //Generate characteristics values
        accuracy.Generate();
        power.Generate();
        portability.Generate();

        //TODO Set importance
        importance = 1;

        return this;
    }
}

public class SmallArm : Design
{
    public Characteristic accuracy;
    public Characteristic power;
    public Characteristic portability;
}

public class Uniform : Design
{
    public Characteristic weatherResistance;
    public Characteristic camouflage;
    public Characteristic comfort;
}