using UnityEngine;
using System.Collections.Generic;

public class Tile : MonoBehaviour
{
    public Dictionary<Orientation, int> CoverValues = new Dictionary<Orientation, int>();

    private UnitManager Occupier;
    private bool bIsWalkable = true;
    
	// Use this for initialization
	void Start ()
    {
        if (!Physics.Raycast(transform.position, Vector3.down, 1f, 1 << 9))
            Destroy(transform.parent.gameObject);

        if (Physics.Raycast(transform.position, Vector3.up, 1f, 1 << 11 | 1 << 12))
            bIsWalkable = false;

        ResetCover();
	}

    void ResetCover()
    {
        CoverValues.Clear();
        CoverValues.Add(Orientation.North, 0);
        CoverValues.Add(Orientation.South, 0);
        CoverValues.Add(Orientation.West, 0);
        CoverValues.Add(Orientation.East, 0);
    }

    /*GETTERY*/
    public UnitManager GetOccupier() { return Occupier; }
    public bool IsWalkable() { return bIsWalkable; }
    /*SETTERY*/
    public void SetOccupier(UnitManager tar)
    {
        Occupier = tar;
    }
}
