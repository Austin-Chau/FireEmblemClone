using System;
using System.Collections;
using UnityEngine;

public class Cursor : MonoBehaviour
{
    private GameManager gameManager;

    public float moveTime = 0.1f;
    private float inverseMoveTime;

    public bool Moving { get; private set; }

    public Tile CurrentTile { get; private set; }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        inverseMoveTime = 1 / moveTime;
        gameManager = GameManager.instance;
        CurrentTile = gameManager.Board.Tiles[0, 0]; //whatever initial position
        transform.position = CurrentTile.Position; //snap to that tile
    }

    public void Move(AdjacentDirection _direction)
    {
        Tile destinationTile = CurrentTile.GetAdjacentTile(_direction);
        if (destinationTile != null && destinationTile != CurrentTile)
        {
            StartCoroutine(SmoothMovement(destinationTile));
        }
    }

    private IEnumerator SmoothMovement(Tile destinationTile)
    {
        Moving = true;
        float sqrRemainingDistance = (transform.position - destinationTile.Position).sqrMagnitude;
        while (sqrRemainingDistance > float.Epsilon)
        {
            Vector3 newPosition = Vector3.MoveTowards(transform.position, destinationTile.Position, inverseMoveTime * Time.deltaTime);
            //rb2D.MovePosition(newPosition); (we might want rigid body for smooth movement)
            transform.position = newPosition;
            sqrRemainingDistance = (transform.position - destinationTile.Position).sqrMagnitude;
            yield return null;
        }
        CurrentTile = destinationTile;
        transform.position = CurrentTile.Position; //snap the position just in case the unit is slightly off
        Moving = false;
    }

    public void JumpToTile(Tile _destinationTile)
    {
        CurrentTile = _destinationTile;
        transform.position = CurrentTile.Position;
    }
}
