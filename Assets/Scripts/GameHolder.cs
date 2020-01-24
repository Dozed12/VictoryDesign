using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GameHolder : MonoBehaviour
{
    //UI State Variables
    public string designChoiceType;
    public Design currentDisplayDesign;

    //Map
    public Texture2D baseMap;
    public GameObject mapHolder;

    //Tooltip
    public GameObject tooltip;

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
        GameObject.Find("Proposition").GetComponent<Animator>().SetBool("open", true);
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

        //Create Institutes
        AddInstitutes(new Type[] { typeof(Rifle) }, 3);

        //Create Base Designs
        designs.Add("Rifle", (Rifle)RequestDesign(typeof(Rifle))[0]);

        //Adjust Designs to Fit criteria
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

    //Highlight Hovered
    public void HoverDesign(string type)
    {
        //TODO Display Desired Info
    }

    //Stop highlight hovered
    public void DeHoverDesign()
    {
        //TODO Clear Info

        //TODO if in process of new design default back to highlight that design type (can call like HoverDesign(changing))
    }

}