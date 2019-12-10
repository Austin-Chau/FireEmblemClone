using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cursor : MonoBehaviour
{
    private _GameManager gameManager;

    public float moveTime = 0.1f;
    private float inverseMoveTime;

    private bool moving;

    public Controller PlayerController;
    public Tile currentTile { get; private set; }
    public Unit selectedUnit;
    private const Team team = Team.Player;
    public Dictionary<Tile, ActionSpace> actionSpaces = new Dictionary<Tile, ActionSpace>();

    private void Start()
    {
        inverseMoveTime = 1 / moveTime;
        gameManager = _GameManager.instance;
        gameManager.Cursor = this;
        PlayerController = gameManager.PlayerController;
        currentTile = gameManager.Board.Tiles[0, 0]; //whatever initial position
        transform.position = currentTile.Position; //snap to that tile
    }

    // Update is called once per frame
    void Update()
    {
        if (!PlayerController.MyTurn)
        {
            return;
        }
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
                Tile tile = currentTile.GetAdjacentTile(horizontal == AdjacentDirection.None ? vertical : horizontal);
                if (tile != null)
                    StartCoroutine(SmoothMovement(tile)); //Traverse horizontally, then vertically
            }
            //Selected unit traps the cursor into controlling a specific unit.
            else if (Input.GetButtonDown("confirm") && PlayerController.Current.Item1 == selectedUnit && PlayerController.Current.Item2 == Action.Attack)
            { //just skip the act phase of the select unit
                selectedUnit.EndActPhase();
                selectedUnit = null;
            }
            else if (Input.GetButtonDown("confirm"))
            {
                //Behavior: priotize any actionspace on the tile over the unit on the tile, and if the unit can't do anything, then perform any move actionspace
                if (actionSpaces.ContainsKey(currentTile) && actionSpaces[currentTile] != null && actionSpaces[currentTile].action != Action.Move)
                {
                    actionSpaces[currentTile].Activate();
                    //actionSpaces.Clear();
                }
                else if (currentTile.CurrentUnit != null && selectedUnit == null && !currentTile.CurrentUnit.moved)
                {
                    selectedUnit = currentTile.CurrentUnit;
                    if (PlayerController.unitsEnum.MoveToUnit(selectedUnit)) //player controls this unit -> all systems go
                    {
                        PlayerController.unitsEnum.MoveToPhase(Action.Move); //set the controller to be in movement mode for the current unit
                    }
                    actionSpaces = selectedUnit.GenerateMoveSpaces(selectedUnit.team == Team.Player);
                }
                else if (actionSpaces.ContainsKey(currentTile) && actionSpaces[currentTile] != null && actionSpaces[currentTile].action == Action.Move)
                {
                    Debug.Log("tryin to activate move space");
                    actionSpaces[currentTile].Activate();
                    //actionSpaces.Clear();
                }
            }
            else if (Input.GetButtonDown("reverse"))
            {
                if (selectedUnit != null)
                {
                    selectedUnit.controller.RevertMaybeTeleport(selectedUnit, selectedUnit.team == Team.Player);
                    selectedUnit = null;
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
