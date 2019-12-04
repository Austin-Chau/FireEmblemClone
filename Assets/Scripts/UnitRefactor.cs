using UnityEngine;
using System.Collections;

public class UnitRefactor : MonoBehaviour
{
    UnitStateMachine stateMachine = new UnitStateMachine();
    bool cursorHovering = false;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        stateMachine.Update(cursorHovering);
    }

    void OnCursorHover()
    {
        cursorHovering = true;
        stateMachine.currentState.CursorHover();
    }

    void OnCursorHoverExit()
    {
        cursorHovering = false;
        stateMachine.currentState.CursorHoverExit();
    }

    void OnCursorSelect()
    {
        stateMachine.currentState.CursorSelect();
    }
    
}

