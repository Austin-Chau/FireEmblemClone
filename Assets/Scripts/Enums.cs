using UnityEngine;
using System.Collections;

public enum ActionNames
{
    Move,
    Attack
}

public enum CommandNames
{
    None,
    InitializeMove,
    GenerateMoveSpaces,
    Move,
    InitializeAttack,
    Attack,
    Cancel,
    EndTurn,
    Revert
}

public enum Team
{
    None,
    Player1,
    Player2,
    Enemy
}

public enum AdjacentDirection
{
    Up,
    Right,
    Down,
    Left,
    None
}