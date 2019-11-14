using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        Naming.SetupNaming();
        SetupNewGame();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetupNewGame()
    {
        //Clear resources
        Nation.resources = new Dictionary<string, Resource>();

        //Setup resources
        Nation.resources.Add("Coal", new Resource("Coal",  Resource.Type.SOLID_FUEL));
        Nation.resources.Add("Charcoal", new Resource("Charcoal",  Resource.Type.SOLID_FUEL));
        Nation.resources.Add("Peat", new Resource("Peat",  Resource.Type.SOLID_FUEL));

        Nation.resources.Add("Oil", new Resource("Oil",  Resource.Type.LIQUID_FUEL));
        Nation.resources.Add("Bio-Oil", new Resource("Bio-Oil",  Resource.Type.LIQUID_FUEL));

        Nation.resources.Add("Natural Gas", new Resource("Natural Gas",  Resource.Type.GAS_FUEL));
        Nation.resources.Add("Wood Gas", new Resource("Wood Gas",  Resource.Type.GAS_FUEL));

        Nation.resources.Add("Iron", new Resource("Iron",  Resource.Type.IRON));

        Nation.resources.Add("Hardening Metals", new Resource("Hardening Metals",  Resource.Type.HARDENING_METALS));

        Nation.resources.Add("Machining Metals", new Resource("Machining Metals",  Resource.Type.MACHINING_METALS));

        Nation.resources.Add("Conductive Metals", new Resource("Conductive Metals",  Resource.Type.CONDUCTIVE_METALS));

        Nation.resources.Add("Light Metals", new Resource("Light Metals",  Resource.Type.LIGHT_METALS));

        Nation.resources.Add("Battery Metals", new Resource("Battery Metals",  Resource.Type.BATTERY_METALS));

        Nation.resources.Add("Electricity", new Resource("Electricity",  Resource.Type.ELECTRICITY));

        Nation.resources.Add("Catalysts", new Resource("Catalysts",  Resource.Type.CATALYSTS));
        
        Nation.resources.Add("Sulphur", new Resource("Sulphur",  Resource.Type.NITRATES));
        Nation.resources.Add("Potash", new Resource("Potash",  Resource.Type.NITRATES));

        Nation.resources.Add("Rubber", new Resource("Rubber",  Resource.Type.RUBBER));

        Nation.resources.Add("White Oil", new Resource("White Oil",  Resource.Type.HYDRAULIC_FLUID));
        Nation.resources.Add("Hydraulic Bio Oil", new Resource("Hydraulic Bio Oil",  Resource.Type.HYDRAULIC_FLUID));
    }

}