using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Unit : MonoBehaviour, IDamageableObject
{


    #region Public Variables
    public GameObject MoveSpace;
    public GameObject ActSpace;

    public float moveTime;
    public int actRadius = 1;
    public int moveRadius = 2;
    public bool debugUnit;

    public int MaxHealth { get; private set; }
    public int CurrentHealth {
        get { return currentHealth; }
        private set {
            if(healthBar != null)
                healthBar.ChangeHealthbar(value);
            currentHealth = value;
            if (value <= 0)
            {
                GameManager.instance.ReportUnitDeath(this);
                dead = true;
            }
        }
    }
    public int Strength { get; private set; }
    public int Defence { get; private set; }
    public int Movement { get; private set; }
    public string Name = "my name jeff";

    public Action currentAction;

    public Tile currentTile { get; private set; }
    public Tile pastTile { get; private set; }
    public Dictionary<Tile, ActionSpace> actionSpaces = new Dictionary<Tile, ActionSpace>();
    public bool Spent
    {
        get
        {
            return spent;
        }
        private set
        {
            spent = value;
            if (value)
            {
                transform.localRotation = Quaternion.Euler(0, 180, 180);
            }
            else
            {
                transform.localRotation = Quaternion.Euler(0, 0, 0);
            }
        }
    }

    public Team Team { get; private set; }
    public Commander Commander
    {
        get
        {
            return commander;
        }
        set
        {
            if (value != null)
            {
                Team = value.Team;
                commander = value;
            }
            else
            {
                Team = Team.None;
                commander = null;
            }
        }
    }
    private Commander commander;

    #endregion

    #region Private variables

    private Animator animator;
    private Rigidbody2D rb2D;
    private UnitHealthBar healthBar;

    private bool dead;
    private float inverseMoveTime;
    private bool spent;
    private int currentHealth;

    private int maxAttackRangeForAttackCheck = 2; //Placeholder variable

    private Dictionary<Tile, Pathfinding.TileNode> moveTree;
    private Dictionary<Tile, int> attackTree;

    private Dictionary<ActionNames, bool> actionsPerformedFlags = new Dictionary<ActionNames, bool>(); //moved, attacked, etc
    private Dictionary<ActionNames, bool> actionsPerformingFlags = new Dictionary<ActionNames, bool>(); //moving, attacking, etc

    private const string HealthBarLocation = "Prefabs/GUI/UnitHealthBar";

    private UnitTilesContainer tilesContainer;

    private MovementTypes movementType = MovementTypes.Ground;

    #endregion

    public void Start()
    {
        animator = transform.Find("Sprite").GetComponent<Animator>();
        rb2D = GetComponent<Rigidbody2D>();
        inverseMoveTime = 1f / moveTime;
        foreach (ActionNames action in (ActionNames[]) Enum.GetValues(typeof(ActionNames)))
        {
            actionsPerformedFlags[action] = false;
            actionsPerformingFlags[action] = false;
        }

    }

    public void Update()
    {
        if (debugUnit)
        {
            Debug.Log("Current rotation is " + transform.rotation);
        }
    }

    public bool GetPhaseFlag(ActionNames action)
    {
        return actionsPerformedFlags[action];
    }
    public bool GetActivePhaseFlag(ActionNames action)
    {
        return actionsPerformingFlags[action];
    }

    /// <summary>
    /// Whether or not this unit is in the middle of an uninterruptable action (such as is moving, is attacking, maybe an animation).
    /// </summary>
    /// <returns></returns>
    public bool IsPerformingAction()
    {
        bool Bool = false;
        foreach (ActionNames action in actionsPerformingFlags.Keys)
        {
            Bool |= actionsPerformingFlags[action];
        }
        return Bool;
    }

    /// <summary>
    /// Sets all of the passed parameters while also moving the unit to the position of its tile.
    /// </summary>
    /// <param name="_spawnTile">The tile the unit should be on.</param>
    /// <param name="_commander">The commander that commands this unit.</param>
    public Unit InitializeUnit(Tile _spawnTile, Commander _commander, UnitStats stats)
    {

        if (_commander.Team == Team.Player2)
        {
            transform.Find("Sprite").GetComponent<SpriteRenderer>().flipX = true;
        }
        Commander = _commander;
        currentTile = _spawnTile;
        pastTile = currentTile;
        transform.position = currentTile.Position;
        currentTile.CurrentUnit = this;

        Strength = stats.Strength;
        MaxHealth = stats.MaxHealth;
        CurrentHealth = stats.CurrentHealth;
        Defence = stats.Defence;
        Movement = stats.Movement;

        //Make the healthbar
        GameObject go = Instantiate(Resources.Load<GameObject>(HealthBarLocation));
        healthBar = go.GetComponent<UnitHealthBar>();
        healthBar.Initialize(MaxHealth, stats.CurrentHealth, this);

        tilesContainer = new UnitTilesContainer(1, 3, currentTile, transform.Find("Sprites"));
        transform.rotation = Quaternion.identity;
        ResetStates();
        return this;
    }

    /// <summary>
    /// Deletes this unit's gameobject from the world (and all baggage).
    /// </summary>
    public void DeleteGameObjects()
    {
        Destroy(healthBar.gameObject);
        Destroy(gameObject);
    }


    #region DamageableObjectInterface


    public void TakeDamage(int AttackingStrength)
    {
        CurrentHealth -= AttackingStrength - Defence;
    }

    #endregion

    #region Data Access Methods

    /// <summary>
    /// Generates and returns the move tree of this unit.
    /// </summary>
    /// <returns></returns>
    public Dictionary<Tile, Pathfinding.TileNode> GetMoveTree()
    {
        Dictionary<Tile, Pathfinding.TileNode> tree = new Dictionary<Tile, Pathfinding.TileNode>();
        tree = Pathfinding.NewGenerateTileTree(currentTile, moveRadius, movementType, tilesContainer, tilesContainer.rotation);
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
        foreach (KeyValuePair<ActionNames, bool> pair in actionsPerformedFlags)
        {
            if (pair.Value)
            {
                continue;
            }
            switch(pair.Key)
            {
                case ActionNames.Attack:
                    bool targetsExist = false;
                    for (int i = 1; i <= maxAttackRangeForAttackCheck; i++)
                    {
                        if (targetsExist)
                        {
                            break;
                        }
                        foreach (Tile tile in GameManager.instance.Board.GenerateDiamond(i, currentTile))
                        {
                            if (targetsExist)
                            {
                                break;
                            }
                            if (tile.CurrentUnit != null && tile.CurrentUnit.Team != Team) //&& ping if attack is possible
                            {
                                targetsExist = true;
                            }
                        }
                    }
                    if (targetsExist)
                    {
                        list.Add(pair.Key);
                    }
                    break;
                default:
                    list.Add(pair.Key);
                    break;
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
        //Debug.Log("reset states");
        pastTile = currentTile;
        if (!dead)
        {
            List<ActionNames> keys = new List<ActionNames>(actionsPerformedFlags.Keys);
            foreach (ActionNames action in keys)
            {
                actionsPerformedFlags[action] = false;
                actionsPerformingFlags[action] = false;
            }
            Spent = false;
        }
        EraseSpaces();
    }

    /// <summary>
    /// Tests if this unit should end its turn. Returns true if so.
    /// </summary>
    public bool QueryEndOfTurn()
    {
        bool Bool = (GetAllPossibleActions().Count == 0);
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
        List<ActionNames> keys = new List<ActionNames>(actionsPerformedFlags.Keys);
        foreach (ActionNames action in keys)
        {
            actionsPerformedFlags[action] = true;
            actionsPerformingFlags[action] = false;
        }
        Spent = true;
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
    /// Allows other objects to easily set hit trigger.
    /// </summary>
    public void SetHitTrigger()
    {
        animator.SetTrigger("playerHit");
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
                moveTree = GetMoveTree(); //add checks for if this changes between drawing squares and metamove
                foreach (KeyValuePair<Tile, Pathfinding.TileNode> pair in moveTree)
                {
                    Tile tile = pair.Key;
                    //Checks if there's a unit already on the tile before adding to the move tree.
                    if (!tile.Occupied || tile.CurrentUnit.Team == Team)
                    {
                        Vector3 position = tile.Position;
                        ActionSpace moveSpaceScript = Instantiate(MoveSpace, position, Quaternion.identity).GetComponent<ActionSpace>();
                        moveSpaceScript.parentUnit = this;
                        moveSpaceScript.currentTile = tile;
                        moveSpaceScript.command = CommandNames.Move;
                        moveSpaceScript.Invalid = tile.Occupied;
                        spaces[tile] = moveSpaceScript;
                    }
                }
                break;
            case ActionNames.Attack:
                attackTree = GetAttackTree();
                foreach (KeyValuePair<Tile,int> pair in attackTree)
                {
                    if (spaces.ContainsKey(pair.Key) && spaces[pair.Key] != null)
                    {
                        //maybe add on some extra flags
                    }
                    else
                    {
                        Vector3 position = pair.Key.Position;
                        ActionSpace attackSpaceScript = Instantiate(ActSpace, position, Quaternion.identity).GetComponent<ActionSpace>();
                        attackSpaceScript.parentUnit = this;
                        attackSpaceScript.currentTile = pair.Key;
                        attackSpaceScript.command = CommandNames.Attack;
                        spaces[pair.Key] = attackSpaceScript;
                    }
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
        //Debug.Log("erasing spaces");
        foreach (KeyValuePair<Tile, ActionSpace> pair in actionSpaces)
        {
            pair.Value.Delete();
        }
        actionSpaces.Clear();
    }

    /// <summary>
    /// Makes the actionspaces invisible.
    /// </summary>
    public void HideSpaces()
    {
        foreach (KeyValuePair<Tile, ActionSpace> pair in actionSpaces)
        {
            pair.Value.Hide();
        }
    }
    #endregion

    #region Actions
    /* 
     * Things needed to implemenet a new action:
     * -in PerformAction, add a case for your action. It needs to pass actionCallbackContainer at the very least.
     * -Call actionCallbackContainer.PerformCallback() at the very end of any animations/action, once control is ready to go back to the cursor.
     * -actionCallbackContainer.PerformCallback() needs to be called if at any time the action ends, so control can be returned to the cursor. If you have some kind of interruption happening
     * (like a new menu is popping up), then just make sure to continue to pass actionCallbackContainer until it can be called.
     * -Go to Commander.parseCommand and add a case for what command should lead to your action (if your action results from a command)
     * -Add the action to the ActionNames enum, and the various applicable CommandNames enum if need be
    */

    /// <summary>
    /// The wraper for performing an action.
    /// </summary>
    /// <returns></returns>
    public void PerformAction(ActionNames _actionName, Tile _targetTile, Action<Unit> _gameManagerCallback)
    {
        Action unitCallback;
        ActionCallbackContainer actionCallbackContainer;

        switch (_actionName)
        {
            case (ActionNames.Move):
                unitCallback = () => { Debug.Log("Setting phaseFlag to true"); actionsPerformedFlags[_actionName] = true; };
                actionCallbackContainer = new ActionCallbackContainer(_gameManagerCallback, unitCallback, this);

                //MetaMove(_targetTile, actionCallbackContainer);
                ExecuteMovementPath(actionCallbackContainer);
                return;
            case (ActionNames.Attack):
                unitCallback = () => { Debug.Log("Setting phaseFlag to true"); actionsPerformedFlags[_actionName] = true; EraseSpaces(); };
                actionCallbackContainer = new ActionCallbackContainer(_gameManagerCallback, unitCallback, this);

                Attack(_targetTile, actionCallbackContainer);
                return;
        }
    }

    /// <summary>
    /// Encapsulates the callbacks (both from the gamemanager and for the unit itself) for after a unit performs an action.
    /// </summary>
    public struct ActionCallbackContainer
    {
        Action<Unit> gameManagerCallback;
        Action unitCallback;
        Unit unit;
        public Dictionary<Action<object[]>, object[]> arbitraryCallbacks;

        public ActionCallbackContainer(Action<Unit> _commanderCallback, Action _unitCallback, Unit _unit)
        {
            gameManagerCallback = _commanderCallback;
            unitCallback = _unitCallback;
            unit = _unit;
            arbitraryCallbacks = new Dictionary<Action<object[]>, object[]>();
        }

        public void PerformCallback()
        {
            if (arbitraryCallbacks.Count > 0)
            {
                foreach (KeyValuePair<Action<object[]>, object[]> pair in arbitraryCallbacks)
                {
                    pair.Key(pair.Value);
                }
            }
            unitCallback();
            gameManagerCallback(unit);
        }
    }

    #region Move
    List<PathCreationStep> movementPath;
    ActionCallbackContainer movementCallback;
    //AdjacentDirection movementPreviousDirection;
    //int movementCurrentRotation;
    //int movementCumulativeDistance;
    //Tile movementSourceTile;
    //TODO: change this to track the tile instead and only take in a direction. wrap it in its own class.

    public void InitializePathCreation(Action<Unit> _commanderCallback)
    {
        Action unitCallback = () => { Debug.Log("Setting phaseFlag to true"); actionsPerformedFlags[ActionNames.Move] = true; EraseSpaces(); };
        ActionCallbackContainer actionCallbackContainer = new ActionCallbackContainer(_commanderCallback, unitCallback, this);

        GameManager.instance.StartUnitMovement(this);

        movementPath = new List<PathCreationStep>();
        movementCallback = actionCallbackContainer;

        PathCreationStep nullStep = new PathCreationStep(currentTile, currentTile, AdjacentDirection.None, tilesContainer.Rotation, 0, 0, PathCreationStepTypes.None);
        movementPath.Add(nullStep);
    }

    public bool CheckIfStepLegal(AdjacentDirection _direction)
    {
        return true;
    }

    /// <summary>
    /// Stores data on each step in the path as it is created. includes the sprite.
    /// </summary>
    public class PathCreationStep
    {
        public Tile initialTile;
        public Tile finalTile;
        public int initialRotation;
        public int deltaRotation;
        public int finalRotation;
        public int cumulativeWeight;
        public AdjacentDirection direction;

        public PathCreationStepTypes movementType { get; private set; }
        private GameObject arrowSprite;
        private const string TranslationArrowPrefab = "Prefabs/TranslationArrow";
        private const string RotationArrowPrefab = "Prefabs/RotationArrow";

        public PathCreationStep(Tile _initialTile, Tile _finalTile, AdjacentDirection _direction, int _initialRotation, int _deltaRotation, int _cumulativeWeight, PathCreationStepTypes _type)
        {
            initialTile = _initialTile;
            finalTile = _finalTile;
            initialRotation = _initialRotation;
            deltaRotation = _deltaRotation;

            finalRotation = initialRotation+deltaRotation;
            finalRotation %= 4;
            finalRotation = finalRotation < 0 ? finalRotation + 4 : finalRotation;

            direction = _direction;
            cumulativeWeight = _cumulativeWeight;

            movementType = _type;
            //tileObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>(TileGameObjectResource));
            switch (movementType)
            {
                case PathCreationStepTypes.Translation:
                    arrowSprite = UnityEngine.Object.Instantiate(
                        Resources.Load<GameObject>(TranslationArrowPrefab),
                        initialTile.Position,
                        Quaternion.Euler(0, 0, 90 * (1 - (int)_direction)));
                    break;
                case PathCreationStepTypes.Rotation:
                    arrowSprite = UnityEngine.Object.Instantiate(
                        Resources.Load<GameObject>(RotationArrowPrefab),
                        initialTile.Position,
                        Quaternion.Euler(0, 0, 90 * (1 - (int)_direction)));
                    break;
            }
        }

        /// <summary>
        /// Do any cleanup on this object. (removes sprite, disables gameobject, deletes).
        /// </summary>
        public void Clear()
        {
            if (arrowSprite == null)
            {
                return;
            }

            arrowSprite.SetActive(false);
            GameObject.Destroy(arrowSprite);
        }
    }

    /// <summary>
    /// Used when moving cardinally while creating a unit's path. Returns false if failed for any reason, true if a step is added or removed. Only commits variables on successful movement.
    /// </summary>
    /// <param name="_direction"></param>
    /// <returns></returns>
    public bool StepPathCreationTranslate(AdjacentDirection _direction)
    {
        PathCreationStep previousStep = movementPath[movementPath.Count - 1];
        AdjacentDirection previousDirection = previousStep.direction;

        if (_direction == AdjacentDirection.None || movementPath.Count == 0)
        {
            Debug.Log("StepPathCreationTranslate should not have been called");
            return false;
        }

        if (
            previousStep.movementType == PathCreationStepTypes.Translation
            && ((int)_direction % 4 == ((int)previousDirection + 2) % 4)) //if the direction we just inputted is opposite of the previous move, then cancel it.
        {
            Debug.Log("resetting previous move");
            movementPath[movementPath.Count - 1].Clear();
            movementPath.RemoveAt(movementPath.Count - 1);
            return true;
        }

        int movementDistance;
        Tile _destinationTile = previousStep.finalTile.GetAdjacentTile(_direction);

        //First check if the movement is legal
        if (_destinationTile == null || tilesContainer.CheckOutOfBounds(_destinationTile.GridPosition, previousStep.finalRotation))
        {
            return false;
        }

        //now grab the weight of the new position if we were to move to it
        movementDistance = tilesContainer.GetWeight(_destinationTile.GridPosition, previousStep.finalRotation, MovementTypes.Ground);
        int newDistance = previousStep.cumulativeWeight + movementDistance;

        if (newDistance <= moveRadius)
        {
            PathCreationStep step = new PathCreationStep(
                previousStep.finalTile,
                _destinationTile,
                _direction,
                previousStep.finalRotation,
                0,
                newDistance,
                PathCreationStepTypes.Translation); ; ;
            movementPath.Add(step);
            return true;
        }
        return false;
    }
    /// <summary>
    /// Used when rotating while creating a unit's path. Returns false if failed for any reason, true if a step is added or removed. Only commits variables on successful movement.
    /// </summary>
    /// <param name="_direction"></param>
    /// <returns></returns>
    public bool StepPathCreationRotation(bool clockwise)
    {
        PathCreationStep previousStep = movementPath[movementPath.Count - 1];

        int _deltaRotation = clockwise ? 1 : -1;

        if (movementPath.Count == 0)
        {
            Debug.Log("StepPathCreationRotation should not have been called");
            return false;
        }

        if (
            previousStep.movementType == PathCreationStepTypes.Rotation
            && _deltaRotation == -1*previousStep.deltaRotation) //if the direction we just inputted is opposite of the previous move, then cancel it.
        {
            Debug.Log("resetting previous move");
            movementPath[movementPath.Count - 1].Clear();
            movementPath.RemoveAt(movementPath.Count - 1);
            return true;
        }

        //int movementDistance;
        Tile _destinationTile = previousStep.finalTile;

        //First check if the movement is legal
        Debug.Log(previousStep.finalRotation);
        if (_destinationTile == null || tilesContainer.CheckOutOfBounds(_destinationTile.GridPosition, previousStep.finalRotation+_deltaRotation))
        {
            return false;
        }

        //now grab the weight of the new position if we were to move to it
        //for now, rotation is free (in terms of movement weight)
        //movementDistance = tilesContainer.GetWeight(_destinationTile.GridPosition, previousStep.finalRotation+_deltaRotation, MovementTypes.Ground);
        int newDistance = previousStep.cumulativeWeight /*+movementDistance*/;

        if (newDistance <= moveRadius)
        {
            PathCreationStep step = new PathCreationStep(
                previousStep.finalTile,
                _destinationTile,
                AdjacentDirection.None,
                previousStep.finalRotation,
                _deltaRotation,
                newDistance,
                PathCreationStepTypes.Rotation);
            movementPath.Add(step);
            return true;
        }
        return false;
    }

    public void ExecuteMovementPath(ActionCallbackContainer _actionCallbackContainer)
    {
        Debug.Log("Path Length" + movementPath.Count);
        if (movementPath.Count > 0)
        {
            StartCoroutine(SequenceOfMoves(movementPath, _actionCallbackContainer));
        }
        else
        {
            _actionCallbackContainer.PerformCallback();
        }
    }

    /// <summary>
    /// Gets the path of the unit using the starting and destination tile, then collapses it down into just the vertices.
    /// </summary>
    /// <param name="destinationTile">The destination tile.</param>
    private void MetaMove(Tile destinationTile, ActionCallbackContainer _callbackContainer)
    {
        Debug.Log("MetaMove called");
        //moveTree = GetMoveTree();
        Stack<Pathfinding.PathfindingStep> steps = Pathfinding.NewGenerateSteps(currentTile, destinationTile, moveTree);

        if (steps.Count > 0)
        {

            StartCoroutine(SequenceOfMoves(steps, _callbackContainer));
        }
        else
        {
            _callbackContainer.PerformCallback();
        }

    }

    /// <summary>
    /// The steps can be any list of tiles, the unit will move to each of them in turn.
    /// </summary>
    /// <param name="steps">The sequence of tile steps.</param>
    /// <returns>A coroutine for every step.</returns>
    private IEnumerator SequenceOfMoves(List<PathCreationStep> steps, ActionCallbackContainer _callbackContainer)
    {
        if (steps.Count > 0)
        {
            actionsPerformingFlags[ActionNames.Move] = true;
            pastTile = currentTile;
            currentTile.CurrentUnit = null;

            Debug.Log("-------------");
            Debug.Log("Unit's steps:");
            while (steps.Count > 0)
            {
                PathCreationStep step = steps[0];
                steps.Remove(step);
                Debug.Log("Step to " + step.finalTile.GridPosition + " and rotate to rotation " + step.finalRotation);
                step.Clear();
                yield return StartCoroutine(SmoothMovement(step));
            }
            actionsPerformingFlags[ActionNames.Move] = false;
            currentTile.CurrentUnit = this;
        }
        _callbackContainer.PerformCallback();
    }
    private IEnumerator SequenceOfMoves(Stack<Pathfinding.PathfindingStep> steps, ActionCallbackContainer _callbackContainer)
    {
        if (steps.Count > 0)
        {
            actionsPerformingFlags[ActionNames.Move] = true;
            pastTile = currentTile;
            currentTile.CurrentUnit = null;

            Debug.Log("-------------");
            Debug.Log("Unit's steps:");
            while (steps.Count > 0)
            {
                Pathfinding.PathfindingStep step = steps.Pop();
                Debug.Log("Step to " + step.finalTile.GridPosition + " and rotate to rotation " + step.finalRotation);
                yield return StartCoroutine(SmoothMovement(step));
            }
            actionsPerformingFlags[ActionNames.Move] = false;
            currentTile.CurrentUnit = this;
        }
        _callbackContainer.PerformCallback();
    }

    /// <summary>
    /// Smooth movement to a single given tile, spread over multiple frames. Also sets the currentTile.
    /// </summary>
    /// <param name="stepToPerform">The destination tile.</param>
    /// <returns>null</returns>
    private IEnumerator SmoothMovement(PathCreationStep stepToPerform)
    {
        float timer = 0;
        float sqrRemainingDistance = (transform.position - stepToPerform.finalTile.Position).sqrMagnitude;
        float maxDistance = sqrRemainingDistance;

        //raw math spaghetti trying to get rotation to work
        Quaternion destinationRotation = transform.rotation;
        int deltaRotation = stepToPerform.deltaRotation;
        //Debug.Log("smoothMovement deltaRotation: " + deltaRotation);
        if (deltaRotation == 1)
        {
            //Debug.Log(Quaternion.AngleAxis(-90, Vector3.forward));
            destinationRotation = transform.rotation*Quaternion.AngleAxis(-90, Vector3.forward);
        }
        else if (deltaRotation == -1)
        {
            //Debug.Log(Quaternion.AngleAxis(90, Vector3.forward));
            destinationRotation = transform.rotation*Quaternion.AngleAxis(90, Vector3.forward);
        }
        //Debug.Log(transform.rotation);
        //Debug.Log(destinationRotation);

        while (timer < moveTime)
        {
            Vector3 newPosition = Vector3.MoveTowards(transform.position, stepToPerform.finalTile.Position, inverseMoveTime * Time.deltaTime);
            //rb2D.MovePosition(newPosition); (we might want rigid body for smooth movement)
            transform.position = newPosition;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, destinationRotation,8*90* inverseMoveTime * Time.deltaTime);
            sqrRemainingDistance = (transform.position - stepToPerform.finalTile.Position).sqrMagnitude;
            timer += Time.deltaTime;
            yield return null;
        }

        currentTile = stepToPerform.finalTile;
        tilesContainer.Rotation = stepToPerform.finalRotation;
        transform.rotation = Quaternion.identity;
        transform.position = currentTile.Position; //snap the position just in case the unit is slightly off
    }
    private IEnumerator SmoothMovement(Pathfinding.PathfindingStep stepToPerform) //old code using the old pathfinding format
    {
        float sqrRemainingDistance = (transform.position - stepToPerform.finalTile.Position).sqrMagnitude;
        float maxDistance = sqrRemainingDistance;
        Quaternion destinationRotation = transform.rotation;
        int deltaRotation = stepToPerform.deltaRotation;
        Debug.Log("smoothMovement deltaRotation: " + deltaRotation);
        if (deltaRotation == 1)
        {
            Debug.Log(Quaternion.AngleAxis(-90, Vector3.forward));
            destinationRotation = transform.rotation * Quaternion.AngleAxis(-90, Vector3.forward);
        }
        else if (deltaRotation == -1)
        {
            Debug.Log(Quaternion.AngleAxis(90, Vector3.forward));
            destinationRotation = transform.rotation * Quaternion.AngleAxis(90, Vector3.forward);
        }
        Debug.Log(transform.rotation);
        Debug.Log(destinationRotation);
        while (sqrRemainingDistance > float.Epsilon)
        {
            Vector3 newPosition = Vector3.MoveTowards(transform.position, stepToPerform.finalTile.Position, 4 * inverseMoveTime * Time.deltaTime);
            //rb2D.MovePosition(newPosition); (we might want rigid body for smooth movement)
            transform.position = newPosition;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, destinationRotation, 8 * 90 * inverseMoveTime * Time.deltaTime);
            sqrRemainingDistance = (transform.position - stepToPerform.finalTile.Position).sqrMagnitude;
            yield return null;
        }
        currentTile = stepToPerform.finalTile;
        tilesContainer.Rotation = stepToPerform.finalRotation;
        transform.rotation = Quaternion.identity;
        transform.position = currentTile.Position; //snap the position just in case the unit is slightly off
    }
    #endregion

    #region Attack
    /// <summary>
    /// The general attack method.
    /// </summary>
    /// <param name="_tile"></param>
    private void Attack(Tile _tile, ActionCallbackContainer _callbackContainer)
    {
        actionsPerformedFlags[ActionNames.Attack] = true;
        HideSpaces();

        Vector2Int dir = Pathfinding.GetTileDirectionVector(currentTile, _tile);
        Debug.Log(dir);

        animator.SetFloat("AttackY", dir.y);
        animator.SetFloat("AttackX", dir.x);

        //Behaviour will perform callback after animation exit
        animator.GetBehaviour<PlayerAttackBehaviour>().callbackContainer = _callbackContainer;

        animator.SetTrigger("playerAttack");


        if (_tile.Occupied)
        {
            _tile.CurrentUnit.TakeDamage(Strength);
        }


    }

    /// <summary>
    /// Generates a list of tiles, indexed by what weapons can access those tiles (currently just a placeholder integer until we set up a weapon/attack class or something)
    /// </summary>
    /// <param name="_currentTile"></param>
    /// <returns></returns>
    private Dictionary<Tile, int> GetAttackTree()
    {
        Dictionary<Tile, int> returnDict = new Dictionary<Tile, int>();
        /*
        foreach (KeyValuePair<Tile, int> pair in Pathfinding.GenerateTileTree(currentTile, 2, MovementTypes.Flying, true, true, Team))
        {
            if (pair.Key.CurrentUnit != null && pair.Key.CurrentUnit.Team != Team)
            {
                returnDict[pair.Key] = pair.Value;
            }
        }
        */
        return returnDict;
    }
    
    #endregion
    #endregion
}
