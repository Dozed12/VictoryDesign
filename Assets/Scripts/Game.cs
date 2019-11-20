using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
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
            if (Nation.institutes[0].CanDesign(typeof(Helmet)))
            {
                Helmet helmet = (Helmet)Nation.institutes[0].GenerateDesign(typeof(Helmet));
                Utils.Dump(helmet);
            }
        }
    }

    public void SetupNewGame()
    {
        //Set start date and turn
        Nation.date = new DateTime(1890, 1, 1);
        Nation.turn = 1;

        //Clear Institutes
        Nation.institutes = new List<DesignInstitute>();

        //Create new institute and add to Nation Institutes
        DesignInstitute institute = new DesignInstitute(new Type[] { typeof(Helmet), typeof(Uniform) });
        Nation.institutes.Add(institute);
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