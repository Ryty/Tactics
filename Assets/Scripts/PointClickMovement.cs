using UnityEngine;

[RequireComponent(typeof (UnitManager))]
[RequireComponent(typeof (NavMeshAgent))]
[RequireComponent(typeof (LineRenderer))]
public class PointClickMovement : MonoBehaviour
{ 

    private NavMeshAgent NavAgent;
    private LineRenderer Line;
    private Tile LastTile;
    private UnitManager Manager;

    void Start()
    {
        NavAgent = GetComponent<NavMeshAgent>();
        Line = GetComponent<LineRenderer>();
        Manager = GetComponent<UnitManager>();
    }

    public void MoveTo(Tile target)
    {
        if (FindPath(target.transform.position))
        {
            //Czyści obecność postaci na polu
            if(LastTile != null)
                LastTile.SetOccupier(null);
            //Zabiera stamine
            NavMeshPath path = new NavMeshPath();
            NavMesh.CalculatePath(transform.position, target.transform.position, NavMesh.AllAreas, path);
            Manager.DrainStamina(FindPathLength(path) * VariableLibrary.MoveCost_Normal);
            //Rozpoczyna ruch, przypisuje nowego tile jako miejsce pobytu postaci
            NavAgent.SetDestination(target.transform.position);
            target.SetOccupier(Manager);
            LastTile = target;
            Manager.CoverValues = LastTile.CoverValues;
        }
    }

    public bool FindPath(Vector3 tar)
    {
        NavMeshPath path = new NavMeshPath();
        NavMesh.CalculatePath(transform.position, tar, NavMesh.AllAreas, path);
        //Sprawdzamy, czy warto nią w ogóle iść
        if (path.status != NavMeshPathStatus.PathPartial && path.status != NavMeshPathStatus.PathInvalid)
        {
            if (FindPathLength(path) * VariableLibrary.MoveCost_Normal <= Manager.GetStamina()) //jeśli mamy wystarczająco staminy by tam pójść
            {
                DrawPath(path);
                Manager.SetCalculatedStamina(Manager.GetStamina() - FindPathLength(path) * VariableLibrary.MoveCost_Normal);
                return true;
            }
            else
            {
                GetLine().SetVertexCount(1);
                Manager.SetCalculatedStamina(Manager.GetStamina());
                return false;
            }
        }

        Manager.SetCalculatedStamina(Manager.GetStamina());
        return false;
    }

    void DrawPath(NavMeshPath path)
    {
        Line.SetVertexCount(path.corners.Length);

        for (int i = 0; i < path.corners.Length; i++)
            Line.SetPosition(i, path.corners[i] + new Vector3(0.0f, GetComponent<CapsuleCollider>().height/2, 0.0f));
    }

    int FindPathLength(NavMeshPath path)
    {
        int returnVal = 0;
        for (int i = 0; i < path.corners.Length - 1; i++)
        {
            returnVal += (int)Vector3.Distance(path.corners[i], path.corners[i + 1]);
        }

        return returnVal;
    }

    /*GETTERY*/
    public LineRenderer GetLine() { return Line; }
    public Tile GetLastTile() { return LastTile; }
    /*SETTERY*/
    public void SetLastTile(Tile tile) { LastTile = tile; }
}
