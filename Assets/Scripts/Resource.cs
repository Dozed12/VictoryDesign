using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resource
{

    public enum Type
    {
        FOOD,
        SOLID_FUEL,
        LIQUID_FUEL,
        GAS_FUEL,
        IRON,
        HARDENING_METALS,
        MACHINING_METALS,
        CONDUCTIVE_METALS,
        LIGHT_METALS,
        BATTERY_METALS,
        ELECTRICITY,
        CATALYSTS,
        NITRATES,
        RUBBER,
        HYDRAULIC_FLUID
    }

    public string name;
    public int storage;
    public int production;
    public int consumption;
    public Type type;

    public Resource(string name, Type type)
    {
        this.name = name;
        this.type = type;
    }
}

//Extraction site for a Resource
public class ExtractionSite
{
    
    //Site characteristics
    public string name;
    public Resource.Type type;
    public int exploited;
    public int nonExploited;

    //Coverage
    public float tractorCoverage;
    public float truckCoverage;

    //Site location
    public int distanceToFront;
    public int distanceToRailNetwork;
    public bool railNetworkConnected;
    public int distanceToRoadNetwork;
    public bool roadNetworkConnected;

}
