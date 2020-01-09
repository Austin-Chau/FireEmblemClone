using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class MainMenuElement : MenuElement
{
    private GameObject stagingAreaObject;

    public MainMenuElement(GameObject _attachedGameObject, string _labelText, GameObject _stagingAreaObject) : base(_attachedGameObject)
    {
        attachedGameObject.transform.Find("Text").GetComponent<Text>().text = _labelText;
        stagingAreaObject = _stagingAreaObject;
        //temp:
        stagingAreaObject.GetComponent<Text>().text = _labelText + " Body";
    }

    public override void SetSelected(bool _selected)
    {
        attachedGameObject.transform.Find("Text").localPosition = new Vector3(_selected ? -30 : 0, 0, 0);
        stagingAreaObject.SetActive(_selected);
        Selected = _selected;
    }

    public override void SetActive(bool _active)
    {
        attachedGameObject.SetActive(_active);

        if (!_active)
            stagingAreaObject.SetActive(false);
    }
}