using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cursor : MonoBehaviour
{
    private _GameManager gameManager;
    private Vector2 position = new Vector2(0,0);
    public float moveTime = 0.1f;
    private float inverseMoveTime;
    private bool moving;

    private void Start()
    {
        inverseMoveTime = 1 / moveTime;
        gameManager = _GameManager.instance;
    }

    // Update is called once per frame
    void Update()
    {
        position.x = Mathf.Floor(transform.position.x);
        position.y = Mathf.Floor(transform.position.y);
        if (!moving)
        {
            float x = Input.GetAxisRaw("Horizontal") * Time.deltaTime;
            float y = Input.GetAxisRaw("Vertical") * Time.deltaTime;
            if (Mathf.Abs(x) > Mathf.Epsilon || Mathf.Abs(y) > Mathf.Epsilon)
            {
                if (Mathf.Abs(x) < Mathf.Epsilon)
                {
                    x = 0;
                }
                else
                {
                    x = x < 0 ? -1 : 1;
                }

                if (Mathf.Abs(y) < Mathf.Epsilon)
                {
                    y = 0;
                }
                else
                {
                    y = y < 0 ? -1 : 1;
                }

                StartCoroutine(SmoothMovement(transform.position + new Vector3(x,y, 0)));
            }
        }
        gameManager.cursorPosition = transform.position;
    }

    IEnumerator SmoothMovement(Vector3 end)
    {
        moving = true;
        float sqrRemainingDistance = (transform.position - end).sqrMagnitude;
        while (sqrRemainingDistance > float.Epsilon)
        {
            Vector3 newPosition = Vector3.MoveTowards(transform.position, end, inverseMoveTime * Time.deltaTime);
            transform.SetPositionAndRotation(newPosition, Quaternion.identity);
            sqrRemainingDistance = (transform.position - end).sqrMagnitude;
            yield return null;
        }
        moving = false;
    }
}
