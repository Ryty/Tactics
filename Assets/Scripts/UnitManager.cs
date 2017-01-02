using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum Class
{
    Rifleman,
    Assault,
    Heavy,
    Medic,
    Sniper
};

public enum Visibility
{
    Black,
    Green,
    Orange,
    Red
};

public enum PeekDir
{
    None,
    Left,
    Right
};

[RequireComponent(typeof (PointClickMovement))]
[RequireComponent(typeof (NavMeshAgent))]
[RequireComponent(typeof (NavMeshObstacle))]
public class UnitManager : MonoBehaviour
{
    public struct CharacterConfig
    {
        public string name;
        public string surname;
        public int healthMax;
        public int staminaMax;
        public Team UnitTeam;
        public Class UnitClass;
    };
        
    public PointClickMovement CharacterMovement;
    public Dictionary<Orientation, int> CoverValues = new Dictionary<Orientation, int>();
    public GameObject ActiveDisplay;
    public int UnitSightRange;
    public CharacterConfig UnitConfig;
    public LayerMask SightMask;
    public List<Ability> UnitAbilities = new List<Ability>();

    private NavMeshAgent NavAgent;
    private NavMeshObstacle NavObstacle;
    private CameraMovement CameraMovementScript;
    private GameStateManager StateManager;
    private CommandUnit CommanderScript;
    private List<UnitManager> Enemies = new List<UnitManager>();
    private SpriteRenderer classIcon;
    private bool isActive;
    private int currentHealth;
    private int currentStamina;
    private int calculatedStamina;    


    // -----------------------------------------------------Use this for initialization-----------------------------------
    void Start ()
    {
        CharacterMovement = GetComponent<PointClickMovement>();
        NavAgent = GetComponent<NavMeshAgent>();
        NavAgent.enabled = false;

        NavObstacle = GetComponent<NavMeshObstacle>();

        CameraMovementScript = GameObject.FindGameObjectWithTag("PlayerManager").GetComponent<CameraMovement>();
        if (CameraMovementScript == null)
            Debug.LogError("CRITICAL ERROR: UnitManager couldn't find CameraMovement script!");

        StateManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameStateManager>();
        if (StateManager == null)
            Debug.LogError("CRITICAL ERROR: UnitManager couldn't find StateManager script!");

        GameObject[] commandGo = GameObject.FindGameObjectsWithTag("PlayerManager");
        foreach(GameObject go in commandGo)
        {
            if (go.GetComponent<CommandUnit>().TeamCommanded == UnitConfig.UnitTeam)
                CommanderScript = go.GetComponent<CommandUnit>();
        }
        if (CommanderScript == null)
            Debug.Log("CRITICAL ERROR: UnitManager couldn't find CommanderScript for " + UnitConfig.UnitTeam.ToString() + " team!");

        isActive = false;

        SpriteRenderer[] renderers = ActiveDisplay.GetComponentsInChildren<SpriteRenderer>();
        foreach(SpriteRenderer sr in renderers)
        {
            if (sr.gameObject.tag == "ClassSign")
                classIcon = sr;
        }
	}

    void Awake()
    {
        ActiveDisplay.SetActive(false);
        StartCoroutine(FindAllEnemies());
        AssumeCoveredPosition();
    }

    void Update()
    {
    //    if(!(NavAgent.velocity.magnitude > 0f))
   //         AssumeCoveredPosition();

        ActiveDisplay.transform.eulerAngles = new Vector3(90.0f, 0.0f, 0.0f);
    }
    //-----------------------------------------------------Movement---------------------------------------------------------
    public void PlotPath(Vector3 target)
    {
        CharacterMovement.FindPath(target);
    }

    public void NavigateTo(Tile target)
    {
        CharacterMovement.MoveTo(target);
    }
    //----------------------------------------------------------Zaznaczanie/odznaczanie---------------------------------------
    /* Wywołuje Coroutine dla konkretnych stanów
     * @bool isSelected - true, jesli zaznaczamy postac, false jesli ją odznaczamy
     */
    public void HandleChangingState(bool isSelected) 
    {
        if (isSelected)
            StartCoroutine(HandleSelect());
        else
            StartCoroutine(HandleDeselect());
    }

