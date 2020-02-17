using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public static class History
{
    public class Event
    {
        public int turn;
        public string message;
        public Action<int> action;
        public int identification;

        public Event(int turn, string message, Action<int> action, int identification)
        {
            this.turn = turn;
            this.message = message;
            this.action = action;
            this.identification = identification;
        }
    }

    public static List<Event> events;

    public static Game game;

    //Setup History Turns
    public static void SetupHistory()
    {
        //Get Game Holder
        game = GameObject.Find("Game").GetComponent<Game>();

        //Clear list
        events = new List<Event>();

        //Initial Doctrine
        events.Add(new Event(3, "The Military Staff has produced the initial Doctrine for our armed forces. You will be allowed to perform singular alterations every 6 months, starting next June.", game.InitialDoctrine, -1));

        //Generate Unification Turn
        events.Add(new Event(6 + UnityEngine.Random.Range(0, 1 + 1), "UNIFICATION 1", Map.ProgressUnification, 0));
        events.Add(new Event(8 + UnityEngine.Random.Range(0, 1 + 1), "UNIFICATION 2", Map.ProgressUnification, 1));

        //Generate Allies Turn
        events.Add(new Event(10 + UnityEngine.Random.Range(0, 1 + 1), "ALLY 1", Map.ProgressAllies, 0));
        events.Add(new Event(12 + UnityEngine.Random.Range(0, 1 + 1), "ALLY 2", Map.ProgressAllies, 1));
        events.Add(new Event(14 + UnityEngine.Random.Range(0, 1 + 1), "ALLY 3", Map.ProgressAllies, 2));

        //Generate Revenge Turns
        events.Add(new Event(16 + UnityEngine.Random.Range(0, 1 + 1), "REVENGE START", null, -1));
        events.Add(new Event(19 + UnityEngine.Random.Range(-1, 1 + 1), "REVENGE END", Map.ProgressRevenge, 0));

        //Generate War Turn
        //TODO Actually start war
        events.Add(new Event(22 + UnityEngine.Random.Range(-1, 1 + 1), "WAR", null, -1));
    }

    //Generate possible messages for Bulletin
    public static List<string> Bulletin()
    {
        List<string> result = new List<string>();

        //If 4 month multiple then include armed forces capacity/doctrine report
        if (Game.turn % 4 == 0)
        {
            //Preffix
            string report = "Armed Forces Report:";

            //Calculations
            float industry = game.IndustryValue();
            float effectiveIndustry = game.EffectiveIndustryValue();
            float capacity = game.CapacityValue();
            float capacityDoctrine = game.CapacityValueDoctrine();
            float final = game.FinalCalculation();

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
        for (int i = 0; i < events.Count; i++)
        {
            if (Game.turn == events[i].turn)
            {
                //Add to bulletin
                result.Add(events[i].message);

                //Apply Effect
                if (events[i].action != null)
                    events[i].action(events[i].identification);
            }
        }

        return result;
    }

}
