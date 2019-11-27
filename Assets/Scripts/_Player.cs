using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _Player : _MovingObject
{
    private Animator animator;
    
    protected override void Start()
    {
        base.Start();
        coordinates = new Vector2(1, 1);
        animator = GetComponent<Animator>();
        rb2D.MovePosition(coordinates);
    }

    protected override void Update()
    {
        base.Update();
        //if it is not the players turn, the player doesn't update
        if (!_GameManager.instance.playersTurn)
        {
            return;
        }

        //gets the mouse position and feeds it as a movement instruction
        if (Input.GetMouseButtonDown(0) && !moving)
        {
            Vector2 mousePosition = Input.mousePosition;
            Vector3 position = new Vector3(mousePosition.x, mousePosition.y, 10f);
            position = Camera.main.ScreenToWorldPoint(position);
            MetaMove((int)Mathf.Floor(position.x+0.5f), (int)Mathf.Floor(position.y+0.5f));
        }
    }

    //after moving, sets the players turn as spent
    protected override void MetaMove(int x, int y) {
        base.MetaMove(x, y);

        _GameManager.instance.playersTurn = false;
    }
}
