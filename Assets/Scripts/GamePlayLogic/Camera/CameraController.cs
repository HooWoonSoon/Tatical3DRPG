using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float rotationSpeed = 10f;
    private Vector3 cameraCurrentTargetPos;

    [Header("Zoom")]
    public Vector3 defaultZoom = new Vector3(0, 10, -8);
    public Vector3 maximunZoomIn = new Vector3(0, 6, -4);
    public Vector3 maximunZoomOut = new Vector3(0, 13, -11);

    [Header("Camera Component")]
    public Transform pivotPoint;
    private Vector3 generalPivotPos;
    private Quaternion generalPivotRotate;
    public Camera cameraBody;
    private Vector3 generalBodyPos;
    private Quaternion generalBodyRotate;

    [Space]
    [Tooltip("Control camera align to the target transform")]
    public Transform followTarget;
    private bool isTargeting = false;

    private bool isResetting = false;

    public bool enableTacticalView = false;
    public static CameraController instance { get; private set; }
    private void Awake()
    {
        instance = this;
    }
    public void Start()
    {
        if (followTarget != null)
        {
            isTargeting = true;
        }
        GameEvent.onLeaderChanged += (CharacterBase newLeader) =>
        {
            ChangeFollowTarget(newLeader.transform);
        };

        RecordPosAndAngle();
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            isTargeting = true;
            isResetting = true;
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            isTargeting = true;
        }
        else if (Input.GetKeyDown(KeyCode.T))
        {
            enableTacticalView = !enableTacticalView;
            if (enableTacticalView)
            {
                isTargeting = true;
                RecordPosAndAngle();
            }
        }
    }

    private void LateUpdate()
    {
        CameraResetAligment();
        MoveCamera();
        RotateCamera();
        ZoomCamera();
        MoveCameraViewAlignment();
        TacticCameraViewAlignment();
    }

    private void RecordPosAndAngle()
    {
        cameraCurrentTargetPos = transform.position;
        generalPivotPos = pivotPoint.localPosition;
        generalPivotRotate = pivotPoint.localRotation;
        generalBodyPos = cameraBody.transform.localPosition;
        generalBodyRotate = cameraBody.transform.localRotation;
    }
    private void CameraResetAligment()
    {
        if (!isResetting) return;

        cameraBody.transform.localPosition = Vector3.Lerp(cameraBody.transform.localPosition, defaultZoom,
            Time.deltaTime * moveSpeed);
        generalBodyPos = cameraBody.transform.localPosition;
        Quaternion defaultPivotRotate = Quaternion.Euler(Vector3.zero);
        pivotPoint.localRotation = Quaternion.Lerp(pivotPoint.localRotation,
            defaultPivotRotate, Time.deltaTime * moveSpeed);
        generalPivotRotate = pivotPoint.localRotation;

        if (Vector3.Distance(cameraBody.transform.localPosition, defaultZoom) < 0.01f &&
    Quaternion.Angle(pivotPoint.localRotation, defaultPivotRotate) < 0.5f)
        {
            isResetting = false;
        }
    }
    private void MoveCamera()
    {
        Vector3 direction = Vector3.zero;

        if (Input.GetKey(KeyCode.J))
        {
            isTargeting = false;
            direction = new Vector3(-1, 0, 0);
        }
        if (Input.GetKey(KeyCode.L))
        {
            isTargeting = false;
            direction = new Vector3(1, 0, 0);
        }
        if (Input.GetKey(KeyCode.I))
        {
            isTargeting = false;
            direction = new Vector3(0, 0, 1);
        }
        if (Input.GetKey(KeyCode.K))
        {
            isTargeting = false;
            direction = new Vector3(0, 0, -1);
        }

        Vector3 rotatedDirection = pivotPoint.transform.TransformDirection(direction);

        if (rotatedDirection != Vector3.zero)
            transform.position += rotatedDirection * Time.deltaTime * moveSpeed;
    }
    private void MoveCameraViewAlignment()
    {
        if (isTargeting && followTarget != null)
        {
            Vector3 targetPosition = followTarget.position;
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed);
        }
    }
    private void RotateCamera()
    {
        if (Input.GetKey(KeyCode.U))
        {
            pivotPoint.localEulerAngles += new Vector3(0, 5, 0) * Time.deltaTime * rotationSpeed;
            generalPivotRotate = pivotPoint.localRotation;
        }
        else if (Input.GetKey(KeyCode.O))
        {
            pivotPoint.localEulerAngles += new Vector3(0, -5, 0) * Time.deltaTime * rotationSpeed;
            generalPivotRotate = pivotPoint.localRotation;
        }
    }
    private void ZoomCamera()
    {
        if (enableTacticalView) { return; }

        if (Input.GetKey(KeyCode.Minus))
        {
            if (cameraBody.transform.localPosition.y < maximunZoomIn.y || cameraBody.transform.localPosition.z > maximunZoomIn.z) { return; }
            cameraBody.transform.localPosition += new Vector3(0, -1, 1) * Time.deltaTime * moveSpeed;
            generalBodyPos = cameraBody.transform.localPosition;
        }
        if (Input.GetKey(KeyCode.Equals))
        {
            if (cameraBody.transform.localPosition.y > maximunZoomOut.y || cameraBody.transform.localPosition.z < maximunZoomOut.z) { return; }
            cameraBody.transform.localPosition += new Vector3(0, 1, -1) * Time.deltaTime * moveSpeed;
            generalBodyPos = cameraBody.transform.localPosition;
        }
    }
    private void TacticCameraViewAlignment()
    {
        Quaternion currentPivotRotation = pivotPoint.localRotation;
        Vector3 currentBodyPosition = cameraBody.transform.localPosition;
        Quaternion currentBodyRotation = cameraBody.transform.localRotation;

        if (enableTacticalView)
        {
            Quaternion alignmentPivotRotation = Quaternion.Euler(Vector3.zero);
            pivotPoint.localRotation = Quaternion.Lerp(currentPivotRotation, 
                alignmentPivotRotation, Time.deltaTime * moveSpeed);

            Quaternion alignmentBodyRotation = Quaternion.Euler(new Vector3(90, 0, 0));
            Vector3 alignmentBodyPosition = new Vector3(0, maximunZoomOut.y, 0);
            cameraBody.transform.localPosition = Vector3.Lerp(currentBodyPosition, alignmentBodyPosition, 
                Time.deltaTime * moveSpeed);
            cameraBody.transform.localRotation = Quaternion.Lerp(currentBodyRotation, alignmentBodyRotation, 
                Time.deltaTime * moveSpeed);
        }
        else
        {
            pivotPoint.localRotation = Quaternion.Lerp(currentPivotRotation, 
                generalPivotRotate, Time.deltaTime * moveSpeed);

            cameraBody.transform.localPosition = Vector3.Lerp(currentBodyPosition, generalBodyPos, Time.deltaTime * moveSpeed);
            cameraBody.transform.localRotation = Quaternion.Lerp(currentBodyRotation, generalBodyRotate, Time.deltaTime * moveSpeed);
        }
    }

    
    public void ChangeFollowTarget(Transform transform)
    {
        Debug.Log($"Change target {transform.name}");
        followTarget = transform;
        isTargeting = true;
    }

    private void OnDrawGizmos()
    {
        if (pivotPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(pivotPoint.position, 0.2f);
        }
    }
}
