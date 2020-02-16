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
        //Designer Name
        name = DesignerNaming.GenerateName();

        //Set random Naming Conventions
        baseName = DesignNaming.BaseNameRules.GetRandom();
        connector = DesignNaming.ConnectorNameRules.GetRandom();
        specific = DesignNaming.SpecificNameRules.GetRandom();

        //Set base names
        baseNames = new Dictionary<Type, string>();
        for (int i = 0; i < types.Length; i++)
        {
            Fare.Xeger xeger = new Fare.Xeger(baseName, DesignNaming.random);
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

        //Add Connector
        name += connector;

        //Setup Xeger
        Fare.Xeger xeger;

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
    public Design GenerateDesign(Type type, int[] mask)
    {
        //Generate Name
        string name = GenerateName(type);

        //Generate Design of desired type
        Design design = (Design)Activator.CreateInstance(type);
        design.Generate(this, name, mask);

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

    //Age of design
    public int age = 0;

    //How long does the design last before new design is needed(months)
    public int redesignPeriod;

    //Name of design
    public string name;

    //Model Graphic
    public Sprite model;

    //Generate Design Generic
    virtual public Design Generate(DesignInstitute developer, string name, int[] mask)
    {
        //Clear Characteristics
        characteristics = new List<Characteristic>();

        //Industrial Characteristics
        characteristics.Add(new Characteristic("Engineering Cost", Impact.ENGINEERING, this, mask[0]));
        characteristics.Add(new Characteristic("Resource Cost", Impact.RESOURCES, this, mask[1]));
        characteristics.Add(new Characteristic("Reliability", Impact.REPLENISH, this, mask[2]));

        //Design Name
        this.name = name;

        //Redesign Period
        redesignPeriod = 12;

        //Add developer
        this.developer = developer;

        //Generate Model
        if(ModelGenerator.CanGenerate(this.GetType().ToString()))
            model = Sprite.Create(ModelGenerator.GenerateModel(this.GetType().ToString()), new Rect(0,0,140,66), Vector3.zero);

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
        return new Characteristic("FIND_FAIL", Impact.ANTI_ARMOR, new Rifle(), 0);
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
    public Characteristic(string name, Impact impact, Design design, int requested)
    {
        this.name = name;
        this.impact = impact;
        this.design = design;

        Generate(requested);
    }

    //Generate values
    public void Generate(int requested = 0)
    {
        //Flip Engineering/Resource Characteristics
        if (name == "Engineering Cost" || name == "Resource Cost")
            requested = -requested;

        /* 
        Requested Values can be -2 to 2
        However 0 has no effect as a request
        -2  ->      Very Low
        -1  ->      Very Low or Low
        1   ->      Very High or High
        2   ->      Very High
        */

        //Predicted value from -2 to 2 or requested
        if (requested != 0)
        {
            if (requested == -2)
                predictedValue = -2;
            else if (requested == -1)
                predictedValue = UnityEngine.Random.Range(-2, -1 + 1);
            else if (requested == 1)
                predictedValue = UnityEngine.Random.Range(1, 2 + 1);
            else if (requested == 2)
                predictedValue = 2;
        }
        else
            predictedValue = UnityEngine.Random.Range(-2, 2 + 1);

        /* Predicted values and True value bounds
        -2  ->      -10     -5
        -1  ->      -10     0
        0   ->      -5      5
        1   ->      0       10
        2   ->      5       10
        */

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

    //String representation of prediction
    public static string PredictedToString(int prediction, bool invert = false)
    {
        if (invert)
        {
            switch (prediction)
            {
                case -2:
                    return "<color=#246E1E>VERY LOW</color>";
                case -1:
                    return "<color=#506E4D>LOW</color>";
                case 0:
                    return "<color=#7D7D7D>NORMAL</color>";
                case 1:
                    return "<color=#815454>HIGH</color>";
                case 2:
                    return "<color=#811919>VERY HIGH</color>";
                default:
                    return "ERROR";
            }
        }
        else
        {
            switch (prediction)
            {
                case -2:
                    return "<color=#811919>VERY LOW</color>";
                case -1:
                    return "<color=#815454>LOW</color>";
                case 0:
                    return "<color=#7D7D7D>NORMAL</color>";
                case 1:
                    return "<color=#506E4D>HIGH</color>";
                case 2:
                    return "<color=#246E1E>VERY HIGH</color>";
                default:
                    return "ERROR";
            }
        }
    }

    //Knowledge to String
    public string KnowledgeToString()
    {
        string result = "";

        if (leftBound != rightBound)
        {
            if (leftBound > 0)
                result += "+" + leftBound;
            else
                result += leftBound;

            result += " to ";

            if (rightBound > 0)
                result += "+" + rightBound;
            else
                result += rightBound;
        }
        else
        {
            if (leftBound > 0)
                result = "+" + leftBound.ToString();
            else
                result = leftBound.ToString();
        }

        return result;
    }
}

//Specific Designs

[Serializable]
public class Rifle : Design
{
    override public Design Generate(DesignInstitute developer, string name, int[] mask)
    {
        //Call Generic
        base.Generate(developer, name, mask);

        //Generate characteristics values
        characteristics.Add(new Characteristic("Accuracy", Impact.ANTI_INFANTRY, this, mask[3]));

        characteristics.Add(new Characteristic("Portability", Impact.MORALE, this, mask[4]));

        return this;
    }
}

[Serializable]
public class SubmachineGun : Design
{
    override public Design Generate(DesignInstitute developer, string name, int[] mask)
    {
        //Call Generic
        base.Generate(developer, name, mask);

        //Generate characteristics values
        characteristics.Add(new Characteristic("Accuracy", Impact.ANTI_INFANTRY, this, mask[3]));

        characteristics.Add(new Characteristic("Rate of Fire", Impact.BREAKTHROUGH, this, mask[4]));

        characteristics.Add(new Characteristic("Portability", Impact.MORALE, this, mask[5]));

        return this;
    }
}

[Serializable]
public class MachineGun : Design
{
    override public Design Generate(DesignInstitute developer, string name, int[] mask)
    {
        //Call Generic
        base.Generate(developer, name, mask);

        //Generate characteristics values
        characteristics.Add(new Characteristic("Rate of Fire", Impact.ANTI_INFANTRY, this, mask[3]));

        characteristics.Add(new Characteristic("Portability", Impact.MORALE, this, mask[4]));

        return this;
    }
}

[Serializable]
public class Mortar : Design
{
    override public Design Generate(DesignInstitute developer, string name, int[] mask)
    {
        //Call Generic
        base.Generate(developer, name, mask);

        //Generate characteristics values
        characteristics.Add(new Characteristic("Shell", Impact.BREAKTHROUGH, this, mask[3]));

        characteristics.Add(new Characteristic("Portability", Impact.MORALE, this, mask[4]));

        return this;
    }
}

[Serializable]
public class AntiTankRifle : Design
{
    override public Design Generate(DesignInstitute developer, string name, int[] mask)
    {
        //Call Generic
        base.Generate(developer, name, mask);

        //Generate characteristics values
        characteristics.Add(new Characteristic("Penetration", Impact.ANTI_ARMOR, this, mask[3]));

        characteristics.Add(new Characteristic("Portability", Impact.MORALE, this, mask[4]));

        return this;
    }
}

[Serializable]
public class AntiTankCannon : Design
{
    override public Design Generate(DesignInstitute developer, string name, int[] mask)
    {
        //Call Generic
        base.Generate(developer, name, mask);

        //Generate characteristics values
        characteristics.Add(new Characteristic("Caliber", Impact.ANTI_ARMOR, this, mask[3]));

        characteristics.Add(new Characteristic("Muzzle Velocity", Impact.ANTI_ARMOR, this, mask[4]));

        characteristics.Add(new Characteristic("Portability", Impact.MORALE, this, mask[5]));

        return this;
    }
}

[Serializable]
public class Artillery : Design
{
    override public Design Generate(DesignInstitute developer, string name, int[] mask)
    {
        //Call Generic
        base.Generate(developer, name, mask);

        //Generate characteristics values
        characteristics.Add(new Characteristic("Shell", Impact.ANTI_INFANTRY, this, mask[3]));

        characteristics.Add(new Characteristic("Range", Impact.BREAKTHROUGH, this, mask[4]));

        characteristics.Add(new Characteristic("Portability", Impact.MORALE, this, mask[5]));

        return this;
    }
}

[Serializable]
public class Tankette : Design
{
    override public Design Generate(DesignInstitute developer, string name, int[] mask)
    {
        //Call Generic
        base.Generate(developer, name, mask);

        //Generate characteristics values
        characteristics.Add(new Characteristic("Machine Gun", Impact.ANTI_INFANTRY, this, mask[3]));

        characteristics.Add(new Characteristic("Speed", Impact.EXPLOITATION, this, mask[4]));

        characteristics.Add(new Characteristic("Maneuverability", Impact.EXPLOITATION, this, mask[5]));

        characteristics.Add(new Characteristic("Accomodation", Impact.MORALE, this, mask[6]));

        return this;
    }
}

[Serializable]
public class CavalryTank : Design
{
    override public Design Generate(DesignInstitute developer, string name, int[] mask)
    {
        //Call Generic
        base.Generate(developer, name, mask);

        //Generate characteristics values
        characteristics.Add(new Characteristic("Machine Gun", Impact.ANTI_INFANTRY, this, mask[3]));

        characteristics.Add(new Characteristic("Speed", Impact.EXPLOITATION, this, mask[4]));

        characteristics.Add(new Characteristic("Accomodation", Impact.MORALE, this, mask[5]));

        return this;
    }
}

[Serializable]
public class CruiserTank : Design
{
    override public Design Generate(DesignInstitute developer, string name, int[] mask)
    {
        //Call Generic
        base.Generate(developer, name, mask);

        //Generate characteristics values
        characteristics.Add(new Characteristic("Cannon", Impact.ANTI_ARMOR, this, mask[3]));

        characteristics.Add(new Characteristic("Armor", Impact.BREAKTHROUGH, this, mask[4]));

        characteristics.Add(new Characteristic("Speed", Impact.EXPLOITATION, this, mask[5]));

        characteristics.Add(new Characteristic("Accomodation", Impact.MORALE, this, mask[6]));

        return this;
    }
}

[Serializable]
public class InfantryTank : Design
{
    override public Design Generate(DesignInstitute developer, string name, int[] mask)
    {
        //Call Generic
        base.Generate(developer, name, mask);

        //Generate characteristics values
        characteristics.Add(new Characteristic("Machine Gun", Impact.ANTI_INFANTRY, this, mask[3]));

        characteristics.Add(new Characteristic("Howitzer", Impact.BREAKTHROUGH, this, mask[4]));

        characteristics.Add(new Characteristic("Armor", Impact.BREAKTHROUGH, this, mask[5]));

        characteristics.Add(new Characteristic("Accomodation", Impact.MORALE, this, mask[6]));

        return this;
    }
}

[Serializable]
public class TankDestroyer : Design
{
    override public Design Generate(DesignInstitute developer, string name, int[] mask)
    {
        //Call Generic
        base.Generate(developer, name, mask);

        //Generate characteristics values
        characteristics.Add(new Characteristic("Caliber", Impact.ANTI_ARMOR, this, mask[3]));

        characteristics.Add(new Characteristic("Muzzle Velocity", Impact.ANTI_ARMOR, this, mask[4]));

        characteristics.Add(new Characteristic("Armor", Impact.BREAKTHROUGH, this, mask[5]));

        characteristics.Add(new Characteristic("Accomodation", Impact.MORALE, this, mask[6]));

        return this;
    }
}

[Serializable]
public class MotorcycleRecon : Design
{
    override public Design Generate(DesignInstitute developer, string name, int[] mask)
    {
        //Call Generic
        base.Generate(developer, name, mask);

        //Generate characteristics values
        characteristics.Add(new Characteristic("Sights", Impact.COMBAT_EFFICIENCY, this, mask[3]));

        characteristics.Add(new Characteristic("Accomodation", Impact.MORALE, this, mask[4]));

        return this;
    }
}

[Serializable]
public class ArmoredCar : Design
{
    override public Design Generate(DesignInstitute developer, string name, int[] mask)
    {
        //Call Generic
        base.Generate(developer, name, mask);

        //Generate characteristics values
        characteristics.Add(new Characteristic("Sights", Impact.COMBAT_EFFICIENCY, this, mask[3]));

        characteristics.Add(new Characteristic("Speed", Impact.EXPLOITATION, this, mask[4]));

        characteristics.Add(new Characteristic("Accomodation", Impact.MORALE, this, mask[5]));

        return this;
    }
}

[Serializable]
public class Truck : Design
{
    override public Design Generate(DesignInstitute developer, string name, int[] mask)
    {
        //Call Generic
        base.Generate(developer, name, mask);

        //Generate characteristics values
        characteristics.Add(new Characteristic("Capacity", Impact.COMBAT_EFFICIENCY, this, mask[3]));

        characteristics.Add(new Characteristic("Accomodation", Impact.MORALE, this, mask[4]));

        return this;
    }
}

[Serializable]
public class Halftrack : Design
{
    override public Design Generate(DesignInstitute developer, string name, int[] mask)
    {
        //Call Generic
        base.Generate(developer, name, mask);

        //Generate characteristics values
        characteristics.Add(new Characteristic("Capacity", Impact.COMBAT_EFFICIENCY, this, mask[3]));

        characteristics.Add(new Characteristic("All-terrain", Impact.EXPLOITATION, this, mask[4]));

        characteristics.Add(new Characteristic("Accomodation", Impact.MORALE, this, mask[5]));

        return this;
    }
}

[Serializable]
public class UtilityCar : Design
{
    override public Design Generate(DesignInstitute developer, string name, int[] mask)
    {
        //Call Generic
        base.Generate(developer, name, mask);

        //Generate characteristics values
        characteristics.Add(new Characteristic("Officer Equipment", Impact.COMBAT_EFFICIENCY, this, mask[3]));

        characteristics.Add(new Characteristic("Accomodation", Impact.MORALE, this, mask[4]));

        return this;
    }
}

[Serializable]
public class PrimeMover : Design
{
    override public Design Generate(DesignInstitute developer, string name, int[] mask)
    {
        //Call Generic
        base.Generate(developer, name, mask);

        //Generate characteristics values
        characteristics.Add(new Characteristic("Torque", Impact.COMBAT_EFFICIENCY, this, mask[3]));

        characteristics.Add(new Characteristic("Accomodation", Impact.MORALE, this, mask[4]));

        return this;
    }
}

[Serializable]
public class Telephone : Design
{
    override public Design Generate(DesignInstitute developer, string name, int[] mask)
    {
        //Call Generic
        base.Generate(developer, name, mask);

        //Generate characteristics values
        characteristics.Add(new Characteristic("Wiring", Impact.COMBAT_EFFICIENCY, this, mask[3]));

        characteristics.Add(new Characteristic("Portability", Impact.MORALE, this, mask[4]));

        return this;
    }
}

[Serializable]
public class Radio : Design
{
    override public Design Generate(DesignInstitute developer, string name, int[] mask)
    {
        //Call Generic
        base.Generate(developer, name, mask);

        //Generate characteristics values
        characteristics.Add(new Characteristic("Range", Impact.COMBAT_EFFICIENCY, this, mask[3]));

        characteristics.Add(new Characteristic("Portability", Impact.MORALE, this, mask[4]));

        return this;
    }
}

[Serializable]
public class Engineer : Design
{
    override public Design Generate(DesignInstitute developer, string name, int[] mask)
    {
        //Call Generic
        base.Generate(developer, name, mask);

        //Generate characteristics values
        characteristics.Add(new Characteristic("Terrain Traversal", Impact.COMBAT_EFFICIENCY, this, mask[3]));

        characteristics.Add(new Characteristic("Portability", Impact.MORALE, this, mask[4]));

        return this;
    }
}

[Serializable]
public class Bridging : Design
{
    override public Design Generate(DesignInstitute developer, string name, int[] mask)
    {
        //Call Generic
        base.Generate(developer, name, mask);

        //Generate characteristics values
        characteristics.Add(new Characteristic("River Traversal", Impact.COMBAT_EFFICIENCY, this, mask[3]));

        characteristics.Add(new Characteristic("Portability", Impact.MORALE, this, mask[4]));

        return this;
    }
}