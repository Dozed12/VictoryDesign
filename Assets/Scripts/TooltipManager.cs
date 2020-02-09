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

        //Add tooltip entries
        tooltips.Add("IndustryValue", IndustrialSpendingTooltip);
    }

    //Tooltip methods
    public static string IndustrialSpendingTooltip()
    {
        return "test";
    }

    //Process Tooltip over Gameobject
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
            //Get top gameobject
            GameObject top = pointerData.pointerCurrentRaycast.gameObject;

            //Debug.Log(top.name);

            //Check if gameobject has assigned tooltip
            if (tooltips.ContainsKey(top.name))
            {
                //Enable tooltip
                tooltip.SetActive(true);

                //Set text
                tooltip.GetComponentInChildren<Text>().text = tooltips[top.name]();

                //Set tooltip position
                RectTransform topTransform = top.GetComponent<RectTransform>();
                RectTransform tooltipTransform = tooltip.GetComponent<RectTransform>();
                tooltip.GetComponent<RectTransform>().SetPositionAndRotation(new Vector3(Input.mousePosition.x,
                Input.mousePosition.y - tooltipTransform.sizeDelta.y/2 - 30, 0), Quaternion.identity);

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
