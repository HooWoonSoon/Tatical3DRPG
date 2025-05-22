using UnityEngine;
using System.Collections.Generic;
public class UnitDetectable : MonoBehaviour
{
    public Vector3Int GetPositionRoundXZIntY()
    {
        return Utils.RoundXZFloorYInt(this.transform.position);
    }
}
