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

public enum Field
{
    INFANTRY_WARFARE,
    ARTILLERY,
    BREAKTHROUGH,
    EXPLOITATION,
    ANTI_ARMOR,
    MORALE
}

//Design Institute
public class DesignInstitute
{
    //Name
    public string name;

    //Designs
    public List<Design> designs;

    //TODO Naming conventions

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
        else if (type == typeof(Uniform))
        {
            return new Uniform().Generate();
        }
        else if (type == typeof(Helmet))
        {
            return new Helmet().Generate();
        }
        else if (type == typeof(MachineGun))
        {
            return new MachineGun().Generate();
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
    public Importance importance;

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
        trueValue = UnityEngine.Random.Range(-10, 10 + 1);
        predictedValue = UnityEngine.Random.Range(-2, 2 + 1);
    }
}

//Specific Designs

public class Rifle : Design
{
    //Characteristics
    public Characteristic accuracy = new Characteristic("Accuracy", Importance.HIGH);
    public Characteristic power = new Characteristic("Power", Importance.MEDIUM);
    public Characteristic portability = new Characteristic("Portability", Importance.LOW);

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

        //Design importance
        importance = Importance.HIGH;

        return this;
    }
}

public class SmallArm : Design
{
    //Characteristics
    public Characteristic accuracy = new Characteristic("Accuracy", Importance.HIGH);
    public Characteristic power = new Characteristic("Power", Importance.MEDIUM);
    public Characteristic portability = new Characteristic("Portability", Importance.LOW);

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

        //Design importance
        importance = Importance.MEDIUM;

        return this;
    }
}

public class Uniform : Design
{
    //Characteristics
    public Characteristic weatherResistance = new Characteristic("Weather Resistance", Importance.MEDIUM);
    public Characteristic camouflage  = new Characteristic("Camouflage", Importance.MEDIUM);
    public Characteristic comfort  = new Characteristic("Comfort", Importance.LOW);

    override public Design Generate()
    {
        //Base redesign period
        redesignPeriodBase = 36;

        //Call Generic
        base.Generate();

        //Generate characteristics values
        weatherResistance.Generate();
        camouflage.Generate();
        comfort.Generate();

        //Design importance
        importance = Importance.LOW;

        return this;
    }
}

public class Helmet : Design
{
    //Characteristics
    public Characteristic protection = new Characteristic("Protection", Importance.HIGH);
    public Characteristic comfort  = new Characteristic("Comfort", Importance.LOW);

    override public Design Generate()
    {
        //Base redesign period
        redesignPeriodBase = 36;

        //Call Generic
        base.Generate();

        //Generate characteristics values
        protection.Generate();
        comfort.Generate();

        //Design importance
        importance = Importance.LOW;

        return this;
    }
}

public class MachineGun : Design
{
    //Characteristics
    public Characteristic rof = new Characteristic("Rate of Fire", Importance.HIGH);
    public Characteristic power  = new Characteristic("Power", Importance.MEDIUM);
    public Characteristic portability  = new Characteristic("Portability", Importance.LOW);

    override public Design Generate()
    {
        //Base redesign period
        redesignPeriodBase = 36;

        //Call Generic
        base.Generate();

        //Generate characteristics values
        rof.Generate();
        power.Generate();
        portability.Generate();

        //Design importance
        importance = Importance.LOW;

        return this;
    }
}