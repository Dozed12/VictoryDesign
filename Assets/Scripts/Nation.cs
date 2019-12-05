using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nation
{
    //Is Player
    public bool isPlayer;

    //Designs in use
    public Dictionary<string, Design> designs;

    //Design Proposals
    public Dictionary<string, Design[]> proposals;

    //Design Institutes
    public List<DesignInstitute> institutes;

    public Nation()
    {
        institutes = new List<DesignInstitute>();
        designs = new Dictionary<string, Design>();
        designs.Add("Rifle", new Rifle());
        designs.Add("SmallArm", new SmallArm());
        designs.Add("Uniform", new Uniform());
        designs.Add("Helmet", new Helmet());
        designs.Add("MachineGun", new MachineGun());

        proposals = new Dictionary<string, Design[]>();
    }

    //Request Designs
    public Design[] RequestDesign(Type type, int minimumValue = -2)
    {
        List<Design> designs = new List<Design>();

        //Cycle Institutes
        for (int i = 0; i < institutes.Count; i++)
        {
            //Check if Institute can Design
            if (institutes[i].CanDesign(type))
            {
                //Design
                designs.Add(institutes[i].GenerateDesign(type, this, minimumValue));
            }
        }

        return designs.ToArray();
    }

    //Add Institutes for Type(s)
    public void AddInstitutes(Type[] types, int number)
    {
        for (int i = 0; i < number; i++)
        {
            institutes.Add(new DesignInstitute(types, this));
        }
    }
}