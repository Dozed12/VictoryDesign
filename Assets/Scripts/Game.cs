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
}
