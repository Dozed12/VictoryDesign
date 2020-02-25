using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public static class TooltipManager
{
    //Tooltips
    public static Dictionary<string, Func<string>> tooltips;

    //Tooltip Object
    public static GameObject tooltip;

    //Setup Tooltips
    public static void SetupTooltips()
    {
        //Setup Tooltip Object
        tooltip = GameObject.Find("Tooltip");

        //Reset dictionary
        tooltips = new Dictionary<string, Func<string>>();

        #region Tooltips

        //Design Hover
        tooltips.Add("DesignsList", delegate { return "List of design types and their progress until requiring a redesign.\nHover over each of them to highlight it.\nDesigns with a red circle are to be redesigned this month while\ngreen circles indicate they will need to be redesigned next month"; });
        tooltips.Add("OriginalChoice", delegate { return "Original approved proposal for highlighted design.\nExpected qualities of design, some flunctuation may occur"; });
        tooltips.Add("CurrentReport", delegate { return "Field report of highlighted design.\nMore detailed than the Proposal but\ninformation takes time to be collected"; });
        tooltips.Add("ModelHolder", delegate { return "Illustration of highlighted design"; });

        //Industry
        tooltips.Add("Industry", delegate { return "Industrial Capacities in relation to our Designs."; });
        tooltips.Add("EngineeringIcon", delegate { return "Engineering Capacity, the engineering complexity of our designs"; });
        tooltips.Add("ResourcesIcon", delegate { return "Resource Capacity, the resource cost of our designs"; });
        tooltips.Add("ReplenishmentIcon", delegate { return "Replenishment Capacity, the reliability of our designs"; });
        tooltips.Add("EngineeringSlider", delegate { return "Engineering Capacity, from 0% to 200%"; });
        tooltips.Add("ResourcesSlider", delegate { return "Resource Capacity, from 0% to 200%"; });
        tooltips.Add("ReplenishmentSlider", delegate { return "Replenishment Capacity, from 0% to 200%"; });

        //Military
        tooltips.Add("Doctrine", delegate { return "Military Capacities and Doctrine that uses them"; });
        tooltips.Add("AIIcon", delegate { return "Anti Infantry Capacity, the capacity to fight infantry"; });
        tooltips.Add("AAIcon", delegate { return "Anti Armor Capacity, the capacity to fight armored vehicles"; });
        tooltips.Add("BreakthroughIcon", delegate { return "Breakthrough Capacity, the capacity to breakthrough enemy defenses"; });
        tooltips.Add("ExploitationIcon", delegate { return "Exploitation Capacity, the capacity of flanking and exploiting enemy position"; });
        tooltips.Add("MoraleIcon", delegate { return "Army Morale, the morale of our armed forces that makes them keep fighitng"; });
        tooltips.Add("EfficiencyIcon", delegate { return "Army Efficiency, how efficient our army is in combat"; });
        tooltips.Add("AISlider", delegate { return "Anti Infantry Capacity, from 0% to 200%"; });
        tooltips.Add("AASlider", delegate { return "Anti Armor Capacity, from 0% to 200%"; });
        tooltips.Add("BreakthroughSlider", delegate { return "Breakthrough Capacity, from 0% to 200%"; });
        tooltips.Add("ExploitationSlider", delegate { return "Exploitation Capacity, from 0% to 200%"; });
        tooltips.Add("MoraleSlider", delegate { return "Army Morale, from 0% to 200%"; });
        tooltips.Add("EfficiencySlider", delegate { return "Army Efficiency, from 0% to 200%"; });

        //Doctrine
        tooltips.Add("AIDoctrine", delegate { return "Anti Infantry importance in Doctrine,\nthe higher the importance the more impact it has on our combat ability"; });
        tooltips.Add("AADoctrine", delegate { return "Anti Armor importance in Doctrine,\nthe higher the importance the more impact it has on our combat ability"; });
        tooltips.Add("BreakthroughDoctrine", delegate { return "Breakthrough importance in Doctrine,\nthe higher the importance the more impact it has on our combat ability"; });
        tooltips.Add("ExploitationDoctrine", delegate { return "Exploitation importance in Doctrine,\nthe higher the importance the more impact it has on our combat ability"; });
        tooltips.Add("MoraleDoctrine", delegate { return "Army Morale importance in Doctrine,\nthe higher the importance the more impact it has on our combat ability"; });
        tooltips.Add("EfficiencyDoctrine", delegate { return "Army Efficiency importance in Doctrine,\nthe higher the importance the more impact it has on our combat ability"; });

        //Control Menu
        tooltips.Add("Bulletin", delegate { return "Monthly Bulletin with key information"; });
        tooltips.Add("Date", delegate { return "Current date"; });
        tooltips.Add("Exit", delegate { return "Quit game and save progress.\nOnly available after completing this month actions"; });
        tooltips.Add("NextTurnText", delegate { return "Proceed to next month. (Space Bar)\nOnly available after completing this month actions"; });

        #endregion
    }

    //Process Tooltip over Gameobject
    public static float counter = 0;
    public static string name = "";
    public static Vector3 mousePos;
    public static void ProcessTooltip()
    {
        //Get pointer data
        CustomStandaloneInputModule module = (CustomStandaloneInputModule)EventSystem.current.currentInputModule;
        PointerEventData pointerData = module.GetPointerData();

        //PointerData may return null during first frame
        if (pointerData == null)
            return;

        //Check if there is a gameobject
        if (pointerData.pointerCurrentRaycast.gameObject != null)
        {
            //Get gameobjects hierarchy until found tooltip or null
            Transform target = pointerData.pointerCurrentRaycast.gameObject.transform;
            //Debug.Log(target.name);
            while (!tooltips.ContainsKey(target.name))
            {
                //Check parent not null
                if(target.parent == null)
                    break;

                //Check parent
                target = target.parent;

                //Debug.Log(target.name);
            }

            //New target or mouse move, reset counter
            if(target.name != name || mousePos != Input.mousePosition)
            {
                counter = 0;
                name = target.name;
                mousePos = Input.mousePosition;
            }
            //Same target, continue timer
            else
            {
                counter += Time.deltaTime;
            }

            //Check if gameobject has assigned tooltip
            if (tooltips.ContainsKey(target.name))
            {
                //Not enough time yet
                if(counter < 0.25f)
                    return;

                //Enable tooltip
                tooltip.SetActive(true);

                //Set text
                tooltip.GetComponentInChildren<Text>().text = tooltips[target.name]();

                //Set tooltip position
                RectTransform topTransform = target.gameObject.GetComponent<RectTransform>();
                RectTransform tooltipTransform = tooltip.GetComponent<RectTransform>();

                //Candidate position
                Vector3 pos = new Vector3(Input.mousePosition.x, Input.mousePosition.y - tooltipTransform.sizeDelta.y / 2 - 40, 0);

                //Check Canvas borders
                if(pos.x - tooltipTransform.sizeDelta.x / 2 < 0)
                    pos.x += 0 - (pos.x - tooltipTransform.sizeDelta.x / 2);
                if(pos.x + tooltipTransform.sizeDelta.x / 2 > Screen.width)
                    pos.x += Screen.width - (pos.x + tooltipTransform.sizeDelta.x / 2);

                //Apply position to tooltip
                tooltip.GetComponent<RectTransform>().SetPositionAndRotation(pos, Quaternion.identity);

                //Fix size of tooltip
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)tooltip.transform);
            }
            else
            {
                //Non found, disable tooltip
                tooltip.SetActive(false);
            }
        }
        else
        {
            //Non found, disable tooltip
            tooltip.SetActive(false);
        }
    }
}
