using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public static class History
{
    public class HistoryEvent
    {
        public int turn;
        public string message;
        public Action<int> action;
        public int identification;

        public HistoryEvent(int turn, string message, Action<int> action, int identification)
        {
            this.turn = turn;
            this.message = message;
            this.action = action;
            this.identification = identification;
        }
    }

    public static List<HistoryEvent> events;

    //Setup History Turns
    public static void SetupHistory()
    {
        //Clear list
        events = new List<HistoryEvent>();

        //TODO Do proper connection to Map class calls

        //Generate Unification Turn
        events.Add(new HistoryEvent(6 + UnityEngine.Random.Range(-1,1 + 1), "UNIFICATION 1", Map.ProgressWar, 1));
        events.Add(new HistoryEvent(8 + UnityEngine.Random.Range(-1,1 + 1), "UNIFICATION 2", Map.ProgressWar, 1));

        //Generate Revenge Turns
        events.Add(new HistoryEvent(16 + UnityEngine.Random.Range(-1,1 + 1), "REVENGE START", null, -1));
        events.Add(new HistoryEvent(19 + UnityEngine.Random.Range(-1,1 + 1), "REVENGE END", Map.ProgressWar, 1));

        //Generate Allies Turn
        events.Add(new HistoryEvent(10 + UnityEngine.Random.Range(-1,1 + 1), "ALLY 1", Map.ProgressWar, 1));
        events.Add(new HistoryEvent(12 + UnityEngine.Random.Range(-1,1 + 1), "ALLY 2", Map.ProgressWar, 1));
        events.Add(new HistoryEvent(14 + UnityEngine.Random.Range(-1,1 + 1), "ALLY 3", Map.ProgressWar, 1));

        //Generate War Turn
        events.Add(new HistoryEvent(10 + UnityEngine.Random.Range(-1,1 + 1), "WAR", null, -1));
    }

    //Generate possible messages for Bulletin
    public static List<string> Bulletin()
    {
        List<string> result = new List<string>();

        //If 3 month then include armed forces capacity/doctrine report
        if(Game.turn % 3 == 0)
        {
            //TODO Report
        }

        //Check if history event
        for (int i = 0; i < events.Count; i++)
        {
            if(Game.turn == events[i].turn)
            {
                //Add to bulletin
                result.Add(events[i].message);

                //Apply Effect
                events[i].action(events[i].identification);
            }
        }

        return result;
    }

}
