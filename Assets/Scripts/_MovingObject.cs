using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class _MovingObject : MonoBehaviour
{
    //how long this boi should take to move between two tiles
    public float moveTime = 0.1f;

    //what layer should this boi look to for collisions
    public LayerMask blockingLayer;

    //the coordinates are the bottom left corner of the tile
    protected Vector2 coordinates;

    protected BoxCollider2D boxCollider;
    protected Rigidbody2D rb2D;
    private float inverseMoveTime;
    protected bool moving = false;

    protected virtual void Start()
    {
        coordinates = new Vector2(0, 0);
        boxCollider = GetComponent<BoxCollider2D>();
        rb2D = GetComponent<Rigidbody2D>();
        inverseMoveTime = 1f / moveTime;
    }

    //Moves in a fire emblem style 
    //The parameters are the position on the field this unit should go to.
    //Currently has no pathfinding, assumes the field is empty.
    protected virtual void MetaMove (int x, int y)
    {
        List<Vector2> steps = new List<Vector2>();

        bool right = coordinates.x < x;
        bool up = coordinates.y < y;

        for (int i = 0; i < Mathf.Abs(x - coordinates.x);i++)
        {
            steps.Add(right ? new Vector2(1,0): new Vector2(-1,0));
        }
        for (int i = 0; i < Mathf.Abs(y - coordinates.y); i++)
        {
            steps.Add(up ? new Vector2(0, 1): new Vector2(0, -1));
        }
        StartCoroutine(SequenceOfMoves(steps));

    }

    //An enumerator for the steps of the movement
    protected IEnumerator SequenceOfMoves (List<Vector2> steps) 
    {
        moving = true;
        foreach (var i in steps) {
            yield return StartCoroutine(SmoothMovement(new Vector3(coordinates.x+i.x,coordinates.y+i.y,0)));
        }
        moving = false;
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

    //there is probably a much more elegant way to do this
    protected IEnumerator EndMovement ()
    {
        moving = false;
        yield return null;
    }

    protected virtual void Update()
    {
        //coordinates.Set(Mathf.Floor(transform.position.x), Mathf.Floor(transform.position.y));
    }
}
