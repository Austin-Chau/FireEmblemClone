using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Unit
{
    protected override void Start()
    {
        base.Start();
        team = "enemy";
        _GameManager.instance.AddEnemyToList(this);
    }
    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    protected override void StartActPhase()
    {
        base.StartActPhase();
        //pop up the menu of actions, have the player select one, unless they are an npc
        //npcs should automatically select an action
        EndActPhase();
    }

    //Called by the gameManager to tell this guy to move, get closer to the player
    public void Move()
    {
        Vector2 playerPosition = _GameManager.instance.playerPosition;
        bool right = playerPosition.x > transform.position.x;
        bool up = playerPosition.y > transform.position.y;

        //kinda scuffed right now, 
        //needs better distance checking to player and smart determination of the closest path
        float xDist = Mathf.Abs(playerPosition.x - transform.position.x);
        float yDist = Mathf.Abs(playerPosition.y - transform.position.y);

        float x = Mathf.Min(moveRadius,Mathf.Max(0,xDist-1));
        float y = Mathf.Min(moveRadius,Mathf.Max(0,yDist - 1));

        x = right ? transform.position.x + x : transform.position.x - x;
        y = up ? transform.position.y + y: transform.position.y - y;
        Vector3 end = new Vector3(x, y, 0);
        MetaMove(end);
    }
}
