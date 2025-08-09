using UnityEngine;

public class BasicMeleeAI : UnitAI
{
    public override GridNode GetDestination()
    {
        GameObject enemy = FindClosestTarget("Enemy");
        CurrentTarget = enemy;

        if (enemy == null) return null;

        GridNode node = GridManager.GetNodeFromWorldPosition(enemy.transform.position);
        if (node.Walkable) return node;

        GridNode self = GridManager.GetNodeFromWorldPosition(transform.position);
        GridNode alternate = GridManager.GetNearestWalkableNeighbour(self, node);
        return alternate ?? node;
    }

    private GameObject FindClosestTarget(string tag)
    {
        GameObject best = null;
        float minDist = float.MaxValue;

        foreach (var obj in GameObject.FindGameObjectsWithTag(tag))
        {
            float d = Vector3.Distance(transform.position, obj.transform.position);
            if (d < minDist)
            {
                minDist = d;
                best = obj;
            }
        }

        return best;
    }
}