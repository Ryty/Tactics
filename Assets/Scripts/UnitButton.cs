using UnityEngine;
using UnityEngine.UI;

public class UnitButton : MonoBehaviour
{
    public UnitManager CorrespondingUnit;

	// Update is called once per frame
	void Update ()
    {
        if (CorrespondingUnit != null && CorrespondingUnit.GetIsActive())
            GetComponent<Button>().interactable = false;
        else
            GetComponent<Button>().interactable = true;
    }
}
