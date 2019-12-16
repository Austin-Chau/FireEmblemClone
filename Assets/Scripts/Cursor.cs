using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cursor : MonoBehaviour
{
    private _GameManager gameManager;

    public float moveTime = 0.1f;
    private float inverseMoveTime;

    private bool moving;
    private bool locked;

    public Commander CurrentCommander;
    public Tile currentTile { get; private set; }

    private void Start()
    {
        inverseMoveTime = 1 / moveTime;
        gameManager = _GameManager.instance;
        gameManager.Cursor = this;
        currentTile = gameManager.Board.Tiles[0, 0]; //whatever initial position
        transform.position = currentTile.Position; //snap to that tile
    }

    // Update is called once per frame
    void Update()
    {
        if (!CurrentCommander.MyTurn)
        {
            return; //something went wrong, but we don't want to go around controlling their units when it isn't their turn
        }
        else if (!moving)
        {
            float x = Input.GetAxisRaw("Horizontal") * Time.deltaTime;
            float y = Input.GetAxisRaw("Vertical") * Time.deltaTime;

            ControlsEnum pressedInput = ControlsEnum.Null;
            if (Input.GetButtonDown("confirm"))
            {
                pressedInput = ControlsEnum.Confirm;
            }
            else if (Input.GetButtonDown("reverse"))
            {
                pressedInput = ControlsEnum.Reverse;
            }

            if (Mathf.Abs(x) > Mathf.Epsilon || Mathf.Abs(y) > Mathf.Epsilon)
            {
                if (!locked) //If the camera isn't locked during unit's action phase, Do normal movement
                {
                    AdjacentDirection horizontal;
                    AdjacentDirection vertical;
                    if (Mathf.Abs(x) <= Mathf.Epsilon)
                    {
                        horizontal = AdjacentDirection.None;
                    }
                    else if (x > 0)
                    {
                        horizontal = AdjacentDirection.Right;
                    }
                    else
                    {
                        horizontal = AdjacentDirection.Left;
                    }
                    if (Mathf.Abs(y) <= Mathf.Epsilon)
                    {
                        vertical = AdjacentDirection.None;
                    }
                    else if (y > 0)
                    {
                        vertical = AdjacentDirection.Up;
                    }
                    else
                    {
                        vertical = AdjacentDirection.Down;
                    }
                    Tile tile = currentTile.GetAdjacentTile(horizontal == AdjacentDirection.None ? vertical : horizontal); //if the cursor isn't moving horizontally, move vertically
                    if (tile != null)
                        StartCoroutine(SmoothMovement(tile)); //Traverse horizontally, then vertically
                }
                else
                {
                    //do special movement (hop to only action spaces, move through menu, etc)
                }
            }
            else if (pressedInput == ControlsEnum.Null)
            {
                CursorContext context = new CursorContext(currentTile, CurrentCommander, currentTile.CurrentUnit, pressedInput);
                CurrentCommander.ParseCursorOutput(context);
            }
        }
        gameManager.cursorPosition = transform.position;
    }

    private IEnumerator SmoothMovement(Tile destinationTile)
    {
        moving = true;
        float sqrRemainingDistance = (transform.position - destinationTile.Position).sqrMagnitude;
        while (sqrRemainingDistance > float.Epsilon)
        {
            Vector3 newPosition = Vector3.MoveTowards(transform.position, destinationTile.Position, inverseMoveTime * Time.deltaTime);
            //rb2D.MovePosition(newPosition); (we might want rigid body for smooth movement)
            transform.position = newPosition;
            sqrRemainingDistance = (transform.position - destinationTile.Position).sqrMagnitude;
            yield return null;
        }
        currentTile = destinationTile;
        transform.position = currentTile.Position; //snap the position just in case the unit is slightly off
        moving = false;
    }
}
