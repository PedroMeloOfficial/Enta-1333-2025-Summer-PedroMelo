using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class UnitBase : MonoBehaviour
{
    [SerializeField] protected UnitType unitType;
    public virtual int Width => unitType.Width;
    public virtual int Height => unitType.Height;
    public abstract void MoveTo(GridNode targetNode);
}