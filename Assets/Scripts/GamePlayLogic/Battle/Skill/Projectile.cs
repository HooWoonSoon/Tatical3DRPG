using UnityEngine;

public class Projectile : Entity
{
    private CharacterBase shooter;
    private SkillData skillData;

    private UnitDetectable unitDetectable;
    private UnitDetectable excludeHitDetectable;

    [Header("Physic")]
    public float gravity = -9.81f;
    [SerializeField] private float terminateGravity = -60f;
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
        transform.rotation = Quaternion.LookRotation(velocity);

        OnHit();
    }

    private void OnHit()
    {
        UnitDetectable[] unitDetectables = unitDetectable.OverlapSelfRange();
        foreach (var unitDetectable in unitDetectables)
        {
            if (unitDetectable == excludeHitDetectable) { continue; }

            if (unitDetectable != null)
            {
                if (unitDetectable.GetComponent<CharacterBase>() == null) { continue; }
                Debug.Log($"Hit {unitDetectable.name}");
                DoDamage(unitDetectable);
                CameraController.instance.ChangeFollowTarget(shooter.transform);
                Destroy(gameObject);
                return;
            }
        }
        if (CheckWorldRightUpForward() || CheckWorldRightDownForward()
            || CheckWorldLeftUpForward() || CheckWorldLeftDownForward())
        {
            Debug.Log("Hit World");
            Destroy(gameObject);
        }
    }

    private void DoDamage(UnitDetectable target)
    {
        CharacterBase targetCharacter = target.GetComponent<CharacterBase>();
        if (targetCharacter == null) { return; }
        int damage = skillData.damageAmount;
        targetCharacter.TakeDamage(damage);
    }
    
    public void Launch(Vector3 direction, float speed)
    {
        velocity = direction.normalized * speed;
    }
    public void LaunchToTarget(Vector3 start, Vector3 end, int elevationAngle)
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
    public void LaunchToTarget(CharacterBase shooter, SkillData skillData,
        Vector3 start, Vector3 end)
    {
        this.shooter = shooter;
        this.skillData = skillData;
        excludeHitDetectable = shooter.detectable;
        int angle = skillData.initialElevationAngle;
        LaunchToTarget(start, end, angle);
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

        if (velocity.z > 0 && unitDetectable.CheckBottomForward() || 
            velocity.z < 0 && unitDetectable.CheckBottomBackward())
            velocity.z = 0;
        if (velocity.x > 0 && unitDetectable.CheckBottomRight() || 
            velocity.x < 0 && unitDetectable.CheckBottomLeft())
            velocity.x = 0;

        if (velocity.y < 0)
            velocity.y = CheckGrounded(velocity.y);
        else if (velocity.y > 0 || unitDetectable.CheckUp())
            velocity.y = CheckGrounded(velocity.y);
    }
    public bool CheckWorldRightUpForward()
    {
        Vector3 half = unitDetectable.size * 0.5f;

        Vector3 localOffset = new Vector3(half.x, half.y, half.z);
        Vector3 worldPoint = transform.TransformPoint(unitDetectable.center + localOffset);

        if (world.CheckSolidNode(worldPoint.x, worldPoint.y, worldPoint.z))
        {
            velocity = Vector3.zero;
            return true;
        }
        else
            return false;
    }
    public bool CheckWorldRightDownForward()
    {
        Vector3 half = unitDetectable.size * 0.5f;

        Vector3 localOffset = new Vector3(half.x, -half.y, half.z);
        Vector3 worldPoint = transform.TransformPoint(unitDetectable.center + localOffset);

        if (world.CheckSolidNode(worldPoint.x, worldPoint.y, worldPoint.z))
        {
            velocity = Vector3.zero;
            return true;
        }
        else
            return false;
    }
    public bool CheckWorldLeftUpForward()
    {
        Vector3 half = unitDetectable.size * 0.5f;

        Vector3 localOffset = new Vector3(-half.x, half.y, half.z);
        Vector3 worldPoint = transform.TransformPoint(unitDetectable.center + localOffset);

        if (world.CheckSolidNode(worldPoint.x, worldPoint.y, worldPoint.z))
        {
            velocity = Vector3.zero;
            return true;
        }
        else
            return false;
    }
    public bool CheckWorldLeftDownForward()
    {
        Vector3 half = unitDetectable.size * 0.5f;

        Vector3 localOffset = new Vector3(-half.x, -half.y, half.z);
        Vector3 worldPoint = transform.TransformPoint(unitDetectable.center + localOffset);

        if (world.CheckSolidNode(worldPoint.x, worldPoint.y, worldPoint.z))
        {
            velocity = Vector3.zero;
            return true;
        }
        else
            return false;
    }

    public void OnDrawGizmos()
    {
        if (unitDetectable == null) { return; }
        Vector3 half = unitDetectable.size * 0.5f;
        Vector3 localCenter = unitDetectable.center;

        Vector3[] corners = new Vector3[]
        {
            new Vector3(+half.x, +half.y, +half.z),
            new Vector3(+half.x, -half.y, +half.z),
            new Vector3(-half.x, +half.y, +half.z),
            new Vector3(-half.x, -half.y, +half.z),
        };

        Gizmos.color = Color.red;
        foreach (var corner in corners)
        {
            Vector3 worldPoint = transform.TransformPoint(localCenter + corner);
            Gizmos.DrawSphere(worldPoint, 0.1f);
        }
    }
}
