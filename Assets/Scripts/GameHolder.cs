using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        //Test Generate new Helmet on same Design Institute
        if (Input.GetKeyDown(KeyCode.R))
        {
            List<Helmet> helmets = Game.RequestDesignUs(typeof(Helmet)).Cast<Helmet>().ToList();
            Utils.Dump(helmets[0]);
        }
    }

    public void SetupNewGame()
    {
        //Set start date and turn
        Game.date = new DateTime(1890, 1, 1);
        Game.turn = 1;

        //Clear Institutes
        Game.ourInstitutes = new List<DesignInstitute>();
        Game.theirInstitutes = new List<DesignInstitute>();

        //Create Our Base Institutes
        Game.ourInstitutes.Add(new DesignInstitute(new Type[] { typeof(Helmet), typeof(Uniform) }));
        Game.ourInstitutes.Add(new DesignInstitute(new Type[] { typeof(Rifle) }));
        Game.ourInstitutes.Add(new DesignInstitute(new Type[] { typeof(SmallArm) }));
        Game.ourInstitutes.Add(new DesignInstitute(new Type[] { typeof(MachineGun) }));

        //Create Their Base Institutes
        Game.theirInstitutes.Add(new DesignInstitute(new Type[] { typeof(Helmet), typeof(Uniform) }));
        Game.theirInstitutes.Add(new DesignInstitute(new Type[] { typeof(Rifle) }));
        Game.theirInstitutes.Add(new DesignInstitute(new Type[] { typeof(SmallArm) }));
        Game.theirInstitutes.Add(new DesignInstitute(new Type[] { typeof(MachineGun) }));
    }

    // Select Design to show
    // Info is of format: "who.type"
    public void SelectDesign(string info)
    {
        // Separate string
        string[] parts = info.Split('.');
        string who = parts[0];
        string what = parts[1];

        Debug.Log("Design Selected: " + who + " " + what);
    }

}