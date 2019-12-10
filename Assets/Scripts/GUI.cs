using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUI : MonoBehaviour
{
    public GameObject upperLeftText;
    public GameObject upperMiddleText;
    private Text actionText;
    private Text turnText;
    public Dictionary<Team, string> TeamNames = new Dictionary<Team, string>
    {
        {Team.Player, "Player" },
        {Team.Enemy, "Enemy" }
    };
    public Dictionary<Action, string> ActionNames = new Dictionary<Action, string>
    {
        {Action.Move, "Move" },
        {Action.Attack, "Attack" }
    };
    private void Start()
    {
        actionText = upperMiddleText.GetComponent<Text>();
        turnText = upperLeftText.GetComponent<Text>();

    }
    // Update is called once per frame
    void Update()
    {
        if (_GameManager.instance.Cursor.selectedUnit != null)
        { //ya this aint good but it's tentative
            actionText.text = "Selected Unit: { Team: "+TeamNames[_GameManager.instance.Cursor.selectedUnit.team]+", Phase: "+ ActionNames[_GameManager.instance.Cursor.selectedUnit.controller.unitsEnum.Current.Item2]+"}" +
                "\n The camera locks post movement of your unit. Press Z to pass attack.";
        }
        else
        {
            actionText.text = "No unit selected"; 
        }
        turnText.text = "Current turn is: " + TeamNames[_GameManager.instance.CurrentController.Team];
    }
}
