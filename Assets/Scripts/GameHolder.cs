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
        //Test Generate new Helmets on same Design Institute
        if (Input.GetKeyDown(KeyCode.R))
        {
            List<Helmet> helmets = Game.RequestDesignUs(typeof(Helmet)).Cast<Helmet>().ToList();
            Utils.DumpArray(helmets.ToArray());
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
        Game.AddInstitutesUs(new Type[] { typeof(Helmet), typeof(Uniform) }, 3);
        Game.AddInstitutesUs(new Type[] { typeof(Rifle) }, 3);
        Game.AddInstitutesUs(new Type[] { typeof(SmallArm) }, 3);
        Game.AddInstitutesUs(new Type[] { typeof(MachineGun) }, 3);

        //Create Their Base Institutes
        Game.AddInstitutesThem(new Type[] { typeof(Helmet), typeof(Uniform) }, 3);
        Game.AddInstitutesThem(new Type[] { typeof(Rifle) }, 3);
        Game.AddInstitutesThem(new Type[] { typeof(SmallArm) }, 3);
        Game.AddInstitutesThem(new Type[] { typeof(MachineGun) }, 3);
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