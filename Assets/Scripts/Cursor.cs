using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cursor : MonoBehaviour
{
    private _GameManager gameManager;

    public float moveTime = 0.1f;
    private float inverseMoveTime;

    private bool moving;

    private Tile currentTile;

    private Dictionary<Tile, ActionSpace> actionSpaces;

    private void Start()
    {
        inverseMoveTime = 1 / moveTime;
        gameManager = _GameManager.instance;
        gameManager.Cursor = this;
        currentTile = gameManager.board.Tiles[0, 0]; //whatever initial position
        transform.position = currentTile.Position; //snap to that tile
    }

    // Update is called once per frame
    void Update()
    {
        if (!moving)
        {
            float x = Input.GetAxisRaw("Horizontal") * Time.deltaTime;
            float y = Input.GetAxisRaw("Vertical") * Time.deltaTime;
            if (Mathf.Abs(x) > Mathf.Epsilon || Mathf.Abs(y) > Mathf.Epsilon)
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

                StartCoroutine(SmoothMovement(currentTile.GetAdjacentTile(horizontal).GetAdjacentTile(vertical))); //Traverse horizontally, then vertically
            }
            else if (Input.GetButtonDown("confirm"))
            {
                //Behavior: priotize any actionspace on the tile over the unit on the tile, and if the unit can't do anything, then perform any move actionspace
                if (actionSpaces[currentTile] != null && actionSpaces[currentTile].action != Action.Move)
                {
                    actionSpaces[currentTile].parentUnit.ParseAction(actionSpaces[currentTile]);
                    actionSpaces.Clear();
                }
                if (currentTile.CurrentUnit != null && !currentTile.CurrentUnit.moved)
                {
                    actionSpaces = currentTile.CurrentUnit.GenerateMoveSpaces();
                }
                if (actionSpaces[currentTile] != null && actionSpaces[currentTile].action == Action.Move)
                {
                    actionSpaces[currentTile].parentUnit.ParseAction(actionSpaces[currentTile]);
                    actionSpaces.Clear();
                }
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
