using UnityEngine;

public class HostileMeleeAI : UnitAI
{
    public override GridNode GetDestination()
    {
        GameObject friendly = FindClosestTarget("Ally");
        CurrentTarget = friendly;

        if (friendly == null) return null;

        GridNode node = GridManager.GetNodeFromWorldPosition(friendly.transform.position);
        if (node.Walkable) return node;

        GridNode self = GridManager.GetNodeFromWorldPosition(transform.position);
        GridNode fallback = GridManager.GetNearestWalkableNeighbour(self, node);
        return fallback ?? node;
    }

    private GameObject FindClosestTarget(string tag)
    {
        float min = float.MaxValue;
        GameObject nearest = null;

        foreach (var go in GameObject.FindGameObjectsWithTag(tag))
        {
            float dist = Vector3.Distance(transform.position, go.transform.position);
            if (dist < min)
            {
                min = dist;
                nearest = go;
            }
        }

        return nearest;
    }
}