using UnityEngine;

[RequireComponent(typeof(Renderer))]
public abstract class BuildingBase : MonoBehaviour, ISelectable
{
    public Team team;
    public Material[] teamMaterials;
    public BuildingData buildingData;

    protected Renderer renderer;

    public int CurrentHealth { get; private set; }
    public int MaxHealth { get; private set; }

    protected virtual void Awake()
    {
        renderer = GetComponent<Renderer>();
        InitializeHealth();
    }

    private void InitializeHealth()
    {
        MaxHealth = buildingData.Health;
        CurrentHealth = MaxHealth;
    }

    public virtual void ApplyTeamMaterial()
    {
        int index = (int)team;
        if (teamMaterials == null || index < 0 || index >= teamMaterials.Length)
            return;

        renderer.material = teamMaterials[index];
    }

    public abstract void OnSelected();
    public abstract void OnDeselected();
}