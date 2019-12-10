using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Unit : MonoBehaviour
{

    #region Public Variables
    public GameObject MoveSpace;
    public GameObject ActSpace;

    public float moveTime = 0.1f;
    public int actRadius = 1;
    public int moveRadius = 3;

    public Tile currentTile { get; private set; }
    public Tile pastTile { get; private set; }

    public bool Spent { get; private set; }

    public Team team { get; private set; }
    public Controller controller { get; private set; }

    #endregion

    #region Private variables

    private Animator animator;
    private Rigidbody2D rb2D;

    private float inverseMoveTime;

    private Dictionary<Tile, ActionSpace> actionSpaces = new Dictionary<Tile, ActionSpace>();

    private Dictionary<Tile, int> moveTree;

    private Dictionary<Action, bool> phaseFlags = new Dictionary<Action, bool>(); //moved, attacked, etc
    private Dictionary<Action, bool> phaseActiveFlags = new Dictionary<Action, bool>(); //moving, attacking, etc

    #endregion

    public void Start()
    {
        animator = GetComponent<Animator>();
        rb2D = GetComponent<Rigidbody2D>();
        inverseMoveTime = 1f / moveTime;
        foreach (Action action in (Action[]) Enum.GetValues(typeof(Action)))
        {
            phaseFlags[action] = false;
            phaseActiveFlags[action] = false;
        }
    }

    public bool GetPhaseFlag(Action action)
    {
        return phaseFlags[action];
    }
    public bool GetActivePhaseFlag(Action action)
    {
        return phaseActiveFlags[action];
    }
    public bool IsActive()
    {
        bool Bool = false;
        foreach (Action action in phaseActiveFlags.Keys)
        {
            Bool |= phaseActiveFlags[action];
        }
        return Bool;
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
        if (team == Team.Enemy)
        {
            GetComponent<SpriteRenderer>().flipX = true;
        }
        controller = _controller;
        currentTile = _spawnTile;
        pastTile = currentTile;
        transform.position = currentTile.Position;
        currentTile.CurrentUnit = this;
        ResetStates();
    }

    #region Data Access Methods

    /// <summary>
    /// Generates and returns the move tree of this unit.
    /// </summary>
    /// <returns></returns>
    public Dictionary<Tile, int> GetMoveTree()
    {
        Dictionary<Tile, int> tree = Pathfinding.GenerateMoveTree(currentTile, moveRadius);
        return tree;
    }

    /// <summary>
    /// Returns a random tile from a given movetree.
    /// </summary>
    /// <param name="_moveTree"></param>
    /// <returns></returns>
    public static Tile GetRandomTileFromMoveTree(Dictionary<Tile, int> _moveTree)
    {
        var random = new System.Random();
        List<Tile> keys = new List<Tile>(_moveTree.Keys);
        int index = random.Next(keys.Count);
        return keys[index];
    }

    #endregion

    #region Turn Flow Methods

    /// <summary>
    /// Initializes the various state checking variables of this unit.
    /// </summary>
    public void ResetStates()
    {
        Debug.Log("Reset states");
        List<Action> keys = new List<Action>(phaseFlags.Keys);
        foreach (Action action in keys)
        {
            phaseFlags[action] = false;
            phaseActiveFlags[action] = false;
        }
        Spent = false;
        EraseSpaces();
    }

    /// <summary>
    /// Tests if this unit should end its turn. Returns true if so.
    /// </summary>
    public bool QueryEndOfTurn()
    {
        bool Bool = true;
        foreach (KeyValuePair<Action, bool> pair in phaseFlags)
        {
            Bool &= pair.Value;
        }
        return Bool;
    }

    /// <summary>
    /// Cleans up this unit to be inert for the rest of its controller's turn.
    /// </summary>
    public void EndActions()
    {
        pastTile = currentTile;
        EraseSpaces();
        Spent = true;
    }

    #endregion

    #region Action Methods
    /// <summary>
    /// Teleports a unit to a certain tile. Erases pastTile information, this is meant as an absolute forced movement.
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
        Debug.Log("MetaMove called");
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
                //There is more than one non-starting tile in the path, the unit is actually moving
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
            else //the first non-starting tile is the only vertex in the movement, so we just add that and move on
            { 
                stepsVertices.Add(firstTile);
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
        if (steps.Count > 0)
        {
            phaseActiveFlags[Action.Move] = true;
            pastTile = currentTile;
            currentTile.CurrentUnit = null;
            foreach (Tile step in steps)
            {
                yield return StartCoroutine(SmoothMovement(step));
            }
            phaseActiveFlags[Action.Move] = false;
            currentTile.CurrentUnit = this;
        }
        phaseFlags[Action.Move] = true;
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
    /// Generates the move spaces of a unit.
    /// </summary>
    /// <param name="_cosmetic">If true, the squares will be inert and ActionSpace.Activate will do nothing (ie, if they are an enemy's)</param>
    /// <returns>A dictionary of tiles with their associated act spaces.</returns>
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
            moveSpaceScript.Active = !_cosmetic;
            spaces[pair.Key] = moveSpaceScript;
        }

        actionSpaces = spaces;
        _GameManager.instance.Cursor.actionSpaces = spaces;
        return spaces;
    }

    /// <summary>
    /// Generates the act spaces.
    /// </summary>
    /// <param name="_cosmetic">Whether to render the squares inert (ie, if they are an enemy's)</param>
    /// <returns>A dictionary of tiles with their associated act spaces.</returns>
    public Dictionary<Tile, ActionSpace> GenerateActSpaces(bool _cosmetic)
    {
        Dictionary<Tile, ActionSpace> spaces = new Dictionary<Tile, ActionSpace>();
        //for every action besides movement, spawn and layer the actionspaces so the player can see what options are available
        actionSpaces = spaces;
        _GameManager.instance.Cursor.actionSpaces = spaces;
        return spaces;
    }

    /// <summary>
    /// The general attack method.
    /// </summary>
    /// <param name="_tile"></param>
    public void Attack(Tile _tile)
    {
        //Currently just doesn't do an attack but acts like it did.
        phaseFlags[Action.Attack] = true;
        controller.StepTurn();
    }

    /// <summary>
    /// Erases the spaces held by the unit, also tells them to delete themselves.
    /// </summary>
    public void EraseSpaces()
    {
        foreach (KeyValuePair<Tile,ActionSpace> pair in actionSpaces)
        {
            pair.Value.Delete();
        }
        actionSpaces.Clear();
    }

    public void PopulateActionMenu()
    {
    }

    /// <summary>
    /// Interprets the action that should be performed by a passed actionspace.
    /// </summary>
    /// <param name="space"></param>
    public void ParseAction(ActionSpace space)
    {
        switch (space.action)
        {
            case Action.Move:
                MetaMove(space.currentTile);
                break;
            case Action.Attack:
                Attack(space.currentTile);
                break;
            default:
                Debug.Log("Error, unhandled action attempted to be performed by " + team + "'s unit");
                break;
        }
    }

    #endregion
}