    IEnumerator HandleSelect()
    {
        //Wyświetlanie podświetlenia
        ActiveDisplay.SetActive(true);
        //Ogarnięcie kamery
        CameraMovementScript.StartSwiping(gameObject);
        //Ogarnięcie siebie
        isActive = true;
        //Ogarnięcie navmesha----------------------------
        NavObstacle.carving = false;
        NavObstacle.enabled = false;
        //Czekamy 3 klatki na navmesha, żeby się ogarnął
        for(int i = 0; i < 4; i++)
            yield return new WaitForEndOfFrame();

        NavAgent.enabled = true;
        //----------------------------------------------
        //Ogarnięcie StateManagera
        StateManager.ActiveUnit = this;      
    }

    IEnumerator HandleDeselect()
    {
        ActiveDisplay.SetActive(false);
        SetCalculatedStamina(GetStamina());
        isActive = false;
        CharacterMovement.GetLine().SetVertexCount(1);
        NavAgent.enabled = false;

        yield return new WaitForEndOfFrame();

        NavObstacle.enabled = true;
        NavObstacle.carving = true;

        StateManager.ActiveUnit = null;
    }
    //Funkcja wywoływana gdy przycisk odpowiadający tej jednostce zostanie przyciśnięty
    public void HandleButtoned()
    {
        CommanderScript.HandleChangingUnit(this);
    }

    public void HandleBeingTargeted()
    {
        ActiveDisplay.SetActive(true);
    }

    public void HandleBeingDetargeted()
    {
        ActiveDisplay.SetActive(false);
    }
    //----------------------------------------------------------Cover, combat---------------------------------------
    public int FindEffectiveCover(UnitManager defender, UnitManager shooter)
    {
        if (FindCoveringCover(defender, shooter) != null) //Jeśli znaleźlismy covera zwykłym raycastem
        {
            return FindCoveringCover(defender, shooter).CoverValue;
        }
        else //Inaczej
        {
            //sprawdzamy kąt między północą obrońcy a atakującym
            Vector3 heading = defender.CharacterMovement.GetLastTile().transform.position - shooter.CharacterMovement.GetLastTile().transform.position;
            int defendAngle = (int)MathLibrary.AngleSigned(defender.CharacterMovement.GetLastTile().transform.forward, heading, Vector3.up);
            if (defendAngle < 0)
                defendAngle = 360 - Mathf.Abs(defendAngle);
            //Zależnie od tego kąta blendujemy obronę zapewnioną przez covera obrońcy
            if (defendAngle >= 0 && defendAngle <= 90)
            {
                //Przepisujemy potrzebne wartości i korzystamy ze wzoru (kąt/makskąt) * obrona w maks + ((makskąt - kąt)/makskąt) * obrona w minkąt
                int coverN;
                defender.CoverValues.TryGetValue(Orientation.North, out coverN);
                int coverE;
                defender.CoverValues.TryGetValue(Orientation.East, out coverE);
                return (((defendAngle * coverE) / 90) + (((90 - defendAngle) * coverN) / 90));
            }
            else if (defendAngle > 90 && defendAngle <= 180)
            {
                //Przepisujemy potrzebne wartości i korzystamy ze wzoru (kąt/makskąt) * obrona w maks + ((makskąt - kąt)/makskąt) * obrona w minkąt
                defendAngle -= 90;
                int coverE;
                defender.CoverValues.TryGetValue(Orientation.East, out coverE);
                int coverS;
                defender.CoverValues.TryGetValue(Orientation.South, out coverS);
                return (((defendAngle * coverS) / 90) + (((90 - defendAngle) * coverE) / 90));
            }
            else if (defendAngle > 180 && defendAngle <= 270)
            {
                //Przepisujemy potrzebne wartości i korzystamy ze wzoru (kąt/makskąt) * obrona w maks + ((makskąt - kąt)/makskąt) * obrona w minkąt
                defendAngle -= 180;
                int coverS;
                defender.CoverValues.TryGetValue(Orientation.South, out coverS);
                int coverW;
                defender.CoverValues.TryGetValue(Orientation.West, out coverW);
                return (((defendAngle * coverW) / 90) + (((90 - defendAngle) * coverS) / 90));
            }
            else if (defendAngle > 270 && defendAngle < 360)
            {
                //Przepisujemy potrzebne wartości i korzystamy ze wzoru (kąt/makskąt) * obrona w maks + ((makskąt - kąt)/makskąt) * obrona w minkąt
                defendAngle -= 270;
                int coverW;
                defender.CoverValues.TryGetValue(Orientation.West, out coverW);
                int CoverN;
                defender.CoverValues.TryGetValue(Orientation.North, out CoverN);
                return (((defendAngle * CoverN) / 90) + (((90 - defendAngle) * coverW) / 90));
            }
            else
                return -100;
        }
    }

