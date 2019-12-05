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

    protected override void StartActPhase()
    {
        base.StartActPhase();
        //pop up the menu of actions, have the player select one, unless they are an npc
        //npcs should automatically select an action
    }

}
