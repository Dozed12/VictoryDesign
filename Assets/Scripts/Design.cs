using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Design Institute
[Serializable]
public class DesignInstitute
{
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
    public Design GenerateDesign(Type type)
    {
        //Generate Name
        string name = GenerateName(type);

        //Generate Design of desired type
        Design design = (Design)Activator.CreateInstance(type);
        design.Generate(this, name);

        return design;
    }
}

//Generic Design
[Serializable]
public abstract class Design
{
    //Characteristics
    public List<Characteristic> characteristics;

    //Design Institute that developed this
    public DesignInstitute developer;

    //How long does the design last before new design is needed(months)
    public int redesignPeriod;

    //Base Value of Redesign Period(variates with type)
    public int redesignPeriodBase;

    //Variation of redesign period +-
    public static float REDESIGN_PERIOD_VARIATION = 0.2f;

    //Name of design
    public string name;

    //Generate Design Generic
    virtual public Design Generate(DesignInstitute developer, string name)
    {
        //Clear Characteristics
        characteristics = new List<Characteristic>();

        //Industrial Characteristics
        Characteristic engineering = new Characteristic("Engineering Cost", Impact.ENGINEERING, this);
        engineering.Generate();
        characteristics.Add(engineering);

        Characteristic resource = new Characteristic("Resource Cost", Impact.RESOURCES, this);
        resource.Generate();
        characteristics.Add(resource);

        Characteristic reliability = new Characteristic("Reliability", Impact.REPLENISH, this);
        reliability.Generate();
        characteristics.Add(reliability);

        //Design Name
        this.name = name;

        //Redesign Period
        redesignPeriod = redesignPeriodBase + UnityEngine.Random.Range(Mathf.RoundToInt(-redesignPeriodBase * REDESIGN_PERIOD_VARIATION / 2),
                                                                        Mathf.RoundToInt(redesignPeriodBase * REDESIGN_PERIOD_VARIATION / 2));

        //Add developer
        this.developer = developer;

        return this;
    }

    //Find Desired Characteristic
    public Characteristic FindCharacteristic(string name)
    {
        for (int i = 0; i < characteristics.Count; i++)
        {
            if (characteristics[i].name == name)
                return characteristics[i];
        }
        return new Characteristic("FIND_FAIL", Impact.ANTI_ARMOR, new Rifle());
    }

    //Progress Characteristic
    public void ProgressCharacteristic(string name, int amount)
    {
        FindCharacteristic(name).ProgressBounds(amount);
    }

    //Possible Progress in Characteristics
    public int PossibleProgress()
    {
        int possible = 0;

        for (int i = 0; i < characteristics.Count; i++)
        {
            possible += characteristics[i].rightBound - characteristics[i].leftBound;
        }

        return possible;
    }

    //Progress Random Characteristics
    public void ProgressRandom(int amount)
    {
        //While we have amount to improve or PossibleProgress to make pick random characteristic and improve it
        while (amount > 0 && PossibleProgress() > 0)
        {
            //Find Progressable Characteristics
            List<int> progressable = new List<int>();
            for (int i = 0; i < characteristics.Count; i++)
            {
                if (!characteristics[i].fullKnowledge)
                {
                    progressable.Add(i);
                }
            }

            //If no more progressable just exit (this shouldn't ever happen because of while condition)
            if (progressable.Count == 0)
                return;

            //Pick random
            int random = UnityEngine.Random.Range(0, progressable.Count);

            //Improve picked by one
            characteristics[progressable[random]].ProgressBounds(1);

            //Reduce amount
            amount--;
        }
    }
}

public enum Impact
{
    ENGINEERING,
    RESOURCES,
    REPLENISH,

    ANTI_INFANTRY,
    ANTI_ARMOR,
    BREAKTHROUGH,
    EXPLOITATION,
    MORALE,
    COMBAT_EFFICIENCY
}

//Characteristic of a design
[Serializable]
public class Characteristic
{
    //Name of characteristic
    public string name;

    // Doctrine/Industry
    public Impact impact;

    //Predicted value of characteristic -2 to 2
    public int predictedValue;

    //Value of characteristic -10 to 10
    public int trueValue;

    //Corresponding Design
    public Design design;

    //Current known bounds of true value
    public int leftBound;
    public int rightBound;

    //Knowledge Flags
    public bool emptyKnowledge = false;
    public bool fullKnowledge = false;

