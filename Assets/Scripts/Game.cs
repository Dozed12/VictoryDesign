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

    }

    public void SetupNewGame()
    {
        //Set start date and turn
        Nation.date = new DateTime(1890, 1, 1);
        Nation.turn = 1;

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