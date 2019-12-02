using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionSpace : MonoBehaviour
{
    public int collidersIndex;
    public Unit unitScript;
    public string action;

    // Update is called once per frame
    void Update()
    {
        if (unitScript == null)
        {
            return;
        }
        if (Input.GetButtonDown("confirm"))
        {
            Vector3 position = _GameManager.instance.cursorPosition;
            if (unitScript.team == "player" && Mathf.Abs(transform.position.x - position.x) < .5 && Mathf.Abs(transform.position.y - position.y) < .5)
            {
                if (action == "act")
                {
                    //unitScript.ActOnCollider(collidersIndex);
                }
                else if (action == "move")
                {
                    unitScript.MetaMove(transform.position);
                }
            }
        }
    }
}
