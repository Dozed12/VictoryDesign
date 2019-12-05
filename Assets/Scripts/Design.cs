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
    public Design GenerateDesign(Type type, Nation owner, int minimumValue = -2)
    {
        //Generate Name
        string name = GenerateName(type);

        //Identify Type
        if (type == typeof(Rifle))
        {
            return new Rifle().Generate(this, name, owner, minimumValue);
        }
        else if (type == typeof(SmallArm))
        {
            return new SmallArm().Generate(this, name, owner, minimumValue);
        }
        else if (type == typeof(Uniform))
        {
            return new Uniform().Generate(this, name, owner, minimumValue);
        }
        else if (type == typeof(Helmet))
        {
            return new Helmet().Generate(this, name, owner, minimumValue);
        }
        else if (type == typeof(MachineGun))
        {
            return new MachineGun().Generate(this, name, owner, minimumValue);
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
    public static float REDESIGN_PERIOD_VARIATION = 0.2f;

    //Name of design
    public string name;

    //Date of design development
    public DateTime date;

    //Age in months
    public int age;

    //Importance of design
    public Importance importance;

    //Generate Design Generic
    virtual public Design Generate(DesignInstitute developer, string name, Nation nation, int minimumValue = -2)
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
        return new Characteristic("FIND_FAIL", Importance.HIGH, new Rifle());
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
            amount --;
        }
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

    //Corresponding Design
    public Design design;

    //Current known bounds of true value
    public int leftBound;
    public int rightBound;

    //Knowledge Flags
    public bool emptyKnowledge = false;
    public bool fullKnowledge = false;

    //Constructor
    public Characteristic(string name, Importance importance, Design design)
    {
        this.name = name;
        this.importance = importance;
        this.design = design;
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

        //If player set base bounds
        if (this.design.owner.isPlayer)
        {
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
        }
        //If not player set base bounds as full and empty knowledge
        else
        {
            leftBound = -10;
            rightBound = 10;
            emptyKnowledge = true;
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
            //Both done, finish
            if (rightBound == trueValue && leftBound == trueValue)
            {
                fullKnowledge = true;
                break;
            }
            //Left Bound already done, progress right
            else if (leftBound == trueValue && rightBound > trueValue)
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

        //At least some progress was made so no longer empty knowledge
        emptyKnowledge = false;
    }
}

//Specific Designs

[Serializable]
public class Rifle : Design
{
    override public Design Generate(DesignInstitute developer, string name, Nation nation, int minimumValue = -2)
    {
        //Base redesign period
        redesignPeriodBase = 24;

        //Call Generic
        base.Generate(developer, name, nation);

        //Generate characteristics values
        Characteristic accuracy = new Characteristic("Accuracy", Importance.HIGH, this);
        accuracy.Generate(minimumValue);
        characteristics.Add(accuracy);

        Characteristic power = new Characteristic("Power", Importance.MEDIUM, this);
        power.Generate(minimumValue);
        characteristics.Add(power);

        Characteristic portability = new Characteristic("Portability", Importance.LOW, this);
        portability.Generate(minimumValue);
        characteristics.Add(portability);

        //Design importance
        importance = Importance.HIGH;

        return this;
    }
}

[Serializable]
public class SmallArm : Design
{
    override public Design Generate(DesignInstitute developer, string name, Nation nation, int minimumValue = -2)
    {
        //Base redesign period
        redesignPeriodBase = 24;

        //Call Generic
        base.Generate(developer, name, nation);

        //Generate characteristics values
        Characteristic accuracy = new Characteristic("Accuracy", Importance.HIGH, this);
        accuracy.Generate(minimumValue);
        characteristics.Add(accuracy);

        Characteristic power = new Characteristic("Power", Importance.MEDIUM, this);
        power.Generate(minimumValue);
        characteristics.Add(power);

        Characteristic portability = new Characteristic("Portability", Importance.LOW, this);
        portability.Generate(minimumValue);
        characteristics.Add(portability);

        //Design importance
        importance = Importance.MEDIUM;

        return this;
    }
}

[Serializable]
public class Uniform : Design
{
    override public Design Generate(DesignInstitute developer, string name, Nation nation, int minimumValue = -2)
    {
        //Base redesign period
        redesignPeriodBase = 36;

        //Call Generic
        base.Generate(developer, name, nation);        

        //Generate characteristics values
        Characteristic weatherResistance = new Characteristic("Weather Resistance", Importance.MEDIUM, this);
        weatherResistance.Generate(minimumValue);
        characteristics.Add(weatherResistance);

        Characteristic camouflage = new Characteristic("Camouflage", Importance.MEDIUM, this);
        camouflage.Generate(minimumValue);
        characteristics.Add(camouflage);

        Characteristic comfort = new Characteristic("Comfort", Importance.LOW, this);
        comfort.Generate(minimumValue);
        characteristics.Add(comfort);

        //Design importance
        importance = Importance.LOW;

        return this;
    }
}

[Serializable]
public class Helmet : Design
{
    override public Design Generate(DesignInstitute developer, string name, Nation nation, int minimumValue = -2)
    {
        //Base redesign period
        redesignPeriodBase = 36;

        //Call Generic
        base.Generate(developer, name, nation);        

        //Generate characteristics values
        Characteristic protection = new Characteristic("Protection", Importance.HIGH, this);
        protection.Generate(minimumValue);
        characteristics.Add(protection);

        Characteristic comfort = new Characteristic("Comfort", Importance.LOW, this);
        comfort.Generate(minimumValue);
        characteristics.Add(comfort);

        //Design importance
        importance = Importance.LOW;

        return this;
    }
}

[Serializable]
public class MachineGun : Design
{
    override public Design Generate(DesignInstitute developer, string name, Nation nation, int minimumValue = -2)
    {
        //Base redesign period
        redesignPeriodBase = 36;

        //Call Generic
        base.Generate(developer, name, nation);

        //Generate characteristics values
        Characteristic rof = new Characteristic("Rate of Fire", Importance.MEDIUM, this);
        rof.Generate(minimumValue);
        characteristics.Add(rof);

        Characteristic power = new Characteristic("Power", Importance.MEDIUM, this);
        power.Generate(minimumValue);
        characteristics.Add(power);

        Characteristic portability = new Characteristic("Portability", Importance.LOW, this);
        portability.Generate(minimumValue);
        characteristics.Add(portability);

        //Design importance
        importance = Importance.HIGH;

        return this;
    }
}