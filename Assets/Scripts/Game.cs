using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Game : MonoBehaviour
{
    //UI State Variables
    public string designChoiceType;
    public Design currentDisplayDesign;

    //Map
    public Texture2D baseMap;
    public GameObject mapHolder;

    //Tooltip
    public GameObject tooltip;

    //Characteristic Final
    public GameObject characteristic;

    //Impact Sprites
    public List<Sprite> impactSprites;

    //Date and Turn
    public int turn;
    public DateTime date;
    private Text dateText;

    //Designs in use
    public Dictionary<string, Design> designs;

    //Design Proposals
    public Dictionary<string, Design[]> proposals;

    //Design Institutes
    public List<DesignInstitute> institutes;

    // Start is called before the first frame update
    void Start()
    {
        //Setup Naming system
        Naming.SetupNaming();

        //Setup Tooltips
        TooltipManager.tooltip = tooltip;
        TooltipManager.SetupTooltips();

        //Setup a New Game
        SetupNewGame();

        Debug.Log("Occurence of each Impact");
        Utils.DumpArray(ImpactOccurences());
        Debug.Log("Starting Coverage");
        Utils.DumpArray(CurrentCoverage());

        //Test Map Builder
        Map.warStage = 1;
        Map.ProgressWar(6);
        Texture2D final = Map.BuildMap(DrawingUtils.TextureCopy(baseMap));
        mapHolder.GetComponent<Image>().sprite = Sprite.Create(final, new Rect(0, 0, final.width, final.height), new Vector2(0, 0), 100, 0, SpriteMeshType.FullRect);
    }

    // Update is called once per frame
    void Update()
    {
        //Test Generate new Helmets Design
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Rifle[] rifles = RequestDesign(typeof(Rifle)).Cast<Rifle>().ToArray();
            Utils.DumpArray(rifles);
        }

        //Test Design Characteristic Progress
        if (Input.GetKeyDown(KeyCode.W))
        {
            designs["Rifle"].FindCharacteristic("Accuracy").ProgressBounds(2);
            Utils.Dump(designs["Rifle"].FindCharacteristic("Accuracy"));
        }

        //Test Design Progress Random
        if (Input.GetKeyDown(KeyCode.E))
        {
            Utils.Dump(designs["Rifle"]);
            designs["Rifle"].ProgressRandom(4);
            Utils.Dump(designs["Rifle"]);
        }

        //Test Map to Proposition
        if (Input.GetKeyDown(KeyCode.R))
        {
            GameObject.Find("MapHolder").GetComponent<Animator>().SetBool("open", true);
            Invoke("ShowProposition", 0.5f);
        }

        //Process Tooltip
        TooltipManager.ProcessTooltip();
    }

    //Show Proposition
    public void ShowProposition()
    {
        GameObject.Find("Request").GetComponent<Animator>().SetBool("open", true);
    }

    //Setup new Game
    public void SetupNewGame()
    {
        //Clear
        institutes = new List<DesignInstitute>();
        designs = new Dictionary<string, Design>();

        //Set start date and turn
        date = new DateTime(1920, 1, 1);
        turn = 1;

        //Get types of designs
        Type[] typesOfDesigns = (from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
                                 from assemblyType in domainAssembly.GetTypes()
                                 where typeof(Design).IsAssignableFrom(assemblyType)
                                 select assemblyType).ToArray();

        //Exclude Design Type from types of designs
        typesOfDesigns = typesOfDesigns.Skip(1).ToArray();

        //Create Institutes for each type (2 or 3)
        for (int i = 0; i < typesOfDesigns.Length; i++)
        {
            AddInstitutes(new Type[] { typesOfDesigns[i] }, UnityEngine.Random.Range(2, 3 + 1));
        }

        //Create Base Designs with Criteria
        //TODO Criteria compares all Impacts together, may make more sense to compare Industrial and Doctrine separately since they will have separate effect in Final Calculation
        bool valid = true;
        float total = 0;
        float min = 0;
        float max = 0;
        do
        {
            //Assume Valid
            valid = true;

            //Generate Some Designs
            for (int i = 0; i < typesOfDesigns.Length; i++)
            {
                //Get Name of Design
                string name = typesOfDesigns[i].ToString();

                //Add space before Capital letters
                name = string.Concat(name.Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');

                //Request Design
                designs[name] = RequestDesign(typesOfDesigns[i])[0];
            }

            //Current Coverage
            float[] coverage = CurrentCoverage();

            //Evaluate Total Coverage is not too far from 0
            total = 0;
            for (int i = 0; i < coverage.Length; i++)
            {
                total += coverage[i];
            }
            if (Mathf.Abs(total - 0.0f) > 0.3f)
            {
                valid = false;
            }

            //Evaluate Range of Coverages
            min = 2;
            max = -2;
            for (int i = 0; i < coverage.Length; i++)
            {
                if (min > coverage[i])
                    min = coverage[i];
                if (max < coverage[i])
                    max = coverage[i];
            }
            if (Mathf.Abs(min - max) < 0.3f)
                valid = false;

            //Evaluate min is negative and max is positive
            if (min > 0 || max < 0)
                valid = false;

        } while (!valid);

        //Result
        Debug.Log("Result from Design Generation");
        Debug.Log("Sum Coverage %: " + total);
        Debug.Log("Min Coverage %: " + min);
        Debug.Log("Max Coverage %: " + max);

        //Update Sliders
        UpdateSliders();
    }

    //Update Sliders
    public void UpdateSliders()
    {
        float[] coverage = CurrentCoverage();

        //Industry
        GameObject.Find("EngineeringSlider").GetComponent<Slider>().value = coverage[0];
        GameObject.Find("ResourcesSlider").GetComponent<Slider>().value = coverage[1];
        GameObject.Find("ReplenishmentSlider").GetComponent<Slider>().value = coverage[2];

        //Doctrine
        GameObject.Find("AISlider").GetComponent<Slider>().value = coverage[3];
        GameObject.Find("AASlider").GetComponent<Slider>().value = coverage[4];
        GameObject.Find("BreakthroughSlider").GetComponent<Slider>().value = coverage[5];
        GameObject.Find("ExploitaitionSlider").GetComponent<Slider>().value = coverage[6];
        GameObject.Find("MoraleSlider").GetComponent<Slider>().value = coverage[7];
        GameObject.Find("EfficiencySlider").GetComponent<Slider>().value = coverage[8];
    }

    //Request Designs
    public Design[] RequestDesign(Type type)
    {
        List<Design> designs = new List<Design>();

        //Cycle Institutes
        for (int i = 0; i < institutes.Count; i++)
        {
            //Check if Institute can Design
            if (institutes[i].CanDesign(type))
            {
                //Design
                designs.Add(institutes[i].GenerateDesign(type));
            }
        }

        return designs.ToArray();
    }

    //Add Institutes for Type(s)
    public void AddInstitutes(Type[] types, int number)
    {
        for (int i = 0; i < number; i++)
        {
            institutes.Add(new DesignInstitute(types));
        }
    }

    //Current Coverage
    public float[] CurrentCoverage()
    {
        //Value of Characteristics
        float[] values = new float[9];
        foreach (var design in designs)
        {
            foreach (var characteristic in design.Value.characteristics)
            {
                values[(int)characteristic.impact] += characteristic.trueValue;
            }
        }

        //Coverage %
        int[] totals = ImpactOccurences();
        for (int i = 0; i < 9; i++)
        {
            if (totals[i] != 0)
                values[i] /= totals[i] * 10;
            else
                values[i] = 0;
        }

        return values;
    }

    //Count Each Impact Occurence
    public int[] ImpactOccurences()
    {
        int[] values = new int[9];
        foreach (var design in designs)
        {
            foreach (var characteristic in design.Value.characteristics)
            {
                values[(int)characteristic.impact]++;
            }
        }
        return values;
    }

    //Highlight Hovered
    public void HoverDesign(string type)
    {
        //Get design
        Design design = designs[type];

        //Highlight Selected
        Utils.GetChild(Utils.GetChild(GameObject.Find("DesignsHolder"), type), "Selected").GetComponent<Image>().enabled = true;

        //Original Choice
        GameObject originalChoice = GameObject.Find("OriginalChoice");

        //Title
        Utils.GetChild(originalChoice, "Title").GetComponent<Text>().text = "Ministry of War\n\nDesign Proposition - " + type;

        //TODO Date
        //Utils.GetChild(originalChoice, "Date").GetComponent<Text>().text = design.date

        //Name & Designer
        Utils.GetChildRecursive(originalChoice, "Designer").GetComponent<Text>().text = "Designer: " + design.developer.name;
        Utils.GetChildRecursive(originalChoice, "Designation").GetComponent<Text>().text = "Designation: " + design.name;

        //Edit Industrial Values
        Utils.GetChildRecursive(originalChoice, "EngineeringValue").GetComponent<Text>().text = Characteristic.PredictedToString(design.characteristics[0].predictedValue);
        Utils.GetChildRecursive(originalChoice, "ResourceValue").GetComponent<Text>().text = Characteristic.PredictedToString(design.characteristics[1].predictedValue);
        Utils.GetChildRecursive(originalChoice, "ReliabilityValue").GetComponent<Text>().text = Characteristic.PredictedToString(design.characteristics[2].predictedValue);

        //Clear Doctrine Values
        Utils.ClearChildren(Utils.GetChildRecursive(originalChoice, "DoctrineData"));

        //Add Doctrine Values
        for (int i = 3; i < design.characteristics.Count; i++)
        {
            //Setup Characteristic
            GameObject doctrineCharacteristic = Instantiate(characteristic);
            Utils.GetChild(doctrineCharacteristic, "Icon").GetComponent<Image>().overrideSprite = ImpactSprite(design.characteristics[i].impact);
            Utils.GetChild(doctrineCharacteristic, "Title").GetComponent<Text>().text = design.characteristics[i].name;
            Utils.GetChild(doctrineCharacteristic, "Value").GetComponent<Text>().text = Characteristic.PredictedToString(design.characteristics[i].predictedValue);

            //Add to holder
            doctrineCharacteristic.transform.SetParent(Utils.GetChildRecursive(originalChoice, "DoctrineData").transform);
        }

        //Rebuild Layout
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)Utils.GetChildRecursive(originalChoice, "IndustrialData").transform);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)Utils.GetChildRecursive(originalChoice, "DoctrineData").transform);
    }

    //Stop highlight hovered
    public void DeHoverDesign()
    {
        //Remove highlight all Designs
        foreach (Transform designObject in GameObject.Find("DesignsHolder").transform)
        {
            Utils.GetChild(designObject.gameObject, "Selected").GetComponent<Image>().enabled = false;
        }

        //TODO Clear Info

        //TODO if in process of new design default back to highlight that design type (can call like HoverDesign(changing))
    }

    //Sprite for Impact
    public Sprite ImpactSprite(Impact impact)
    {
        return impactSprites[(int)impact];
    }

}