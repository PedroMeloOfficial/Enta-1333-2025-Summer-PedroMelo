using UnityEngine;

public class HostileMeleeAI : UnitAI
{
    [SerializeField] private float targetDistance;
    [SerializeField] private EnemyFindStyle findStyle;

    public override GridNode GetDestination()
    {
        GameObject target = FindClosestTarget("Ally");
        CurrentTarget = target;

        // if (target == null) return null;
        if (target == null)
        {
            return GridManager.GetNodeAt(Random.Range(0, GridManager.GridSettings.GridSizeX), Random.Range(0, GridManager.GridSettings.GridSizeY));
        }

        GridNode node = GridManager.GetNodeFromWorldPosition(target.transform.position);
        if (node.Walkable) return node;

        GridNode self = GridManager.GetNodeFromWorldPosition(transform.position);
        GridNode fallback = GridManager.GetNearestWalkableNeighbour(self, node);
        return fallback ?? node;
    }

    private GameObject FindClosestTarget(string tag)
    {
        float min;
        if (findStyle == EnemyFindStyle.ByDistance)
        {
            min = targetDistance;
        }
        else 
        {
            min = float.MaxValue;
        }

        GameObject nearest = null;

        foreach (var go in GameObject.FindGameObjectsWithTag(tag))
        {
            float dist = Vector3.Distance(transform.position, go.transform.position);
            if (dist < min)
            {
                if (go.GetComponent("UnitMover")) 
                {
                    min = dist;
                }
                else if (go.GetComponent("ArcheryTower"))
                {
                    min = dist;
                }
                else
                {
                    min = dist;
                }
                nearest = go;
            }
        }

        return nearest;
    }

    enum EnemyFindStyle {
        Anywhere,
        ByDistance
    }
}