    IEnumerator FindAllEnemies()
    {
        GameObject[] gos = GameObject.FindGameObjectsWithTag("Unit");
        yield return new WaitForEndOfFrame();
        foreach (GameObject go in gos)
        {
            if (go.GetComponent<UnitManager>().UnitConfig.UnitTeam != this.UnitConfig.UnitTeam)
                Enemies.Add(go.GetComponent<UnitManager>());
        }
    }

    UnitManager FindMostDangerousEnemy()
    {
        UnitManager mostDangerous = null;
        //Dajemy pierwszego z brzegu przeciwnika
        if (Enemies.Count != 0)
        {
            mostDangerous = Enemies[0];

            for (int i = 0; i < Enemies.Count; i++)
            {
                if (Vector3.Distance(this.transform.position, Enemies[i].transform.position) <= UnitSightRange && FindEffectiveCover(this, Enemies[i]) < FindEffectiveCover(this, mostDangerous) && CheckSight(Enemies[i], this) != Visibility.Black)
                    mostDangerous = Enemies[i];
                else if (Vector3.Distance(this.transform.position, Enemies[i].transform.position) < Vector3.Distance(this.transform.position, mostDangerous.transform.position) && FindEffectiveCover(this, Enemies[i]) == FindEffectiveCover(this, mostDangerous) && CheckSight(Enemies[i], this) != Visibility.Black)
                    mostDangerous = Enemies[i];
            }

            if (Vector3.Distance(this.transform.position, mostDangerous.transform.position) <= UnitSightRange)
                return mostDangerous;
            else
                return null;            
        }
        else
            return null;
    }

