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

    //Pause Play Time Sprites
    public Sprite pauseSprite;
    public Sprite playSprite;

    //Date and Turn
    public int turn;
    public DateTime date;
    private Text dateText;

    //Time control
    public bool playing = false;
    public float monthClock = 0;
    private float monthAdvance = 0.3f;

    //Designs in use
    public Dictionary<string, Design> designs;

    //Design Proposals
    public Dictionary<string, Design[]> proposals;

    //Design Institutes
    public List<DesignInstitute> institutes;

    //State
    public enum State
    {
        NORMAL,
        REQUEST,
        CHOICE
    }
    public static State state = State.NORMAL;

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

        //Results
        Debug.Log("Occurence of each Impact");
        Utils.DumpArray(ImpactOccurences());
        Debug.Log("Starting Coverage");
        Utils.DumpArray(CurrentCoverage());

        //Update Display of Design Ages
        foreach (KeyValuePair<string, Design> design in designs)
        {
            //Update Design Age Progress
            Utils.GetChild(Utils.GetChild(GameObject.Find("DesignsHolder"), design.Key), "Progress").GetComponent<Image>().fillAmount = ((float)design.Value.age / design.Value.redesignPeriod);
        }

        //Test Map Builder
        Map.warStage = 1;
        Map.ProgressWar(6);
        Texture2D final = Map.BuildMap(DrawingUtils.TextureCopy(baseMap));
        mapHolder.GetComponent<Image>().sprite = Sprite.Create(final, new Rect(0, 0, final.width, final.height), new Vector2(0, 0), 100, 0, SpriteMeshType.FullRect);
    }

    // Update is called once per frame
    void Update()
    {
        //Test Generate new Design
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Rifle[] rifles = RequestDesign(typeof(Rifle), new int[9]{999, 999, 999, 999, 999, 999, 999, 999, 999}).Cast<Rifle>().ToArray();
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
            state = State.REQUEST;
            ToggleMap(true);
            Invoke("ShowRequest", 0.5f);
        }

        //Time
        if(playing)
        {
            //Add Time
            monthClock += monthAdvance * Time.deltaTime;
            GameObject.Find("ProgressAmount").GetComponent<Image>().fillAmount = 1 - monthClock;

            //If End of Month
            if(monthClock > 1)
            {
                //Reset clock
                monthClock = 0;

                //Update Time
                turn++;
                date = date.AddMonths(1);
                GameObject.Find("Time").GetComponentInChildren<Text>().text = date.ToString("MMMM yyyy");

                //Perform checks on redesigns
                foreach (KeyValuePair<string, Design> design in designs)
                {
                    //Age Design
                    design.Value.age++;

                    //Check Age Limit
                    if(design.Value.age > design.Value.redesignPeriod)
                    {
                        //TODO New Design Required
                    }

                    //Update Design Age Progress
                    Utils.GetChild(Utils.GetChild(GameObject.Find("DesignsHolder"), design.Key), "Progress").GetComponent<Image>().fillAmount = ((float)design.Value.age / design.Value.redesignPeriod);
                }
            }
        }          

        //Process Tooltip
        TooltipManager.ProcessTooltip();
    }

    //Time Button
    public void ToggleTime()
    {
        if(playing)
        {
            Utils.GetChild(GameObject.Find("TimeControl"), "Icon").GetComponent<Image>().overrideSprite = playSprite;
            playing = false;
        }
        else
        {
            Utils.GetChild(GameObject.Find("TimeControl"), "Icon").GetComponent<Image>().overrideSprite = pauseSprite;
            playing = true;
        }
    }

    //Map Peeker
    public void PeekMap()
    {
        //Not Active in NORMAL state
        if(state == State.NORMAL)
            return;

        GameObject.Find("MapHolder").GetComponent<Animator>().SetBool("open", false);
    }
    public void UnPeekMap()
    {
        //Not Active in NORMAL state
        if(state == State.NORMAL)
            return;

        GameObject.Find("MapHolder").GetComponent<Animator>().SetBool("open", true);
    }

    //Toggle Map
    public void ToggleMap(bool value)
    {
        GameObject.Find("MapHolder").GetComponent<Animator>().SetBool("open", value);
    }

    //Show Request
    public void ShowRequest()
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
        date = new DateTime(1915, 1, 1);
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
                designs[name] = RequestDesign(typesOfDesigns[i], new int[7]{999, 999, 999, 999, 999, 999, 999})[0];
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
            if (Mathf.Abs(min - max) < 0.4f)
                valid = false;

            //Evaluate min is negative and max is positive
            if (min > 0 || max < 0)
                valid = false;

        } while (!valid);

        //Randomize Design Age
        List<int> ages = Utils.RandomAverage(designs.Count, 4, 0, 8);
        int n = 0;
        foreach (KeyValuePair<string, Design> design in designs)
        {
            design.Value.age = ages[n];
            n++;
        }

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
    public Design[] RequestDesign(Type type, int[] mask)
    {
        List<Design> designs = new List<Design>();

        //Cycle Institutes
        for (int i = 0; i < institutes.Count; i++)
        {
            //Check if Institute can Design
            if (institutes[i].CanDesign(type))
            {
                //Design
                designs.Add(institutes[i].GenerateDesign(type, mask));
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

        //Enginnering and Resources are reversed (higher is worse)
        totals[0] = -totals[0];
        totals[1] = -totals[1];

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

        #region Original Choice UI

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

        //TODO Indicate Deprecated

        //Rebuild Layout
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)Utils.GetChildRecursive(originalChoice, "IndustrialData").transform);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)Utils.GetChildRecursive(originalChoice, "DoctrineData").transform);

        #endregion

        #region Combat Report UI

        //Combat Report
        GameObject combatReport = GameObject.Find("CurrentReport");
        
        //Info
        Utils.GetChild(combatReport, "Info").GetComponent<Text>().text = design.name + " " + type + " - " + design.developer.name;

        //Edit Industrial Values
        if(design.characteristics[0].leftBound != design.characteristics[0].rightBound)
            Utils.GetChildRecursive(combatReport, "EngineeringValue").GetComponent<Text>().text = design.characteristics[0].leftBound + " to " + design.characteristics[0].rightBound;
        else
            Utils.GetChildRecursive(combatReport, "EngineeringValue").GetComponent<Text>().text = design.characteristics[0].leftBound.ToString();

        if(design.characteristics[1].leftBound != design.characteristics[1].rightBound)
            Utils.GetChildRecursive(combatReport, "ResourceValue").GetComponent<Text>().text = design.characteristics[1].leftBound + " to " + design.characteristics[1].rightBound;
        else
            Utils.GetChildRecursive(combatReport, "ResourceValue").GetComponent<Text>().text = design.characteristics[1].leftBound.ToString();

        if(design.characteristics[2].leftBound != design.characteristics[2].rightBound)
            Utils.GetChildRecursive(combatReport, "ReliabilityValue").GetComponent<Text>().text = design.characteristics[2].leftBound + " to " + design.characteristics[2].rightBound;
        else
            Utils.GetChildRecursive(combatReport, "ReliabilityValue").GetComponent<Text>().text = design.characteristics[2].leftBound.ToString();

        //Clear Doctrine Values
        Utils.ClearChildren(Utils.GetChildRecursive(combatReport, "DoctrineData"));

        //Add Doctrine Values
        for (int i = 3; i < design.characteristics.Count; i++)
        {
            //Setup Characteristic
            GameObject doctrineCharacteristic = Instantiate(characteristic);
            Utils.GetChild(doctrineCharacteristic, "Icon").GetComponent<Image>().overrideSprite = ImpactSprite(design.characteristics[i].impact);
            Utils.GetChild(doctrineCharacteristic, "Title").GetComponent<Text>().text = design.characteristics[i].name;
            if(design.characteristics[i].leftBound != design.characteristics[i].rightBound)
                Utils.GetChild(doctrineCharacteristic, "Value").GetComponent<Text>().text = design.characteristics[i].leftBound + " to " + design.characteristics[i].rightBound;
            else
                Utils.GetChild(doctrineCharacteristic, "Value").GetComponent<Text>().text = design.characteristics[i].leftBound.ToString();

            //Add to holder
            doctrineCharacteristic.transform.SetParent(Utils.GetChildRecursive(combatReport, "DoctrineData").transform);
        }

        //Rebuild Layout
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)Utils.GetChildRecursive(combatReport, "IndustrialData").transform);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)Utils.GetChildRecursive(combatReport, "DoctrineData").transform);

        #endregion
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