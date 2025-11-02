using UnityEngine;
using System.Collections.Generic;

public class UnitDetectable : Entity
{
    public Vector3 center;
    public Vector3 size;

    [SerializeField] private int mahhatassRange;

    public static List<UnitDetectable> all = new List<UnitDetectable>();

    private void OnEnable() { all.Add(this); }
    private void OnDisable() { all.Remove(this); }

    protected override void Start()
    {
        base.Start();
    }

    public UnitDetectable[] OverlapSelfRange()
    {
        List<UnitDetectable> hits = new List<UnitDetectable>();

        Bounds selfBounds = GetRotatedBounds(transform.position, transform.rotation, center, size);

        foreach (UnitDetectable unit in all)
        {
            if (unit == this) continue;

            Bounds otherBounds = GetRotatedBounds(unit.transform.position, unit.transform.rotation, unit.center, unit.size);

            if (selfBounds.Intersects(otherBounds))
            {
                hits.Add(unit);
            }
        }

        return hits.ToArray();
    }

    private Bounds GetRotatedBounds(Vector3 position, Quaternion rotation, Vector3 center, Vector3 size)
    {
        Vector3 worldCenter = position + rotation * center;
        Vector3 half = size * 0.5f;

        Vector3[] corners = new Vector3[8]
        {
        rotation * new Vector3(-half.x, -half.y, -half.z),
        rotation * new Vector3( half.x, -half.y, -half.z),
        rotation * new Vector3(-half.x,  half.y, -half.z),
        rotation * new Vector3( half.x,  half.y, -half.z),
        rotation * new Vector3(-half.x, -half.y,  half.z),
        rotation * new Vector3( half.x, -half.y,  half.z),
        rotation * new Vector3(-half.x,  half.y,  half.z),
        rotation * new Vector3( half.x,  half.y,  half.z),
        };

        for (int i = 0; i < 8; i++)
            corners[i] += position;

        Vector3 min = corners[0];
        Vector3 max = corners[0];
        for (int i = 1; i < 8; i++)
        {
            min = Vector3.Min(min, corners[i]);
            max = Vector3.Max(max, corners[i]);
        }

        Bounds b = new Bounds();
        b.SetMinMax(min, max);
        return b;
    }


    /// <summary>
    /// Start from the unit center extend with 3D mahhatass range to obtain other unit detectable
    /// </summary>
    /// <param name="mahhatassRange"></param>
    /// <returns></returns>
    public UnitDetectable[] OverlapMahhatassRange(int mahhatassRange)
    {
        this.mahhatassRange = mahhatassRange;
        List<UnitDetectable> hits = new List<UnitDetectable>();

        foreach (UnitDetectable unit in all)
        {
            if (unit == this) { continue; }

            Vector3Int selfCenter = Utils.RoundXZFloorYInt(transform.position);

            Vector3 otherCenter = unit.transform.position + unit.center;
            Vector3 otherSize = unit.size * 0.5f;
            Vector3 otherMax = otherCenter + otherSize;
            Vector3 otherMin = otherCenter - otherSize;

            Vector3Int closet = new Vector3Int(
                Mathf.Clamp(selfCenter.x, Mathf.FloorToInt(otherMin.x), Mathf.FloorToInt(otherMax.x)),
                Mathf.Clamp(selfCenter.y, Mathf.FloorToInt(otherMin.y), Mathf.FloorToInt(otherMax.y)),
                Mathf.Clamp(selfCenter.z, Mathf.FloorToInt(otherMin.z), Mathf.FloorToInt(otherMax.z))
                );

            int mahhatassDistance = Mathf.Abs(selfCenter.x - closet.x) + Mathf.Abs(selfCenter.y - closet.y) + Mathf.Abs(selfCenter.z - closet.z);
            if (mahhatassDistance <= this.mahhatassRange)
            {
                hits.Add(unit);
            }
        }
        return hits.ToArray();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(center, size);
        
        Gizmos.matrix = oldMatrix;

        if (world != null && mahhatassRange > 0)
        {
            List<Vector3Int> coverage = world.GetManhattas3DRangePosition(Utils.RoundXZFloorYInt(transform.position), mahhatassRange, false);

            Gizmos.color = new Color(1f, 1f, 0f, 0.5f);
            for (int i = 0; i < coverage.Count; i++)
            {
                Gizmos.DrawWireCube(coverage[i], Vector3.one);
            }
        }
    }
}
