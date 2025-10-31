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
                Debug.Log($"Hit {unitDetectable.name}");
                DoDamage(unitDetectable);
                Destroy(gameObject);
                return;
            }
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
