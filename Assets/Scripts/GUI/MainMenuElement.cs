using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class MainMenuElement : MenuElementMonoBehavior, IMenuElement
{

    [SerializeField]
    /// <summary>
    /// Where this element shall place its contents (the text it's meant to be a header for).
    /// </summary>
    public GameObject stagingAreaObject;

    public override void SetSelected(bool _selected)
    {
        gameObject.transform.Find("Text").localPosition = new Vector3(_selected ? -30 : 0, 0, 0);
        stagingAreaObject.SetActive(_selected);
        Selected = _selected;
    }

    public override void SetActive(bool _active)
    {
        gameObject.SetActive(_active);

        if (!_active) //We do not want to make the stagingAreaObject active (visible) just becase this header is visible. We only want to ever disable the stagingAreaObject here.
            stagingAreaObject.SetActive(false);
    }
}