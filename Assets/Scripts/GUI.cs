using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUI : MonoBehaviour
{
    public GameObject textPrefab;
    public GameObject upperLeftText;
    public GameObject upperRightText;
    private Text actionText;
    private Text turnText;
    public Unit SelectedUnit
    {
        get
        {
            return selectedUnit;
        }
        set
        {
            UpdateSelectedUnit(value);
            selectedUnit = value;
        }
    }
    private Unit selectedUnit;
    private Dictionary<Team, string> teamNames = new Dictionary<Team, string>
    {
        {Team.Player1, "Player One" },
        {Team.Player2, "Player Two" },
        {Team.Enemy, "Enemy" }
    };
    private Dictionary<ActionNames, string> actionNames = new Dictionary<ActionNames, string>
    {
        {ActionNames.Move, "Move" },
        {ActionNames.Attack, "Attack" }
    };
    private void Awake()
    {
        upperRightText = Instantiate(textPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        upperRightText.name = "UpperRightText";
        upperRightText.transform.parent = transform;
        upperRightText.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
        upperRightText.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
        upperRightText.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
        upperRightText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);

        actionText = upperRightText.GetComponent<Text>();
        actionText.alignment = TextAnchor.MiddleLeft;

        upperLeftText = Instantiate(textPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        upperLeftText.name = "UpperRightText";
        upperLeftText.transform.parent = transform;
        upperLeftText.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
        upperLeftText.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
        upperLeftText.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
        upperLeftText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -80);

        turnText = upperLeftText.GetComponent<Text>();
        turnText.alignment = TextAnchor.MiddleLeft;
    }

    public void UpdateSelectedUnit(Unit _unit)
    {
        if (_unit == null)
        {
            actionText.text = "No unit is currently selected.";
        }
        else
        {
            actionText.text = "A unit of " + teamNames[_unit.team] + " is selected";
        }
    }

    public void UpdateCurrentTeam(Commander _commander)
    {
        if (_commander == null)
        {
            turnText.text = "No player's turn";
        }
        else
        {
            turnText.text = teamNames[_commander.Team] + "'s turn";
        }
    }
}
