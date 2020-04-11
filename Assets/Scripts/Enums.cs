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
    Reverse,
    Rotate
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
    None,
    Up,
    Right,
    Down,
    Left
}

public enum MenuBufferingType
{
    None,
    Initial,
    Full
}

public enum MovementTypes
{
    None,
    Ground,
    Flying
}

public enum WinConditions
{
    None,
    Rout
}

public enum GameStates
{
    None,
    UnitPathCreation,
    UnitPathConclusion,
    GUIMenuing
}

public enum PathCreationStepTypes
{
    None,
    Rotation,
    Translation
}