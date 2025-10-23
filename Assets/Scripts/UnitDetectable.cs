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

        Vector3 selfCenter = transform.position + center;
        Vector3 selfHalfSize = size * 0.5f;
        Vector3 selfMin = selfCenter - selfHalfSize;
        Vector3 selfMax = selfCenter + selfHalfSize;

        foreach (UnitDetectable unit in all)
        {
            if (unit == this) { continue; }

            Vector3 otherCenter = unit.transform.position + unit.center;
            Vector3 otherHalfSize = unit.size * 0.5f;
            Vector3 otherMin = otherCenter - otherHalfSize;
            Vector3 otherMax = otherCenter + otherHalfSize;

            bool isOverlapping = (selfMin.x <= otherMax.x && selfMax.x >= otherMin.x) &&
                             (selfMin.y <= otherMax.y && selfMax.y >= otherMin.y) &&
                             (selfMin.z <= otherMax.z && selfMax.z >= otherMin.z);
            if (isOverlapping)
            {
                hits.Add(unit);
            }
        }
        return hits.ToArray();
    }

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
        Gizmos.DrawWireCube(transform.position + center, size);

        if (world != null && mahhatassRange > 0)
        {
            List<Vector3Int> coverage = world.GetManhattas3DRangePosition(Utils.RoundXZFloorYInt(transform.position), mahhatassRange, false);

            Gizmos.color = new Color(1f, 1f, 0f, 0.5f);
            for (int i = 0; i < coverage.Count; i++)
            {
                Gizmos.DrawCube(coverage[i], Vector3.one);
            }
        }
    }
}
