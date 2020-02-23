using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

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

    //Doctrine Sprites
    public List<Sprite> doctrineSpritesTemp;
    public static List<Sprite> doctrineSprites;

    //Pause Play Time Sprites
    public Sprite pauseSprite;
    public Sprite playSprite;

    //Audio Managers
    public SFXManager SFXManager;

    //Cursor
    public Texture2D cursorTexture;
    public CursorMode cursorMode = CursorMode.Auto;
    public Vector2 hotSpot = Vector2.zero;

    //Main Menu Object
    public GameObject mainMenu;

    //Doctrines
    public enum Doctrine
    {
        ANTI_INFANTRY,
        ANTI_ARMOR,
        BREAKTHROUGH,
        EXPLOITATION,
        MORALE,
        COMBAT_EFFICIENCY
    }

    //State
    public enum State
    {
        NORMAL,
        REQUEST,
        CHOICE
    }

    //Game Data in use
    public static GameData data;

    //Game Data
    [Serializable]
    public class GameData
    {
        //Map
        public Map map;

        //Date and Turn
        public int turn;
        public DateTime date;

        //War
        public bool atWar = false;
        public int lastProgress = 0;
        public float warRequired = 0.7f;
        public float finalWarRequired = 0.1f;
        public float warRequiredDecrease = 0.075f;

        //History
        public List<Event> events;

        //List of designs needed
        public List<Type> designsNeeded;

        //Current Redesign
        public Type redesignType;
        public int[] requestMask;
        public Design[] choices;

        //Time control
        public bool playing = false;
        public bool blockMenu = false;
        public float monthClock = 0;
        public float monthAdvance = 1f;

        //Previous Turn Coverage
        public float[] prevCoverage;

        //Doctrine
        public Dictionary<Doctrine, float> doctrine;
        public Dictionary<Doctrine, float> changedDoctrine;

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

        //Game State
        public State state = State.NORMAL;

        //Initial Doctrine (i unused)
        public void InitialDoctrine(int i)
        {
            //Generate Doctrine
            List<int> genDoctrine;
            bool valid = true;
            int tries = 0;
            do
            {
                //Generate random with Sum 0
                genDoctrine = Utils.RandomSum(6, 0, -2, 2);

                //Proper Doctrine Values
                for (int j = 0; j < genDoctrine.Count; j++)
                {
                    data.doctrine[(Doctrine)j] = 1 + 0.25f * genDoctrine[j];
                }

                //Assume valid
                valid = true;

                //Initial Doctrine must make capacity worse
                if (Game.CapacityValueDoctrine() > Game.CapacityValue())
                    valid = false;

                //At least 1 Very High and 1 High
                bool foundVH = false;
                bool foundH = false;
                for (int j = 0; j < genDoctrine.Count; j++)
                {
                    if (genDoctrine[j] == 2)
                        foundVH = true;
                    if (genDoctrine[j] == 1)
                        foundH = true;
                }
                if (!foundVH || !foundH)
                    valid = false;

                //Check Give Up
                tries++;
                if (tries == 1000)
                {
                    Debug.Log("Initial Doctrine - GAVE UP");
                    break;
                }

            } while (!valid);

            //Update Doctrine Graphic
            Game.UpdateDoctrineGraphic();
        }

        //Start War
        public void StartWar(int i)
        {
            atWar = true;
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        //Set Sprites
        doctrineSprites = doctrineSpritesTemp;

        //Get SFX Manager
        SFXManager = GameObject.Find("SFXManager").GetComponent<SFXManager>();
        SFXManager.PlayDoubleExplosion();

        //Set Cursor
        Cursor.SetCursor(cursorTexture, hotSpot, cursorMode);

        //Setup Naming system
        DesignNaming.SetupNaming();
        DesignerNaming.LoadLocations();

        //Setup Tooltips
        TooltipManager.tooltip = tooltip;
        TooltipManager.SetupTooltips();

        //Setup Model Generator
        ModelGenerator.LoadAssets();

        //Get Main Menu
        mainMenu = GameObject.Find("MainMenu");
    }

    // Update is called once per frame
    void Update()
    {
        //Check no main menu
        if (mainMenu.activeInHierarchy)
            return;

        //Process Tooltip
        TooltipManager.ProcessTooltip();

        //Space Bar Toggle Time
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ToggleTime();
        }

        //Time
        if (data.playing)
        {
            //Add Time
            data.monthClock += data.monthAdvance * Time.deltaTime;
            GameObject.Find("ProgressAmount").GetComponent<Image>().fillAmount = 1 - data.monthClock;

            //If End of Month - Turn
            data.designsNeeded = new List<Type>();
            if (data.monthClock > 1)
            {
                //Current Coverage will be Previous
                data.prevCoverage = CurrentCoverage();

                //Reset clock
                data.monthClock = 0;
                GameObject.Find("ProgressAmount").GetComponent<Image>().fillAmount = 1 - data.monthClock;

                //Update Time
                data.turn++;
                data.date = data.date.AddMonths(1);
                GameObject.Find("Time").GetComponentInChildren<Text>().text = data.date.ToString("MMMM yyyy");

                //War Progress
                if (data.atWar)
                {
                    //Get Final Value
                    float finalValue = FinalCalculation();

                    //Fix old save not having values TODO Remove later
                    if (data.warRequired == 0)
                        data.warRequired = 0.7f;
                    if (data.finalWarRequired == 0)
                        data.finalWarRequired = 0.1f;
                    if (data.warRequiredDecrease == 0)
                        data.warRequiredDecrease = 0.075f;

                    //Modifier
                    int modifier = 20;

                    //War Progress in Regions
                    data.lastProgress = Mathf.RoundToInt((data.warRequired - finalValue) * modifier);

                    //Decrement War Required
                    if (data.warRequired > data.finalWarRequired)
                        data.warRequired -= data.warRequiredDecrease;
                    if (data.warRequired < data.finalWarRequired)
                        data.warRequired = data.finalWarRequired;

                    //Apply Progress
                    if (data.lastProgress > 0)
                        data.map.LoseWar(data.lastProgress);
                    else if (data.lastProgress < 0)
                        data.map.WinWar(-data.lastProgress);
                }

                //Bulletin
                UpdateBulletin();

                //Update Map
                Texture2D final = data.map.BuildMap(baseMap);
                mapHolder.GetComponent<Image>().sprite = Sprite.Create(final, new Rect(0, 0, final.width, final.height), new Vector2(0, 0), 100, 0, SpriteMeshType.FullRect);

                //Progress Design Intel
                foreach (KeyValuePair<string, Design> design in data.designs)
                {
                    design.Value.ProgressRandom(5);
                }

                //Full Progress if half age
                foreach (KeyValuePair<string, Design> design in data.designs)
                {
                    if (design.Value.age == 5)
                        design.Value.ProgressRandom(999);
                }

                //Update Hover
                HoverDesign(data.lastHover);

                //Perform checks on redesigns
                foreach (KeyValuePair<string, Design> design in data.designs)
                {
                    //Age Design
                    design.Value.age++;

                    //Check Age Limit
                    if (design.Value.age > design.Value.redesignPeriod)
                    {
                        //New Design Required
                        data.designsNeeded.Add(design.Value.GetType());
                    }

                    //Update Redesign Progress
                    UpdateRedesignProgress();
                }

                //Update Sliders
                UpdateSliders();

                //Doctrine Proposal if month multiple of 6 (except 1920)
                if (((data.date.Month == 1 || data.date.Month == 6) && data.date.Year != 1920) || (data.date.Month == 6 && data.date.Year == 1920))
                {
                    //Set state to REQUEST
                    data.state = State.REQUEST;

                    //Pause
                    ToggleTime();

                    //Block Playing
                    data.blockMenu = true;

                    //Make Menu Icon Red
                    GameObject.Find("TimeIcon").GetComponent<Image>().color = new Color32(130, 25, 25, 255);
                    GameObject.Find("ExitIcon").GetComponent<Image>().color = new Color32(130, 25, 25, 255);

                    //Close Map
                    CloseMap(true);

                    //Update Design Choice Title
                    GameObject.Find("DesignDecisionTitle").GetComponent<Text>().text = "DOCTRINE CHANGE";

                    //Invoke Show Request for new Design
                    Invoke("ShowDoctrineChange", 0.5f);
                }
                //Initiate Redesign if needed
                else if (data.designsNeeded.Count > 0)
                {
                    //Set state to REQUEST
                    data.state = State.REQUEST;

                    //Set redesign type
                    data.redesignType = data.designsNeeded[0];

                    //Remove highlight all Designs
                    foreach (Transform designObject in GameObject.Find("DesignsHolder").transform)
                    {
                        Utils.GetChild(designObject.gameObject, "Selected").GetComponent<Image>().enabled = false;
                    }

                    //Highlight Design of Redesign Type
                    HoverDesign(string.Concat(data.redesignType.ToString().Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' '));

                    //Pause
                    ToggleTime();

                    //Block Playing
                    data.blockMenu = true;

                    //Make Menu Icon Red
                    GameObject.Find("TimeIcon").GetComponent<Image>().color = new Color32(130, 25, 25, 255);
                    GameObject.Find("ExitIcon").GetComponent<Image>().color = new Color32(130, 25, 25, 255);

                    //Close Map
                    CloseMap(true);

                    //Update Design Choice Title
                    string nameSpaced = string.Concat(data.redesignType.ToString().Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');
                    GameObject.Find("DesignDecisionTitle").GetComponent<Text>().text = nameSpaced.ToUpper() + " DESIGN DECISION";

                    //Invoke Show Request for new Design
                    Invoke("ShowRequest", 0.5f);
                }
            }
        }
    }

    //New Game
    public void NewGame()
    {
        SetupNewGame();

        mainMenu.SetActive(false);
    }

    //Continue Save
    public void Continue()
    {
        LoadGame();

        mainMenu.SetActive(false);
    }

    //Exit Game
    public void Exit()
    {
        Application.Quit();
    }

    //Exit to Menu
    public void MainMenu()
    {
        //Block if pending actions
        if (data.blockMenu)
            return;

        //Save Game
        SaveGame();

        mainMenu.SetActive(true);
    }

    //Setup new Game
    public void SetupNewGame()
    {
        //New Data
        data = new GameData();

        //Clear
        data.institutes = new List<DesignInstitute>();
        data.designs = new Dictionary<string, Design>();
        data.prevDesigns = new Dictionary<string, Design>();

        //Set start date and turn
        data.date = new DateTime(1920, 1, 1);
        data.turn = 1;

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
            data.designs[name] = RequestDesign(typesOfDesigns[i], new int[7] { 0, 0, 0, 0, 0, 0, 0 }, 1)[0];
        }

        //Generate Industrial Coverage
        int[] industrialCoverage = new int[3];
        bool valid = false;
        int tries = 0;
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

            //Check Give Up
            tries++;
            if (tries == 1000)
            {
                Debug.Log("Generate Industrial Coverage - GAVE UP");
                break;
            }

        } while (!valid);

        //Generate Capacity Coverage
        int[] capacityCoverage = new int[6];
        valid = false;
        tries = 0;
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

            //Check Give Up
            tries++;
            if (tries == 1000)
            {
                Debug.Log("Generate Capacity Coverage - GAVE UP");
                break;
            }

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
            Design phoney = data.designs[name];
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
            data.designs[name] = RequestDesign(typesOfDesigns[i], mask, 1)[0];
        }

        //Randomize Design Age
        List<int> ages = Utils.RandomMax(data.designs.Count, 3, 1, 12);
        int n = 0;
        foreach (KeyValuePair<string, Design> design in data.designs)
        {
            design.Value.age = ages[n];
            n++;
        }

        //Apply progress from age
        foreach (KeyValuePair<string, Design> design in data.designs)
        {
            design.Value.ProgressRandom(5 * design.Value.age);
        }

        //Full Progress if over half age
        foreach (KeyValuePair<string, Design> design in data.designs)
        {
            if (design.Value.age > 6)
                design.Value.ProgressRandom(999);
        }

        //Assign Current Designs to Previous
        foreach (KeyValuePair<string, Design> design in data.designs)
        {
            data.prevDesigns.Add(design.Key, design.Value);
        }

        //Default Previous Coverage to Starting
        data.prevCoverage = CurrentCoverage();

        //Update Sliders
        UpdateSliders(false);

        //Setup Doctrine
        data.doctrine = new Dictionary<Doctrine, float>();
        for (int i = 0; i < 6; i++)
        {
            data.doctrine[(Doctrine)(i)] = 1;
        }

        //Setup Map
        data.map = new Map();
        data.map.warStage = 0;
        data.map.SetupPixels(DrawingUtils.TextureCopy(baseMap));

        //Setup History
        SetupHistory();

        //Update UI
        UpdateDoctrineGraphic();
        UpdateRedesignProgress();
        UpdateSliders();
        HoverDesign("Rifle");
        UpdateBulletin();

        //Build Map
        Texture2D final = data.map.BuildMap(baseMap);
        mapHolder.GetComponent<Image>().sprite = Sprite.Create(final, new Rect(0, 0, final.width, final.height), new Vector2(0, 0), 100, 0, SpriteMeshType.FullRect);

        //Update Date but restart Clock timer
        data.monthClock = 0;
        GameObject.Find("Time").GetComponentInChildren<Text>().text = data.date.ToString("MMMM yyyy");
    }

    //Setup History Turns
    public void SetupHistory()
    {
        //Clear list
        data.events = new List<Event>();

        //Initial Doctrine
        data.events.Add(new Event(3, "The Military Staff has produced the initial Doctrine for our armed forces. You will be allowed to perform a single alteration every 6 months, starting next June.", data.InitialDoctrine, -1));

        //Generate Unification Turn
        data.events.Add(new Event(6 + UnityEngine.Random.Range(0, 1 + 1), "The Schanz Principiate has joined the Steimle Empire following a referendum.", data.map.ProgressUnification, 0));
        data.events.Add(new Event(8 + UnityEngine.Random.Range(0, 1 + 1), "Haguenau has been peacefully annexed by the Steimle Empire as a result of the IX Continental Comission Meeting.", data.map.ProgressUnification, 1));

        //Generate Allies Turn
        data.events.Add(new Event(10 + UnityEngine.Random.Range(0, 1 + 1), "Ciatari has joined the Steimle Empire alliance.", data.map.ProgressAllies, 0));
        data.events.Add(new Event(12 + UnityEngine.Random.Range(0, 1 + 1), "The Brana Republic has joined the Steimle Empire alliance.", data.map.ProgressAllies, 1));
        data.events.Add(new Event(14 + UnityEngine.Random.Range(0, 1 + 1), "Dathalom  has joined the Steimle Empire alliance.", data.map.ProgressAllies, 2));

        //Generate Revenge Turns
        data.events.Add(new Event(16 + UnityEngine.Random.Range(0, 1 + 1), "The Steimle Empire has declared war on Polandia.", null, -1));
        data.events.Add(new Event(19 + UnityEngine.Random.Range(-1, 1 + 1), "The Steimle Empire has occupied Polandia after a lightning campaign.", data.map.ProgressRevenge, 0));

        //Generate War Turn
        data.events.Add(new Event(22 + UnityEngine.Random.Range(-1, 1 + 1), "The Steimle Empire has unexpectedly declared war on our great Nation, your strategy and planning will now be put to the ultimate test.", data.StartWar, -1));
    }

    //Generate possible messages for Bulletin
    public List<string> Bulletin()
    {
        List<string> result = new List<string>();

        //If 4 month multiple then include armed forces capacity/doctrine report
        if (data.turn % 3 == 0)
        {
            //Preffix
            string report = "Armed Forces Report:";

            //Calculations
            float industry = IndustryValue();
            float effectiveIndustry = EffectiveIndustryValue();
            float capacity = CapacityValue();
            float capacityDoctrine = CapacityValueDoctrine();
            float final = FinalCalculation();

            //Add to report
            report += "\n";
            report += "Industry: " + Mathf.Round((1 + industry) * 100) + "%";
            report += "\n";
            report += "Effective Industry: " + Mathf.Round((1 + effectiveIndustry) * 100) + "%";
            report += "\n";
            report += "Capacity: " + Mathf.Round((1 + capacity) * 100) + "%";
            report += "\n";
            report += "Effective Capacity: " + Mathf.Round((1 + capacityDoctrine) * 100) + "%";
            report += "\n";
            report += "Total: " + Mathf.Round((1 + final) * 100) + "%";

            //Add to bulletin
            result.Add(report);
        }

        //Check if history event
        for (int i = 0; i < data.events.Count; i++)
        {
            if (data.turn == data.events[i].turn)
            {
                //Add to bulletin
                result.Add(data.events[i].message);

                //Apply Effect
                if (data.events[i].action != null)
                    data.events[i].action(data.events[i].identification);
            }
        }

        //War Progress Report
        if (data.lastProgress != 0)
        {
            if (data.lastProgress > 0)
            {
                result.Add("We lost " + data.lastProgress + " regions during the last month");
            }
            if (data.lastProgress < 0)
            {
                result.Add("We recovered " + (-data.lastProgress) + " regions during the last month");
            }
        }

        return result;
    }

    //Save Game
    public void SaveGame()
    {
        //Create Save Directory
        System.IO.Directory.CreateDirectory(Application.streamingAssetsPath + "/Save");

        //Create Stream for GameData
        Stream stream = new FileStream(Application.streamingAssetsPath + "/Save/data.vds", FileMode.Create, FileAccess.Write);

        //Formatter
        IFormatter formatter = new BinaryFormatter();

        //Serialize GameData and close stream
        formatter.Serialize(stream, data);
        stream.Close();

        //Create Directory for Models
        System.IO.Directory.CreateDirectory(Application.streamingAssetsPath + "/Save/Models");

        //Save Models
        foreach (var item in data.designs)
        {
            byte[] bytes = item.Value.model.texture.EncodeToPNG();
            File.WriteAllBytes(Application.streamingAssetsPath + "/Save/Models/" + item.Key + ".png", bytes);
        }
    }

    //Load Game
    public void LoadGame()
    {
        //Check Save exists
        if (!Directory.Exists(Application.streamingAssetsPath + "/Save") || !File.Exists(Application.streamingAssetsPath + "/Save/data.vds") || !Directory.Exists(Application.streamingAssetsPath + "/Save/Models"))
            return;

        //Create Stream for GameData
        Stream stream = new FileStream(Application.streamingAssetsPath + "/Save/data.vds", FileMode.Open, FileAccess.Read);

        //Formatter
        IFormatter formatter = new BinaryFormatter();

        //Serialize GameData and close stream
        data = (GameData)formatter.Deserialize(stream);
        stream.Close();

        //Models Directory
        string[] models = Directory.GetFiles(Application.streamingAssetsPath + "/Save/Models");

        //Replace \\ with /
        for (int i = 0; i < models.Length; i++)
        {
            models[i] = models[i].Replace("\\", "/");
        }

        //Load Models
        for (int i = 0; i < models.Length; i++)
        {
            //Ignore .meta files
            if (models[i].Contains(".meta"))
                continue;

            //Path parts
            string[] path = models[i].Split('/');

            //Get Name
            string name = path[path.Length - 1];

            //Remove ".png"
            name = name.Remove(name.Length - 4, 4);

            //Load bytes
            byte[] bytes = File.ReadAllBytes(models[i]);

            //Create Texture
            Texture2D tex = new Texture2D(140, 66, TextureFormat.RGB24, false);
            tex.filterMode = FilterMode.Point;
            tex.LoadImage(bytes);

            //Create Sprite
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector3.zero);

            //Add as design model
            data.designs[name].model = sprite;
        }

        //Update UI
        UpdateDoctrineGraphic();
        UpdateRedesignProgress();
        UpdateSliders();
        HoverDesign("Rifle");
        UpdateBulletin();

        //Build Map
        Texture2D final = data.map.BuildMap(baseMap);
        mapHolder.GetComponent<Image>().sprite = Sprite.Create(final, new Rect(0, 0, final.width, final.height), new Vector2(0, 0), 100, 0, SpriteMeshType.FullRect);

        //Update Date but restart Clock timer
        data.monthClock = 0;
        GameObject.Find("Time").GetComponentInChildren<Text>().text = data.date.ToString("MMMM yyyy");
    }

    //Update Bulletin
    public void UpdateBulletin()
    {
        List<string> bulletin = Bulletin();
        GameObject.Find("BulletinText").GetComponent<Text>().text = "";
        for (int i = 0; i < bulletin.Count; i++)
        {
            //Add Space Line
            if (GameObject.Find("BulletinText").GetComponent<Text>().text != "")
                GameObject.Find("BulletinText").GetComponent<Text>().text += "\n";

            //Add Bulletin Line
            GameObject.Find("BulletinText").GetComponent<Text>().text += "_" + bulletin[i];
        }
        GameObject.Find("BulletinText").GetComponent<Animator>().Play(0);
    }

    //Industry Value
    public float IndustryValue()
    {
        //Get current coverage
        float[] coverage = CurrentCoverage();

        //Average of industry values
        float value = (coverage[0] + coverage[1] + coverage[2]) / 3;

        return value;
    }

    //EffectiveIndustry Value
    public float EffectiveIndustryValue()
    {
        //Get current coverage
        float[] coverage = CurrentCoverage();

        //Average of industry values
        float value = 0;
        for (int i = 0; i < 3; i++)
        {
            value += coverage[i] * 1.5f;
        }
        value /= 3;

        return value;
    }

    //Capacity Value
    public static float CapacityValue()
    {
        //Get current coverage
        float[] coverage = CurrentCoverage();

        //Average of capacity values
        float value = (coverage[3] + coverage[4] + Mathf.Min(coverage[5], coverage[6]) * 2 + coverage[7] + coverage[8]) / 6;

        return value;
    }

    //Capacity Value with Doctrine
    public static float CapacityValueDoctrine()
    {
        //Get current coverage
        float[] coverage = CurrentCoverage();

        //Average of capacity values
        float value = 0;
        for (int i = 3; i < coverage.Length; i++)
        {
            //Case of Breakthrough and Exploitation
            if (i == 5 || i == 6)
                value += Mathf.Min(coverage[5], coverage[6]) * data.doctrine[(Doctrine)(i - 3)];
            //Normal Case
            else
                value += coverage[i] * data.doctrine[(Doctrine)(i - 3)];
        }
        value /= coverage.Length - 3;

        return value;
    }

    //Final Calculation
    public float FinalCalculation()
    {
        //Capacity with Doctrine
        float final = CapacityValueDoctrine();

        //Effective Industry
        float industry = EffectiveIndustryValue();

        //Apply industry to coverage
        final = (industry + final) / 2;

        return final;
    }

    //Update Doctrine Graphic
    public static void UpdateDoctrineGraphic()
    {
        //Get Holder
        GameObject doctrineHolder = GameObject.Find("Doctrine");

        //Doctrine Displays
        List<string> doctrineDisplays = new List<string>()
        {
            "AIDoctrine",
            "AADoctrine",
            "BreakthroughDoctrine",
            "ExploitationDoctrine",
            "MoraleDoctrine",
            "EfficiencyDoctrine"
        };

        //Each Doctrine
        for (int i = 0; i < doctrineDisplays.Count; i++)
        {
            switch (data.doctrine[(Doctrine)i])
            {
                case 0.5f:
                    Utils.GetChild(doctrineHolder, doctrineDisplays[i]).GetComponent<Image>().overrideSprite = doctrineSprites[0];
                    break;
                case 0.75f:
                    Utils.GetChild(doctrineHolder, doctrineDisplays[i]).GetComponent<Image>().overrideSprite = doctrineSprites[1];
                    break;
                case 1:
                    Utils.GetChild(doctrineHolder, doctrineDisplays[i]).GetComponent<Image>().overrideSprite = doctrineSprites[2];
                    break;
                case 1.25f:
                    Utils.GetChild(doctrineHolder, doctrineDisplays[i]).GetComponent<Image>().overrideSprite = doctrineSprites[3];
                    break;
                case 1.5f:
                    Utils.GetChild(doctrineHolder, doctrineDisplays[i]).GetComponent<Image>().overrideSprite = doctrineSprites[4];
                    break;
            }
        }

    }

    //Show Doctrine Change
    public void ShowDoctrineChange()
    {
        //Setup Changed Doctrine
        data.changedDoctrine = new Dictionary<Doctrine, float>();
        foreach (var item in data.doctrine)
        {
            data.changedDoctrine[item.Key] = item.Value;
        }

        //Doctrine Object
        GameObject doctrine = GameObject.Find("DoctrineChange");

        //Doctrine Values
        List<string> doctrineValues = new List<string>()
        {
            "AIValue",
            "AAValue",
            "BreakthroughValue",
            "ExploitationValue",
            "MoraleValue",
            "EfficiencyValue"
        };

        for (int i = 0; i < doctrineValues.Count; i++)
        {
            switch (data.doctrine[(Doctrine)i])
            {
                case 0.5f:
                    Utils.GetChildRecursive(doctrine, doctrineValues[i]).GetComponent<Image>().overrideSprite = doctrineSprites[0];
                    break;
                case 0.75f:
                    Utils.GetChildRecursive(doctrine, doctrineValues[i]).GetComponent<Image>().overrideSprite = doctrineSprites[1];
                    break;
                case 1:
                    Utils.GetChildRecursive(doctrine, doctrineValues[i]).GetComponent<Image>().overrideSprite = doctrineSprites[2];
                    break;
                case 1.25f:
                    Utils.GetChildRecursive(doctrine, doctrineValues[i]).GetComponent<Image>().overrideSprite = doctrineSprites[3];
                    break;
                case 1.5f:
                    Utils.GetChildRecursive(doctrine, doctrineValues[i]).GetComponent<Image>().overrideSprite = doctrineSprites[4];
                    break;
            }

        }

        //Setup Increase/Decrease Buttons
        int k = 0;
        foreach (Transform capacity in Utils.GetChildRecursive(doctrine, "Capacities").transform)
        {
            int j = k;
            Utils.GetChild(capacity.gameObject, "Increase").GetComponent<Button>().onClick.RemoveAllListeners();
            Utils.GetChild(capacity.gameObject, "Increase").GetComponent<Button>().onClick.AddListener(delegate { ChangeDoctrine(j, 0.25f); });
            Utils.GetChild(capacity.gameObject, "Decrease").GetComponent<Button>().onClick.RemoveAllListeners();
            Utils.GetChild(capacity.gameObject, "Decrease").GetComponent<Button>().onClick.AddListener(delegate { ChangeDoctrine(j, -0.25f); });
            k++;
        }

        //Setup Apply Button
        Utils.GetChildRecursive(doctrine, "Issue").GetComponent<Button>().onClick.RemoveAllListeners();
        Utils.GetChildRecursive(doctrine, "Issue").GetComponent<Button>().onClick.AddListener(delegate { ApplyDoctrine(); });

        //Fix Layout
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)Utils.GetChildRecursive(doctrine, "Data").transform);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)Utils.GetChildRecursive(doctrine, "Capacities").transform);

        //Set Points Info
        GameObject.Find("RequestPoints").GetComponent<Text>().enabled = true;
        GameObject.Find("RequestPoints").GetComponent<Text>().text = "CHANGES LEFT: 2      BALANCE: 0";

        //Fire Animation
        doctrine.GetComponent<Animator>().SetBool("open", true);
    }

    //Change Doctrine
    public void ChangeDoctrine(int i, float val)
    {
        //Check change doesnt overflow
        if (data.changedDoctrine[(Doctrine)i] + val > 1.5f)
            return;
        if (data.changedDoctrine[(Doctrine)i] + val < 0.5f)
            return;

        //Apply change
        data.changedDoctrine[(Doctrine)i] += val;

        //Calculate Changes
        float changes = 0.5f;
        foreach (var item in data.changedDoctrine)
        {
            changes -= Mathf.Abs(data.doctrine[item.Key] - item.Value);
        }
        changes /= 0.25f;

        //Calculate Balance
        float balance = -1 * 6;
        foreach (var item in data.changedDoctrine)
        {
            balance += item.Value;
        }
        balance /= 0.25f;

        //Update Points Info
        GameObject.Find("RequestPoints").GetComponent<Text>().text = "CHANGES LEFT: " + changes + "      BALANCE: " + balance;

        //Update Graphic
        GameObject doctrineHolder = GameObject.Find("DoctrineChange");
        List<string> doctrineValues = new List<string>()
        {
            "AIValue",
            "AAValue",
            "BreakthroughValue",
            "ExploitationValue",
            "MoraleValue",
            "EfficiencyValue"
        };
        for (int j = 0; j < doctrineValues.Count; j++)
        {
            switch (data.changedDoctrine[(Doctrine)j])
            {
                case 0.5f:
                    Utils.GetChildRecursive(doctrineHolder, doctrineValues[j]).GetComponent<Image>().overrideSprite = doctrineSprites[0];
                    break;
                case 0.75f:
                    Utils.GetChildRecursive(doctrineHolder, doctrineValues[j]).GetComponent<Image>().overrideSprite = doctrineSprites[1];
                    break;
                case 1:
                    Utils.GetChildRecursive(doctrineHolder, doctrineValues[j]).GetComponent<Image>().overrideSprite = doctrineSprites[2];
                    break;
                case 1.25f:
                    Utils.GetChildRecursive(doctrineHolder, doctrineValues[j]).GetComponent<Image>().overrideSprite = doctrineSprites[3];
                    break;
                case 1.5f:
                    Utils.GetChildRecursive(doctrineHolder, doctrineValues[j]).GetComponent<Image>().overrideSprite = doctrineSprites[4];
                    break;
            }
        }
    }

    //Apply Doctrine Change
    public void ApplyDoctrine()
    {
        //Check Valid Changes
        float changes = 0.5f;
        foreach (var item in data.changedDoctrine)
        {
            changes -= Mathf.Abs(data.doctrine[item.Key] - item.Value);
        }
        changes /= 0.25f;

        if (changes < 0)
            return;

        //Check Valid Balance
        float balance = -1 * 6;
        foreach (var item in data.changedDoctrine)
        {
            balance += item.Value;
        }
        balance /= 0.25f;

        if (balance != 0)
            return;

        //Apply Doctrine
        foreach (var item in data.changedDoctrine)
        {
            data.doctrine[item.Key] = data.changedDoctrine[item.Key];
        }

        //Close Doctrine Change
        GameObject.Find("DoctrineChange").GetComponent<Animator>().SetBool("open", false);

        //Update Doctrine Graphic
        UpdateDoctrineGraphic();

        //Transition into redesigns
        //If no redesigns to do
        if (data.designsNeeded.Count == 0)
        {
            //Return to Normal State
            data.state = State.NORMAL;

            //Unblock Playing
            data.blockMenu = false;

            //Open Map
            CloseMap(false);

            //Make Menu Icon Normal Color
            GameObject.Find("TimeIcon").GetComponent<Image>().color = new Color32(50, 50, 50, 255);
            GameObject.Find("ExitIcon").GetComponent<Image>().color = new Color32(50, 50, 50, 255);

            //Nullify redesign
            data.redesignType = null;
        }
        //If there are, do
        else
        {
            //Set redesign type
            data.redesignType = data.designsNeeded[0];

            //Remove highlight all Designs
            foreach (Transform designObject in GameObject.Find("DesignsHolder").transform)
            {
                Utils.GetChild(designObject.gameObject, "Selected").GetComponent<Image>().enabled = false;
            }

            //Highlight Design of Redesign Type
            HoverDesign(string.Concat(data.redesignType.ToString().Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' '));

            //Update Design Choice Title
            string nameSpaced = string.Concat(data.redesignType.ToString().Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');
            GameObject.Find("DesignDecisionTitle").GetComponent<Text>().text = nameSpaced.ToUpper() + " DESIGN DECISION";

            //Invoke Show Request for new Design
            Invoke("ShowRequest", 0.5f);
        }
    }

    //Update Redesign Progress
    public void UpdateRedesignProgress()
    {
        foreach (KeyValuePair<string, Design> design in data.designs)
        {
            //Get Warn Image
            Image warnImage = Utils.GetChild(GameObject.Find(design.Key), "Warn").GetComponent<Image>();

            //This Turn warning
            if (design.Value.age - 1 == design.Value.redesignPeriod)
            {
                warnImage.enabled = true;
                warnImage.color = new Color32(130, 25, 25, 255);
            }
            //Next Turn warning
            else if (design.Value.age == design.Value.redesignPeriod)
            {
                warnImage.enabled = true;
                warnImage.color = new Color32(36, 110, 30, 255);
            }
            //No warning
            else
                warnImage.enabled = false;

            //Set value
            Utils.GetChild(GameObject.Find(design.Key), "Progress").GetComponent<Image>().fillAmount = ((float)design.Value.age / design.Value.redesignPeriod);
        }
    }

    //Time Button
    public void ToggleTime()
    {
        if (data.blockMenu)
            return;

        if (data.playing)
        {
            Utils.GetChild(GameObject.Find("TimeControl"), "TimeIcon").GetComponent<Image>().overrideSprite = playSprite;
            data.playing = false;
        }
        else
        {
            Utils.GetChild(GameObject.Find("TimeControl"), "TimeIcon").GetComponent<Image>().overrideSprite = pauseSprite;
            data.playing = true;
        }
    }

    //Map Peeker
    public void PeekMap()
    {
        //Not Active in NORMAL state
        if (data.state == State.NORMAL)
            return;

        GameObject.Find("MapHolder").GetComponent<Animator>().SetBool("open", false);
    }
    public void UnPeekMap()
    {
        //Not Active in NORMAL state
        if (data.state == State.NORMAL)
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
        data.requestMask = new int[7] { 0, 0, 0, 0, 0, 0, 0 };

        //Show Focus Points
        focusPoints.GetComponent<Text>().enabled = true;
        focusPoints.GetComponent<Text>().text = "FOCUS POINTS REMAINING: 0";

        //Request Object
        GameObject request = GameObject.Find("Request");

        //Clear Issue/Signature
        Utils.GetChildRecursive(request, "Border").GetComponent<Image>().enabled = false;
        Utils.GetChildRecursive(request, "Text").GetComponent<Text>().enabled = false;
        Utils.GetChildRecursive(request, "Signature").GetComponent<Text>().enabled = false;

        //Request Info
        string nameSpaced = string.Concat(data.redesignType.ToString().Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');
        Utils.GetChild(request, "Title").GetComponent<Text>().text = "Ministry of War\n\nDesign Request - " + nameSpaced;
        Utils.GetChild(request, "Date").GetComponent<Text>().text = data.date.ToString("MMMM yyyy");

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
        for (int i = 3; i < data.designs[nameSpaced].characteristics.Count; i++)
        {
            //Instantiate new
            GameObject doctrineCharacteristic = Instantiate(requestCharacteristicPrefab);
            Utils.GetChild(doctrineCharacteristic, "Icon").GetComponent<Image>().overrideSprite = ImpactSprite(data.designs[nameSpaced].characteristics[i].impact);
            Utils.GetChild(doctrineCharacteristic, "Title").GetComponent<Text>().text = data.designs[nameSpaced].characteristics[i].name + ":";
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
        List<Design> finalChoices = RequestDesign(data.redesignType, data.requestMask, numChoices).ToList();

        //Final Choices
        data.choices = finalChoices.ToArray();

        //Setup Design Choices Display
        for (int i = 0; i < data.choices.Length; i++)
        {
            //Choice
            GameObject choice = Instantiate(choicePrefab);

            //Title
            string nameSpaced = string.Concat(data.redesignType.ToString().Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');
            Utils.GetChild(choice, "Title").GetComponent<Text>().text = "Ministry of War\n\nDesign Proposal - " + nameSpaced;

            //Date
            Utils.GetChild(choice, "Date").GetComponent<Text>().text = data.date.ToString("MMMM yyyy");

            //Name & Designer
            Utils.GetChildRecursive(choice, "Designer").GetComponent<Text>().text = "Designer: " + data.choices[i].developer.name;
            Utils.GetChildRecursive(choice, "Designation").GetComponent<Text>().text = "Designation: " + data.choices[i].name;

            //Edit Industrial Values
            Utils.GetChildRecursive(choice, "EngineeringValue").GetComponent<Text>().text = Characteristic.PredictedToString(data.choices[i].characteristics[0].predictedValue, true);
            Utils.GetChildRecursive(choice, "ResourceValue").GetComponent<Text>().text = Characteristic.PredictedToString(data.choices[i].characteristics[1].predictedValue, true);
            Utils.GetChildRecursive(choice, "ReliabilityValue").GetComponent<Text>().text = Characteristic.PredictedToString(data.choices[i].characteristics[2].predictedValue);

            //Clear Doctrine Values
            Utils.ClearChildren(Utils.GetChildRecursive(choice, "DoctrineData"));

            //Add Doctrine Values
            for (int c = 3; c < data.choices[i].characteristics.Count; c++)
            {
                //Setup Characteristic
                GameObject doctrineCharacteristic = Instantiate(characteristicPrefab);
                Utils.GetChild(doctrineCharacteristic, "Icon").GetComponent<Image>().overrideSprite = ImpactSprite(data.choices[i].characteristics[c].impact);
                Utils.GetChild(doctrineCharacteristic, "Title").GetComponent<Text>().text = data.choices[i].characteristics[c].name + ":";
                Utils.GetChild(doctrineCharacteristic, "Value").GetComponent<Text>().text = Characteristic.PredictedToString(data.choices[i].characteristics[c].predictedValue);

                //Add to holder
                doctrineCharacteristic.transform.SetParent(Utils.GetChildRecursive(choice, "DoctrineData").transform);
            }

            //Model View
            GameObject model = Utils.GetChild(choice, "Model");
            Utils.GetChild(model, "Image").GetComponent<Image>().sprite = data.choices[i].model;

            //Callback Choice
            int id = i;
            EventTrigger.Entry applyChoice = new EventTrigger.Entry();
            applyChoice.eventID = EventTriggerType.PointerClick;
            applyChoice.callback.AddListener((eventData) => { ApplyChoice(id); });
            choice.GetComponent<EventTrigger>().triggers.Add(applyChoice);

            //Rebuild Layout
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)Utils.GetChildRecursive(choice, "IndustrialData").transform);
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)Utils.GetChildRecursive(choice, "DoctrineData").transform);

            //Add to Holder
            choice.transform.SetParent(GameObject.Find("Choices").transform);
        }

        //Remove Design Decision Title
        GameObject.Find("DesignDecisionTitle").GetComponent<Text>().text = "";

        //Show Choices
        GameObject.Find("Choices").GetComponent<Animator>().SetBool("open", true);
    }

    //Apply Choice
    public void ApplyChoice(int id)
    {
        //Design Spaced
        string designSpaced = string.Concat(data.redesignType.ToString().Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');

        //Set Previous Design
        data.prevDesigns[designSpaced] = data.designs[designSpaced];

        //Set New Design
        data.designs[designSpaced] = data.choices[id];

        //Update Redesign Progress
        UpdateRedesignProgress();

        //Hide Choices
        GameObject.Find("Choices").GetComponent<Animator>().SetBool("open", false);

        //Remove from list of redesigns
        data.designsNeeded.RemoveAt(0);

        //If no more to do exit
        if (data.designsNeeded.Count == 0)
        {
            //Return to Normal State
            data.state = State.NORMAL;

            //Unblock Playing
            data.blockMenu = false;

            //Open Map
            CloseMap(false);

            //Make Menu Icons Normal Color
            GameObject.Find("TimeIcon").GetComponent<Image>().color = new Color32(50, 50, 50, 255);
            GameObject.Find("ExitIcon").GetComponent<Image>().color = new Color32(50, 50, 50, 255);

            //Highlight Design of Redesign Type
            HoverDesign(string.Concat(data.redesignType.ToString().Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' '));

            //Nullify redesign
            data.redesignType = null;
        }
        //If there is do next
        else
        {
            //Set redesign type
            data.redesignType = data.designsNeeded[0];

            //Remove highlight all Designs
            foreach (Transform designObject in GameObject.Find("DesignsHolder").transform)
            {
                Utils.GetChild(designObject.gameObject, "Selected").GetComponent<Image>().enabled = false;
            }

            //Highlight Design of Redesign Type
            HoverDesign(string.Concat(data.redesignType.ToString().Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' '));

            //Update Design Choice Title
            string nameSpaced = string.Concat(data.redesignType.ToString().Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');
            GameObject.Find("DesignDecisionTitle").GetComponent<Text>().text = nameSpaced.ToUpper() + " DESIGN DECISION";

            //Invoke Show Request for new Design
            Invoke("ShowRequest", 0.5f);
        }
    }

    //Request Change
    public void RequestChange(int id, Text text, int value)
    {
        //Excess
        if (data.requestMask[id] + value > 2)
            return;
        if (data.requestMask[id] + value < -2)
            return;

        //Check Request Point Limit
        if (data.requestMask.Sum() + value > 0)
            return;

        //Add to mask
        data.requestMask[id] += value;

        //Update Focus Text
        focusPoints.GetComponent<Text>().text = "FOCUS POINTS REMAINING: " + (0 - data.requestMask.Sum());

        //Update Text
        switch (data.requestMask[id])
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
            if (displayProgress)
            {
                //Decrease
                if (data.prevCoverage[i] > coverage[i])
                {
                    GameObject.Find(objectProgress[i]).GetComponent<Image>().enabled = true;
                    GameObject.Find(objectProgress[i]).GetComponent<Image>().color = new Color32(130, 25, 25, 255);
                    RectTransform rectTrans = (RectTransform)GameObject.Find(objectProgress[i]).transform;
                    rectTrans.SetPositionAndRotation(rectTrans.position, Quaternion.Euler(new Vector3(0, 0, 270)));

                    GameObject.Find(objectProgressAmount[i]).GetComponent<Text>().enabled = true;
                    GameObject.Find(objectProgressAmount[i]).GetComponent<Text>().color = new Color32(130, 25, 25, 255);
                }
                //Increase
                else if (data.prevCoverage[i] < coverage[i])
                {
                    GameObject.Find(objectProgress[i]).GetComponent<Image>().enabled = true;
                    GameObject.Find(objectProgress[i]).GetComponent<Image>().color = new Color32(36, 110, 30, 255);
                    RectTransform rectTrans = (RectTransform)GameObject.Find(objectProgress[i]).transform;
                    rectTrans.SetPositionAndRotation(rectTrans.position, Quaternion.Euler(new Vector3(0, 0, 90)));

                    GameObject.Find(objectProgressAmount[i]).GetComponent<Text>().enabled = true;
                    GameObject.Find(objectProgressAmount[i]).GetComponent<Text>().color = new Color32(36, 110, 30, 255);
                }
                //Same
                else
                {
                    GameObject.Find(objectProgress[i]).GetComponent<Image>().enabled = false;

                    GameObject.Find(objectProgressAmount[i]).GetComponent<Text>().enabled = false;

                }

                //Set Difference
                GameObject.Find(objectProgressAmount[i]).GetComponent<Text>().text = Mathf.CeilToInt(Mathf.Abs(data.prevCoverage[i] - coverage[i]) * 100).ToString();
            }

            //Set Value
            GameObject.Find(objectSliders[i]).GetComponent<Slider>().value = coverage[i];
        }
    }

    //Request Designs
    public Design[] RequestDesign(Type type, int[] mask, int num)
    {
        List<Design> designs = new List<Design>();

        //Institutes indexes
        List<int> list = new List<int>();
        for (int n = 0; n < data.institutes.Count; n++)
        {
            list.Add(n);
        }

        //Choose Institutes and Generate
        for (int i = 0; i < num; i++)
        {
            //Pick id
            int id = UnityEngine.Random.Range(0, list.Count);

            //Generate Design
            designs.Add(data.institutes[id].GenerateDesign(type, mask));

            //Remove id to not repeat
            list.RemoveAt(id);
        }

        return designs.ToArray();
    }

    //Add Institutes for Type(s)
    public void AddInstitutes(Type[] types, int number)
    {
        for (int i = 0; i < number; i++)
        {
            data.institutes.Add(new DesignInstitute(types));
        }
    }

    //Current Coverage
    public static float[] CurrentCoverage()
    {
        //Value of Characteristics
        float[] values = new float[9];
        foreach (KeyValuePair<string, Design> design in data.designs)
        {
            for (int i = 0; i < design.Value.characteristics.Count; i++)
            {
                //Transition Production until 6 months
                if (design.Value.age < 6)
                {
                    float ratio = (float)design.Value.age / 6;
                    values[(int)design.Value.characteristics[i].impact] += Mathf.RoundToInt(design.Value.characteristics[i].trueValue * ratio + data.prevDesigns[design.Key].characteristics[i].trueValue * (1 - ratio));
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
    public static int[] ImpactOccurences()
    {
        int[] values = new int[9];
        foreach (var design in data.designs)
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
        data.lastHover = type;

        //Get design
        Design design = data.designs[type];

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
        Utils.GetChild(originalChoice, "Date").GetComponent<Text>().text = data.date.AddMonths(-design.age).ToString("MMMM yyyy");

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

        //Edit Industrial Values
        Utils.GetChildRecursive(combatReport, "EngineeringValue").GetComponent<Text>().text = design.characteristics[0].KnowledgeToString();
        Utils.GetChildRecursive(combatReport, "ResourceValue").GetComponent<Text>().text = design.characteristics[1].KnowledgeToString();
        Utils.GetChildRecursive(combatReport, "ReliabilityValue").GetComponent<Text>().text = design.characteristics[2].KnowledgeToString();

        //Clear Doctrine Values
        Utils.ClearChildren(Utils.GetChildRecursive(combatReport, "CapacityData"));

        //Add Doctrine Values
        for (int i = 3; i < design.characteristics.Count; i++)
        {
            //Setup Characteristic
            GameObject doctrineCharacteristic = Instantiate(characteristicPrefab);
            Utils.GetChild(doctrineCharacteristic, "Icon").GetComponent<Image>().overrideSprite = ImpactSprite(design.characteristics[i].impact);
            Utils.GetChild(doctrineCharacteristic, "Title").GetComponent<Text>().text = design.characteristics[i].name + ":";
            Utils.GetChild(doctrineCharacteristic, "Value").GetComponent<Text>().text = design.characteristics[i].KnowledgeToString();

            //Add to holder
            doctrineCharacteristic.transform.SetParent(Utils.GetChildRecursive(combatReport, "CapacityData").transform);
        }

        //Rebuild Layout
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)Utils.GetChildRecursive(combatReport, "CapacityData").transform);

        #endregion

        //Model
        GameObject.Find("ModelHolder").GetComponent<Image>().overrideSprite = design.model;
    }

    //Stop highlight hovered
    public void DeHoverDesign()
    {
        //Default to redesign type if in redesign process
        if (data.redesignType != null)
            HoverDesign(string.Concat(data.redesignType.ToString().Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' '));
    }

    //Sprite for Impact
    public Sprite ImpactSprite(Impact impact)
    {
        return impactSprites[(int)impact];
    }

}