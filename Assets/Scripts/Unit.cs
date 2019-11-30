using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Unit : MonoBehaviour
{
    /*
    Major current issues:
    -no actual collision detection/intelligent pathfinding, units can move anywhere any time and stand on each other
     */
       
    //Various layerMasks to test collisions against for various actions. Should be 
    //layer updated for custom usage of multiple layermasks (using binary)
    public LayerMask blockingLayer;
    public LayerMask actingLayer;

    public GameObject MoveSpace;
    public GameObject ActSpace;

    protected ActionSpace actionSpaceScript;

    //coordinates of this unit, i wasn't very consistant with the usage of this
    protected Vector2 coordinates;

    protected Animator animator;
    protected BoxCollider2D boxCollider;
    protected Rigidbody2D rb2D;

    //how long this boi should take to move between two tiles (kinda jank since it's implemented as a distance),
    //high values might not do anything since it just clamps the movement in each frame
    public float moveTime = 0.1f;
    //calculate the inversemovetime once to save computation time later on
    private float inverseMoveTime;

    protected bool unSelected;

    public bool moving = false;
    public bool acting = false;

    //which team is this guy on
    public string side;

    protected bool selected = false;

    //how far the object can act (at most)
    protected int actRadius = 1;
    protected List<ActionSpace> actSpaces = new List<ActionSpace>();
    //how far the object can move (at most)
    protected int moveRadius = 3;
    protected List<ActionSpace> moveSpaces = new List<ActionSpace>();

    //a bool for if a character has moved yet.
    public bool moved = false;

    //a bool for if a character has acted yet.
    public bool acted = false;

    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
        coordinates = new Vector2(0, 0);
        coordinates.Set(Mathf.Floor(transform.position.x), Mathf.Floor(transform.position.y));
        boxCollider = GetComponent<BoxCollider2D>();
        rb2D = GetComponent<Rigidbody2D>();
        inverseMoveTime = 1f / moveTime;
    }

    //Moves in a fire emblem style 
    //The parameters are the position on the field this unit should go to.
    //Currently has no pathfinding, assumes the field is empty (nothing that could stop its movement).
    public virtual void MetaMove (Vector3 position)
    {
        int x = (int)Mathf.Round(position.x);
        int y = (int)Mathf.Round(position.y);
        Stack<Vector2> steps = new Stack<Vector2>();

        //First, constructs a series of unit vectors to get the player to the desired spot.
        //This should eventually be replaced by pathfinding.
        bool right = coordinates.x < x;
        bool up = coordinates.y < y;
        for (int i = 0; i < Mathf.Abs(x - coordinates.x);i++)
        {
            steps.Push(right ? new Vector2(1,0): new Vector2(-1, 0));
        }
        for (int i = 0; i < Mathf.Abs(y - coordinates.y); i++)
        {
            steps.Push(up ? new Vector2(0, 1) : new Vector2(0, -1));
        }

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
        foreach (var step in steps) {
            yield return StartCoroutine(SmoothMovement(new Vector3(coordinates.x+step.x,coordinates.y+step.y,0)));
        }
        moving = false;
        moved = true;
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
        moveSpaces = new List<ActionSpace>();
        for (int i = -moveRadius; i <= moveRadius; i++)
        {
            for (int j = -(moveRadius - Mathf.Abs(i)); j <= moveRadius - Mathf.Abs(i); j++)
            {
                Vector3 end = new Vector3(transform.position.x, transform.position.y,0) + new Vector3(i, j,0);
                ActionSpace moveSpaceScript = Instantiate(MoveSpace, end, Quaternion.identity).GetComponent<ActionSpace>();
                moveSpaces.Add(moveSpaceScript);
            }
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
