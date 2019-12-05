using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Unit : MonoBehaviour
{

    #region Public Variables
    public LayerMask blockingLayer;
    public LayerMask actingLayer;

    public GameObject MoveSpace;
    public GameObject ActSpace;

    public float moveTime = 0.1f;

    public string team;

    public bool moving = false;
    public bool moved = false;
    public bool acting = false;
    public bool acted = false;

    public int actRadius = 1;
    public int moveRadius = 3;
    #endregion

    #region Protected Variables

    protected Vector2 coordinates;

    protected Animator animator;
    protected BoxCollider2D boxCollider;
    protected Rigidbody2D rb2D;

    protected float inverseMoveTime;

    protected bool unSelected = false;
    protected bool selected = false;

    protected List<ActionSpace> actSpaces = new List<ActionSpace>();
    protected Dictionary<Tile, ActionSpace> moveSpaces = new Dictionary<Tile, ActionSpace>();

    #endregion

    #region Private variables

    private Tile currentTile;
    private Dictionary<Tile, int> moveTree;

    #endregion

    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
        coordinates = new Vector2(0, 0);
        coordinates.Set(Mathf.Floor(transform.position.x), Mathf.Floor(transform.position.y));
        boxCollider = GetComponent<BoxCollider2D>();
        rb2D = GetComponent<Rigidbody2D>();
        inverseMoveTime = 1f / moveTime;

        //Tentative
        currentTile = _GameManager.instance.board.Tiles[(int)coordinates.x, (int)coordinates.y];
        currentTile.CurrentUnit = this;
    }

    /// <summary>
    /// Gets the path of the unit using the starting and destination tile, then collapses it down into just the vertices.
    /// </summary>
    /// <param name="destinationTile">The destination tile.</param>
    public virtual void MetaMove(Tile destinationTile)
    {

        Stack<Tile> steps = Pathfinding.GenerateSteps(currentTile, destinationTile, moveTree);

        //Now, given a list of unit vectors, 
        //combine consecutive vectors in the same direction to create smooth movements.
        List<Tile> stepsVertices = new List<Tile>();
        if (steps.Count != 0)
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

        StartCoroutine(SequenceOfMoves(stepsVertices));

    }

    /// <summary>
    /// The steps can be any list of tiles, the unit will move to each of them in turn.
    /// </summary>
    /// <param name="steps">The sequence of tile steps.</param>
    /// <returns>A coroutine for every step.</returns>
    protected IEnumerator SequenceOfMoves(List<Tile> steps)
    {
        moving = true;
        currentTile.CurrentUnit = null;
        foreach (Tile step in steps)
        {
            yield return StartCoroutine(SmoothMovement(step));
        }
        moving = false;
        moved = true;
        currentTile.CurrentUnit = this;
        StartActPhase();
    }

    /// <summary>
    /// Smooth movement to a single given tile, spread over multiple frames. Also sets the currentTile.
    /// </summary>
    /// <param name="destinationTile">The destination tile.</param>
    /// <returns>null</returns>
    protected IEnumerator SmoothMovement(Tile destinationTile)
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

    protected virtual void Update()
    {
        //The basic behavior for non-player units is to draw move squares upon selection, and erase them otherwise
        Vector3 position = _GameManager.instance.cursorPosition;
        if (Input.GetButtonDown("confirm"))
        {
            if (Mathf.Abs(transform.position.x - position.x) < .5 && Mathf.Abs(transform.position.y - position.y) < .5)
            {
                DrawMoveSquares();
            }
            else
            {
                EraseMoveSquares();
            }
        }
    }

    /// <summary>
    /// Draws all the move squares, assumes the squares are erased first.
    /// </summary>
    protected virtual void DrawMoveSquares()
    {
        if (moveSpaces.Count > 0)
        {
            return;
        }

        moveTree = Pathfinding.GenerateMoveTree(currentTile, moveRadius); //add checks for if this changes between drawing squares and metamove

        foreach (KeyValuePair<Tile,int> pair in moveTree)
        {
            Vector3 position = pair.Key.Position;
            ActionSpace moveSpaceScript = Instantiate(MoveSpace, position, Quaternion.identity).GetComponent<ActionSpace>();
            moveSpaceScript.currentTile = pair.Key;
            moveSpaces[pair.Key] = moveSpaceScript;
        }

    }

    //Erases the move squares around the unit
    protected void EraseMoveSquares()
    {
        foreach (KeyValuePair<Tile,ActionSpace> pair in moveSpaces)
        {
            Destroy(pair.Value.gameObject);
        }
        moveSpaces.Clear();
    }

    protected virtual void StartActPhase()
    {
        acting = true;
        EraseMoveSquares();
        DrawActSquares();
        //pop up the menu of actions, have the player select one, unless they are an npc
        //npcs should automatically select an action
    }

    protected virtual void EndActPhase()
    {
        EraseActSquares();
        acting = false;
        acted = true;
    }

    protected virtual void DrawActSquares()
    {

    }

    protected void EraseActSquares()
    {
        foreach (var i in actSpaces)
        {
            Destroy(i.gameObject);
        }
        actSpaces = new List<ActionSpace>();
    }

    protected void TakeDamage()
    {
        animator.SetTrigger("playerHit");
    }
}
