using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameHolder : MonoBehaviour
{
    //UI Elements and Resources
    public Sprite HIGH_IMPORTANCE_SPRITE;
    public Sprite MEDIUM_IMPORTANCE_SPRITE;
    public Sprite LOW_IMPORTANCE_SPRITE;

    public GameObject CHARACTERISTIC;
    public GameObject CHARACTERISTIC_ENEMY;

    public Sprite ACCURACY_ICON;
    public Sprite POWER_ICON;
    public Sprite PORTABILITY_ICON;
    public Sprite RATE_OF_FIRE_ICON;
    public Sprite COMFORT_ICON;
    public Sprite CAMOUFLAGE_ICON;
    public Sprite WEATHER_RESISTANCE_ICON;
    public Sprite ARMOR_ICON;

    // Start is called before the first frame update
    void Start()
    {
        //Setup Naming system
        Naming.SetupNaming();

        //Setup a New Game
        SetupNewGame();
    }

    // Update is called once per frame
    void Update()
    {
        //Test Generate new Helmets Design
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Helmet[] helmets = Game.us.RequestDesign(typeof(Helmet)).Cast<Helmet>().ToArray();
            Utils.DumpArray(helmets);
        }

        //Test Design Characteristic Progress
        if (Input.GetKeyDown(KeyCode.W))
        {
            Game.us.helmet.FindCharacteristic("Protection").ProgressBounds(2);
            Utils.Dump(Game.us.helmet.FindCharacteristic("Protection"));
        }
    }

    public void SetupNewGame()
    {
        //Set start date and turn
        Game.date = new DateTime(1890, 1, 1);
        Game.turn = 1;

        //Setup nations
        Game.us = new Nation();
        Game.us.isPlayer = true;
        Game.them = new Nation();
        Game.them.isPlayer = false;

        //Create Our Base Institutes
        Game.us.AddInstitutes(new Type[] { typeof(Helmet), typeof(Uniform) }, 3);
        Game.us.AddInstitutes(new Type[] { typeof(Rifle) }, 3);
        Game.us.AddInstitutes(new Type[] { typeof(SmallArm) }, 3);
        Game.us.AddInstitutes(new Type[] { typeof(MachineGun) }, 3);

        //Add our base designs
        Game.us.rifle = (Rifle)Game.us.RequestDesign(typeof(Rifle))[0];
        Game.us.smallArm = (SmallArm)Game.us.RequestDesign(typeof(SmallArm))[0];
        Game.us.uniform = (Uniform)Game.us.RequestDesign(typeof(Uniform))[0];
        Game.us.helmet = (Helmet)Game.us.RequestDesign(typeof(Helmet))[0];
        Game.us.machineGun = (MachineGun)Game.us.RequestDesign(typeof(MachineGun))[0];

        //Create Their Base Institutes
        Game.them.AddInstitutes(new Type[] { typeof(Helmet), typeof(Uniform) }, 3);
        Game.them.AddInstitutes(new Type[] { typeof(Rifle) }, 3);
        Game.them.AddInstitutes(new Type[] { typeof(SmallArm) }, 3);
        Game.them.AddInstitutes(new Type[] { typeof(MachineGun) }, 3);

        //Add their base designs
        Game.them.rifle = (Rifle)Game.them.RequestDesign(typeof(Rifle))[0];
        Game.them.smallArm = (SmallArm)Game.them.RequestDesign(typeof(SmallArm))[0];
        Game.them.uniform = (Uniform)Game.them.RequestDesign(typeof(Uniform))[0];
        Game.them.helmet = (Helmet)Game.them.RequestDesign(typeof(Helmet))[0];
        Game.them.machineGun = (MachineGun)Game.them.RequestDesign(typeof(MachineGun))[0];
    }

    // Select Design to show
    // Info is of format: "who.type"
    public void SelectDesign(string info)
    {
        // Separate string
        string[] parts = info.Split('.');
        string who = parts[0];
        string what = parts[1];

        //Identify side
        Nation side;
        switch (who)
        {
            case "our":
                side = Game.us;
                break;
            case "their":
                side = Game.them;
                break;
            default:
                side = new Nation();
                break;
        }

        //Identify design
        Design selectedDesign;
        switch (what)
        {
            case "rifle":
                selectedDesign = side.rifle;
                break;
            case "smallArm":
                selectedDesign = side.smallArm;
                break;
            case "uniform":
                selectedDesign = side.uniform;
                break;
            case "helmet":
                selectedDesign = side.helmet;
                break;
            case "machineGun":
                selectedDesign = side.machineGun;
                break;
            default:
                selectedDesign = new Helmet();
                break;
        }

        //Dump Selected Design
        Utils.Dump(selectedDesign);

        //Display Design
        DisplayDesign(selectedDesign);
    }

    //Display Design
    public void DisplayDesign(Design design)
    {
        //Find Design Panel
        GameObject designPanel = GameObject.Find("DesignPanel");

        //All childs of Panel
        Transform[] transforms = designPanel.GetComponentsInChildren<Transform>();
        GameObject[] children = new GameObject[transforms.Length];
        for (int i = 0; i < transforms.Length; i++)
        {
            children[i] = transforms[i].gameObject;
        }

        //Characteristics Holder
        GameObject characteristics = new GameObject();

        //Update Info
        for (int i = 0; i < children.Length; i++)
        {
            //Check if null (May happen when deleting characteristics)
            if (children[i] == null)
                continue;

            //Design Name in Title
            if (children[i].name == "DesignName")
            {
                children[i].GetComponent<Text>().text = design.name;
            }
            //Design Type
            else if (children[i].name == "TypeName")
            {
                //Get type from class type   
                string type = design.GetType().ToString();

                //Add space before Capital Letters
                type = string.Concat(type.Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');

                //Apply
                children[i].GetComponent<Text>().text = type;
            }
            //Design Name in Info
            else if (children[i].name == "Name")
            {
                children[i].GetComponent<Text>().text = "Name: " + design.name;
            }
            //Developer
            else if (children[i].name == "Developer")
            {
                children[i].GetComponent<Text>().text = "Developer: " + design.developer.name;
            }
            //Date
            else if (children[i].name == "Date")
            {
                children[i].GetComponent<Text>().text = "Date: " + design.date.ToString("MMMM yyyy");
            }
            //Age Months
            else if (children[i].name == "Months")
            {
                children[i].GetComponent<Text>().text = "Months Age: " + design.age + " (" + design.redesignPeriod + ")";
            }
            //Design Importance
            else if (children[i].name == "Importance")
            {
                switch (design.importance)
                {
                    case Importance.HIGH:
                        children[i].GetComponent<Image>().sprite = HIGH_IMPORTANCE_SPRITE;
                        break;
                    case Importance.MEDIUM:
                        children[i].GetComponent<Image>().sprite = MEDIUM_IMPORTANCE_SPRITE;
                        break;
                    case Importance.LOW:
                        children[i].GetComponent<Image>().sprite = LOW_IMPORTANCE_SPRITE;
                        break;
                }
            }
            //Delete current Characteristics
            else if (children[i].name == "Characteristics")
            {
                //Destroy children
                foreach (Transform child in children[i].transform)
                {
                    Destroy(child.gameObject);
                }

                //Save Characteristics Holder(used to add Characteristic prefabs)
                characteristics = children[i];
            }
        }

        //Instantiate Characteristics
        for (int i = 0; i < design.characteristics.Count; i++)
        {
            //Instantiate normal Characteristic Prefab if player design
            GameObject newCharacteristic = new GameObject();
            if (design.owner.isPlayer)
            {
                newCharacteristic = Instantiate(CHARACTERISTIC);
            }
            //Instantiate enemy Characteristic Prefab if not player design
            else
            {
                newCharacteristic = Instantiate(CHARACTERISTIC_ENEMY);
            }

            //Edit values
            foreach (Transform child in newCharacteristic.transform)
            {
                //Set Icon (Icon is set using the Characteristic name and associating it with variable name in this class that stores Sprites)
                //UpperCase(characteristic.name) + "_ICON"
                if (child.name == "Icon")
                {
                    string name = design.characteristics[i].name;
                    name = name.ToUpper();
                    name += "_ICON";
                    name = name.Replace(" ", "_");
                    child.gameObject.GetComponent<Image>().sprite = (Sprite)typeof(GameHolder).GetField(name).GetValue(this);
                }
                //Name
                else if (child.name == "Name")
                {
                    child.gameObject.GetComponent<Text>().text = design.characteristics[i].name;
                }
                //Estimate Value
                else if (child.name == "Estimate")
                {
                    string value = design.characteristics[i].predictedValue.ToString();
                    if (design.characteristics[i].predictedValue > 0)
                        value = "+" + value;
                    child.gameObject.GetComponent<Text>().text = value;
                }
                //True Value
                else if (child.name == "True")
                {
                    string left = design.characteristics[i].leftBound.ToString();
                    if (design.characteristics[i].leftBound > 0)
                        left = "+" + left;

                    string right = design.characteristics[i].rightBound.ToString();
                    if (design.characteristics[i].rightBound > 0)
                        right = "+" + right;

                    child.gameObject.GetComponent<Text>().text = left + " to " + right;
                }
                //Design Importance
                else if (child.name == "Importance")
                {
                    switch (design.characteristics[i].importance)
                    {
                        case Importance.HIGH:
                            child.gameObject.GetComponent<Image>().sprite = HIGH_IMPORTANCE_SPRITE;
                            break;
                        case Importance.MEDIUM:
                            child.gameObject.GetComponent<Image>().sprite = MEDIUM_IMPORTANCE_SPRITE;
                            break;
                        case Importance.LOW:
                            child.gameObject.GetComponent<Image>().sprite = LOW_IMPORTANCE_SPRITE;
                            break;
                    }
                }
            }

            //Add to characteristics holder
            newCharacteristic.transform.SetParent(characteristics.transform);
        }

        //Force update on DesignTitle to fix size
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)GameObject.Find("DesignTitle").transform);
    }

}