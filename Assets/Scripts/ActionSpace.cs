using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionSpace : MonoBehaviour
{
    public Unit parentUnit;
    public Tile currentTile;
    public Action action;

    public void Delete()
    {
        Destroy(this.gameObject);
    }
}
