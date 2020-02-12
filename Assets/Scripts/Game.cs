using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Game : MonoBehaviour
{
    //Map
    public Texture2D baseMap;
    public GameObject mapHolder;

    //Tooltip
    public GameObject tooltip;

    //Focus Points Text
    public GameObject focusPoints;

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
    public static int turn;
    public DateTime date;
    private Text dateText;

    //List of designs needed
    List<Type> designsNeeded;

    //Current Redesign
    public Type redesignType;
    public int[] requestMask;
    public Design[] choices;

    //Time control
    public bool playing = false;
    public bool blockTimeControl = false;
    public float monthClock = 0;
    private float monthAdvance = 1f;

    //Last Hover
    public string lastHover = "";

    //Designs previous
    public Dictionary<string, Design> prevDesigns;

    //Designs in use
    public Dictionary<string, Design> designs;

    //Design Proposals
    public Dictionary<string, Design[]> proposals;

    //Design Institutes
    public List<DesignInstitute> institutes;

    //Cursor
    public Texture2D cursorTexture;
    public CursorMode cursorMode = CursorMode.Auto;
    public Vector2 hotSpot = Vector2.zero;

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
        //Set Cursor
        Cursor.SetCursor(cursorTexture, hotSpot, cursorMode);

        //Setup Naming system
        DesignNaming.SetupNaming();
        DesignerNaming.LoadLocations();

        //Setup Tooltips
        TooltipManager.tooltip = tooltip;
        TooltipManager.SetupTooltips();

        //Setup History
        History.SetupHistory();

        //Setup a New Game
        SetupNewGame();

        //Results
        Debug.Log("Occurence of each Impact");
        Utils.DumpArray(ImpactOccurences());
        Debug.Log("Starting Coverage");
        Utils.DumpArray(CurrentCoverage());

        //Update Display of Design Ages
        UpdateRedesignProgress();

        //Default Hover to First(Rifle)
        HoverDesign("Rifle");

        //Setup Map
        Map.matrixMap = new PixelMatrix(baseMap);
        Map.SetupPixels(DrawingUtils.TextureCopy(baseMap));
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

        //Space Bar Toggle Time
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ToggleTime();
        }

        //Time
        if (playing)
        {
            //Add Time
            monthClock += monthAdvance * Time.deltaTime;
            GameObject.Find("ProgressAmount").GetComponent<Image>().fillAmount = 1 - monthClock;

            //If End of Month
            designsNeeded = new List<Type>();
            if (monthClock > 1)
            {
                //Reset clock
                monthClock = 0;
                GameObject.Find("ProgressAmount").GetComponent<Image>().fillAmount = 1 - monthClock;

                //Update Time
                turn++;
                date = date.AddMonths(1);
                GameObject.Find("Time").GetComponentInChildren<Text>().text = date.ToString("MMMM yyyy");

                //Bulletin
                List<string> bulletin = History.Bulletin();
                GameObject.Find("BulletinText").GetComponent<Text>().text = "";
                for (int i = 0; i < bulletin.Count; i++)
                {
                    //Add Space Line
                    if (GameObject.Find("BulletinText").GetComponent<Text>().text != "")
                        GameObject.Find("BulletinText").GetComponent<Text>().text += "\n";

                    //Add Bulletin Line
                    GameObject.Find("BulletinText").GetComponent<Text>().text += "_" + bulletin[i];
                }

                //Update Map
                Texture2D final = Map.BuildMap(baseMap);
                mapHolder.GetComponent<Image>().sprite = Sprite.Create(final, new Rect(0, 0, final.width, final.height), new Vector2(0, 0), 100, 0, SpriteMeshType.FullRect);

                //Progress Design Intel
                foreach (KeyValuePair<string, Design> design in designs)
                {
                    design.Value.ProgressRandom(5);
                }

                //Full Progress if half age
                foreach (KeyValuePair<string, Design> design in designs)
                {
                    if (design.Value.age == 5)
                        design.Value.ProgressRandom(999);
                }

                //Update Hover
                HoverDesign(lastHover);

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

                //Update Sliders
                UpdateSliders();
            }

            //Initiate Redesign if needed
            if (designsNeeded.Count > 0)
            {
                //Set state to REQUEST
                state = State.REQUEST;

                //Set redesign type
                redesignType = designsNeeded[0];

                //Remove highlight all Designs
                foreach (Transform designObject in GameObject.Find("DesignsHolder").transform)
                {
                    Utils.GetChild(designObject.gameObject, "Selected").GetComponent<Image>().enabled = false;
                }

                //Highlight Design of Redesign Type
                HoverDesign(string.Concat(redesignType.ToString().Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' '));

                //Pause
                ToggleTime();

                //Block Playing
                blockTimeControl = true;

                //Make Time Icon Red
                GameObject.Find("TimeIcon").GetComponent<Image>().color = new Color32(130, 25, 25, 255);

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

    //Setup new Game
    public void SetupNewGame()
    {
        //Clear
        institutes = new List<DesignInstitute>();
        designs = new Dictionary<string, Design>();
        prevDesigns = new Dictionary<string, Design>();

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

        //Create Institutes for each type
        AddInstitutes(typesOfDesigns, UnityEngine.Random.Range(5, 6 + 1));

        //Generate Phoney Designs
        for (int i = 0; i < typesOfDesigns.Length; i++)
        {
            //Get Name of Design
            string name = typesOfDesigns[i].ToString();

            //Add space before Capital letters
            name = string.Concat(name.Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');

            //Request Design
            designs[name] = RequestDesign(typesOfDesigns[i], new int[7] { 0, 0, 0, 0, 0, 0, 0 })[0];
        }

        //Generate Industrial Coverage
        int[] industrialCoverage = new int[3];
        bool valid = false;
        do
        {
            //Assume valid
            valid = true;

            //Generate
            for (int i = 0; i < industrialCoverage.Length; i++)
            {
                industrialCoverage[i] = UnityEngine.Random.Range(-1, 1 + 1);
            }

            //Validate Sum
            if (industrialCoverage.Sum() > -1 || industrialCoverage.Sum() < -3)
                valid = false;

            //Validate 1 Positive or Zero
            bool min1 = false;
            for (int i = 0; i < industrialCoverage.Length; i++)
            {
                if (industrialCoverage[i] >= 0)
                    min1 = true;
            }
            if (!min1)
                valid = false;

        } while (!valid);

        //Generate Capacity Coverage
        int[] capacityCoverage = new int[6];
        valid = false;
        do
        {
            //Assume valid
            valid = true;

            //Generate
            for (int i = 0; i < capacityCoverage.Length; i++)
            {
                capacityCoverage[i] = UnityEngine.Random.Range(-1, 1 + 1);
            }

            //Validate Sum
            if (capacityCoverage.Sum() > -2 || capacityCoverage.Sum() < -4)
                valid = false;
        } while (!valid);

        //Error vars
        int[] error = new int[2] { -1, 1 };

        //Generate Designs to Fit Coverage
        for (int i = 0; i < typesOfDesigns.Length; i++)
        {
            //Get Name of Design
            string name = typesOfDesigns[i].ToString();

            //Add space before Capital letters
            name = string.Concat(name.Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');

            //Setup Mask
            int[] mask = new int[7] { industrialCoverage[0], industrialCoverage[1], industrialCoverage[2], 0, 0, 0, 0 };
            Design phoney = designs[name];
            for (int j = 3; j < phoney.characteristics.Count; j++)
            {
                mask[j] = capacityCoverage[(int)phoney.characteristics[j].impact - 3];
            }

            //Add Error to Mask
            for (int j = 0; j < phoney.characteristics.Count; j++)
            {
                //Chance of error
                if (UnityEngine.Random.Range(0, 100) < 70)
                {
                    //Add error
                    mask[j] += error[UnityEngine.Random.Range(0, 1 + 1)];
                }

                //Clamp mask
                if (mask[j] > 2)
                    mask[j] = 2;
                if (mask[j] < -2)
                    mask[j] = -2;
            }

            //Request Design - from random designer
            designs[name] = RequestDesign(typesOfDesigns[i], mask)[UnityEngine.Random.Range(0, institutes.Count)];
        }

        //Randomize Design Age
        List<int> ages = Utils.RandomMax(designs.Count, 3, 1, 11);
        int n = 0;
        foreach (KeyValuePair<string, Design> design in designs)
        {
            design.Value.age = ages[n];
            n++;
        }

        //Apply progress from age
        foreach (KeyValuePair<string, Design> design in designs)
        {
            design.Value.ProgressRandom(5 * design.Value.age);
        }

        //Full Progress if over half age
        foreach (KeyValuePair<string, Design> design in designs)
        {
            if (design.Value.age > 6)
                design.Value.ProgressRandom(999);
        }

        //Assign Current Designs to Previous
        foreach (KeyValuePair<string, Design> design in designs)
        {
            prevDesigns.Add(design.Key, design.Value);
        }

        //Update Sliders
        UpdateSliders(false);
    }

    //Update Redesign Progress
    public void UpdateRedesignProgress()
    {
        foreach (KeyValuePair<string, Design> design in designs)
        {
            //This Turn warning
            if(design.Value.age - 1 == design.Value.redesignPeriod)
            {
                Utils.GetChild(Utils.GetChild(GameObject.Find("DesignsHolder"), design.Key), "Warn").GetComponent<Image>().enabled = true;
                Utils.GetChild(Utils.GetChild(GameObject.Find("DesignsHolder"), design.Key), "Warn").GetComponent<Image>().color = new Color32(130, 25, 25, 255);
            }
            //Next Turn warning
            else if(design.Value.age == design.Value.redesignPeriod)
            {
                Utils.GetChild(Utils.GetChild(GameObject.Find("DesignsHolder"), design.Key), "Warn").GetComponent<Image>().enabled = true;
                Utils.GetChild(Utils.GetChild(GameObject.Find("DesignsHolder"), design.Key), "Warn").GetComponent<Image>().color = new Color32(219, 213, 50, 255);
            }
            //No warning
            else
                Utils.GetChild(Utils.GetChild(GameObject.Find("DesignsHolder"), design.Key), "Warn").GetComponent<Image>().enabled = false;

            //Set value
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
            Utils.GetChild(GameObject.Find("TimeControl"), "TimeIcon").GetComponent<Image>().overrideSprite = playSprite;
            playing = false;
        }
        else
        {
            Utils.GetChild(GameObject.Find("TimeControl"), "TimeIcon").GetComponent<Image>().overrideSprite = pauseSprite;
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

        //Show Focus Points
        focusPoints.GetComponent<Text>().enabled = true;
        focusPoints.GetComponent<Text>().text = "FOCUS POINTS REMAINING: 2";

        //Request Object
        GameObject request = GameObject.Find("Request");

        //Clear Issue/Signature
        Utils.GetChildRecursive(request, "Border").GetComponent<Image>().enabled = false;
        Utils.GetChildRecursive(request, "Text").GetComponent<Text>().enabled = false;
        Utils.GetChildRecursive(request, "Signature").GetComponent<Text>().enabled = false;

        //Request Info
        string nameSpaced = string.Concat(redesignType.ToString().Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');
        Utils.GetChild(request, "Title").GetComponent<Text>().text = "Ministry of War\n\nDesign Request - " + nameSpaced;
        Utils.GetChild(request, "Date").GetComponent<Text>().text = date.ToString("MMMM yyyy");

        //Request Industrial
        Utils.GetChildRecursive(request, "EngineeringValue").GetComponent<Text>().text = "???";
        Utils.GetChildRecursive(request, "ResourcesValue").GetComponent<Text>().text = "???";
        Utils.GetChildRecursive(request, "ReliabilityValue").GetComponent<Text>().text = "???";

        //Industrial Increase Decrease Callbacks
        Utils.GetChild(Utils.GetChildRecursive(request, "Engineering"), "Increase").GetComponent<Button>().onClick.RemoveAllListeners();
        Utils.GetChild(Utils.GetChildRecursive(request, "Engineering"), "Increase").GetComponent<Button>().onClick.AddListener(delegate { RequestChange(0, Utils.GetChild(Utils.GetChildRecursive(request, "Engineering"), "EngineeringValue").GetComponent<Text>(), 1); });
        Utils.GetChild(Utils.GetChildRecursive(request, "Engineering"), "Decrease").GetComponent<Button>().onClick.RemoveAllListeners();
        Utils.GetChild(Utils.GetChildRecursive(request, "Engineering"), "Decrease").GetComponent<Button>().onClick.AddListener(delegate { RequestChange(0, Utils.GetChild(Utils.GetChildRecursive(request, "Engineering"), "EngineeringValue").GetComponent<Text>(), -1); });

        Utils.GetChild(Utils.GetChildRecursive(request, "Resources"), "Increase").GetComponent<Button>().onClick.RemoveAllListeners();
        Utils.GetChild(Utils.GetChildRecursive(request, "Resources"), "Increase").GetComponent<Button>().onClick.AddListener(delegate { RequestChange(1, Utils.GetChild(Utils.GetChildRecursive(request, "Resources"), "ResourcesValue").GetComponent<Text>(), 1); });
        Utils.GetChild(Utils.GetChildRecursive(request, "Resources"), "Decrease").GetComponent<Button>().onClick.RemoveAllListeners();
        Utils.GetChild(Utils.GetChildRecursive(request, "Resources"), "Decrease").GetComponent<Button>().onClick.AddListener(delegate { RequestChange(1, Utils.GetChild(Utils.GetChildRecursive(request, "Resources"), "ResourcesValue").GetComponent<Text>(), -1); });

        Utils.GetChild(Utils.GetChildRecursive(request, "Reliability"), "Increase").GetComponent<Button>().onClick.RemoveAllListeners();
        Utils.GetChild(Utils.GetChildRecursive(request, "Reliability"), "Increase").GetComponent<Button>().onClick.AddListener(delegate { RequestChange(2, Utils.GetChild(Utils.GetChildRecursive(request, "Reliability"), "ReliabilityValue").GetComponent<Text>(), 1); });
        Utils.GetChild(Utils.GetChildRecursive(request, "Reliability"), "Decrease").GetComponent<Button>().onClick.RemoveAllListeners();
        Utils.GetChild(Utils.GetChildRecursive(request, "Reliability"), "Decrease").GetComponent<Button>().onClick.AddListener(delegate { RequestChange(2, Utils.GetChild(Utils.GetChildRecursive(request, "Reliability"), "ReliabilityValue").GetComponent<Text>(), -1); });

        //Clear Doctrine Characteristics Holder
        Utils.ClearChildren(Utils.GetChild(request, "DoctrineCharacteristicsHolder"));

        //Setup Doctrine Characteristics
        for (int i = 3; i < designs[nameSpaced].characteristics.Count; i++)
        {
            //Instantiate new
            GameObject doctrineCharacteristic = Instantiate(requestCharacteristicPrefab);
            Utils.GetChild(doctrineCharacteristic, "Icon").GetComponent<Image>().overrideSprite = ImpactSprite(designs[nameSpaced].characteristics[i].impact);
            Utils.GetChild(doctrineCharacteristic, "Title").GetComponent<Text>().text = designs[nameSpaced].characteristics[i].name + ":";
            Utils.GetChild(doctrineCharacteristic, "Value").GetComponent<Text>().text = "???";

            //Increase Decrease Callbacks
            int id = i;
            Utils.GetChild(doctrineCharacteristic, "Increase").GetComponent<Button>().onClick.RemoveAllListeners();
            Utils.GetChild(doctrineCharacteristic, "Increase").GetComponent<Button>().onClick.AddListener(delegate { RequestChange(id, Utils.GetChild(doctrineCharacteristic, "Value").GetComponent<Text>(), 1); });
            Utils.GetChild(doctrineCharacteristic, "Decrease").GetComponent<Button>().onClick.RemoveAllListeners();
            Utils.GetChild(doctrineCharacteristic, "Decrease").GetComponent<Button>().onClick.AddListener(delegate { RequestChange(id, Utils.GetChild(doctrineCharacteristic, "Value").GetComponent<Text>(), -1); });

            //Add to Holder
            doctrineCharacteristic.transform.SetParent(Utils.GetChild(request, "DoctrineCharacteristicsHolder").transform);
        }

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
        //Hide Focus Points
        focusPoints.GetComponent<Text>().enabled = false;

        //Issue Display (Signature and Stamp)
        GameObject request = GameObject.Find("Request");
        Utils.GetChildRecursive(request, "Border").GetComponent<Image>().enabled = true;
        Utils.GetChildRecursive(request, "Text").GetComponent<Text>().enabled = true;
        Utils.GetChildRecursive(request, "Signature").GetComponent<Text>().enabled = true;

        //Hide Request
        GameObject.Find("Request").GetComponent<Animator>().SetBool("open", false);

        //Clear Propositions Holder
        Utils.ClearChildren(GameObject.Find("Choices"));

        //Request Design Choices (2 or 3)
        int numChoices = UnityEngine.Random.Range(2, 3 + 1);
        List<Design> finalChoices = RequestDesign(redesignType, requestMask).ToList();
        for (int i = 0; finalChoices.Count > numChoices; i++)
        {
            finalChoices.RemoveAt(UnityEngine.Random.Range(0, finalChoices.Count));
        }

        //Final Choices
        choices = finalChoices.ToArray();

        //Setup Design Choices Display
        for (int i = 0; i < choices.Length; i++)
        {
            //Choice
            GameObject choice = Instantiate(choicePrefab);

            //Title
            string nameSpaced = string.Concat(redesignType.ToString().Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');
            Utils.GetChild(choice, "Title").GetComponent<Text>().text = "Ministry of War\n\nDesign Proposal - " + nameSpaced;

            //Date
            Utils.GetChild(choice, "Date").GetComponent<Text>().text = date.ToString("MMMM yyyy");

            //Name & Designer
            Utils.GetChildRecursive(choice, "Designer").GetComponent<Text>().text = "Designer: " + choices[i].developer.name;
            Utils.GetChildRecursive(choice, "Designation").GetComponent<Text>().text = "Designation: " + choices[i].name;

            //Edit Industrial Values
            Utils.GetChildRecursive(choice, "EngineeringValue").GetComponent<Text>().text = Characteristic.PredictedToString(choices[i].characteristics[0].predictedValue, true);
            Utils.GetChildRecursive(choice, "ResourceValue").GetComponent<Text>().text = Characteristic.PredictedToString(choices[i].characteristics[1].predictedValue, true);
            Utils.GetChildRecursive(choice, "ReliabilityValue").GetComponent<Text>().text = Characteristic.PredictedToString(choices[i].characteristics[2].predictedValue);

            //Clear Doctrine Values
            Utils.ClearChildren(Utils.GetChildRecursive(choice, "DoctrineData"));

            //Add Doctrine Values
            for (int c = 3; c < choices[i].characteristics.Count; c++)
            {
                //Setup Characteristic
                GameObject doctrineCharacteristic = Instantiate(characteristicPrefab);
                Utils.GetChild(doctrineCharacteristic, "Icon").GetComponent<Image>().overrideSprite = ImpactSprite(choices[i].characteristics[c].impact);
                Utils.GetChild(doctrineCharacteristic, "Title").GetComponent<Text>().text = choices[i].characteristics[c].name + ":";
                Utils.GetChild(doctrineCharacteristic, "Value").GetComponent<Text>().text = Characteristic.PredictedToString(choices[i].characteristics[c].predictedValue);

                //Add to holder
                doctrineCharacteristic.transform.SetParent(Utils.GetChildRecursive(choice, "DoctrineData").transform);
            }

            //Callback Choice
            int id = i;
            Utils.GetChild(choice, "Approve").GetComponent<Button>().onClick.RemoveAllListeners();
            Utils.GetChild(choice, "Approve").GetComponent<Button>().onClick.AddListener(delegate { ApplyChoice(id); });

            //Rebuild Layout
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)Utils.GetChildRecursive(choice, "IndustrialData").transform);
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)Utils.GetChildRecursive(choice, "DoctrineData").transform);

            //Add to Holder
            choice.transform.SetParent(GameObject.Find("Choices").transform);
        }

        //Show Choices
        GameObject.Find("Choices").GetComponent<Animator>().SetBool("open", true);
    }

    //Apply Choice
    public void ApplyChoice(int id)
    {
        //Design Spaced
        string designSpaced = string.Concat(redesignType.ToString().Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');

        //Set Previous Design
        prevDesigns[designSpaced] = designs[designSpaced];

        //Set New Design
        designs[designSpaced] = choices[id];

        //Update Redesign Progress
        UpdateRedesignProgress();

        //Hide Choices
        GameObject.Find("Choices").GetComponent<Animator>().SetBool("open", false);

        //Remove from list of redesigns
        designsNeeded.RemoveAt(0);

        //If no more to do exit
        if (designsNeeded.Count == 0)
        {
            //Return to Normal State
            state = State.NORMAL;

            //Unblock Playing
            blockTimeControl = false;

            //Open Map
            CloseMap(false);

            //Make Time Icon Normal Color
            GameObject.Find("TimeIcon").GetComponent<Image>().color = new Color32(50, 50, 50, 255);

            //Highlight Design of Redesign Type
            HoverDesign(string.Concat(redesignType.ToString().Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' '));

            //Nullify redesign
            redesignType = null;
        }
        //If there is do next
        else
        {
            //Set redesign type
            redesignType = designsNeeded[0];

            //Remove highlight all Designs
            foreach (Transform designObject in GameObject.Find("DesignsHolder").transform)
            {
                Utils.GetChild(designObject.gameObject, "Selected").GetComponent<Image>().enabled = false;
            }

            //Highlight Design of Redesign Type
            HoverDesign(string.Concat(redesignType.ToString().Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' '));

            //Update Design Choice Title
            string nameSpaced = string.Concat(redesignType.ToString().Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');
            GameObject.Find("DesignDecisionTitle").GetComponent<Text>().text = nameSpaced.ToUpper() + " DESIGN DECISION";

            //Invoke Show Request for new Design
            Invoke("ShowRequest", 0.5f);
        }
    }

    //Request Change
    public void RequestChange(int id, Text text, int value)
    {
        //Excess
        if (requestMask[id] + value > 2)
            return;
        if (requestMask[id] + value < -2)
            return;

        //Check Request Point Limit
        if (requestMask.Sum() + value > 2)
            return;

        //Add to mask
        requestMask[id] += value;

        //Update Focus Text
        focusPoints.GetComponent<Text>().text = "FOCUS POINTS REMAINING: " + (2 - requestMask.Sum());

        //Update Text
        switch (requestMask[id])
        {
            case -2:
                text.text = "<color=#811919>Not Important</color>";
                break;
            case -1:
                text.text = "<color=#815454>Less Important</color>";
                break;
            case 0:
                text.text = "<color=#7D7D7D>???</color>";
                break;
            case 1:
                text.text = "<color=#506E4D>More Important</color>";
                break;
            case 2:
                text.text = "<color=#246E1E>Very Important</color>";
                break;
        }
    }

    //Update Sliders
    public void UpdateSliders(bool displayProgress = true)
    {
        //Get Current Coverage
        float[] coverage = CurrentCoverage();

        //Sliders
        List<string> objectSliders = new List<string>()
        {
            "EngineeringSlider",
            "ResourcesSlider",
            "ReplenishmentSlider",
            "AISlider",
            "AASlider",
            "BreakthroughSlider",
            "ExploitaitionSlider",
            "MoraleSlider",
            "EfficiencySlider"
        };

        //Progress Arrows
        List<string> objectProgress = new List<string>()
        {
            "EngineeringProgress",
            "ResourcesProgress",
            "ReplenishmentProgress",
            "AIProgress",
            "AAProgress",
            "BreakthroughProgress",
            "ExploitationProgress",
            "MoraleProgress",
            "EfficiencyProgress"
        };

        //Progress Amounts
        List<string> objectProgressAmount = new List<string>()
        {
            "EngineeringProgressAmount",
            "ResourcesProgressAmount",
            "ReplenishmentProgressAmount",
            "AIProgressAmount",
            "AAProgressAmount",
            "BreakthroughProgressAmount",
            "ExploitationProgressAmount",
            "MoraleProgressAmount",
            "EfficiencyProgressAmount"
        };

        //For each coverage
        for (int i = 0; i < coverage.Length; i++)
        {
            //Setup Progress Arrow and Amount
            if(displayProgress)
            {
                //Decrease
                if(GameObject.Find(objectSliders[i]).GetComponent<Slider>().value > coverage[i])
                {
                    GameObject.Find(objectProgress[i]).GetComponent<Image>().enabled = true;
                    GameObject.Find(objectProgress[i]).GetComponent<Image>().color = new Color32(130,25,25,255);
                    RectTransform rectTrans = (RectTransform)GameObject.Find(objectProgress[i]).transform;
                    rectTrans.SetPositionAndRotation(rectTrans.position, Quaternion.Euler(new Vector3(0, 0, 270)));

                    GameObject.Find(objectProgressAmount[i]).GetComponent<Text>().enabled = true;
                    GameObject.Find(objectProgressAmount[i]).GetComponent<Text>().color = new Color32(130,25,25,255);         
                }
                //Increase
                else if (GameObject.Find(objectSliders[i]).GetComponent<Slider>().value < coverage[i])
                {
                    GameObject.Find(objectProgress[i]).GetComponent<Image>().enabled = true;
                    GameObject.Find(objectProgress[i]).GetComponent<Image>().color = new Color32(36,110,30,255);
                    RectTransform rectTrans = (RectTransform)GameObject.Find(objectProgress[i]).transform;
                    rectTrans.SetPositionAndRotation(rectTrans.position, Quaternion.Euler(new Vector3(0, 0, 90)));

                    GameObject.Find(objectProgressAmount[i]).GetComponent<Text>().enabled = true;
                    GameObject.Find(objectProgressAmount[i]).GetComponent<Text>().color = new Color32(36,110,30,255);
                }
                //Same
                else
                {
                    GameObject.Find(objectProgress[i]).GetComponent<Image>().enabled = false;

                    GameObject.Find(objectProgressAmount[i]).GetComponent<Text>().enabled = false;
                    
                }

                //Set Difference
                GameObject.Find(objectProgressAmount[i]).GetComponent<Text>().text = Mathf.CeilToInt(Mathf.Abs(GameObject.Find(objectSliders[i]).GetComponent<Slider>().value - coverage[i]) * 100).ToString();
            }

            //Set Value
            GameObject.Find(objectSliders[i]).GetComponent<Slider>().value = coverage[i];
        }
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
        foreach (KeyValuePair<string, Design> design in designs)
        {
            for (int i = 0; i < design.Value.characteristics.Count; i++)
            {
                //Transition Production until 6 months
                if (design.Value.age < 6)
                {
                    float ratio = (float)design.Value.age / 6;
                    values[(int)design.Value.characteristics[i].impact] += Mathf.RoundToInt(design.Value.characteristics[i].trueValue * ratio + prevDesigns[design.Key].characteristics[i].trueValue * (1 - ratio));
                }
                //Full Production
                else
                {
                    values[(int)design.Value.characteristics[i].impact] += design.Value.characteristics[i].trueValue;
                }
            }
        }

        //Enginnering and Resources are reversed (higher is worse)
        values[0] = -values[0];
        values[1] = -values[1];

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
        //Last hover is current
        lastHover = type;

        //Get design
        Design design = designs[type];

        //Remove highlight all Designs
        foreach (Transform designObject in GameObject.Find("DesignsHolder").transform)
        {
            Utils.GetChild(designObject.gameObject, "Selected").GetComponent<Image>().enabled = false;
        }

        //Highlight Selected
        Utils.GetChild(Utils.GetChild(GameObject.Find("DesignsHolder"), type), "Selected").GetComponent<Image>().enabled = true;

        #region Original Choice UI

        //Original Choice
        GameObject originalChoice = GameObject.Find("OriginalChoice");

        //Title
        Utils.GetChild(originalChoice, "Title").GetComponent<Text>().text = "Ministry of War\n\nDesign Proposal - " + type;

        //Date
        Utils.GetChild(originalChoice, "Date").GetComponent<Text>().text = date.AddMonths(-design.age).ToString("MMMM yyyy");

        //Name & Designer
        Utils.GetChildRecursive(originalChoice, "Designer").GetComponent<Text>().text = "Designer: " + design.developer.name;
        Utils.GetChildRecursive(originalChoice, "Designation").GetComponent<Text>().text = "Designation: " + design.name;

        //Edit Industrial Values
        Utils.GetChildRecursive(originalChoice, "EngineeringValue").GetComponent<Text>().text = Characteristic.PredictedToString(design.characteristics[0].predictedValue, true);
        Utils.GetChildRecursive(originalChoice, "ResourceValue").GetComponent<Text>().text = Characteristic.PredictedToString(design.characteristics[1].predictedValue, true);
        Utils.GetChildRecursive(originalChoice, "ReliabilityValue").GetComponent<Text>().text = Characteristic.PredictedToString(design.characteristics[2].predictedValue);

        //Clear Doctrine Values
        Utils.ClearChildren(Utils.GetChildRecursive(originalChoice, "DoctrineData"));

        //Add Doctrine Values
        for (int i = 3; i < design.characteristics.Count; i++)
        {
            //Setup Characteristic
            GameObject doctrineCharacteristic = Instantiate(characteristicPrefab);
            Utils.GetChild(doctrineCharacteristic, "Icon").GetComponent<Image>().overrideSprite = ImpactSprite(design.characteristics[i].impact);
            Utils.GetChild(doctrineCharacteristic, "Title").GetComponent<Text>().text = design.characteristics[i].name + ":";
            Utils.GetChild(doctrineCharacteristic, "Value").GetComponent<Text>().text = Characteristic.PredictedToString(design.characteristics[i].predictedValue);

            //Add to holder
            doctrineCharacteristic.transform.SetParent(Utils.GetChildRecursive(originalChoice, "DoctrineData").transform);
        }

        //Indicate Deprecated
        if (design.age - 1 == design.redesignPeriod)
        {
            Utils.GetChild(originalChoice, "Deprecated").GetComponent<Image>().enabled = true;
            Utils.GetChild(originalChoice, "Deprecated").GetComponentInChildren<Text>().enabled = true;
        }
        else
        {
            Utils.GetChild(originalChoice, "Deprecated").GetComponent<Image>().enabled = false;
            Utils.GetChild(originalChoice, "Deprecated").GetComponentInChildren<Text>().enabled = false;
        }

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
            Utils.GetChild(doctrineCharacteristic, "Title").GetComponent<Text>().text = design.characteristics[i].name + ":";
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
        //Default to redesign type if in redesign process
        if (redesignType != null)
            HoverDesign(string.Concat(redesignType.ToString().Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' '));
    }

    //Sprite for Impact
    public Sprite ImpactSprite(Impact impact)
    {
        return impactSprites[(int)impact];
    }

}