    public void AssumeCoveredPosition(UnitManager coverAgainst = default(UnitManager))
    {
        //Spisujemy wartości coverów postaci
        int[] covervals = new int[4];
        CoverValues.TryGetValue(Orientation.North, out covervals[0]);
        CoverValues.TryGetValue(Orientation.South, out covervals[1]);
        CoverValues.TryGetValue(Orientation.West, out covervals[2]);
        CoverValues.TryGetValue(Orientation.East, out covervals[3]);
        //Jeśli nie mamy konkretnego celu, to chowamy się przed najniebezpieczniejszym
        if(coverAgainst == null)
            coverAgainst = FindMostDangerousEnemy();

        if (coverAgainst != null) 
        {
            //Jeśli tak - sprawdźmy czy mamy jakikolwiek cover przeciwko niemu
            if (FindEffectiveCover(this, coverAgainst) > 0)
            {
                //Jeśli mamy - sprawdźmy kąt 
                Vector3 heading = this.CharacterMovement.GetLastTile().transform.position - coverAgainst.CharacterMovement.GetLastTile().transform.position;
                int defendAngle = (int)MathLibrary.AngleSigned(this.CharacterMovement.GetLastTile().transform.forward, heading, Vector3.up);
                if (defendAngle < 0)
                    defendAngle = 360 - Mathf.Abs(defendAngle);
                //i ustawmy się w stronę covera który jest najbliższy kątem
                if (defendAngle > 315 || defendAngle <= 45 && covervals[0] != 0)
                    this.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                else if (defendAngle > 45 && defendAngle <= 135 && covervals[3] != 0)
                    this.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
                else if (defendAngle > 135 && defendAngle <= 225 && covervals[1] != 0)
                    this.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                else if (defendAngle > 225 && defendAngle <= 315 && covervals[2] != 0)
                    this.transform.rotation = Quaternion.Euler(0f, 270f, 0f);
            }
            else
            {
                //Jeśli nie mamy - ustawmy się do niego przodem
                this.transform.LookAt(coverAgainst.transform.position);
                Debug.Log(gameObject.name + " is flanked!");
            }
        }
        else
        {
            Obstacle bestObst = null;
            int bestVal = 0;
            //Jeśli nikogo nie ma - ustawmy sie przodem do najlepszego covera
            foreach(Obstacle obst in FindAdjacentCovers())
            {
                if (obst.CoverValue > bestVal)
                    bestObst = obst;
            }
            if (bestObst != null)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position + new Vector3(0.0f, 0.2f, 0.0f), Vector3.forward, out hit, 1.0f, SightMask, QueryTriggerInteraction.Collide))
                {
                    if (hit.collider.GetComponentInParent<Obstacle>() == bestObst)
                        this.transform.LookAt(hit.point);
                }
                else if (Physics.Raycast(transform.position + new Vector3(0.0f, 0.2f, 0.0f), Vector3.right, out hit, 1.0f, SightMask, QueryTriggerInteraction.Collide))
                {
                    if (hit.collider.GetComponentInParent<Obstacle>() == bestObst)
                        this.transform.LookAt(hit.point);
                }
                else if (Physics.Raycast(transform.position + new Vector3(0.0f, 0.2f, 0.0f), -Vector3.forward, out hit, 1.0f, SightMask, QueryTriggerInteraction.Collide))
                {
                    if (hit.collider.GetComponentInParent<Obstacle>() == bestObst)
                        this.transform.LookAt(hit.point);
                }
                else if (Physics.Raycast(transform.position + new Vector3(0.0f, 0.2f, 0.0f), -Vector3.right, out hit, 1.0f, SightMask, QueryTriggerInteraction.Collide))
                {
                    if (hit.collider.GetComponentInParent<Obstacle>() == bestObst)
                        this.transform.LookAt(hit.point);
                }

