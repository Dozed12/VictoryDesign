using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Game
{
    //Date and Turn
    public static int turn;
    public static DateTime date;

    //Design Institutes
    public static List<DesignInstitute> ourInstitutes;
    public static List<DesignInstitute> theirInstitutes;

    //Our Designs in use
    public static Rifle ourRifle;
    public static SmallArm ourSmallArm;
    public static Uniform ourUniform;
    public static Helmet ourHelmet;
    public static MachineGun ourMachineGun;

    //Their Designs in use
    public static Rifle theirRifle;
    public static SmallArm theirSmallArm;
    public static Uniform theirUniform;
    public static Helmet theirHelmet;
    public static MachineGun theirMachineGun;

    //Request Designs for Us
    public static List<Design> RequestDesignUs(Type type)
    {
        List<Design> designs = new List<Design>();

        //Cycle Institutes
        for (int i = 0; i < ourInstitutes.Count; i++)
        {
            //Check if Institute can Design
            if (ourInstitutes[i].CanDesign(type))
            {
                //Design
                designs.Add(ourInstitutes[i].GenerateDesign(type));
            }
        }

        return designs;
    }

    //Request Designs for Them
    public static List<Design> RequestDesignThem(Type type)
    {
        List<Design> designs = new List<Design>();

        //Cycle Institutes
        for (int i = 0; i < theirInstitutes.Count; i++)
        {
            //Check if Institute can Design
            if (theirInstitutes[i].CanDesign(type))
            {
                //Design
                designs.Add(theirInstitutes[i].GenerateDesign(type));
            }
        }

        return designs;
    }

}
