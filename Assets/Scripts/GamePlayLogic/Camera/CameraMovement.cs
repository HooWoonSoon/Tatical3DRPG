using System.Collections;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float recoverSpeed = 0.5f;
    public float moveSpeed = 10f;
    public Transform followTarget;
    private Vector3 cameraCurrentTargetPos;
    private bool isTargeting = false;
    public static CameraMovement instance { get; private set; }
    private void Awake()
    {
        instance = this;
    }
    public void Start()
    {
        cameraCurrentTargetPos = transform.position;
        if (followTarget != null)
        {
            isTargeting = true;
        }
        //StartCoroutine(LerpCameraMovement());
    }

    public void Update()
    {
        if (Input.GetKey(KeyCode.J))
        {
            isTargeting = false;
            transform.position += new Vector3(-1, 0, 0) * Time.deltaTime * moveSpeed;
        }
        if (Input.GetKey(KeyCode.L))
        {
            isTargeting = false;
            transform.position += new Vector3(1, 0, 0) * Time.deltaTime * moveSpeed;
        }
        if (Input.GetKey(KeyCode.I))
        {
            isTargeting = false;
            transform.position += new Vector3(0, 0, 1) * Time.deltaTime * moveSpeed;
        }
        if (Input.GetKey(KeyCode.K))
        {
            isTargeting = false;
            transform.position += new Vector3(0, 0, -1) * Time.deltaTime * moveSpeed;
        }
        if (Input.GetKey(KeyCode.N))
        {
            isTargeting = false;
            transform.position += new Vector3(0, -1, 0) * Time.deltaTime * moveSpeed;
        }
        if (Input.GetKey(KeyCode.M))
        {
            isTargeting = false;
            transform.position += new Vector3(0, 1, 0) * Time.deltaTime * moveSpeed;
        }
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            isTargeting = true;
        }
        if (isTargeting && followTarget != null)
        {
            Vector3 targetPosition = cameraCurrentTargetPos + followTarget.position;
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed);
        }
    }

    private IEnumerator LerpCameraMovement()
    {
        float esplasedTime = 0;
        Vector3 startOffset = transform.position;
        while (esplasedTime < recoverSpeed)
        {
            esplasedTime += Time.deltaTime;
            float t = esplasedTime / recoverSpeed;
            transform.position = Vector3.Lerp(startOffset, cameraCurrentTargetPos, t);
            yield return null;
        }
        transform.position = cameraCurrentTargetPos;
    }

    public void ChangeFollowTarget(Transform transform)
    {
        followTarget = transform;
        isTargeting = true;
    }
}
