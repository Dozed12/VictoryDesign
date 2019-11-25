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
    //Owner
    public Nation side;

    //Name
    public string name = "NONE";

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

    public DesignInstitute(Type[] types, Nation nation)
    {
        //Set Nation
        this.side = nation;

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
        //Get Base Name
        string name = baseNames[type];

        //Setup Xeger
        Fare.Xeger xeger;

        //Connector (if connector is empty just ignore it, Xeger doesn't like empty string rule)
        if (connector != "")
        {
            xeger = new Fare.Xeger(connector, Utils.random);
            name += xeger.Generate();
        }

        //Specific
        xeger = new Fare.Xeger(specific, Utils.random);
        name += xeger.Generate();

        return name;
    }

    //Check if this Insitute can design specified type
    public bool CanDesign(Type type)
    {
        return Array.Exists(types, element => element == type);
    }

    //Generate Design
    public Design GenerateDesign(Type type, Nation owner)
    {
        //Generate Name
        string name = GenerateName(type);

        //Identify Type
        if (type == typeof(Rifle))
        {
            return new Rifle().Generate(this, name, owner);
        }
        else if (type == typeof(SmallArm))
        {
            return new SmallArm().Generate(this, name, owner);
        }
        else if (type == typeof(Uniform))
        {
            return new Uniform().Generate(this, name, owner);
        }
        else if (type == typeof(Helmet))
        {
            return new Helmet().Generate(this, name, owner);
        }
        else if (type == typeof(MachineGun))
        {
            return new MachineGun().Generate(this, name, owner);
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
    //Owner
    public Nation owner;

    //Characteristics
    public List<Characteristic> characteristics;

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

    //Age in months
    public int age;

    //Importance of design
    public Importance importance;

    //Generate Design Generic
    virtual public Design Generate(DesignInstitute developer, string name, Nation nation)
    {
        //Set Nation
        this.owner = nation;

        //Clear Characteristics
        characteristics = new List<Characteristic>();

        //Design Date
        date = Game.date;
        age = 0;

        //Design Name
        this.name = name;

        //Redesign Period
        redesignPeriod = redesignPeriodBase + UnityEngine.Random.Range(-redesignPeriodBase * Mathf.RoundToInt(REDESIGN_PERIOD_VARIATION / 2),
                                                                        redesignPeriodBase * Mathf.RoundToInt(REDESIGN_PERIOD_VARIATION / 2));

        //Add developer
        this.developer = developer;

        return this;
    }

    //Find Desired Characteristic
    public Characteristic FindCharacteristic(string name)
    {
        for (int i = 0; i < characteristics.Count; i++)
        {
            if(characteristics[i].name == name)
                return characteristics[i];
        }
        return new Characteristic("FIND_FAIL", Importance.HIGH);
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

    //Predicted value of characteristic -2 to 2
    public int predictedValue;

    //Value of characteristic -10 to 10
    public int trueValue;

    //Current known bounds of true value
    public int leftBound;
    public int rightBound;

    //Full Knowledge
    public bool fullKnowledge = false;

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
        leftBound = 0;
        rightBound = 0;

        switch (predictedValue)
        {
            case -2:
                leftBound = -10;
                rightBound = -5;
                break;
            case -1:
                leftBound = -10;
                rightBound = 0;
                break;
            case 0:
                leftBound = -5;
                rightBound = 5;
                break;
            case 1:
                leftBound = 0;
                rightBound = 10;
                break;
            case 2:
                leftBound = 5;
                rightBound = 10;
                break;
        }

        //Randomize true value from bounds
        trueValue = UnityEngine.Random.Range(leftBound, rightBound + 1);
    }

    //Progress Known Bounds
    public void ProgressBounds(int amount)
    {
        //If amount is enough for full knowledge just finish it
        if (rightBound - leftBound <= amount)
        {
            leftBound = trueValue;
            rightBound = trueValue;
            fullKnowledge = true;
            return;
        }

        //Randomly split Knowledge for each bound
        for (int i = 0; i < amount; i++)
        {
            //Left Bound already done, progress right
            if (leftBound == trueValue && rightBound > amount)
            {
                rightBound--;
            }
            //Right Bound already done, progress left
            else if (rightBound == trueValue && leftBound < amount)
            {
                leftBound++;
            }
            //None done, randomly improve one
            else
            {
                int choice = UnityEngine.Random.Range(0, 2);

                if (choice == 0)
                    rightBound--;
                else
                    leftBound++;
            }
        }
    }
}

//Specific Designs

[Serializable]
public class Rifle : Design
{
    override public Design Generate(DesignInstitute developer, string name, Nation nation)
    {
        //Call Generic
        base.Generate(developer, name, nation);

        //Base redesign period
        redesignPeriodBase = 24;

        //Generate characteristics values
        Characteristic accuracy = new Characteristic("Accuracy", Importance.HIGH);
        accuracy.Generate();
        characteristics.Add(accuracy);

        Characteristic power = new Characteristic("Power", Importance.MEDIUM);
        power.Generate();
        characteristics.Add(power);

        Characteristic portability = new Characteristic("Portability", Importance.LOW);
        portability.Generate();
        characteristics.Add(portability);

        //Design importance
        importance = Importance.HIGH;

        return this;
    }
}

[Serializable]
public class SmallArm : Design
{
    override public Design Generate(DesignInstitute developer, string name, Nation nation)
    {
        //Call Generic
        base.Generate(developer, name, nation);

        //Base redesign period
        redesignPeriodBase = 24;

        //Generate characteristics values
        Characteristic accuracy = new Characteristic("Accuracy", Importance.HIGH);
        accuracy.Generate();
        characteristics.Add(accuracy);

        Characteristic power = new Characteristic("Power", Importance.MEDIUM);
        power.Generate();
        characteristics.Add(power);

        Characteristic portability = new Characteristic("Portability", Importance.LOW);
        portability.Generate();
        characteristics.Add(portability);

        //Design importance
        importance = Importance.MEDIUM;

        return this;
    }
}

[Serializable]
public class Uniform : Design
{
    override public Design Generate(DesignInstitute developer, string name, Nation nation)
    {
        //Call Generic
        base.Generate(developer, name, nation);

        //Base redesign period
        redesignPeriodBase = 36;

        //Generate characteristics values
        Characteristic weatherResistance = new Characteristic("Weather Resistance", Importance.MEDIUM);
        weatherResistance.Generate();
        characteristics.Add(weatherResistance);

        Characteristic camouflage = new Characteristic("Camouflage", Importance.MEDIUM);
        camouflage.Generate();
        characteristics.Add(camouflage);

        Characteristic comfort = new Characteristic("Comfort", Importance.LOW);
        comfort.Generate();
        characteristics.Add(comfort);

        //Design importance
        importance = Importance.LOW;

        return this;
    }
}

[Serializable]
public class Helmet : Design
{
    override public Design Generate(DesignInstitute developer, string name, Nation nation)
    {
        //Call Generic
        base.Generate(developer, name, nation);

        //Base redesign period
        redesignPeriodBase = 36;

        //Generate characteristics values
        Characteristic armor = new Characteristic("Armor", Importance.HIGH);
        armor.Generate();
        characteristics.Add(armor);

        Characteristic comfort = new Characteristic("Comfort", Importance.LOW);
        comfort.Generate();
        characteristics.Add(comfort);

        //Design importance
        importance = Importance.LOW;

        return this;
    }
}

[Serializable]
public class MachineGun : Design
{
    override public Design Generate(DesignInstitute developer, string name, Nation nation)
    {
        //Call Generic
        base.Generate(developer, name, nation);

        //Base redesign period
        redesignPeriodBase = 36;

        //Generate characteristics values
        Characteristic rof = new Characteristic("Rate of Fire", Importance.MEDIUM);
        rof.Generate();
        characteristics.Add(rof);

        Characteristic power = new Characteristic("Power", Importance.MEDIUM);
        power.Generate();
        characteristics.Add(power);

        Characteristic portability = new Characteristic("Portability", Importance.LOW);
        portability.Generate();
        characteristics.Add(portability);

        //Design importance
        importance = Importance.HIGH;

        return this;
    }
}