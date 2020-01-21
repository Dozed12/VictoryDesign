using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CustomStandaloneInputModule : StandaloneInputModule
{
    public PointerEventData GetPointerData()
    {
        if(m_PointerData.ContainsKey(kMouseLeftId))
            return m_PointerData[kMouseLeftId];
        else 
            return null;
    }
}