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
    public GameObject characteristicPrefab;

    //Characteristic Request
    public GameObject requestCharacteristicPrefab;

    //Choice
    public GameObject choicePrefab;

    //Impact Sprites
    public List<Sprite> impactSprites;

    //Pause Play Time Sprites
    public Sprite pauseSprite;
    public Sprite playSprite;

    //Date and Turn
    public int turn;
    public DateTime date;
    private Text dateText;

    //Current Redesign
    public Type redesignType;
    public int[] requestMask;

    //Time control
    public bool playing = false;
    public bool blockTimeControl = false;
    public float monthClock = 0;
    private float monthAdvance = 1f;

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
        UpdateRedesignProgress();

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
            Rifle[] rifles = RequestDesign(typeof(Rifle), new int[9] { 0, 0, 0, 0, 0, 0, 0, 0, 0 }).Cast<Rifle>().ToArray();
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

        //Time
        if (playing)
        {
            //Add Time
            monthClock += monthAdvance * Time.deltaTime;
            GameObject.Find("ProgressAmount").GetComponent<Image>().fillAmount = 1 - monthClock;

            //If End of Month
            List<Type> designsNeeded = new List<Type>();
            if (monthClock > 1)
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
                    if (design.Value.age > design.Value.redesignPeriod)
                    {
                        //New Design Required
                        designsNeeded.Add(design.Value.GetType());
                    }

                    //Update Redesign Progress
                    UpdateRedesignProgress();
                }
            }

            //Initiate Redesign if needed
            if (designsNeeded.Count > 0)
            {
                //Set state to REQUEST
                state = State.REQUEST;

                //Set redesign type
                redesignType = designsNeeded[0];

                //Pause
                ToggleTime();

                //Block Playing
                blockTimeControl = true;

                //Close Map
                CloseMap(true);

                //Update Design Choice Title
                string nameSpaced = string.Concat(redesignType.ToString().Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');
                GameObject.Find("DesignDecisionTitle").GetComponent<Text>().text = nameSpaced.ToUpper() + " DESIGN DECISION";

                //Invoke Show Request for new Design
                Invoke("ShowRequest", 0.5f);
            }
        }

        //Process Tooltip
        TooltipManager.ProcessTooltip();
    }

    //Update Redesign Progress
    public void UpdateRedesignProgress()
    {
        foreach (KeyValuePair<string, Design> design in designs)
        {
            Utils.GetChild(Utils.GetChild(GameObject.Find("DesignsHolder"), design.Key), "Progress").GetComponent<Image>().fillAmount = ((float)design.Value.age / design.Value.redesignPeriod);
        }
    }

    //Time Button
    public void ToggleTime()
    {
        if (blockTimeControl)
            return;

        if (playing)
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
        if (state == State.NORMAL)
            return;

        GameObject.Find("MapHolder").GetComponent<Animator>().SetBool("open", false);
    }
    public void UnPeekMap()
    {
        //Not Active in NORMAL state
        if (state == State.NORMAL)
            return;

        GameObject.Find("MapHolder").GetComponent<Animator>().SetBool("open", true);
    }

    //Toggle Map
    public void CloseMap(bool value)
    {
        GameObject.Find("MapHolder").GetComponent<Animator>().SetBool("open", value);
    }

    //Show Request
    public void ShowRequest()
    {
        //Reset Mask
        requestMask = new int[7] { 0, 0, 0, 0, 0, 0, 0 };

        //Request Object
        GameObject request = GameObject.Find("Request");

        //Request Info
        string nameSpaced = string.Concat(redesignType.ToString().Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');
        Utils.GetChild(request, "Title").GetComponent<Text>().text = "Ministry of War\n\nDesign Request - " + nameSpaced;
        Utils.GetChild(request, "Date").GetComponent<Text>().text = date.ToString("MMMM yyyy");

        //Request Industrial
        Utils.GetChildRecursive(request, "EngineeringValue").GetComponent<Text>().text = "???";
        Utils.GetChildRecursive(request, "ResourcesValue").GetComponent<Text>().text = "???";
        Utils.GetChildRecursive(request, "ReliabilityValue").GetComponent<Text>().text = "???";

        //Industrial Increase Decrease Callbacks
        Utils.GetChild(Utils.GetChildRecursive(request, "Engineering"), "Increase").GetComponent<Button>().onClick.AddListener(delegate { RequestChange(0, Utils.GetChild(Utils.GetChildRecursive(request, "Engineering"), "EngineeringValue").GetComponent<Text>(), 1); });
        Utils.GetChild(Utils.GetChildRecursive(request, "Engineering"), "Decrease").GetComponent<Button>().onClick.AddListener(delegate { RequestChange(0, Utils.GetChild(Utils.GetChildRecursive(request, "Engineering"), "EngineeringValue").GetComponent<Text>(), -1); });

        Utils.GetChild(Utils.GetChildRecursive(request, "Resources"), "Increase").GetComponent<Button>().onClick.AddListener(delegate { RequestChange(1, Utils.GetChild(Utils.GetChildRecursive(request, "Resources"), "ResourcesValue").GetComponent<Text>(), 1); });
        Utils.GetChild(Utils.GetChildRecursive(request, "Resources"), "Decrease").GetComponent<Button>().onClick.AddListener(delegate { RequestChange(1, Utils.GetChild(Utils.GetChildRecursive(request, "Resources"), "ResourcesValue").GetComponent<Text>(), -1); });

        Utils.GetChild(Utils.GetChildRecursive(request, "Reliability"), "Increase").GetComponent<Button>().onClick.AddListener(delegate { RequestChange(2, Utils.GetChild(Utils.GetChildRecursive(request, "Reliability"), "ReliabilityValue").GetComponent<Text>(), 1); });
        Utils.GetChild(Utils.GetChildRecursive(request, "Reliability"), "Decrease").GetComponent<Button>().onClick.AddListener(delegate { RequestChange(2, Utils.GetChild(Utils.GetChildRecursive(request, "Reliability"), "ReliabilityValue").GetComponent<Text>(), -1); });

        //Clear Doctrine Characteristics Holder
        Utils.ClearChildren(Utils.GetChild(request, "DoctrineCharacteristicsHolder"));

        //Setup Doctrine Characteristics
        for (int i = 3; i < designs[nameSpaced].characteristics.Count; i++)
        {
            //Instantiate new
            GameObject doctrineCharacteristic = Instantiate(requestCharacteristicPrefab);
            Utils.GetChild(doctrineCharacteristic, "Icon").GetComponent<Image>().overrideSprite = ImpactSprite(designs[nameSpaced].characteristics[i].impact);
            Utils.GetChild(doctrineCharacteristic, "Title").GetComponent<Text>().text = designs[nameSpaced].characteristics[i].name;
            Utils.GetChild(doctrineCharacteristic, "Value").GetComponent<Text>().text = "???";

            //Increase Decrease Callbacks
            int id = i;
            Utils.GetChild(doctrineCharacteristic, "Increase").GetComponent<Button>().onClick.AddListener(delegate { RequestChange(id, Utils.GetChild(doctrineCharacteristic, "Value").GetComponent<Text>(), 1); });
            Utils.GetChild(doctrineCharacteristic, "Decrease").GetComponent<Button>().onClick.AddListener(delegate { RequestChange(id, Utils.GetChild(doctrineCharacteristic, "Value").GetComponent<Text>(), -1); });

            //Add to Holder
            doctrineCharacteristic.transform.SetParent(Utils.GetChild(request, "DoctrineCharacteristicsHolder").transform);
        }

        //Issue Request Callback
        Utils.GetChild(request, "Issue").GetComponent<Button>().onClick.AddListener(delegate { IssueRequest(); });

        //Fix Layout
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)Utils.GetChildRecursive(request, "IndustrialCharacteristicsHolder").transform);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)Utils.GetChildRecursive(request, "DoctrineCharacteristicsHolder").transform);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)Utils.GetChildRecursive(request, "Data").transform);

        //Fire Animation
        GameObject.Find("Request").GetComponent<Animator>().SetBool("open", true);
    }

    //Issue Request
    public void IssueRequest()
    {
        //TODO Stamp

        //Hide Request
        GameObject.Find("Request").GetComponent<Animator>().SetBool("open", false);

        //Clear Propositions Holder
        Utils.ClearChildren(GameObject.Find("Choices"));

        //Request Design Choices
        Design[] choices = RequestDesign(redesignType, requestMask);

        //Setup Design Choices Display
        for (int i = 0; i < choices.Length; i++)
        {
            //Choice
            GameObject choice = Instantiate(choicePrefab);

            //Title
            Utils.GetChild(choice, "Title").GetComponent<Text>().text = "Ministry of War\n\nDesign Proposition - " + redesignType;

            //Date
            Utils.GetChild(choice, "Date").GetComponent<Text>().text = date.ToString("MMMM yyyy");

            //Name & Designer
            Utils.GetChildRecursive(choice, "Designer").GetComponent<Text>().text = "Designer: " + choices[i].developer.name;
            Utils.GetChildRecursive(choice, "Designation").GetComponent<Text>().text = "Designation: " + choices[i].name;

            //Edit Industrial Values
            Utils.GetChildRecursive(choice, "EngineeringValue").GetComponent<Text>().text = Characteristic.PredictedToString(choices[i].characteristics[0].predictedValue);
            Utils.GetChildRecursive(choice, "ResourceValue").GetComponent<Text>().text = Characteristic.PredictedToString(choices[i].characteristics[1].predictedValue);
            Utils.GetChildRecursive(choice, "ReliabilityValue").GetComponent<Text>().text = Characteristic.PredictedToString(choices[i].characteristics[2].predictedValue);

            //Clear Doctrine Values
            Utils.ClearChildren(Utils.GetChildRecursive(choice, "DoctrineData"));

            //Add Doctrine Values
            for (int c = 3; c < choices[i].characteristics.Count; c++)
            {
                //Setup Characteristic
                GameObject doctrineCharacteristic = Instantiate(characteristicPrefab);
                Utils.GetChild(doctrineCharacteristic, "Icon").GetComponent<Image>().overrideSprite = ImpactSprite(choices[i].characteristics[c].impact);
                Utils.GetChild(doctrineCharacteristic, "Title").GetComponent<Text>().text = choices[i].characteristics[c].name;
                Utils.GetChild(doctrineCharacteristic, "Value").GetComponent<Text>().text = Characteristic.PredictedToString(choices[i].characteristics[c].predictedValue);

                //Add to holder
                doctrineCharacteristic.transform.SetParent(Utils.GetChildRecursive(choice, "DoctrineData").transform);
            }

            //TODO Callback Choice

            //Rebuild Layout
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)Utils.GetChildRecursive(choice, "IndustrialData").transform);
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)Utils.GetChildRecursive(choice, "DoctrineData").transform);

            //Add to Holder
            choice.transform.SetParent(GameObject.Find("Choices").transform);
        }

        //Show Choices
        GameObject.Find("Choices").GetComponent<Animator>().SetBool("open", true);
    }

    //Request Change
    public void RequestChange(int id, Text text, int value)
    {
        //Excess
        if (requestMask[id] + value > 2)
            return;
        if (requestMask[id] + value < -2)
            return;

        //TODO Check Request Point Limit

        //Add to mask
        requestMask[id] += value;

        //Update Text
        switch (requestMask[id])
        {
            case -2:
                text.text = "Very Low";
                break;
            case -1:
                text.text = "Low";
                break;
            case 0:
                text.text = "???";
                break;
            case 1:
                text.text = "High";
                break;
            case 2:
                text.text = "Very High";
                break;
        }
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
                designs[name] = RequestDesign(typesOfDesigns[i], new int[7] { 0, 0, 0, 0, 0, 0, 0 })[0];
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
        List<int> ages = Utils.RandomAverage(designs.Count, 5, 1, 10);
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

        //Date
        Utils.GetChild(originalChoice, "Date").GetComponent<Text>().text = date.AddMonths(-design.age).ToString("MMMM yyyy");

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
            GameObject doctrineCharacteristic = Instantiate(characteristicPrefab);
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
        if (design.characteristics[0].leftBound != design.characteristics[0].rightBound)
            Utils.GetChildRecursive(combatReport, "EngineeringValue").GetComponent<Text>().text = design.characteristics[0].leftBound + " to " + design.characteristics[0].rightBound;
        else
            Utils.GetChildRecursive(combatReport, "EngineeringValue").GetComponent<Text>().text = design.characteristics[0].leftBound.ToString();

        if (design.characteristics[1].leftBound != design.characteristics[1].rightBound)
            Utils.GetChildRecursive(combatReport, "ResourceValue").GetComponent<Text>().text = design.characteristics[1].leftBound + " to " + design.characteristics[1].rightBound;
        else
            Utils.GetChildRecursive(combatReport, "ResourceValue").GetComponent<Text>().text = design.characteristics[1].leftBound.ToString();

        if (design.characteristics[2].leftBound != design.characteristics[2].rightBound)
            Utils.GetChildRecursive(combatReport, "ReliabilityValue").GetComponent<Text>().text = design.characteristics[2].leftBound + " to " + design.characteristics[2].rightBound;
        else
            Utils.GetChildRecursive(combatReport, "ReliabilityValue").GetComponent<Text>().text = design.characteristics[2].leftBound.ToString();

        //Clear Doctrine Values
        Utils.ClearChildren(Utils.GetChildRecursive(combatReport, "DoctrineData"));

        //Add Doctrine Values
        for (int i = 3; i < design.characteristics.Count; i++)
        {
            //Setup Characteristic
            GameObject doctrineCharacteristic = Instantiate(characteristicPrefab);
            Utils.GetChild(doctrineCharacteristic, "Icon").GetComponent<Image>().overrideSprite = ImpactSprite(design.characteristics[i].impact);
            Utils.GetChild(doctrineCharacteristic, "Title").GetComponent<Text>().text = design.characteristics[i].name;
            if (design.characteristics[i].leftBound != design.characteristics[i].rightBound)
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