using System;
using System.Collections.Generic;
using UnityEngine;

public class ActionSpace : MonoBehaviour
{
    public Unit parentUnit;
    public Tile currentTile;
    public ActionNames action;

    public void Delete()
    {
        Destroy(this.gameObject);
    }

}
