using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{

    #region Public Variables
    public LayerMask blockingLayer;
    public LayerMask actingLayer;

    public GameObject MoveSpace;
    public GameObject ActSpace;

    public float moveTime = 0.1f;

    public bool moving = false;
    public bool moved = false;
    public bool acting = false;
    public bool acted = false;

    public int actRadius = 1;
    public int moveRadius = 3;

    public Tile currentTile { get; private set; }
    public Tile pastTile { get; private set; }

    public Team team { get; private set; }
    public Controller controller { get; private set; }
    #endregion

    #region Private variables

    private Animator animator;
    private Rigidbody2D rb2D;

    private float inverseMoveTime;

    private Dictionary<Tile, ActionSpace> actionSpaces = new Dictionary<Tile, ActionSpace>();

    private Dictionary<Tile, int> moveTree;

    #endregion

    public void Start()
    {
        animator = GetComponent<Animator>();
        rb2D = GetComponent<Rigidbody2D>();
        inverseMoveTime = 1f / moveTime;
    }

    /// <summary>
    /// Sets all of the passed parameters while also moving the unit to the position of its tile.
    /// </summary>
    /// <param name="_spawnTile">The tile the unit should be on.</param>
    /// <param name="_team">What team the unit is on.</param>
    /// <param name="_controller">The controller that controls this unit.</param>
    public void InitializeUnit(Tile _spawnTile, Team _team, Controller _controller)
    {
        team = _team;
        controller = _controller;
        currentTile = _spawnTile;
        transform.position = currentTile.Position;
        currentTile.CurrentUnit = this;
        ResetStates();
    }
    /// <summary>
    /// Initializes the various state checking variables of this unit.
    /// </summary>
    public void ResetStates()
    {
        moving = false;
        moved = false;
        acting = false;
        acted = false;
        EraseSpaces();
    }
    /// <summary>
    /// Teleports a unit to a certain tile. Warning: erases pastTile information, this is meant as an absolute movement.
    /// </summary>
    /// <param name="destinationTile"></param>
    public virtual void Teleport(Tile destinationTile)
    {
        currentTile.CurrentUnit = null;
        currentTile = destinationTile;
        transform.position = currentTile.Position;
        currentTile.CurrentUnit = this;
        pastTile = currentTile;
    }

    
    /// <summary>
    /// Gets the path of the unit using the starting and destination tile, then collapses it down into just the vertices.
    /// </summary>
    /// <param name="destinationTile">The destination tile.</param>
    public virtual void MetaMove(Tile destinationTile)
    {
        moveTree = Pathfinding.GenerateMoveTree(currentTile, moveRadius);
        Stack<Tile> steps = Pathfinding.GenerateSteps(currentTile, destinationTile, moveTree);

        //Now, given a list of unit vectors, 
        //combine consecutive vectors in the same direction to create smooth movements.
        List<Tile> stepsVertices = new List<Tile>();
        if (steps.Count > 1)
        {
            Tile startingTile = steps.Pop();
            Tile firstTile = steps.Pop();
            Tile secondTile;
            AdjacentDirection baseDirection = Pathfinding.GetAdjacentTilesDirection(startingTile, firstTile); //this is the direction we check further directions against

            if (steps.Count > 0)
            {
                //There is more than one tile in the path, the unit is actually moving
                do
                {
                    secondTile = steps.Pop();
                    AdjacentDirection newDirection = Pathfinding.GetAdjacentTilesDirection(firstTile, secondTile);
                    //if newDirection = none, panic

                    if (newDirection != baseDirection)
                    {
                        //If this nextTile bends out of the way, the middleTile is a vertex
                        stepsVertices.Add(firstTile);
                        baseDirection = newDirection; //the new bent direction is now what we compare to
                    }
                    //Now, step along, the newest tile becomes the first tile
                    firstTile = secondTile;
                }
                while (steps.Count > 0);

                stepsVertices.Add(secondTile);
            }
        }
        else
        {
            stepsVertices.Add(steps.Pop());
        }

        StartCoroutine(SequenceOfMoves(stepsVertices));

    }

    /// <summary>
    /// The steps can be any list of tiles, the unit will move to each of them in turn.
    /// </summary>
    /// <param name="steps">The sequence of tile steps.</param>
    /// <returns>A coroutine for every step.</returns>
    private IEnumerator SequenceOfMoves(List<Tile> steps)
    {
        moving = true;
        pastTile = currentTile;
        currentTile.CurrentUnit = null;
        foreach (Tile step in steps)
        {
            yield return StartCoroutine(SmoothMovement(step));
        }
        moving = false;
        moved = true;
        currentTile.CurrentUnit = this;
        controller.StepTurn();
    }

    /// <summary>
    /// Smooth movement to a single given tile, spread over multiple frames. Also sets the currentTile.
    /// </summary>
    /// <param name="destinationTile">The destination tile.</param>
    /// <returns>null</returns>
    private IEnumerator SmoothMovement(Tile destinationTile)
    {
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
    }

    /// <summary>
    /// Generates the move spaces.
    /// </summary>
    /// <param name="_cosmetic">Whether to render the squares inert (ie, if they are an enemy's)</param>
    public Dictionary<Tile, ActionSpace> GenerateMoveSpaces(bool _cosmetic)
    {
        Dictionary<Tile, ActionSpace> spaces = new Dictionary<Tile, ActionSpace>();

        moveTree = Pathfinding.GenerateMoveTree(currentTile, moveRadius); //add checks for if this changes between drawing squares and metamove
        foreach (KeyValuePair<Tile,int> pair in moveTree)
        {
            Vector3 position = pair.Key.Position;
            ActionSpace moveSpaceScript = Instantiate(MoveSpace, position, Quaternion.identity).GetComponent<ActionSpace>();
            moveSpaceScript.parentUnit = this;
            moveSpaceScript.currentTile = pair.Key;
            moveSpaceScript.action = Action.Move;
            moveSpaceScript.Active = _cosmetic;
            spaces[pair.Key] = moveSpaceScript;
        }

        actionSpaces = spaces;
        _GameManager.instance.Cursor.actionSpaces = spaces;
        return spaces;
    }

    public Dictionary<Tile, ActionSpace> GenerateActSpaces()
    {
        Dictionary<Tile, ActionSpace> spaces = new Dictionary<Tile, ActionSpace>();

        actionSpaces = spaces;
        _GameManager.instance.Cursor.actionSpaces = spaces;
        return spaces;
    }

    //Erases the move squares around the unit
    public void EraseSpaces()
    {
        foreach (KeyValuePair<Tile,ActionSpace> pair in actionSpaces)
        {
            pair.Value.Delete();
        }
        actionSpaces.Clear();
    }

    public void StartActPhase()
    {
        Debug.Log("starting act phase");
        acting = true;
        EraseSpaces();
        GenerateActSpaces();
        //pop up the menu of actions, have the player select one, unless they are an npc
        //npcs should automatically select an action
    }

    public void EndActPhase()
    {
        Debug.Log("ending act phase");
        EraseSpaces();
        acting = false;
        acted = true;
        if (!controller.RetireUnit(this))
        {
            controller.StepTurn();
        }
    }

    private void TakeDamage()
    {
        animator.SetTrigger("playerHit");
    }

    public void ParseAction(ActionSpace space)
    {
        if (space.action == Action.Move)
        {
            Debug.Log("Unit moving");
            Debug.Log(space.currentTile.Position);
            MetaMove(space.currentTile);
        }
    }
}
