using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Controller
{
    public Enemy (Team _team, ControllerBehavior _behavior) : base(_team)
    {
        behavior = _behavior;
    }

    //For reference
    /*
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
    */
}
