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
    protected List<ActionSpace> moveSpaces = new List<ActionSpace>();

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
        _GameManager.instance.board.Tiles[(int)coordinates.x, (int)coordinates.y].Occupied = true;
    }

    /// <summary>
    /// Moves a unit to the given position, using the unit's pathfinding parameters.
    /// </summary>
    /// <param name="position">The final position.</param>
    public virtual void MetaMove (Vector3 position)
    {
        int x = (int)Mathf.Round(position.x);
        int y = (int)Mathf.Round(position.y);

        Stack<Vector2> steps = Pathfinding.GenerateSteps(coordinates, new Vector2(x,y));

        //Now, given a list of unit vectors, 
        //combine consecutive vectors in the same direction to create smooth movements.
        List<Vector2> collapsedSteps = new List<Vector2>();
        if (steps.Count != 0)
        {
            Vector2 accumulateVector = steps.Pop();
            if (steps.Count > 0)
            {
                while (steps.Count > 0)
                {
                    Vector2 tempVector = steps.Pop();
                    if (Vector2.Angle(accumulateVector, tempVector) < Mathf.Epsilon)
                    {
                        accumulateVector = accumulateVector + tempVector;
                    }
                    else
                    {
                        collapsedSteps.Add(accumulateVector);
                        accumulateVector = tempVector;
                    }
                }
                collapsedSteps.Add(accumulateVector);
            }
            else
            {
                collapsedSteps.Add(accumulateVector);
            }
        }

        StartCoroutine(SequenceOfMoves(collapsedSteps));

    }

    //An enumerator for the steps of the movement, moves the player according to each vector
    protected IEnumerator SequenceOfMoves (List<Vector2> steps) 
    {
        moving = true;
        _GameManager.instance.board.Tiles[(int)coordinates.x, (int)coordinates.y].Occupied = false;
        foreach (var step in steps) {
            yield return StartCoroutine(SmoothMovement(new Vector3(coordinates.x+step.x,coordinates.y+step.y,0)));
        }
        moving = false;
        moved = true;
        _GameManager.instance.board.Tiles[(int)coordinates.x, (int)coordinates.y].Occupied = true;
        StartActPhase();
    }

    //Moves the object smoothly to the end point 
    protected IEnumerator SmoothMovement (Vector3 end)
    {
        float sqrRemainingDistance = (transform.position - end).sqrMagnitude;
        while (sqrRemainingDistance > float.Epsilon)
        {
            Vector3 newPosition = Vector3.MoveTowards(rb2D.position, end, inverseMoveTime * Time.deltaTime);
            rb2D.MovePosition(newPosition);
            sqrRemainingDistance = (transform.position - end).sqrMagnitude;
            coordinates.Set(Mathf.Floor(transform.position.x), Mathf.Floor(transform.position.y));
            yield return null;
        }
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

    //Draws the move squares all around the unit and prepares that unit for movement
    protected virtual void DrawMoveSquares()
    {
        if (moveSpaces.Count > 0)
        {
            return;
        }
        List<Vector2> movePositions = Pathfinding.GenerateMoveTree(coordinates,moveRadius);

        moveSpaces = new List<ActionSpace>();
        foreach (Vector2 position in movePositions)
        {
            ActionSpace moveSpaceScript = Instantiate(MoveSpace, position, Quaternion.identity).GetComponent<ActionSpace>();
            moveSpaces.Add(moveSpaceScript);
        }
    }

    //Erases the move squares around the unit
    protected void EraseMoveSquares()
    {
        foreach (var i in moveSpaces)
        {
            Destroy(i.gameObject);
        }
        moveSpaces = new List<ActionSpace>();
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
