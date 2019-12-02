using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _Player : Unit
{
    protected override void Start()
    {
        base.Start();
        team = "player";
        coordinates = new Vector2(1, 1);
        rb2D.MovePosition(coordinates);
    }

    protected override void Update()
    {
        Vector3 position = _GameManager.instance.cursorPosition;
        _GameManager.instance.playerPosition = transform.position;
        if (unSelected && !moving && !acting)
        {
            //if the unit was unselected the last frame, checks the next update frame to see if they didn't
            //start moving or acting.
            EraseActSquares();
            EraseMoveSquares();
        }
        unSelected = false;
        //logic here probably needs complete overhaul
        if (Input.GetButtonDown("confirm") && _GameManager.instance.playersTurn && !(acted && moved))
        {
            //Temporary: if the player is in the acting phase, just end it immediately upon pressing z
            if (acting)
            {
                EndActPhase();
                return;
            }
            //otherwise, if the player's tile is selected
            if (Mathf.Abs(transform.position.x - position.x) < .5 && Mathf.Abs(transform.position.y - position.y) < .5)
            {
                //has not moved yet, selection automatically assumes the player wants to move them
                selected = true;
                if (!moved && moveSpaces.Count == 0)
                {
                    EraseActSquares(); //just to make sure
                    DrawMoveSquares();
                }
                else if (!acted)
                {
                    //act here, but this is probably defunct since moving should force the player into the action
                    //menu, grabbing control from the cursor
                }
            }
            //otherwise, if the player has been selected, but now another tile has been clicked on
            else if (selected)
            {
                selected = false;
                unSelected = true;
            }
        }
        if (acted && moved)
        {
            _GameManager.instance.playersTurn = false;
            acted = false;
            moved = false;
        }
    }

    //like the unit's drawmovesquares, but adds extra scripting capabilities since these squares are interactable
    protected override void DrawMoveSquares()
    {
        moveSpaces = new List<ActionSpace>();
        for (int i = -moveRadius; i <= moveRadius; i++)
        {
            for (int j = -(moveRadius - Mathf.Abs(i)); j <= moveRadius - Mathf.Abs(i); j++)
            {
                Vector3 end = new Vector3(transform.position.x, transform.position.y, 0) + new Vector3(i, j, 0);
                ActionSpace moveSpaceScript = Instantiate(MoveSpace, end, Quaternion.identity).GetComponent<ActionSpace>();
                moveSpaces.Add(moveSpaceScript);
                moveSpaceScript.collidersIndex = moveSpaces.Count - 1;
                moveSpaceScript.unitScript = this;
                moveSpaceScript.action = "move";
            }
        }
    }

    protected override void StartActPhase()
    {
        base.StartActPhase();
        //pop up the menu of actions, have the player select one, unless they are an npc
        //npcs should automatically select an action
    }

    //runs a collision check on all nearby tiles to see if there is anything actionable
    //later, implement layers for each action (heal, attack, etc)
    protected override void DrawActSquares()
    {
        actSpaces = new List<ActionSpace>();
        for (int i = -actRadius; i <= actRadius; i++)
        {
            for (int j = -(actRadius - Mathf.Abs(i)); j <= actRadius - Mathf.Abs(i); j++)
            {
                Vector2 end = new Vector2(transform.position.x, transform.position.y) + new Vector2(i, j);
                boxCollider.enabled = false;
                RaycastHit2D hit = Physics2D.Linecast(end + new Vector2(0.1f, 0),end,actingLayer);
                boxCollider.enabled = true;
                if (hit.transform != null)
                {
                    ActionSpace actSpaceScript = Instantiate(ActSpace, end, Quaternion.identity).GetComponent<ActionSpace>();
                    actSpaces.Add(actSpaceScript);
                    actSpaceScript.collidersIndex = actSpaces.Count - 1;
                    actSpaceScript.unitScript = this;
                    actSpaceScript.action = "act";
                }
            }
        }
    }
}
