using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class TestScript : MonoBehaviour
{
    HoldingsManager hm;
    public Text text;
    int counter = 0;

    // Start is called before the first frame update
    void Start()
    {
        text.text = Application.persistentDataPath;
    }

    // Update is called once per frame
    void Update()
    {
        if (++counter == 500)
        {
            hm = new HoldingsManager();
            Holding holding = hm.GetHoldingAt("11B", DateTime.Now);

            hm.AddNewUserHolding(holding, 20);
        }
        else
        {
            if (hm != null)
            {
                text.text = Application.persistentDataPath + " " + hm.GetUserHoldings().Count;
            }
            else
            {
                text.text = Application.persistentDataPath + " HLO! " + counter;
            }
        }
    }
}