    //Constructor
    public Characteristic(string name, Impact impact, Design design)
    {
        this.name = name;
        this.impact = impact;
        this.design = design;

        Generate();
    }

    //Generate values
    public void Generate(int minimumValue = -2)
    {
        /* Predicted values and True value bounds
        -2      -10     -5
        -1      -10     0
        0       -5      5
        1       0       10
        2       5       10
        */

        //Predicted value from -2 to 2
        predictedValue = UnityEngine.Random.Range(minimumValue, 2 + 1);

        //Calculate bounds from predicted
        leftBound = 0;
        rightBound = 0;

        //Bounds
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
        //Randomly split Knowledge for each bound
        for (int i = 0; i < amount; i++)
        {
            //Left Bound already done, progress right
            if (leftBound == trueValue && rightBound > trueValue)
            {
                rightBound--;
            }
            //Right Bound already done, progress left
            else if (rightBound == trueValue && leftBound < trueValue)
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

        //If both bounds done then signal full Knowledge
        if (rightBound == trueValue && leftBound == trueValue)
        {
            fullKnowledge = true;
        }

        //At least some progress was made so no longer empty knowledge
        emptyKnowledge = false;
    }
}

//Specific Designs

[Serializable]
public class Rifle : Design
{
    override public Design Generate(DesignInstitute developer, string name)
    {
        //Base redesign period
        redesignPeriodBase = 12;

        //Call Generic
        base.Generate(developer, name);

        //Generate characteristics values
        characteristics.Add(new Characteristic("Accuracy", Impact.ANTI_INFANTRY, this));

        characteristics.Add(new Characteristic("Portability", Impact.MORALE, this));

        return this;
    }
}

[Serializable]
public class SubmachineGun : Design
{
    override public Design Generate(DesignInstitute developer, string name)
    {
        //Base redesign period
        redesignPeriodBase = 12;

        //Call Generic
        base.Generate(developer, name);

        //Generate characteristics values
        characteristics.Add(new Characteristic("Accuracy", Impact.ANTI_INFANTRY, this));

        characteristics.Add(new Characteristic("Rate of Fire", Impact.BREAKTHROUGH, this));

        characteristics.Add(new Characteristic("Portability", Impact.MORALE, this));

        return this;
    }
}

[Serializable]
public class MachineGun : Design
{
    override public Design Generate(DesignInstitute developer, string name)
    {
        //Base redesign period
        redesignPeriodBase = 12;

        //Call Generic
        base.Generate(developer, name);

        //Generate characteristics values
        characteristics.Add(new Characteristic("Rate of Fire", Impact.ANTI_INFANTRY, this));

        characteristics.Add(new Characteristic("Portability", Impact.MORALE, this));

        return this;
    }
}

[Serializable]
public class Mortar : Design
{
    override public Design Generate(DesignInstitute developer, string name)
    {
        //Base redesign period
        redesignPeriodBase = 12;

        //Call Generic
        base.Generate(developer, name);

        //Generate characteristics values
        characteristics.Add(new Characteristic("Shell", Impact.BREAKTHROUGH, this));

        characteristics.Add(new Characteristic("Portability", Impact.MORALE, this));

        return this;
    }
}

[Serializable]
public class AntiTankRifle : Design
{
    override public Design Generate(DesignInstitute developer, string name)
    {
        //Base redesign period
        redesignPeriodBase = 12;

        //Call Generic
        base.Generate(developer, name);

        //Generate characteristics values
        characteristics.Add(new Characteristic("Penetration", Impact.ANTI_ARMOR, this));

        characteristics.Add(new Characteristic("Portability", Impact.MORALE, this));

        return this;
    }
}

[Serializable]
public class AntiTankCannon : Design
{
    override public Design Generate(DesignInstitute developer, string name)
    {
        //Base redesign period
        redesignPeriodBase = 12;

        //Call Generic
        base.Generate(developer, name);

        //Generate characteristics values
        characteristics.Add(new Characteristic("Caliber", Impact.ANTI_ARMOR, this));

        characteristics.Add(new Characteristic("Muzzle Velocity", Impact.ANTI_ARMOR, this));

        characteristics.Add(new Characteristic("Portability", Impact.MORALE, this));

        return this;
    }
}

[Serializable]
public class Artillery : Design
{
    override public Design Generate(DesignInstitute developer, string name)
    {
        //Base redesign period
        redesignPeriodBase = 12;

        //Call Generic
        base.Generate(developer, name);

        //Generate characteristics values
        characteristics.Add(new Characteristic("Shell", Impact.ANTI_INFANTRY, this));

        characteristics.Add(new Characteristic("Range", Impact.BREAKTHROUGH, this));

        characteristics.Add(new Characteristic("Portability", Impact.MORALE, this));

        return this;
    }
}

[Serializable]
public class Tankette : Design
{
    override public Design Generate(DesignInstitute developer, string name)
    {
        //Base redesign period
        redesignPeriodBase = 12;

        //Call Generic
        base.Generate(developer, name);

        //Generate characteristics values
        characteristics.Add(new Characteristic("Machine Gun", Impact.ANTI_INFANTRY, this));

        characteristics.Add(new Characteristic("Speed", Impact.EXPLOITATION, this));

        characteristics.Add(new Characteristic("Maneuverability", Impact.EXPLOITATION, this));

        characteristics.Add(new Characteristic("Accomodation", Impact.MORALE, this));

        return this;
    }
}

[Serializable]
public class CavalryTank : Design
{
    override public Design Generate(DesignInstitute developer, string name)
    {
        //Base redesign period
        redesignPeriodBase = 12;

        //Call Generic
        base.Generate(developer, name);

        //Generate characteristics values
        characteristics.Add(new Characteristic("Machine Gun", Impact.ANTI_INFANTRY, this));

        characteristics.Add(new Characteristic("Speed", Impact.EXPLOITATION, this));

        characteristics.Add(new Characteristic("Accomodation", Impact.MORALE, this));

        return this;
    }
}

[Serializable]
public class CruiserTank : Design
{
    override public Design Generate(DesignInstitute developer, string name)
    {
        //Base redesign period
        redesignPeriodBase = 12;

        //Call Generic
        base.Generate(developer, name);

        //Generate characteristics values
        characteristics.Add(new Characteristic("Cannon", Impact.ANTI_ARMOR, this));

        characteristics.Add(new Characteristic("Armor", Impact.BREAKTHROUGH, this));

        characteristics.Add(new Characteristic("Speed", Impact.EXPLOITATION, this));

        characteristics.Add(new Characteristic("Accomodation", Impact.MORALE, this));

        return this;
    }
}

[Serializable]
public class InfantryTank : Design
{
    override public Design Generate(DesignInstitute developer, string name)
    {
        //Base redesign period
        redesignPeriodBase = 12;

        //Call Generic
        base.Generate(developer, name);

        //Generate characteristics values
        characteristics.Add(new Characteristic("Machine Gun", Impact.ANTI_INFANTRY, this));

        characteristics.Add(new Characteristic("Howitzer", Impact.BREAKTHROUGH, this));

        characteristics.Add(new Characteristic("Armor", Impact.BREAKTHROUGH, this));

        characteristics.Add(new Characteristic("Accomodation", Impact.MORALE, this));

        return this;
    }
}

[Serializable]
public class TankDestroyer : Design
{
    override public Design Generate(DesignInstitute developer, string name)
    {
        //Base redesign period
        redesignPeriodBase = 12;

        //Call Generic
        base.Generate(developer, name);

        //Generate characteristics values
        characteristics.Add(new Characteristic("Caliber", Impact.ANTI_ARMOR, this));

        characteristics.Add(new Characteristic("Muzzle Velocity", Impact.ANTI_ARMOR, this));

        characteristics.Add(new Characteristic("Armor", Impact.BREAKTHROUGH, this));

        characteristics.Add(new Characteristic("Accomodation", Impact.MORALE, this));

        return this;
    }
}

[Serializable]
public class MotorcycleRecon : Design
{
    override public Design Generate(DesignInstitute developer, string name)
    {
        //Base redesign period
        redesignPeriodBase = 12;

        //Call Generic
        base.Generate(developer, name);

        //Generate characteristics values
        characteristics.Add(new Characteristic("Sights", Impact.COMBAT_EFFICIENCY, this));

        characteristics.Add(new Characteristic("Accomodation", Impact.MORALE, this));

        return this;
    }
}

[Serializable]
public class ArmoredCar : Design
{
    override public Design Generate(DesignInstitute developer, string name)
    {
        //Base redesign period
        redesignPeriodBase = 12;

        //Call Generic
        base.Generate(developer, name);

        //Generate characteristics values
        characteristics.Add(new Characteristic("Sights", Impact.COMBAT_EFFICIENCY, this));

        characteristics.Add(new Characteristic("Speed", Impact.EXPLOITATION, this));

        characteristics.Add(new Characteristic("Accomodation", Impact.MORALE, this));

        return this;
    }
}

[Serializable]
public class Truck : Design
{
    override public Design Generate(DesignInstitute developer, string name)
    {
        //Base redesign period
        redesignPeriodBase = 12;

        //Call Generic
        base.Generate(developer, name);

        //Generate characteristics values
        characteristics.Add(new Characteristic("Capacity", Impact.COMBAT_EFFICIENCY, this));

        characteristics.Add(new Characteristic("Accomodation", Impact.MORALE, this));

        return this;
    }
}

[Serializable]
public class Halftrack : Design
{
    override public Design Generate(DesignInstitute developer, string name)
    {
        //Base redesign period
        redesignPeriodBase = 12;

        //Call Generic
        base.Generate(developer, name);

        //Generate characteristics values
        characteristics.Add(new Characteristic("Capacity", Impact.COMBAT_EFFICIENCY, this));

        characteristics.Add(new Characteristic("All-terrain", Impact.EXPLOITATION, this));

        characteristics.Add(new Characteristic("Accomodation", Impact.MORALE, this));

        return this;
    }
}

[Serializable]
public class UtilityCar : Design
{
    override public Design Generate(DesignInstitute developer, string name)
    {
        //Base redesign period
        redesignPeriodBase = 12;

        //Call Generic
        base.Generate(developer, name);

        //Generate characteristics values
        characteristics.Add(new Characteristic("Officer Equipment", Impact.COMBAT_EFFICIENCY, this));

        characteristics.Add(new Characteristic("Accomodation", Impact.MORALE, this));

        return this;
    }
}

[Serializable]
public class PrimeMover : Design
{
    override public Design Generate(DesignInstitute developer, string name)
    {
        //Base redesign period
        redesignPeriodBase = 12;

        //Call Generic
        base.Generate(developer, name);

        //Generate characteristics values
        characteristics.Add(new Characteristic("Torque", Impact.COMBAT_EFFICIENCY, this));

        characteristics.Add(new Characteristic("Accomodation", Impact.MORALE, this));

        return this;
    }
}

[Serializable]
public class Telephone : Design
{
    override public Design Generate(DesignInstitute developer, string name)
    {
        //Base redesign period
        redesignPeriodBase = 12;

        //Call Generic
        base.Generate(developer, name);

        //Generate characteristics values
        characteristics.Add(new Characteristic("Wiring", Impact.COMBAT_EFFICIENCY, this));

        characteristics.Add(new Characteristic("Portability", Impact.MORALE, this));

        return this;
    }
}

[Serializable]
public class Radio : Design
{
    override public Design Generate(DesignInstitute developer, string name)
    {
        //Base redesign period
        redesignPeriodBase = 12;

        //Call Generic
        base.Generate(developer, name);

        //Generate characteristics values
        characteristics.Add(new Characteristic("Range", Impact.COMBAT_EFFICIENCY, this));

        characteristics.Add(new Characteristic("Portability", Impact.MORALE, this));

        return this;
    }
}

[Serializable]
public class Engineer : Design
{
    override public Design Generate(DesignInstitute developer, string name)
    {
        //Base redesign period
        redesignPeriodBase = 12;

        //Call Generic
        base.Generate(developer, name);

        //Generate characteristics values
        characteristics.Add(new Characteristic("Terrain Traversal", Impact.COMBAT_EFFICIENCY, this));

        characteristics.Add(new Characteristic("Portability", Impact.MORALE, this));

        return this;
    }
}

[Serializable]
public class Bridging : Design
{
    override public Design Generate(DesignInstitute developer, string name)
    {
        //Base redesign period
        redesignPeriodBase = 12;

        //Call Generic
        base.Generate(developer, name);

        //Generate characteristics values
        characteristics.Add(new Characteristic("River Traversal", Impact.COMBAT_EFFICIENCY, this));

        characteristics.Add(new Characteristic("Portability", Impact.MORALE, this));

        return this;
    }
}