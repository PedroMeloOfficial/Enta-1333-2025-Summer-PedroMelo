// Assets/Scripts/Units/UnitAI.cs
using UnityEngine;

public abstract class UnitAI : MonoBehaviour
{
    [HideInInspector] public GridManager GridManager;
    [HideInInspector] public Pathfinding Pathfinding;

    /// The GameObject this unit is currently pursuing / attacking.
    public GameObject CurrentTarget { get; protected set; }  

    public virtual void Initialise(GridManager gm, Pathfinding pf)
    {
        GridManager  = gm;
        Pathfinding  = pf;
    }

    /// Must return the node this agent wants to reach.
    public abstract GridNode GetDestination();
}
