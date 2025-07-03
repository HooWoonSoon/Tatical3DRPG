using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
public class UnitDetectable : Entity
{
    public Vector3 center;
    public Vector3 size;
    private int mahhatassRange;

    public static List<UnitDetectable> all = new List<UnitDetectable>();

    private void OnEnable() { all.Add(this); }
    private void OnDisable() { all.Remove(this); }

    protected override void Start()
    {
        base.Start();
    }

    public void AABBRange()
    {
        Vector3 worldcenter = transform.position + center;
        Vector3 halfSize = size / 2;

        Vector3 max = worldcenter + halfSize;
        Vector3 min = worldcenter - halfSize;
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

        if (world != null)
        {
            Gizmos.color = Color.yellow;
            List<Vector3Int> coverage = world.GetManhattas3DRange(Utils.RoundXZFloorYInt(transform.position), mahhatassRange, false);

            if (coverage.Count == 0 || mahhatassRange == 0) { return; }
            for (int i = 0; i < coverage.Count; i++)
            {
                Gizmos.DrawWireCube(coverage[i], Vector3.one);
            }
        }
    }
}