                if (this.transform.rotation.x != 0f)
                    this.transform.Rotate(new Vector3(-this.transform.rotation.eulerAngles.x, 0, 0));
            }         
        }
    }

    List<Obstacle> FindAdjacentCovers()
    {
        List<Obstacle> returnArray = new List<Obstacle>();
        RaycastHit hit;
        if (Physics.Raycast(transform.position + new Vector3(0.0f, 0.2f, 0.0f), transform.forward, out hit, 1.0f, SightMask, QueryTriggerInteraction.Collide))
            if (hit.collider.gameObject.GetComponentInParent<Obstacle>() != null)
                returnArray.Add(hit.collider.gameObject.GetComponentInParent<Obstacle>());

        if (Physics.Raycast(transform.position + new Vector3(0.0f, 0.2f, 0.0f), transform.right, out hit, 1.0f, SightMask, QueryTriggerInteraction.Collide))
            if (hit.collider.gameObject.GetComponentInParent<Obstacle>() != null)
                returnArray.Add(hit.collider.gameObject.GetComponentInParent<Obstacle>());

        if (Physics.Raycast(transform.position + new Vector3(0.0f, 0.2f, 0.0f), -transform.forward, out hit, 1.0f, SightMask, QueryTriggerInteraction.Collide))
            if (hit.collider.gameObject.GetComponentInParent<Obstacle>() != null)
                returnArray.Add(hit.collider.gameObject.GetComponentInParent<Obstacle>());

        if (Physics.Raycast(transform.position + new Vector3(0.0f, 0.2f, 0.0f), -transform.right, out hit, 1.0f, SightMask, QueryTriggerInteraction.Collide))
            if (hit.collider.gameObject.GetComponentInParent<Obstacle>() != null)
                returnArray.Add(hit.collider.gameObject.GetComponentInParent<Obstacle>());

        return returnArray;
    }

    public Obstacle FindShootingCover(UnitManager target)
    {
        //Spisujemy wartości coverów postaci
        int[] covervals = new int[4];
        CoverValues.TryGetValue(Orientation.North, out covervals[0]);
        CoverValues.TryGetValue(Orientation.South, out covervals[1]);
        CoverValues.TryGetValue(Orientation.West, out covervals[2]);
        CoverValues.TryGetValue(Orientation.East, out covervals[3]);

        if (FindCoveringCover(this, target)) //Jeśli mamy raycastowalnego covera przed danym przeciwnikiem
        {
            return FindCoveringCover(this, target); //To na pewno jest to cover, którego powinniśmy użyć by do niego strzelić
        }
        else if (FindEffectiveCover(this, target) > 0) //Inaczej, jeśli nie mamy takiego covera, ale mamy jakiegokolwiek covera przeciwko niemu
        {
            //Jeśli mamy - sprawdźmy kąt 
            Vector3 heading = this.CharacterMovement.GetLastTile().transform.position - target.CharacterMovement.GetLastTile().transform.position;
            int shootAngle = (int)MathLibrary.AngleSigned(this.CharacterMovement.GetLastTile().transform.forward, heading, Vector3.up);
            if (shootAngle < 0)
                shootAngle = 360 - Mathf.Abs(shootAngle);
            //deklarujemy ray, którego wartość przypiszemy zależnie od kątu strzelania
            Ray ray = new Ray();
            //i ustawmy się w stronę covera który jest najbliższy kątem
            if (shootAngle > 315 || shootAngle <= 45 && covervals[0] != 0)
                ray = new Ray(this.transform.position + new Vector3(0f, 0.3f), Vector3.forward);
            else if (shootAngle > 45 && shootAngle <= 135 && covervals[3] != 0)
                ray = new Ray(this.transform.position + new Vector3(0f, 0.3f), Vector3.right);
            else if (shootAngle > 135 && shootAngle <= 225 && covervals[1] != 0)
                ray = new Ray(this.transform.position + new Vector3(0f, 0.3f), -Vector3.forward);
            else if (shootAngle > 225 && shootAngle <= 315 && covervals[2] != 0)
                ray = new Ray(this.transform.position + new Vector3(0f, 0.3f), -Vector3.right);

            //Spisujemy covery używane przez tę postać
            List<Obstacle> usedCovers = FindAdjacentCovers();

            Debug.DrawRay(ray.origin, ray.direction, Color.black, 5f);

            //Robimy raycasta i szukamy tam covera
            RaycastHit hit;
            Debug.DrawRay(ray.origin, ray.direction, Color.blue, 10f);
            if (Physics.Raycast(ray, out hit, 1f, SightMask, QueryTriggerInteraction.Collide)) //Sprawdzamy, czy jest jakiś łapacz sighta w kierunku na odległości metra
                if (hit.collider.gameObject.GetComponentInParent<Obstacle>()) //Jeśli tam coś jest i ma obstacle
                    for (int i = 0; i < usedCovers.Count; i++) //Sprawdzamy, czy jest to cover używany przez tę postać
                        if (hit.collider.gameObject.GetComponentInParent<Obstacle>() == usedCovers[i]) //Jeśli tak
                            return hit.collider.gameObject.GetComponentInParent<Obstacle>(); //zwracamy to jako cover, w kierunku którego powinnismy się obrócić by strzelić
        }

        //Jeśli nie mamy nic
        return null;
    }

    Obstacle FindCoveringCover(UnitManager defender, UnitManager shooter)
    {
        //Deklarujemy tabelę trafień i robimy raycasta
        RaycastHit[] hits;   
        hits = Physics.RaycastAll(shooter.transform.position + new Vector3(0, 1f), (defender.transform.position - shooter.transform.position), (float)UnitSightRange, SightMask, QueryTriggerInteraction.Collide);
        //Deklarujemy tabelę obron obrońcy
        List<Obstacle> defenderCovers = defender.FindAdjacentCovers();
        //Rozpoczynamy pętlę
        foreach(RaycastHit hit in hits) //dla każdego trafienia z trafień
        {
            if (hit.collider.GetComponentInParent<Obstacle>() != null) //jeśli w ogóle tam jest przeszkoda
                for (int i = 0; i < defenderCovers.Count; i++) //sprawdzamy czy ta przeszkoda jest używana przez obrońce
                    if (hit.collider.GetComponentInParent<Obstacle>() == defenderCovers[i] && Vector3.Distance(shooter.transform.position, hit.transform.position) < Vector3.Distance(shooter.transform.position, defender.transform.position)) //jesli jest i jeśli czasem nie łapiemy czegoś za obrońcą
                        return hit.collider.GetComponentInParent<Obstacle>(); //to ją zwracamy
        }

        return null;
    }

    public Visibility CheckSight(UnitManager shooter, UnitManager defender)
    {
        //Odwracamy postać by była gotowa do strzału
        shooter.AssumeCoveredPosition(defender);
        //Robimy raycasta wymiany ognia     
        RaycastHit[] hits;
        hits = Physics.RaycastAll(shooter.transform.position + new Vector3(0f, 1f), (defender.transform.position - shooter.transform.position), Vector3.Distance(shooter.transform.position, defender.transform.position), SightMask, QueryTriggerInteraction.Collide);
        
        //Deklarujemy boole potrzebne do określenia widoczności
        bool bShooterHitHighCov = false;
        bool bDefenderHitHighCov = false;
        //Sprawdzamy widoczność od shootera do defendera
        if(shooter.FindShootingCover(defender) != null && shooter.FindShootingCover(defender).CoverValue >= 80)    //Jeśli strzelec musi strzelić przez coś wysokiego, to musimy sprawdzić wychylanie
        {
            FindPeekDirection(defender);
        }
        if (FindEffectiveCover(defender, shooter) == 100)
        {
            bShooterHitHighCov = true;
        }       //Jeśli obrońca ma 100 obrony przed atakującym, to atakujący nie może strzelić
        else
        {
            //Deklarujemy zmienne potrzebne zmienne do pętli
            int i = 0;
            while (i < hits.Length)
            {
                if (hits[i].collider.gameObject.GetComponentInParent<Obstacle>()) //Jeśli trafiliśmy w przeszkodę
                    if (hits[i].collider.gameObject.GetComponentInParent<Obstacle>() != shooter.FindShootingCover(defender)) //Jeśli nie trafiliśmy w covera używanego przez strzelcę
                        if (hits[i].collider.gameObject.GetComponentInParent<Obstacle>().CoverValue >= 80 && hits[i].collider.gameObject.GetComponentInParent<Obstacle>() != FindCoveringCover(defender, shooter)) //Jeśli trafiliśmy w coś wysokiego i nie jest to obrona używana przez obrońcę
                        {
                            bShooterHitHighCov = true;      //To to notujemy
                            break;                          //I przerywamy pętlę
                        }
                i++;
            }
        }                                                    //Inaczej
        //Sprawdamy widoczność od defendera do shootera
        if (FindEffectiveCover(shooter, defender) == 100)
        {
            bDefenderHitHighCov = true;
        }       //Jeśli atakujący ma 100 obrony przed obrońcą, to obrońca nie może strzelić
        else
        {
            int j = 0;
            while (j < hits.Length)
            {
                if (hits[j].collider.gameObject.GetComponentInParent<Obstacle>()) //Jeśli trafiliśmy w przeszkodę
                    if (hits[j].collider.gameObject.GetComponentInParent<Obstacle>() != FindCoveringCover(defender, shooter)) //Jeśli nie trafiliśmy w covera używanego przez obrońcę
                        if (hits[j].collider.gameObject.GetComponentInParent<Obstacle>().CoverValue >= 80 && hits[j].collider.gameObject.GetComponentInParent<Obstacle>() != FindCoveringCover(shooter, defender))
                        {
                            bDefenderHitHighCov = true;
                            break;
                        }
                j++;
            }
        }                                                    //inaczej

        //Zwracamy wartość zależną od bShooterHitHighCov i bDefenderHitHighCov
        if (!bShooterHitHighCov && !bDefenderHitHighCov)
            return Visibility.Orange;
        else if (!bShooterHitHighCov && bDefenderHitHighCov)
            return Visibility.Green;
        else if (bShooterHitHighCov && !bDefenderHitHighCov)
            return Visibility.Red;
        else
            return Visibility.Black;
    }

    public PeekDir FindPeekDirection(UnitManager peekAgainst)
    {
        Debug.DrawRay(transform.position + new Vector3(0, 1f, 0f), transform.right + new Vector3(0, -1f, 0), Color.blue, 10f);
        Debug.DrawRay(transform.position + new Vector3(0, 1f, 0f), -transform.right + new Vector3(0, -1f, 0), Color.red, 10f);
        RaycastHit hit;
        //Sprawdzamy czy możemy wychylić się na prawo (operacje bitowe to layermask dla: grid, character, obstacle i sightcatcher)
        if (Physics.Raycast(transform.position + new Vector3(0, 1f, 0f), transform.right + new Vector3(0, -1f, 0), out hit, 2f, 1 << 8 | 1 << 10 | 1 << 11 | 1 << 12))
        {
            //Czy trafiliśmy grida? i czy to nie jest czasem to na czym stoimy i czy po tym tilu można chodzić?
            if (hit.collider.GetComponent<Tile>() && hit.collider.GetComponent<Tile>() != CharacterMovement.GetLastTile() && hit.collider.GetComponent<Tile>().IsWalkable())
                return PeekDir.Right;
        }
        //Sprawdzamy czy możemy wychylić się na lewo
        if (Physics.Raycast(transform.position + new Vector3(0, 1f, 0f), -transform.right + new Vector3(0, -1f, 0), out hit, 2f, 1 << 8 | 1 << 10 | 1 << 11 | 1 << 12))
        {
            //Czy trafiliśmy grida? i czy to nie jest czasem to na czym stoimy i czy po tym tilu można chodzić?
            if (hit.collider.GetComponent<Tile>() && hit.collider.GetComponent<Tile>() != CharacterMovement.GetLastTile() && hit.collider.GetComponent<Tile>().IsWalkable())
                return PeekDir.Left;
        }
        //Jak nic nie pasi to zwracamy none
        return PeekDir.None;
    }

    public void UseAbility(int index, UnitManager target)
    {
        if (CheckSight(this, target) != Visibility.Red && CheckSight(this, target) != Visibility.Black) //Jeśli w ogóle możemy strzelić w przeciwnika
        {
            if (UnitAbilities[index] != null && currentStamina >= UnitAbilities[index].StaminaCost) //Jeśli mamy na tym slocie jakiegoś skilla i jeśli mamy na niego staminę
            {
                UnitAbilities[index].OnActivation(target);
                CommanderScript.StateManager.CurrentAbility = UnitAbilities[index];
                CommanderScript.StateManager.SetState(PlayerState.Targeting, UnitAbilities[index]);
            }
        }
    }
    //----------------------------------------------------------Zabieranie hp/staminy---------------------------------------
    public void DrainStamina(int val)
    {
        currentStamina -= val;
        calculatedStamina = currentStamina;
    }

    /*GETTERY*/
    public bool GetIsActive() { return isActive; }
    public int GetHealth() { return currentHealth; }
    public int GetStamina() { return currentStamina; }
    public int GetCalculatedStamina() { return calculatedStamina; }
    public CommandUnit GetCommander() { return CommanderScript; }
    /*SETTERY*/
    public void SetCoverValues(Dictionary<Orientation, int> val) { CoverValues = val; }
    public void SetCalculatedStamina(int val) { calculatedStamina = val; }
    public void SetClassIcon(Sprite val) { classIcon.sprite = val; }
    public void SetHealth(int val) { currentHealth = val; }
    public void SetStamina(int val) { currentStamina = val; }
    public void SetCalcStamina(int val) { calculatedStamina = val; }
    public void SetCommander(CommandUnit val) { CommanderScript = val; }
 }
