using UnityEngine;

public class Entity : MonoBehaviour
{
    protected World world;
    protected PathFinding pathFinding;
    protected virtual void Start()
    {
        world = WorldGeneration.instance.world;
        pathFinding = new PathFinding(world);
    }
}

