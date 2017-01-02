using UnityEngine;

[RequireComponent(typeof (Camera))]
public class CameraMovement : MonoBehaviour
{
    public enum MovementType { free, follow, swipe};

    public Camera MainCam;
    public GameObject FocusPoint;

    public MovementType CurrentMovementType;

    public float MovementMargin = 50f;
    public float MaxMoveSpeed = 10f;
    public float ZoomSpeed = 3f;
    public float RotationSpeed = 3f;
    public float CameraAngle = 45f;
    public float MaxCameraDist = 100f;
    public float MinCameraDist = 80f;

    private GameObject followTarget = null;
    private float mousePosX;
    private float mousePosY;
    private float cameraHeight;
    private float swipeStartDist;

	// Use this for initialization
	void Awake ()
    {
        if (FocusPoint == null)
            Debug.LogError("CRITICAL ERROR: No focus point for camera!");

        MainCam.transform.position = new Vector3(0f, Mathf.Cos(CameraAngle * Mathf.Deg2Rad) * (MaxCameraDist + MinCameraDist) / 2, -Mathf.Sin(CameraAngle * Mathf.Deg2Rad) * (MaxCameraDist + MinCameraDist));
        CurrentMovementType = MovementType.free;
	}

    // Update is called once per frame
    void Update()
    {
        switch (CurrentMovementType)
        {
            case MovementType.free:
                RefreshFocus();
                HandleFreeMovement();
                break;

            case MovementType.follow:
                HandleFollowMovement();
                break;

            case MovementType.swipe:
                HandleSwipe();
                break;

            default:
                Debug.LogError("Camera entered unknown state of MovementType!");
                break;
        }

        if (Input.GetAxis("RotationAxis") != 0f)
            Rotate(Input.GetAxis("RotationAxis"));

        if (Input.GetAxis("Mouse ScrollWheel") != 0f)
            HandleZoom(Input.GetAxis("Mouse ScrollWheel"));
    }

    private void RefreshFocus()
    {
        RaycastHit hitInfo;
        if (Physics.Raycast(MainCam.transform.position, MainCam.transform.forward, out hitInfo, 100))
        {
            FocusPoint.transform.position = hitInfo.point;
        }
    }

    private void HandleFreeMovement()
    {
        mousePosX = Input.mousePosition.x;
        mousePosY = Input.mousePosition.y;

        if (mousePosX < MovementMargin && mousePosX >= 0)
            MainCam.transform.Translate(-Vector3.right * (MaxMoveSpeed - ((mousePosX / MovementMargin) * MaxMoveSpeed)) * Time.deltaTime);
        else if (Screen.width - mousePosX < MovementMargin && mousePosX <= Screen.width)
            MainCam.transform.Translate(Vector3.right * (MaxMoveSpeed - (((Screen.width - mousePosX) / MovementMargin) * MaxMoveSpeed)) * Time.deltaTime);
        if (mousePosY < MovementMargin && mousePosY >= 0)
            MainCam.transform.Translate(-(Vector3.forward + Vector3.up)/(90/CameraAngle) * (MaxMoveSpeed - ((mousePosY / MovementMargin) * MaxMoveSpeed)) * Time.deltaTime);
        else if (Screen.height - mousePosY < MovementMargin && mousePosY <= Screen.height)
            MainCam.transform.Translate((Vector3.forward + Vector3.up) / (90 / CameraAngle) * (MaxMoveSpeed - (((Screen.height - mousePosY) / MovementMargin) * MaxMoveSpeed)) * Time.deltaTime);
    }

    private void HandleFollowMovement()
    {
        Vector3 tar = followTarget.transform.position;

        FocusPoint.transform.position = Vector3.Lerp(FocusPoint.transform.position, tar, 25f * Time.deltaTime);
    }

    private void Rotate(float val)
    {
        MainCam.transform.RotateAround(FocusPoint.transform.position, FocusPoint.transform.up, val * RotationSpeed * Time.deltaTime);
    }

    private void HandleZoom(float val)
    {
        float dist = Vector3.Distance(MainCam.transform.position, FocusPoint.transform.position);
        if (dist >= MinCameraDist &&  val > 0f || dist <= MaxCameraDist && val < 0f)
            MainCam.transform.position = Vector3.MoveTowards(MainCam.transform.position, FocusPoint.transform.position, val * ZoomSpeed * Time.deltaTime );
    }

    private void HandleSwipe() //----------------------PRĘDKOSC DO ZMIANY?
    {
        Vector3 tar = followTarget.transform.position;
        float dist = Vector3.Distance(FocusPoint.transform.position, tar);

        if (dist > 0.2f)
            FocusPoint.transform.position = Vector3.Lerp(FocusPoint.transform.position, tar, (MaxMoveSpeed/3 - (dist/swipeStartDist * MaxMoveSpeed/4)) * Time.deltaTime);
        else
            StartFreeMovement();
    }

    public void StartFreeMovement()
    {
        MainCam.transform.parent = null;
        CurrentMovementType = MovementType.free;
    }

    public void StartFollowing(GameObject tar)
    {
        MainCam.transform.parent = FocusPoint.transform;
        followTarget = tar;
        CurrentMovementType = MovementType.follow;
    }

    public void StartSwiping(GameObject tar)
    {
        swipeStartDist = Vector3.Distance(FocusPoint.transform.position, tar.transform.position);
        MainCam.transform.parent = FocusPoint.transform;
        followTarget = tar;
        CurrentMovementType = MovementType.swipe;
    }

    /*GETTERY*/
    public GameObject GetFollowTarget() { return followTarget; }
    /*SETTERY*/
    public void SetFollowTarget(GameObject tar)   {  followTarget = tar; }
}
