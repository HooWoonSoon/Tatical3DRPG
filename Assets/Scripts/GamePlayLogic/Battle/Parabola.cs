    using System.Collections.Generic;
    using UnityEngine;
using UnityEngine.UIElements;
using static Unity.Cinemachine.IInputAxisOwner.AxisDescriptor;

public class Parabola
{
    private World world;
    private bool debugMode;

    public Parabola(World world, bool debugMode = false)
    {
        this.world = world;
        this.debugMode = debugMode;
    }

    public List<UnitDetectable> GetParabolaHitUnit(UnitDetectable projectileDetectable, GameNode start, GameNode target, int elevationAngle)
    {
        return GetParabolaHitUnit(projectileDetectable, start.GetNodeVector(), target.GetNodeVector(), elevationAngle);
    }

    public List<UnitDetectable> GetParabolaHitUnit(UnitDetectable projectileDetectable, Vector3 start, Vector3 target, int elevationAngle)
    {
        List<UnitDetectable> hits = new List<UnitDetectable>();

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

            Vector3 direction = nextPoint - previousPoint;
            Vector3 center = projectileDetectable.center;
            Vector3 size = projectileDetectable.size;
            if (direction != Vector3.zero)
            {
                Quaternion rotation = Quaternion.LookRotation(direction);
                Bounds projectileBound = projectileDetectable.GetRotatedBounds(nextPoint, rotation, center, size);

                if (world.CheckSolidNodeBound(projectileBound))
                {
                    if (debugMode)
                        Debug.Log("Hit Solid");
                }

                UnitDetectable unit = GetHitUnitDetectable(projectileBound);
                if (unit != null && !hits.Contains(unit))
                {
                    hits.Add(unit);
                }
            }
            previousPoint = nextPoint;
        }
        return hits;
    }

    //private UnitDetectable GetHitUnitDetectable(Vector3 position)
    //{
    //    List<UnitDetectable> unitDetectables = UnitDetectable.all;

    //    foreach (UnitDetectable unit in unitDetectables)
    //    {
    //        Bounds selfBound = unit.GetRotatedBoundSelf();
    //        if (selfBound.Contains(position))
    //        {
    //            return unit;
    //        }
    //    }
    //    return null;
    //}

    public UnitDetectable GetHitUnitDetectable(Bounds bounds)
    {
        Vector3[] positions =
        {
            new Vector3(bounds.min.x, bounds.min.y, bounds.min.z),
            new Vector3(bounds.max.x, bounds.min.y, bounds.min.z),
            new Vector3(bounds.min.x, bounds.max.y, bounds.min.z),
            new Vector3(bounds.min.x, bounds.min.y, bounds.max.z),
            new Vector3(bounds.max.x, bounds.max.y, bounds.min.z),
            new Vector3(bounds.max.x, bounds.min.y, bounds.max.z),
            new Vector3(bounds.min.x, bounds.max.y, bounds.max.z),
            new Vector3(bounds.max.x, bounds.max.y, bounds.max.z)
        };

        List<UnitDetectable> unitDetectables = UnitDetectable.all;

        foreach (UnitDetectable unit in unitDetectables)
        {
            Bounds selfBound = unit.GetRotatedBoundSelf();

            foreach (Vector3 position in positions)
            {
                if (selfBound.Contains(position))
                {
                    return unit;
                }
            }
        }
        return null;
    }

}