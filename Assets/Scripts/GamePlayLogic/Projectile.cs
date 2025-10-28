using UnityEngine;

public class Projectile : Entity
{
    private UnitDetectable unitDetectable;
    private UnitDetectable exludeHitDetectable;

    [Header("Physic")]
    public float gravity = -9.81f;
    [SerializeField] private float terminateGravity = -60f;
    [Range(0, 90)][SerializeField] private float elevationAngle = 60; //  Debug
    private Vector3 velocity;

    protected override void Start()
    {
        base.Start();
        unitDetectable = GetComponent<UnitDetectable>();
    }
    private void Update()
    {
        CalculateVelocity();
        transform.position += velocity * Time.deltaTime;

        float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        OnHit();
    }

    private void OnHit()
    {
        UnitDetectable[] unitDetectables = unitDetectable.OverlapSelfRange();
        foreach (var unitDetectable in unitDetectables)
        {
            if (unitDetectable == exludeHitDetectable) { continue; }

            if (unitDetectable != null)
            {
                Debug.Log($"Hit {unitDetectable.name}");
                Destroy(gameObject);
                return;
            }
        }
    }
    
    public void Launch(Vector3 direction, float speed)
    {
        velocity = direction.normalized * speed;
    }
    public void LaunchToTarget(Vector3 start, Vector3 end)
    {
        Vector3 displacementXZ = new Vector3(end.x - start.x, 0, end.z - start.z);
        float distanceXZ = displacementXZ.magnitude;
        float heightDifference = end.y - start.y;

        float g = -gravity;
        float angleRad = elevationAngle * Mathf.Deg2Rad;

        float numerator = g * distanceXZ * distanceXZ;
        float denominator = 2f * Mathf.Cos(angleRad) * Mathf.Cos(angleRad) * (distanceXZ * Mathf.Tan(angleRad) - heightDifference);

        if (denominator <= 0f)
        {
            Debug.LogWarning("Invalid parameters: Target is too close or below trajectory path.");
            return;
        }

        float velocity = Mathf.Sqrt(numerator / denominator);

        float rotationY = Mathf.Atan2(displacementXZ.x, displacementXZ.z) * Mathf.Rad2Deg;
        Vector3 forwardDir = Quaternion.Euler(0, rotationY, 0) * Vector3.forward;

        transform.position = start;
        Launch(Quaternion.Euler(0, rotationY, 0) * Quaternion.Euler(-elevationAngle, 0, 0) * Vector3.forward, velocity);
    }
    public void LaunchToTarget(UnitDetectable exludeHitDetectable, Vector3 start, Vector3 end)
    {
        this.exludeHitDetectable = exludeHitDetectable;
        LaunchToTarget(start, end);
    }

    private float CheckGrounded(float downSpeed)
    {
        Vector3 half = unitDetectable.size * 0.5f;
        Vector3 centerPos = transform.position + unitDetectable.center;

        Vector3 min = centerPos - half;
        Vector3 max = centerPos + half;

        float checkY = min.y + downSpeed * Time.deltaTime;

        if (world.CheckSolidNode(min.x, checkY, min.z) ||
            world.CheckSolidNode(max.x, checkY, min.z) ||
            world.CheckSolidNode(min.x, checkY, max.z) ||
            world.CheckSolidNode(max.x, checkY, max.z))
        {
            return 0;
        }
        else
        {
            return downSpeed;
        }
    }
    public void CalculateVelocity()
    {
        velocity.y = Mathf.Max(velocity.y + gravity * Time.deltaTime, terminateGravity);

        if (velocity.z > 0 && CheckBottomForward() || velocity.z < 0 && CheckBottomBackward())
            velocity.z = 0;
        if (velocity.x > 0 && CheckBottomRight() || velocity.x < 0 && CheckBottomLeft())
            velocity.x = 0;

        if (velocity.y < 0)
            velocity.y = CheckGrounded(velocity.y);
        else if (velocity.y > 0 || CheckUp())
            velocity.y = CheckGrounded(velocity.y);
    }
    public bool CheckBottomForward()
    {
        Vector3 half = unitDetectable.size * 0.5f;
        Vector3 centerPos = transform.position + unitDetectable.center;
        float checkBottom = centerPos.y - half.y;
        float checkForward = centerPos.z + half.z;

        if (world.CheckSolidNode(transform.position.x, checkBottom, checkForward))
            return true;
        else
            return false;
    }
    public bool CheckBottomBackward()
    {
        Vector3 half = unitDetectable.size * 0.5f;
        Vector3 centerPos = transform.position + unitDetectable.center;
        float checkBottom = centerPos.y - half.y;
        float checkBackward = centerPos.z - half.z;

        if (world.CheckSolidNode(transform.position.x, checkBottom, checkBackward))
            return true;
        else
            return false;
    }
    public bool CheckBottomRight()
    {
        Vector3 half = unitDetectable.size * 0.5f;
        Vector3 centerPos = transform.position + unitDetectable.center;
        float checkBottom = centerPos.y - half.y;
        float checkRight = centerPos.x + half.x;

        if (world.CheckSolidNode(checkRight, checkBottom, transform.position.z))
            return true;
        else
            return false;
    }
    public bool CheckBottomLeft()
    {
        Vector3 half = unitDetectable.size * 0.5f;
        Vector3 centerPos = transform.position + unitDetectable.center;
        float checkBottom = centerPos.y - half.y;
        float checkLeft = centerPos.x - half.x;

        if (world.CheckSolidNode(checkLeft, checkBottom, transform.position.z))
            return true;
        else
            return false;
    }
    private bool CheckUp()
    {
        Vector3 half = unitDetectable.size * 0.5f;
        Vector3 centerPos = transform.position + unitDetectable.center;
        float checkUp = centerPos.y + half.y;

        if (world.CheckSolidNode(transform.position.x, checkUp, transform.position.z))
            return true;
        else
            return false;
    }
}
