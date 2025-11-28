using System.Collections.Generic;
using UnityEngine;

public class Parabola
{
    private World world;

    public Parabola(World world)
    {
        this.world = world;
    }

    public UnitDetectable GetParabolaHitUnit(GameNode start, GameNode target, int elevationAngle)
    {
        return GetParabolaHitUnit(start.GetVector(), target.GetVector(), elevationAngle);
    }

    public UnitDetectable GetParabolaHitUnit(Vector3 start, Vector3 target, int elevationAngle)
    {
        Vector3 displacementXZ = new Vector3(target.x - start.x, 0, target.z - start.z);
        float distanceXZ = displacementXZ.magnitude;
        float heightDifference = target.y - start.y;

        float g = 9.81f;
        float angleRad = elevationAngle * Mathf.Deg2Rad;

        float numerator = g * distanceXZ * distanceXZ;
        float denominator = 2f * Mathf.Cos(angleRad) * Mathf.Cos(angleRad) * (distanceXZ * Mathf.Tan(angleRad) - heightDifference);

        if (denominator <= 0f)
        {
            Debug.LogWarning("Invalid parameters: Target is too close or below trajectory path.");
            return null;
        }
        float velocity = Mathf.Sqrt(numerator / denominator);

        float rotationY = Mathf.Atan2(displacementXZ.x, displacementXZ.z) * Mathf.Rad2Deg;
        Vector3 forwardDir = Quaternion.Euler(0, rotationY, 0) * Vector3.forward;

        int segments = Mathf.Clamp((int)(distanceXZ * 8f), 50, 800);
        Vector3 previousPoint = start;

        for (int i = 0; i < segments; i++)
        {
            float t = (float)i / segments; // 0~1
            float time = t * (distanceXZ / (velocity * Mathf.Cos(angleRad)));

            //  Parabolic equation
            float x = velocity * Mathf.Cos(angleRad) * time;
            float y = velocity * Mathf.Sin(angleRad) * time + 0.5f * -9.81f * time * time;

            Vector3 nextPoint = start + forwardDir * x + Vector3.up * y;
            if (world.CheckSolidNodeLine(previousPoint, nextPoint))
            {
                Debug.Log("Hit Solid");
                return null;
            }

            UnitDetectable unit = GetHitUnitDetectable(nextPoint);
            if (unit != null)
            {
                Debug.Log($"Hit Unit Detectable: {unit.name}");
                return unit;
            }
            previousPoint = nextPoint;
        }
        if (world.CheckSolidNode(target))
        {
            Debug.Log($"Hit Solid: {target}");
            return null;
        }
        return null;
    }

    private UnitDetectable GetHitUnitDetectable(Vector3 position)
    {
        List<UnitDetectable> unitDetectables = UnitDetectable.all;

        foreach (UnitDetectable unit in unitDetectables)
        {
            Bounds selfBound = unit.GetRotatedBoundSelf();
            if (selfBound.Contains(position))
            {
                return unit;
            }
        }
        return null;
    }
}
