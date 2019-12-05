using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player
{
    public List<Unit> childUnit;
    public void AddUnit(Tile spawnTile, Unit babyBoy)
    {
        babyBoy.CreateUnit(spawnTile, Team.Enemy);
        childUnit.Add(babyBoy);
    }
}
