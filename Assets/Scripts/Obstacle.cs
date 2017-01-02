using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum Orientation
{
    North,
    South,
    East,
    West
};

[RequireComponent(typeof(NavMeshObstacle))]
[RequireComponent(typeof (BoxCollider))]
public class Obstacle : MonoBehaviour
{
    public int CoverValue;
    public bool[] CoverClearances = new bool[4]; //0 - north, 1 - south, 2 - west, 3 -east
    public LayerMask OrientCheckMask = 2048;
    public GameObject sightCatcher;
    
    private BoxCollider BoxComp;
    private Dictionary<Tile, Orientation> CoverTiles = new Dictionary<Tile, Orientation>(); //Słownik zawierający tile przywierające do osłony i ich orientację względem niej
    private NavMeshObstacle NavObstacle;
   
	// Use this for initialization
	void Start ()
    {
        this.gameObject.layer = 11;

        BoxComp = GetComponent<BoxCollider>();
        NavObstacle = GetComponent<NavMeshObstacle>();

        CreateSightCatcher();
	}

    void Awake()
    {
        StartCoroutine(HandleCoverCreation());
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.GetComponent<Tile>() != null)
            CheckOrientation(col.gameObject.GetComponent<Tile>());
    }

    IEnumerator HandleCoverCreation()
    {
        yield return new WaitForEndOfFrame();
        ProvideCover();
        yield return new WaitForEndOfFrame();
        BoxComp.enabled = false;
    }
 
	void ProvideCover()
    {
        foreach (KeyValuePair<Tile, Orientation> valpar in CoverTiles)
        {
            if (valpar.Value == Orientation.North && CoverClearances[0] != false) //Jeśli tile jest na północy, to musimy zmienić jego value covera południowego
            {
                //Sprawdzamy, czy czasem tam już nie ma lepszego covera
                int oldVal = 0;
                valpar.Key.CoverValues.TryGetValue(Orientation.South, out oldVal);
                if (oldVal < CoverValue)
                {
                    valpar.Key.CoverValues.Remove(key: Orientation.South);
                    valpar.Key.CoverValues.Add(Orientation.South, CoverValue);
                }
            }
            if (valpar.Value == Orientation.South && CoverClearances[1] != false) //Jeśli tile jest na południe, to musimy zmienić jego value covera północnego
            {
                int oldVal = 0;
                valpar.Key.CoverValues.TryGetValue(Orientation.North, out oldVal);
                if (oldVal < CoverValue)
                {
                    valpar.Key.CoverValues.Remove(key: Orientation.North);
                    valpar.Key.CoverValues.Add(Orientation.North, CoverValue);
                }
            }
            if (valpar.Value == Orientation.West && CoverClearances[2] != false) //Jeśli tile jest na zachodzie, to musimy zmienić jego value covera wschodniego
            {
                int oldVal = 0;
                valpar.Key.CoverValues.TryGetValue(Orientation.East, out oldVal);
                if (oldVal < CoverValue)
                {
                    valpar.Key.CoverValues.Remove(key: Orientation.East);
                    valpar.Key.CoverValues.Add(Orientation.East, CoverValue);
                }
            }
            if (valpar.Value == Orientation.East && CoverClearances[3] != false) //Jeśli tile jest na wschodzie, to musimy zmienić jego value covera zachodniego
            {
                int oldVal = 0;
                valpar.Key.CoverValues.TryGetValue(Orientation.West, out oldVal);
                if (oldVal < CoverValue)
                {
                    valpar.Key.CoverValues.Remove(key: Orientation.West);
                    valpar.Key.CoverValues.Add(Orientation.West, CoverValue);
                }
            }
        }
    }

    void CheckOrientation(Tile tile)
    {
        Collider col = gameObject.GetComponent<Collider>();

        RaycastHit hit;
        Ray[] rays = new Ray[4];
        rays[0] = new Ray(tile.transform.position, -Vector3.forward);
        rays[1] = new Ray(tile.transform.position, Vector3.forward);
        rays[2] = new Ray(tile.transform.position, Vector3.right);
        rays[3] = new Ray(tile.transform.position, Vector3.left);
        
        if (col.Raycast(rays[0], out hit, 1f) && CoverClearances[0]) //Sprawdzamy górę
        {
            if (hit.collider.gameObject == this.gameObject)
            {
                CoverTiles.Add(tile, Orientation.North);
            }
        }
        else if (col.Raycast(rays[1], out hit, 1f) && CoverClearances[1]) //Sprawdzamy dół
        {
            if (hit.collider.gameObject == this.gameObject)
            {
                CoverTiles.Add(tile, Orientation.South);
            }
        }
        else if (col.Raycast(rays[2], out hit, 1f) && CoverClearances[2]) //Sprawdzamy prawo
        {
            if (hit.collider.gameObject == this.gameObject)
            {
                CoverTiles.Add(tile, Orientation.West);
            }
        }
        else if (col.Raycast(rays[3], out hit, 1f) && CoverClearances[3]) //Sprawdzamy lewo
        {
            if (hit.collider.gameObject == this.gameObject)
            {
                CoverTiles.Add(tile, Orientation.East);
            }
        }
    }

    void CreateSightCatcher()
    {
        sightCatcher = new GameObject(gameObject.name + " sightcatcher");
        sightCatcher.transform.position = gameObject.transform.position;
        sightCatcher.transform.rotation = gameObject.transform.rotation;
        sightCatcher.transform.SetParent(gameObject.transform);
        sightCatcher.layer = 12;
        sightCatcher.isStatic = true;

        BoxCollider boxColl = sightCatcher.AddComponent<BoxCollider>();
        boxColl.isTrigger = true;
        boxColl.size = new Vector3(NavObstacle.size.x * gameObject.transform.localScale.x, GameObject.FindGameObjectWithTag("GameController").GetComponent<LevelInit>().FloorHeight - 0.5f, NavObstacle.size.z * gameObject.transform.localScale.z);
    }
}
