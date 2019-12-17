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

    public Action currentAction;

    public Tile currentTile { get; private set; }
    public Tile pastTile { get; private set; }
    public Dictionary<Tile, ActionSpace> actionSpaces = new Dictionary<Tile, ActionSpace>();
    public bool Spent { get; private set; }

    public Team team { get; private set; }
    public Commander commander { get; private set; }

    #endregion

    #region Private variables

    private Animator animator;
    private Rigidbody2D rb2D;

    private float inverseMoveTime;



    private Dictionary<Tile, int> moveTree;

    private Dictionary<ActionNames, bool> phaseFlags = new Dictionary<ActionNames, bool>(); //moved, attacked, etc
    private Dictionary<ActionNames, bool> phaseActiveFlags = new Dictionary<ActionNames, bool>(); //moving, attacking, etc

    #endregion

    public void Start()
    {
        animator = GetComponent<Animator>();
        rb2D = GetComponent<Rigidbody2D>();
        inverseMoveTime = 1f / moveTime;
        foreach (ActionNames action in (ActionNames[]) Enum.GetValues(typeof(ActionNames)))
        {
            phaseFlags[action] = false;
            phaseActiveFlags[action] = false;
        }
    }

    public bool GetPhaseFlag(ActionNames action)
    {
        return phaseFlags[action];
    }
    public bool GetActivePhaseFlag(ActionNames action)
    {
        return phaseActiveFlags[action];
    }

    /// <summary>
    /// Whether or not this unit is in the middle of an uninterruptable action (such as is moving, is attacking, maybe an animation).
    /// </summary>
    /// <returns></returns>
    public bool IsPerformingAction()
    {
        bool Bool = false;
        foreach (ActionNames action in phaseActiveFlags.Keys)
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
    public void InitializeUnit(Tile _spawnTile, Team _team, Commander _commander)
    {
        team = _team;
        if (team == Team.Enemy)
        {
            GetComponent<SpriteRenderer>().flipX = true;
        }
        commander = _commander;
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
    /// <summary>
    /// Returns all actions that a unit has not performed yet.
    /// (That means the flag was FALSE)
    /// </summary>
    public List<ActionNames> GetAllPossibleActions()
    {
        List<ActionNames> list = new List<ActionNames>();
        foreach (KeyValuePair<ActionNames, bool> pair in phaseFlags)
        {
            if (!pair.Value)
            {
                list.Add(pair.Key);
            }
        }

        return list;
    }
    #endregion

    #region Control Methods

    /// <summary>
    /// Initializes the various state checking variables of this unit.
    /// </summary>
    public void ResetStates()
    {
        Debug.Log("reset states");
        pastTile = currentTile;
        List<ActionNames> keys = new List<ActionNames>(phaseFlags.Keys);
        foreach (ActionNames action in keys)
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
        foreach (KeyValuePair<ActionNames, bool> pair in phaseFlags)
        {
            Bool &= pair.Value;
        }
        if (Bool)
        {
            Spent = true;
        }
        return Bool;
    }

    /// <summary>
    /// Forces up this unit to be inert for the rest of its controller's turn. Also retires it in the commander.
    /// </summary>
    public void EndActions()
    {
        pastTile = currentTile;
        List<ActionNames> keys = new List<ActionNames>(phaseFlags.Keys);
        foreach (ActionNames action in keys)
        {
            phaseFlags[action] = true;
            phaseActiveFlags[action] = false;
        }
        Spent = true;
        commander.RetireUnit(this);
        EraseSpaces();
    }

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
    /// Tells a specific unit to teleport back to its past tile and resets the state.
    /// </summary>
    /// <param name="_teleport">Whether or not it should teleport.</param>
    public void RevertMaybeTeleport(bool _teleport)
    {
        if (_teleport)
        {
            Teleport(pastTile);
        }
        ResetStates();
    }

    /// <summary>
    /// Generates the act spaces for a specific action.
    /// </summary>
    /// <param name="_action">the action to generate for</param>
    /// <returns></returns>
    public Dictionary<Tile, ActionSpace> GenerateActSpaces(ActionNames _action)
    {
        Dictionary<Tile, ActionSpace> spaces = new Dictionary<Tile, ActionSpace>();

        switch (_action)
        {
            case ActionNames.Move:
                moveTree = Pathfinding.GenerateMoveTree(currentTile, moveRadius); //add checks for if this changes between drawing squares and metamove
                foreach (KeyValuePair<Tile, int> pair in moveTree)
                {
                    Vector3 position = pair.Key.Position;
                    ActionSpace moveSpaceScript = Instantiate(MoveSpace, position, Quaternion.identity).GetComponent<ActionSpace>();
                    moveSpaceScript.parentUnit = this;
                    moveSpaceScript.currentTile = pair.Key;
                    moveSpaceScript.action = ActionNames.Move;
                    spaces[pair.Key] = moveSpaceScript;
                }
                break;

            default:
                break;
        }

        actionSpaces = spaces;
        return spaces;
    }

    /// <summary>
    /// Erases the spaces held by the unit, also tells them to delete themselves.
    /// </summary>
    public void EraseSpaces()
    {
        Debug.Log("erasing spaces");
        foreach (KeyValuePair<Tile, ActionSpace> pair in actionSpaces)
        {
            pair.Value.Delete();
        }
        actionSpaces.Clear();
    }
    #endregion

    #region Actions
    /* 
     * Your action should call "actionCallbackContainer.PerformCallback(false)" immediately if no action ends up being performed
     * (I think that in this case, we should consider the input as the player interacting with the board and selecting units, which is what
     * the callback does).
     * Otherwise, you need to pass this actionCallbackContainer all the way to the end of the animation and call "actionCallbackContainer.PerformCallback(true)"
     * when you are ready to return control to the cursor.
     * 
     * Then, you need to set up a case in the switch within PerformAction that calls your action's method if the actionspace has that action.
     * Also, you need to set up GenerateActSpaces for the case of your action.
    */

    /// <summary>
    /// The wrapper for performing an action. Returns whether or not the cursor should lock (aka, if a multi-frame action is being performed).
    /// </summary>
    /// <returns></returns>
    public void PerformAction(CursorContext _contextToPassToCommander, Action<bool, CursorContext> _commanderCallback)
    {
        Tile targetTile = _contextToPassToCommander.currentTile;
        ActionCallbackContainer actionCallbackContainer = new ActionCallbackContainer(_contextToPassToCommander, _commanderCallback, EraseSpaces);
        if (actionSpaces.ContainsKey(targetTile))
        {
            ActionSpace tempSpace = actionSpaces[targetTile];
            switch (tempSpace.action)
            {
                case (ActionNames.Move):
                    MetaMove(targetTile, actionCallbackContainer);
                    break;
                default:
                    break;
            }
        }
        else
        {
            actionCallbackContainer.PerformCallback(false);
        }
    }

    private struct ActionCallbackContainer
    {
        public CursorContext context;
        public Action<bool, CursorContext> commanderCallback;
        public Action unitCallback;

        public ActionCallbackContainer(CursorContext _context, Action<bool, CursorContext> _commanderCallback, Action _unitCallback)
        {
            context = _context;
            commanderCallback = _commanderCallback;
            unitCallback = _unitCallback;
        }
        public void PerformCallback(bool _wasActionPerformed)
        {
            context.releaseCursorCallback();
            commanderCallback(_wasActionPerformed, context);
            unitCallback();
        }
    }

    #region Move
    /// <summary>
    /// Gets the path of the unit using the starting and destination tile, then collapses it down into just the vertices.
    /// </summary>
    /// <param name="destinationTile">The destination tile.</param>
    private void MetaMove(Tile destinationTile, ActionCallbackContainer _callbackContainer)
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

        if (stepsVertices.Count > 0)
        {
            StartCoroutine(SequenceOfMoves(stepsVertices, _callbackContainer));
        }
        else
        {
            _callbackContainer.PerformCallback(false);
        }

    }

    /// <summary>
    /// The steps can be any list of tiles, the unit will move to each of them in turn.
    /// </summary>
    /// <param name="steps">The sequence of tile steps.</param>
    /// <returns>A coroutine for every step.</returns>
    private IEnumerator SequenceOfMoves(List<Tile> steps, ActionCallbackContainer _callbackContainer)
    {
        if (steps.Count > 0)
        {
            phaseActiveFlags[ActionNames.Move] = true;
            pastTile = currentTile;
            currentTile.CurrentUnit = null;
            foreach (Tile step in steps)
            {
                yield return StartCoroutine(SmoothMovement(step));
            }
            phaseActiveFlags[ActionNames.Move] = false;
            currentTile.CurrentUnit = this;
        }
        _callbackContainer.PerformCallback(true);
        phaseFlags[ActionNames.Move] = true;
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
    #endregion

    #region Attack
    /// <summary>
    /// The general attack method.
    /// </summary>
    /// <param name="_tile"></param>
    private bool Attack(Tile _tile, ActionCallbackContainer _callbackContainer)
    {
        phaseFlags[ActionNames.Attack] = true;
        return false;
    }
    #endregion
    #endregion
}
