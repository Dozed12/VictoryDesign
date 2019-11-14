using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Nation
{
    //Industrial capacity available to the player
    public static int industrialCapacity;

    //Progress until surrender
    public static float surrenderProgress;

    //Lost territory in kilometers
    public static int lostTerritory;
    
    //Resources stored by name
    public static Dictionary<string, Resource> resources;

    //Extraction sites List
    public static List<ExtractionSite> sites;

}
