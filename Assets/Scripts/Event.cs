using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[Serializable]
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