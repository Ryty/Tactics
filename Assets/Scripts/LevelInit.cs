using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelInit : MonoBehaviour
{
    public Tile TilePrefab;

    public int TileSize = 1;
    public int GridCountX;
    public int GridCountZ;
    public int FloorHeight = 3;
    public int FloorAmount;
    public int TileVisibilityRange;

    public int UnitsPerTeam;

    private List<Spawner> Spawners = new List<Spawner>();

    // Use this for initialization
    void Start()
    {
        if (TilePrefab == null)
            Debug.LogError("CRITICAL ERROR: No tile prefab selected!");

        StartCoroutine(CreateWorld());
    }

    IEnumerator CreateWorld()
    {
        CreateFloors();

        yield return new WaitForEndOfFrame();

        CreateUnits();
    }
    void CreateFloors()
    {
        for (int i = 0; i < FloorAmount; i++)
        {
            GameObject go = new GameObject();
            go.name = "Floor " + i;
            CreateGrid(i * FloorHeight, go);
        }
    }

    void CreateGrid(int y, GameObject parent)
    {
        for (int i = 0; i < GridCountX; i++)
            for (int j = 0; j < GridCountZ; j++)
            {
                Tile spawner = (Tile)Instantiate(TilePrefab, new Vector3((i + TileSize / 2f) * TileSize, y + 0.1f, (j + TileSize / 2f) * TileSize), TilePrefab.transform.rotation);
                spawner.transform.parent = parent.transform;
                spawner.name = i + " " + j;
            }
    }

    void CreateUnits()
    {
        FindSpawners();

        SpawnUnits(Team.Green);
        SpawnUnits(Team.Red);
    }

    void FindSpawners()
    {
        GameObject[] gos = GameObject.FindGameObjectsWithTag("Spawner");
        foreach (GameObject go in gos)
        {
            if (go.GetComponent<Spawner>() != null)
                Spawners.Add(go.GetComponent<Spawner>());
        }
    }

    void SpawnUnits(Team team)
    {
        List<Spawner> possibleSpawns = new List<Spawner>();

        for(int i = 0; i < Spawners.Count; i++)
            if (Spawners[i].SpawningTeam == team)
                possibleSpawns.Add(Spawners[i]);

        for (int i = 0; i < UnitsPerTeam; i++)
        {
            //Jeśli skończą nam sie spawnery
            if (possibleSpawns.Count <= 0)
            {
                Debug.Log("No more spawners avaible for " + team + ", but there are still units left to spawn.");
                break;
            }
            //Dopóki mamy gdzie spawnować, robimy to
            else
            {
                //Wybieramy spawnera do zespawnowania
                int num = Random.Range(0, possibleSpawns.Count);

                //spawnujemy tym spawnerem
                possibleSpawns[num].Spawn();

                //niszczymy ten spawner
                Destroy(possibleSpawns[num].gameObject);
                possibleSpawns.Remove(possibleSpawns[num]);
            }
        }
        //Jeśli pozostały po tym jakieś spawnery, to je niszczymy
        if(possibleSpawns.Count > 0)
            for (int i = 0; i < possibleSpawns.Count; i++)
                Destroy(possibleSpawns[i].gameObject);
    }
}
