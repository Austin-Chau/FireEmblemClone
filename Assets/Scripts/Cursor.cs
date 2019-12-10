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
                    Tile tile = currentTile.GetAdjacentTile(horizontal == AdjacentDirection.None ? vertical : horizontal);
                    if (tile != null)
                        StartCoroutine(SmoothMovement(tile)); //Traverse horizontally, then vertically
                }
                else
                {
                    //do special movement (hop to only action spaces, move through menu, etc)
                }
            }
            /*
            After getting movement out of the way, the general workflow is as follows (upon pressing Z):
                -Check if the cursor has a currently selected unit AND the unit isn't currently doing anything.
                    --If it is a player unit AND if the current tile is an action space:
                        ---perform that action space.
                            Lock the camera if it was a mmovement tile, the player is now forced to finish
                            that unit's turn (or may press X to revert it, see the branch with reverse).
                    --Else, deselect the unit and possibly select a unit underneath (spawning its movespaces).
                        Note, in the case where the currentTile has the selectedUnit:
                        The cursor should be locked to non-movement action spaces (to force the player to take action),
                        so there shouldn't be a case where the player selects its selectedUnit with no action space
                        (in general, unless no actions are possible currently i just have it so pressing z
                        after moving automatically skips the rest of the turn).
                -Else, if we have no selected unit, check if the current tile has a unit.
                    --If it is spent, ignore it
                    --Else, it really should not have moved (since units must perform all their actions at once), but we shall check that too
                        ---If it hasn't moved, generate its movespaces and select it
             Pressing X, the reverse button, checks if the unit isn't currently performing some parallel action (like moving), and if it passes,
             we revert the unit to before being selected (for now, it just assumes that means the unit is fresh, later on we might need more comprehensive
             history tracking, it would be frustrating to have everything you did on a unit revert (like items));
             */
            else if (Input.GetButtonDown("confirm"))
            {

                //TEMPORARY:
                if (PlayerController.Current.Item1 == selectedUnit && PlayerController.Current.Item2 == Action.Attack)
                { //just skip the attack phase of the player controlled unit
                    Debug.Log("Temporary automatic performance of attack upon pressing Z");
                    selectedUnit.Attack(currentTile);
                    selectedUnit = null;
                    locked = false;
                }

                if (selectedUnit != null && !selectedUnit.IsActive())
                {
                    if (selectedUnit.team == Team.Player && actionSpaces.ContainsKey(currentTile))
                    {
                        Debug.Log("Selected unit is player, action space on tile");
                        if (actionSpaces[currentTile].action == Action.Move)
                        {
                            locked = true;
                        }
                        actionSpaces[currentTile].Activate();
                    }
                    else {
                        selectedUnit.controller.RevertMaybeTeleport(selectedUnit, selectedUnit.team == Team.Player);
                        if (currentTile.CurrentUnit != null && !currentTile.CurrentUnit.Spent)
                        {
                            Debug.Log("Selected unit is not player or no action space on tile, and the current tile has a selectable unit");

                            selectedUnit = currentTile.CurrentUnit;
                            if (PlayerController.unitsEnum.MoveToUnit(selectedUnit)) //player controls this unit -> all systems go
                            {
                                PlayerController.unitsEnum.MoveToPhase(Action.Move); //set the controller to be in movement mode for the current unit
                            }
                            actionSpaces = selectedUnit.GenerateMoveSpaces(selectedUnit.team != Team.Player);
                        }
                        else
                        {
                            Debug.Log("No action space, no unit to select, no god");
                            selectedUnit = null;
                        }
                    }
                }
                else if (selectedUnit == null)
                {
                    if (currentTile.CurrentUnit != null && !currentTile.CurrentUnit.Spent)
                    {
                        Debug.Log("Selected unit is null, unit on the tile, the unit is not spent");
                        selectedUnit = currentTile.CurrentUnit;
                        if (PlayerController.unitsEnum.MoveToUnit(selectedUnit)) //player controls this unit -> all systems go
                        {
                            PlayerController.unitsEnum.MoveToPhase(Action.Move); //set the controller to be in movement mode for the current unit
                        }
                        actionSpaces = selectedUnit.GenerateMoveSpaces(selectedUnit.team != Team.Player);
                    }
                }
            }
            else if (Input.GetButtonDown("reverse"))
            {
                if (selectedUnit != null && !selectedUnit.IsActive())
                {
                    selectedUnit.controller.RevertMaybeTeleport(selectedUnit, selectedUnit.team == Team.Player);
                    selectedUnit = null;
                    locked = false;
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
