using UnityEngine;
using System.Collections.Generic;

public class NamesLibrary : MonoBehaviour
{
    public TextAsset TextNamesGreen;
    public TextAsset TextSurnamesGreen;

    public TextAsset TextNamesRed;
    public TextAsset TextSurnamesRed;

    private List<string> NamesGreen = new List<string>();
    private List<string> SurnamesGreen = new List<string>();
    private List<string> NamesRed = new List<string>();
    private List<string> SurnamesRed = new List<string>();


    // Use this for initialization
    void Start ()
    {
        readTextFileLines(TextNamesGreen, NamesGreen);
        readTextFileLines(TextSurnamesGreen, SurnamesGreen);
        readTextFileLines(TextNamesRed, NamesRed);
        readTextFileLines(TextSurnamesRed, SurnamesRed);
    }

    void readTextFileLines(TextAsset file, List<string> inputList)
    {
        string[] linesInFile = file.text.Split('\n');

        foreach (string line in linesInFile)
        {
            inputList.Add(line);
        }
    }

    public string AssignRandomName(Team team)
    {
        List<string> assignList = new List<string>();
        switch(team)
        {
            case Team.Green:
                assignList = NamesGreen;
                break;
            case Team.Red:
                assignList = NamesRed;
                break;
            default:
                break;
        }

        int randomNum = Random.Range(0, assignList.Count);

        return assignList[randomNum];
    }

    public string AssignRandomSurname(Team team)
    {
        List<string> assignList = new List<string>();
        switch (team)
        {
            case Team.Green:
                assignList = SurnamesGreen;
                break;
            case Team.Red:
                assignList = SurnamesRed;
                break;
            default:
                break;
        }

        int randomNum = Random.Range(0, assignList.Count);

        return assignList[randomNum];
    }
}
