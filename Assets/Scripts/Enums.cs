using UnityEngine;
using System.Collections;

public enum ActionNames
{
    Move,
    Attack
}

public enum ControlsEnum
{
    Null,
    Confirm,
    OpenMainMenu,
    Reverse
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

public enum MenuBufferingType
{
    None,
    Initial,
    Full
}