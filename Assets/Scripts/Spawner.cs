using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[RequireComponent(typeof (BoxCollider))]
public class Spawner : MonoBehaviour
{
    public Team SpawningTeam;
    public GameObject UnitPrefab;

    private Tile SpawningTile;

    // Use this for initialization
    void Start ()
    {
        if (UnitPrefab == null)
            Debug.LogError("CRITICAL ERROR: No UnitPrefab in spawner!");
    }

    void OnTriggerEnter(Collider col)
    {
        if(col.gameObject.GetComponent<Tile>() != null)
            SpawningTile = col.gameObject.GetComponent<Tile>();         
    }

    public void Spawn()
    {
        GameObject spawner = (GameObject)Instantiate(UnitPrefab, SpawningTile.gameObject.transform.position, UnitPrefab.transform.rotation);
        if (spawner)
        {
            //Spawnujemy postac
            spawner.GetComponent<UnitInit>().UnitTeam = SpawningTeam;
            //Przypisujemy tile, na którym stoi,że ta postać tam stoi
            SpawningTile.SetOccupier(spawner.GetComponent<UnitManager>());
            spawner.GetComponent<PointClickMovement>().SetLastTile(SpawningTile);
            spawner.GetComponent<UnitManager>().CoverValues = SpawningTile.CoverValues;
            //Przypisujemy postaci imię i nazwisko
            spawner.GetComponent<UnitManager>().UnitConfig.name = GameObject.FindGameObjectWithTag("GameController").GetComponent<NamesLibrary>().AssignRandomName(SpawningTeam);
            spawner.GetComponent<UnitManager>().UnitConfig.surname = GameObject.FindGameObjectWithTag("GameController").GetComponent<NamesLibrary>().AssignRandomSurname(SpawningTeam);
            //Nazywamy odpowiednio postać
            spawner.name = spawner.GetComponent<UnitManager>().UnitConfig.name + " " + spawner.GetComponent<UnitManager>().UnitConfig.surname;
        }
    }
}
