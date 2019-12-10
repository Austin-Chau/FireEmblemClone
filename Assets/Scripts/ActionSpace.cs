using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionSpace : MonoBehaviour
{
    public Unit parentUnit;
    public bool Active;
    public Tile currentTile;
    public Action action;

    public void Delete()
    {
        Destroy(this.gameObject);
    }

    /// <summary>
    /// Tells this space to activate. For now, only does something if the unit is a player controlled unit.
    /// </summary>
    public void Activate()
    {
        if (Active)
        {
            parentUnit.ParseAction(this);
        }
    }
}
