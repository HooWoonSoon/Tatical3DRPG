using UnityEngine;

public class CharacterTestingEMove : MonoBehaviour
{
    private World world;
    [Header("Physic")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = 9.8f;
    private float velocity;

    private Vector3? targetPosition;
    private bool isBusy = false;


    private void Start()
    {
        world = WorldManager.instance.world;
    }

    private void Update()
    {
        float inputX = Input.GetAxis("Horizontal");
        float inputZ = Input.GetAxis("Vertical");
        Move(inputX, inputZ);

        UpDownHill(inputX, inputZ);
    }

    private void Move(float x, float z)
    {
        if (x != 0 || z != 0)
        {
            Vector3 targetPosition = transform.position + new Vector3(x, 0, z) * moveSpeed * Time.deltaTime;
            if (world.worldMinX < targetPosition.x && world.worldMinY < targetPosition.y && world.worldMinZ < targetPosition.z
                && world.worldMaxX > targetPosition.x && world.worldMaxY > targetPosition.y && world.worldMaxZ > targetPosition.z)
            {
                transform.position = targetPosition;
            }
        }
    }

    #region UpDownHill
    private void UpDownHill(float xInput, float zInput)
    {
        DownHill();
        UpHill(xInput, zInput);

        if (targetPosition != null)
        {
            float currentX = Mathf.Lerp(transform.position.x, targetPosition.Value.x, Time.deltaTime);
            float currentY = Mathf.Lerp(transform.position.y, targetPosition.Value.y, Time.deltaTime * 10);
            float currentZ = Mathf.Lerp(transform.position.z, targetPosition.Value.z, Time.deltaTime);
            transform.position = new Vector3(currentX, currentY, currentZ);

            if (Mathf.Abs(transform.position.y - targetPosition.Value.y) < 0.1f)
            {
                transform.position = new(transform.position.x, targetPosition.Value.y, transform.position.z);
                targetPosition = null;
                isBusy = false;
            }
        }
    }

    private void DownHill()
    {
        if (DDADectector.DDARaycast(transform.position, Vector3.down, 16, world.loadedNodes,
            out Vector3Int? cubePosition))
        {
            int distance = Mathf.FloorToInt(transform.position.y - cubePosition.Value.y);
            //Debug.Log(isBusy);
            if (!isBusy)
            {
                if (distance == 1)
                {
                    if (cubePosition != null)
                    {
                        targetPosition = transform.position - new Vector3(0, 1, 0);
                        isBusy = true;
                    }
                }
            }
        }
    }

    private void UpHill(float xInput, float zInput)
    {
        Vector3Int direction = Utils.GetInputDirection(xInput, zInput);
        if (DDADectector.DDARaycast(transform.position, direction, 1, world.loadedNodes,
            out Vector3Int? cubePosition))
        {
            if (!isBusy)
            {
                Debug.Log(cubePosition);
                float distance1 = Vector3.Distance(transform.position, cubePosition.Value);
                Debug.Log(distance1);
                if (distance1 < 0.8f)
                {
                    targetPosition = transform.position + new Vector3(0, 1, 0) + direction;
                    isBusy = true;
                }
            }
        }
    }
    #endregion

    private void Drop()
    {
        //Debug.Log(Utils.CheckCubeAtPosition(transform.position, world.loadedNodes));
        
        velocity += gravity * Time.deltaTime;
        transform.position -= new Vector3(0, 1 * velocity * Time.deltaTime, 0);

        if (DDADectector.CheckCubeAtPosition(transform.position, world.loadedNodes))
        {
            Vector3 alignedPosition = new Vector3(
                transform.position.x,
                Mathf.Floor(transform.position.y) + 0.5f,
                transform.position.z
            );
            transform.position = alignedPosition;

            velocity = 0;
        }
    }
}