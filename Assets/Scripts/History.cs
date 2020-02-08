using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class History
{
    //Turns of Proxy Wars
    public static int proxy1Turn;
    public static int proxy2Turn;
    public static int proxy3Turn;

    //Turn of Enemy Allies 
    public static int ally1Turn;
    public static int ally2Turn;
    public static int ally3Turn;

    //Turn of War
    public static int warTurn;

    public static void SetupHistory()
    {
        //Generate Proxy Turn
        proxy1Turn = 6 + UnityEngine.Random.Range(-1,1 + 1);
        proxy2Turn = 8 + UnityEngine.Random.Range(-1,1 + 1);
        proxy3Turn = 16 + UnityEngine.Random.Range(-1,1 + 1);

        //Generate Allies Turn
        ally1Turn = 10 + UnityEngine.Random.Range(-1,1 + 1);
        ally2Turn = 12 + UnityEngine.Random.Range(-1,1 + 1);
        ally3Turn = 14 + UnityEngine.Random.Range(-1,1 + 1);

        //Generate War Turn
        warTurn = 20 + UnityEngine.Random.Range(-1,1 + 1);
    }

}
