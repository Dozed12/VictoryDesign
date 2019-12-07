using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameHolder : MonoBehaviour
{
    //Characteristic UI Elements and Resources
    public Sprite HIGH_IMPORTANCE_SPRITE;
    public Sprite MEDIUM_IMPORTANCE_SPRITE;
    public Sprite LOW_IMPORTANCE_SPRITE;

    public GameObject CHARACTERISTIC_PLAYER;
    public GameObject CHARACTERISTIC_ENEMY;
    public GameObject CHARACTERISTIC_PLAYER_BRIEF;
    public GameObject CHARACTERISTIC_ENEMY_BRIEF;

    public GameObject PROPOSAL;

    public Sprite ACCURACY_ICON;
    public Sprite POWER_ICON;
    public Sprite PORTABILITY_ICON;
    public Sprite RATE_OF_FIRE_ICON;
    public Sprite COMFORT_ICON;
    public Sprite CAMOUFLAGE_ICON;
    public Sprite WEATHER_RESISTANCE_ICON;
    public Sprite ARMOR_ICON;
    public Sprite PROTECTION_ICON;

    //Intel Focus
    public Sprite VIEW;
    public Sprite NO_VIEW;

    //Monthly Report UI Objects
    public GameObject NEW_DESIGN_BUTTON;
    public GameObject DIVIDER;

    //Fixed UI Objects
    public GameObject designSelectorPopup;

    //UI State Variables
    public string designChoiceType;
    public Design currentDisplayDesign;

    //Map
    public Texture2D baseMap;
    public GameObject mapHolder;

    //Date
    public Text date;

    // Start is called before the first frame update
    void Start()
    {
        //Setup Naming system
        Naming.SetupNaming();

        //Setup a New Game
        SetupNewGame();

        //Test Map Builder
        Map.warStage = 1;
        Map.ProgressWar(6);
        Texture2D final = Map.BuildMap(DrawingUtils.TextureCopy(baseMap));
        mapHolder.GetComponent<Image>().sprite = Sprite.Create(final, new Rect(0, 0, final.width, final.height), new Vector2(0, 0), 100, 0, SpriteMeshType.FullRect);

        //Test True Nation Difference
        Debug.Log("True Nation Difference: " + Game.NationDifference(true));

        //Test Average Nation Difference
        Debug.Log("Average Nation Difference: " + Game.NationDifference(false));

        //Test True Deep Difference Analysis
        Debug.Log("True Deep Analysis:");
        Utils.DumpArray(Game.DeepDifferenceAnalysis(true));

        //Test Average Deep Difference Analysis
        Debug.Log("Average Deep Analysis:");
        Utils.DumpArray(Game.DeepDifferenceAnalysis(false));
    }

    // Update is called once per frame
    void Update()
    {
        //Test Generate new Helmets Design
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Helmet[] helmets = Game.us.RequestDesign(typeof(Helmet)).Cast<Helmet>().ToArray();
            Utils.DumpArray(helmets);
        }

        //Test Design Characteristic Progress
        if (Input.GetKeyDown(KeyCode.W))
        {
            Game.us.designs["Helmet"].FindCharacteristic("Protection").ProgressBounds(2);
            Utils.Dump(Game.us.designs["Helmet"].FindCharacteristic("Protection"));
        }

        //Test Design Progress Random
        if (Input.GetKeyDown(KeyCode.E))
        {
            Utils.Dump(Game.us.designs["Helmet"]);
            Game.us.designs["Helmet"].ProgressRandom(4);
            Utils.Dump(Game.us.designs["Helmet"]);
        }
    }

    //Setup new Game
    public void SetupNewGame()
    {
        //Set start date and turn
        Game.date = new DateTime(1890, 1, 1);
        Game.turn = 1;

        //Setup nations
        Game.us = new Nation();
        Game.us.isPlayer = true;
        Game.them = new Nation();
        Game.them.isPlayer = false;

        //Create Our Base Institutes
        Game.us.AddInstitutes(new Type[] { typeof(Helmet), typeof(Uniform) }, 3);
        Game.us.AddInstitutes(new Type[] { typeof(Rifle) }, 3);
        Game.us.AddInstitutes(new Type[] { typeof(SmallArm) }, 3);
        Game.us.AddInstitutes(new Type[] { typeof(MachineGun) }, 3);

        //Add our base designs
        Game.us.designs["Rifle"] = (Rifle)Game.us.RequestDesign(typeof(Rifle))[0];
        Game.us.designs["SmallArm"] = (SmallArm)Game.us.RequestDesign(typeof(SmallArm))[0];
        Game.us.designs["Uniform"] = (Uniform)Game.us.RequestDesign(typeof(Uniform))[0];
        Game.us.designs["Helmet"] = (Helmet)Game.us.RequestDesign(typeof(Helmet))[0];
        Game.us.designs["MachineGun"] = (MachineGun)Game.us.RequestDesign(typeof(MachineGun))[0];

        //Create Their Base Institutes
        Game.them.AddInstitutes(new Type[] { typeof(Helmet), typeof(Uniform) }, 3);
        Game.them.AddInstitutes(new Type[] { typeof(Rifle) }, 3);
        Game.them.AddInstitutes(new Type[] { typeof(SmallArm) }, 3);
        Game.them.AddInstitutes(new Type[] { typeof(MachineGun) }, 3);

        //Add their base designs
        Game.them.designs["Rifle"] = (Rifle)Game.them.RequestDesign(typeof(Rifle))[0];
        Game.them.designs["SmallArm"] = (SmallArm)Game.them.RequestDesign(typeof(SmallArm))[0];
        Game.them.designs["Uniform"] = (Uniform)Game.them.RequestDesign(typeof(Uniform))[0];
        Game.them.designs["Helmet"] = (Helmet)Game.them.RequestDesign(typeof(Helmet))[0];
        Game.them.designs["MachineGun"] = (MachineGun)Game.them.RequestDesign(typeof(MachineGun))[0];

        //Default current display design to rifle
        currentDisplayDesign = Game.them.designs["Rifle"];
        DisplayDesign(currentDisplayDesign);
    }

    //Next Turn
    public void NextTurn()
    {
        //Get monthly report holder
        GameObject monthlyReport = GameObject.Find("MonthNewsPanel").GetComponentInChildren<VerticalLayoutGroup>().gameObject;

        //Check turn can be passed(no interactable New Design Button)
        foreach (Transform child in monthlyReport.transform)
        {
            if (child.gameObject.GetComponent<Button>() != null)
                if (child.gameObject.GetComponent<Button>().interactable == true)
                    return;
        }

        //Add turn
        Game.turn++;

        //Add month
        Game.date = Game.date.AddMonths(1);

        //Update Date
        date.text = Game.date.ToString("MMMM yyyy");

        //TODO War Processing

        //Update Player Designs Age
        foreach (KeyValuePair<string, Design> item in Game.us.designs)
        {
            item.Value.age++;
        }

        //Update Enemy Designs Age
        foreach (KeyValuePair<string, Design> item in Game.them.designs)
        {
            item.Value.age++;
        }

        //Clear monthly report holder
        foreach (Transform child in monthlyReport.transform)
        {
            Destroy(child.gameObject);
        }

        //Clear Proposals
        Game.us.proposals = new Dictionary<string, Design[]>();

        //New Player Designs Needed 
        foreach (KeyValuePair<string, Design> item in Game.us.designs)
        {
            //Find expired
            if (item.Value.age == item.Value.redesignPeriod)
            {
                //Instantiate Button
                GameObject newDesignButton = Instantiate(NEW_DESIGN_BUTTON);

                //Design Type
                string type = item.Value.GetType().ToString();

                //Add space before Capital Letters
                type = string.Concat(type.Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');

                //Description of button
                newDesignButton.GetComponentInChildren<Text>().text = "New Design Decision for " + type;

                //Generate Proposals
                Game.us.proposals.Add(item.Value.GetType().ToString(), Game.us.RequestDesign(item.Value.GetType()));

                //Button on click event to NewDesignPopup
                string temp = (string)item.Value.GetType().ToString().Clone();
                newDesignButton.GetComponent<Button>().onClick.AddListener(delegate { NewDesignPopup(temp); });

                //Add to Monthly Report
                newDesignButton.transform.SetParent(monthlyReport.transform);

                //Add Divider
                GameObject divider = Instantiate(DIVIDER);
                divider.transform.SetParent(monthlyReport.transform);
            }
        }

        //New Enemy Designs Needed (this kind of dictionary iterating allows for changes)
        foreach (string key in Game.them.designs.Keys.ToList())
        {
            //Find expired
            if (Game.them.designs[key].age == Game.them.designs[key].redesignPeriod)
            {
                //Request proposals
                //TODO Minimum value for the design will control difficulty
                Design[] proposals = Game.them.RequestDesign(Game.them.designs[key].GetType(), -2);

                //Pick random proposal
                Game.them.designs[key] = proposals[UnityEngine.Random.Range(0, proposals.Count())];
            }
        }

        //TODO Base Knowledge Increase

        //TODO Intel Events Us

        //TODO Intel Events Them

        //Re draw Selected Design(update things like Age)
        DisplayDesign(currentDisplayDesign);
    }

    //Focus Intel Enemy Design
    public void FocusEnemyDesign(string design)
    {
        //Already in list
        if (Game.focuses.Contains(design))
            return;

        //Remove last if more or equal to 3
        if (Game.focuses.Count >= 3)
            Game.focuses.Dequeue();

        //Add new
        Game.focuses.Enqueue(design);

        Utils.DumpArray(Game.focuses.ToArray());

        //Update Icons
        GameObject enemyDesignsPanel = GameObject.Find("EnemyDesignsPanel");
        foreach (Transform child in enemyDesignsPanel.transform)
        {
            Debug.Log(child.name);
            GameObject info = Utils.GetChildRecursive(child.gameObject, "Info");
            info.GetComponent<Image>().sprite = null;
            if (Game.focuses.Contains(child.name))
                info.GetComponent<Image>().sprite = VIEW;
            else
                info.GetComponent<Image>().sprite = NO_VIEW;
        }
    }

    // Select Design to show
    // Info is of format: "who.Type"
    public void SelectDesign(string info)
    {
        // Separate string
        string[] parts = info.Split('.');
        string who = parts[0];
        string what = parts[1];

        //Identify side
        Nation side;
        switch (who)
        {
            case "our":
                side = Game.us;
                break;
            case "their":
                side = Game.them;
                break;
            default:
                Debug.Log("SelectDesign Identification failed");
                side = new Nation();
                break;
        }

        //Identify design
        Design selectedDesign = side.designs[what];

        //Dump Selected Design
        Debug.Log("Selected Design");
        Utils.Dump(selectedDesign);

        //Display Design
        DisplayDesign(selectedDesign);
    }

    //Display Design
    public void DisplayDesign(Design design)
    {
        //Save current displayed design
        currentDisplayDesign = design;

        //Find Design Panel
        GameObject designPanel = GameObject.Find("DesignPanel");

        //All childs of Panel
        Transform[] transforms = designPanel.GetComponentsInChildren<Transform>();
        GameObject[] children = new GameObject[transforms.Length];
        for (int i = 0; i < transforms.Length; i++)
        {
            children[i] = transforms[i].gameObject;
        }

        //Characteristics Holder
        GameObject characteristics = null;

        //Update Info
        for (int i = 0; i < children.Length; i++)
        {
            //Check if null (May happen when deleting characteristics)
            if (children[i] == null)
                continue;

            //Design Name in Title
            if (children[i].name == "DesignName")
            {
                children[i].GetComponent<Text>().text = design.name;
            }
            //Design Type
            else if (children[i].name == "TypeName")
            {
                //Get type from class type   
                string type = design.GetType().ToString();

                //Add space before Capital Letters
                type = string.Concat(type.Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');

                //Apply
                children[i].GetComponent<Text>().text = type;
            }
            //Design Name in Info
            else if (children[i].name == "Name")
            {
                children[i].GetComponent<Text>().text = "Name: " + design.name;
            }
            //Developer
            else if (children[i].name == "Developer")
            {
                children[i].GetComponent<Text>().text = "Developer: " + design.developer.name;
            }
            //Date
            else if (children[i].name == "Date")
            {
                children[i].GetComponent<Text>().text = "Date: " + design.date.ToString("MMMM yyyy");
            }
            //Age Months
            else if (children[i].name == "Months")
            {
                //Only display redesign period if player design
                if (design.owner.isPlayer)
                    children[i].GetComponent<Text>().text = "Months Age: " + design.age + " (" + design.redesignPeriod + ")";
                else
                    children[i].GetComponent<Text>().text = "Months Age: " + design.age;
            }
            //Design Importance
            else if (children[i].name == "Importance")
            {
                switch (design.importance)
                {
                    case Importance.HIGH:
                        children[i].GetComponent<Image>().sprite = HIGH_IMPORTANCE_SPRITE;
                        break;
                    case Importance.MEDIUM:
                        children[i].GetComponent<Image>().sprite = MEDIUM_IMPORTANCE_SPRITE;
                        break;
                    case Importance.LOW:
                        children[i].GetComponent<Image>().sprite = LOW_IMPORTANCE_SPRITE;
                        break;
                }
            }
            //Delete current Characteristics
            else if (children[i].name == "Characteristics")
            {
                //Destroy children
                foreach (Transform child in children[i].transform)
                {
                    Destroy(child.gameObject);
                }

                //Save Characteristics Holder(used to add Characteristic prefabs)
                characteristics = children[i];
            }
        }

        //Instantiate Characteristics
        for (int i = 0; i < design.characteristics.Count; i++)
        {
            //Instantiate player Characteristic Prefab if player design
            GameObject newCharacteristic = null;
            if (design.owner.isPlayer)
            {
                newCharacteristic = Instantiate(CHARACTERISTIC_PLAYER);
            }
            //Instantiate enemy Characteristic Prefab if not player design
            else
            {
                newCharacteristic = Instantiate(CHARACTERISTIC_ENEMY);
            }

            //Edit Instance values
            foreach (Transform child in newCharacteristic.transform)
            {
                //Set Icon (Icon is set using the Characteristic name and associating it with variable name in this class that stores Sprites)
                //UpperCase(characteristic.name) + "_ICON"
                if (child.name == "Icon")
                {
                    string name = design.characteristics[i].name;
                    name = name.ToUpper();
                    name += "_ICON";
                    name = name.Replace(" ", "_");
                    child.gameObject.GetComponent<Image>().sprite = (Sprite)typeof(GameHolder).GetField(name).GetValue(this);
                }
                //Name
                else if (child.name == "Name")
                {
                    child.gameObject.GetComponent<Text>().text = design.characteristics[i].name;
                }
                //Estimate Value
                else if (child.name == "Estimate")
                {
                    string value = design.characteristics[i].predictedValue.ToString();
                    if (design.characteristics[i].predictedValue > 0)
                        value = "+" + value;
                    child.gameObject.GetComponent<Text>().text = value;
                }
                //True Value
                else if (child.name == "True")
                {
                    //Empty knowledge case
                    if (design.characteristics[i].emptyKnowledge)
                    {
                        child.gameObject.GetComponent<Text>().text = "? ? ?";
                    }
                    //Full Knowledge case
                    else if (design.characteristics[i].fullKnowledge)
                    {
                        child.gameObject.GetComponent<Text>().text = design.characteristics[i].trueValue.ToString();
                    }
                    //Base case
                    else
                    {
                        string left = design.characteristics[i].leftBound.ToString();
                        if (design.characteristics[i].leftBound > 0)
                            left = "+" + left;

                        string right = design.characteristics[i].rightBound.ToString();
                        if (design.characteristics[i].rightBound > 0)
                            right = "+" + right;

                        child.gameObject.GetComponent<Text>().text = left + " to " + right;
                    }
                }
                //Design Importance
                else if (child.name == "Importance")
                {
                    switch (design.characteristics[i].importance)
                    {
                        case Importance.HIGH:
                            child.gameObject.GetComponent<Image>().sprite = HIGH_IMPORTANCE_SPRITE;
                            break;
                        case Importance.MEDIUM:
                            child.gameObject.GetComponent<Image>().sprite = MEDIUM_IMPORTANCE_SPRITE;
                            break;
                        case Importance.LOW:
                            child.gameObject.GetComponent<Image>().sprite = LOW_IMPORTANCE_SPRITE;
                            break;
                    }
                }
            }

            //Add to characteristics holder
            newCharacteristic.transform.SetParent(characteristics.transform);
        }

        //Force update on DesignTitle to fix size
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)GameObject.Find("DesignTitle").transform);
    }

    //New Design Popup
    public void NewDesignPopup(string type)
    {
        //Set Type
        designChoiceType = type;

        //Activate Poppup
        designSelectorPopup.SetActive(true);

        //Get Actual Panel
        GameObject panel = Utils.GetChild(designSelectorPopup, "NewDesignPanel");

        //Get Title
        GameObject title = Utils.GetChild(panel, "DesignTitle");

        //Update Title
        string designType = designChoiceType;
        designType = string.Concat(designType.Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');
        Utils.GetChild(title, "TypeName").GetComponent<Text>().text = designType;

        //Force update on DesignTitle to fix size
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)title.transform);

        //Update Importance
        switch (Game.us.designs[designChoiceType].importance)
        {
            case Importance.HIGH:
                Utils.GetChild(title, "Importance").GetComponent<Image>().sprite = HIGH_IMPORTANCE_SPRITE;
                break;
            case Importance.MEDIUM:
                Utils.GetChild(title, "Importance").GetComponent<Image>().sprite = MEDIUM_IMPORTANCE_SPRITE;
                break;
            case Importance.LOW:
                Utils.GetChild(title, "Importance").GetComponent<Image>().sprite = LOW_IMPORTANCE_SPRITE;
                break;
        }

        //Update Save Button Action
        Utils.GetChild(panel, "Confirm").GetComponent<Button>().onClick.RemoveAllListeners();
        Utils.GetChild(panel, "Confirm").GetComponent<Button>().onClick.AddListener(SaveNewDesign);

        //Get Layout manager
        GameObject layout = Utils.GetChildRecursive(designSelectorPopup, "Layout");

        //Get Enemy Intel Panel
        GameObject enemyIntel = Utils.GetChild(layout, "EnemyInfo");

        //Get Enemy Intel Content
        GameObject enemyIntelContent = Utils.GetChildRecursive(enemyIntel, "Content");

        //Clear content
        Utils.ClearChildren(enemyIntelContent);

        //Enemy Design
        Design enemyDesign = Game.them.designs[designChoiceType];

        //Place Brief Characteristics of Enemy Intel
        for (int i = 0; i < enemyDesign.characteristics.Count; i++)
        {
            //Instantiate
            GameObject newCharacteristic = Instantiate(CHARACTERISTIC_ENEMY_BRIEF);

            //Edit Instance values
            foreach (Transform child in newCharacteristic.transform)
            {
                //Set Icon (Icon is set using the Characteristic name and associating it with variable name in this class that stores Sprites)
                //UpperCase(characteristic.name) + "_ICON"
                if (child.name == "Icon")
                {
                    string name = enemyDesign.characteristics[i].name;
                    name = name.ToUpper();
                    name += "_ICON";
                    name = name.Replace(" ", "_");
                    child.gameObject.GetComponent<Image>().sprite = (Sprite)typeof(GameHolder).GetField(name).GetValue(this);
                }
                //Name
                else if (child.name == "Name")
                {
                    child.gameObject.GetComponent<Text>().text = enemyDesign.characteristics[i].name;
                }
                //Intel
                else if (child.name == "Intel")
                {
                    //Empty knowledge case
                    if (enemyDesign.characteristics[i].emptyKnowledge)
                    {
                        child.gameObject.GetComponent<Text>().text = "? ? ?";
                    }
                    //Full Knowledge case
                    else if (enemyDesign.characteristics[i].fullKnowledge)
                    {
                        child.gameObject.GetComponent<Text>().text = enemyDesign.characteristics[i].trueValue.ToString();
                    }
                    //Base case
                    else
                    {
                        string left = enemyDesign.characteristics[i].leftBound.ToString();
                        if (enemyDesign.characteristics[i].leftBound > 0)
                            left = "+" + left;

                        string right = enemyDesign.characteristics[i].rightBound.ToString();
                        if (enemyDesign.characteristics[i].rightBound > 0)
                            right = "+" + right;

                        child.gameObject.GetComponent<Text>().text = left + " to " + right;
                    }
                }
            }

            //Add to list
            newCharacteristic.transform.SetParent(enemyIntelContent.transform);
        }

        //Get Select Design Panel
        GameObject selectDesign = Utils.GetChild(layout, "SelectDesign");

        //Clear Content
        Utils.ClearChildren(selectDesign);

        //Get List of Proposals
        Design[] proposals = Game.us.proposals[designChoiceType];

        //Instantiate Proposals
        for (int i = 0; i < proposals.Count(); i++)
        {
            //Instantiate
            GameObject proposal = Instantiate(PROPOSAL);

            //Set Designer
            GameObject designer = Utils.GetChildRecursive(proposal, "Designer");
            designer.GetComponent<Text>().text = proposals[i].developer.name;

            //Set Name
            GameObject name = Utils.GetChildRecursive(proposal, "Name");
            name.GetComponent<Text>().text = proposals[i].name;

            //Set Estimate
            GameObject estimate = Utils.GetChildRecursive(proposal, "Estimate");
            int value = 0;
            for (int j = 0; j < proposals[i].characteristics.Count(); j++)
            {
                value += proposals[i].characteristics[j].predictedValue * (int)proposals[i].characteristics[j].importance;
            }
            estimate.GetComponent<Text>().text = value.ToString();

            //Set Choice Delegate
            ChoiceSelectDelegate(proposal.GetComponent<Toggle>(), i);

            //Add Toggle Group
            proposal.GetComponent<Toggle>().group = selectDesign.GetComponent<ToggleGroup>();

            //If first one then select it by default
            if (i == 0)
                ChoiceSelect(proposal.GetComponent<Toggle>(), 0);

            //Add to list
            proposal.transform.SetParent(selectDesign.transform);
        }

    }

    //Save Selected Choice
    public void SaveNewDesign()
    {
        //Get content of design selector
        GameObject content = Utils.GetChildRecursive(designSelectorPopup, "SelectDesign");

        //Find selected
        Toggle[] toggles = content.GetComponentsInChildren<Toggle>();
        int selectedId = -1;
        for (int i = 0; i < toggles.Count(); i++)
        {
            if (toggles[i].isOn)
            {
                selectedId = i;
            }
        }

        //Apply Selection to Game
        Game.us.designs[designChoiceType] = Game.us.proposals[designChoiceType][selectedId];

        //Design Type to Name
        string designType = designChoiceType;
        designType = string.Concat(designType.Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');

        //Disable button in Monthly Report for this proposal
        GameObject monthlyReport = GameObject.Find("MonthNewsPanel");
        content = Utils.GetChildRecursive(monthlyReport, "Content");
        foreach (Transform item in content.transform)
        {
            //If not a button, ignore
            if (!item.GetComponent<Button>())
                continue;

            //If text has design type, disable
            Text text = item.gameObject.GetComponentInChildren<Text>();
            if (text.text.Contains(designType))
            {
                item.GetComponent<Button>().interactable = false;
            }
        }

        //Exit popup
        designSelectorPopup.SetActive(false);

        //Display the new design on Overview
        DisplayDesign(Game.us.designs[designChoiceType]);
    }

    //Choice Select Delegate (circumvents a problem with delegates reference on i variable)
    private void ChoiceSelectDelegate(Toggle toggle, int i)
    {
        toggle.onValueChanged.AddListener(delegate { ChoiceSelect(toggle, i); });
    }

    //Design Choice Selector
    public void ChoiceSelect(Toggle toggle, int id)
    {
        //Check toggle is on (with Toggle Group this is called on previous and new Toggle)
        if (toggle.isOn == false || !designSelectorPopup.active || !Game.us.proposals.ContainsKey(designChoiceType))
            return;

        //Get Design
        Design design = Game.us.proposals[designChoiceType][id];

        //Get Layout
        GameObject layout = Utils.GetChildRecursive(designSelectorPopup, "Layout");

        //Get SelectedInfo
        GameObject selectedInfo = Utils.GetChild(layout, "SelectedInfo");

        //Get Content
        GameObject content = Utils.GetChildRecursive(selectedInfo, "Content");

        //Clear Content
        Utils.ClearChildren(content);

        //Display Characteristics
        for (int i = 0; i < design.characteristics.Count(); i++)
        {
            //Instantiate
            GameObject newCharacteristic = Instantiate(CHARACTERISTIC_PLAYER_BRIEF);

            //Edit Instance values
            foreach (Transform child in newCharacteristic.transform)
            {
                //Set Icon (Icon is set using the Characteristic name and associating it with variable name in this class that stores Sprites)
                //UpperCase(characteristic.name) + "_ICON"
                if (child.name == "Icon")
                {
                    string name = design.characteristics[i].name;
                    name = name.ToUpper();
                    name += "_ICON";
                    name = name.Replace(" ", "_");
                    child.gameObject.GetComponent<Image>().sprite = (Sprite)typeof(GameHolder).GetField(name).GetValue(this);
                }
                //Name
                else if (child.name == "Name")
                {
                    child.gameObject.GetComponent<Text>().text = design.characteristics[i].name;
                }
                //Intel
                else if (child.name == "Estimate")
                {
                    string value = design.characteristics[i].predictedValue.ToString();
                    if (design.characteristics[i].predictedValue > 0)
                        value = "+" + value;
                    child.gameObject.GetComponent<Text>().text = value;
                }
            }

            //Add to list
            newCharacteristic.transform.SetParent(content.transform);
        }
    }
}