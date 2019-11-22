using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameHolder : MonoBehaviour
{

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
        Game.them = new Nation();

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

        //Update Info
        for (int i = 0; i < children.Length; i++)
        {
            //Design Name Title
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
        }

    }

}