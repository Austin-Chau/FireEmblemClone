using System;
using System.Collections.Generic;
using UnityEngine;

public class ActionSpace : MonoBehaviour
{
    public Unit parentUnit;
    public Tile currentTile;
    public CommandNames command;

    public void Delete()
    {
        Destroy(this.gameObject);
    }

    public void Hide()
    {
        gameObject.GetComponent<Renderer>().enabled = false;
    }

}
