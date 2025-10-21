using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UnitDetectable))]
public class MapSwitchTrigger : Entity
{
    public MapData mapData;
    public Transform teleportPoint;
    public Transform returnPoint;
    private UnitDetectable selfDetectable;

    public static MapSwitchTrigger currentTrigger;

    public void Update()
    {
        if (currentTrigger != null) { return; }

        UnitDetectable selfDetectable = GetComponentInChildren<UnitDetectable>();
        UnitDetectable[] unitDetectables = selfDetectable.OverlapSelfRange();
        
        foreach (UnitDetectable detectable in unitDetectables)
        {
            PlayerCharacter playerCharacter = detectable.GetComponent<PlayerCharacter>();
            if (playerCharacter == null) { continue; }
            if (playerCharacter.isLeader)
            {
                if (teleportPoint != null)
                {
                    currentTrigger = this;
                    MapTransitionManger.instance.SaveSnapShot(MapManager.instance.currentActivatedMap, returnPoint, playerCharacter);
                    MapTransitionManger.instance.RequestMapTransition(mapData, 
                        teleportPoint.position, playerCharacter,
                        () => { currentTrigger = null; });
                }
                return;
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (teleportPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(teleportPoint.position, 0.5f);
        }
        if (returnPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(returnPoint.position, 0.4f);
        }
    }
}
