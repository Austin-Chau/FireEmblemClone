using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUI : MonoBehaviour
{
    public Unit player;
    public GameObject actText;

    // Update is called once per frame
    void Update()
    {
        if (player.acting)
        {
            actText.GetComponent<Text>().enabled = true;
        }
        else
        {
            actText.GetComponent<Text>().enabled = false;
        }
    }
}
