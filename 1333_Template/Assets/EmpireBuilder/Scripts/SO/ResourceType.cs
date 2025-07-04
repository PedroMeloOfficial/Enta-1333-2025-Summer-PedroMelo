using UnityEngine;

[CreateAssetMenu(fileName = "ResourceData", menuName = "Game/Resource Data")]
public class ResourceType : ScriptableObject
{
    [Header("Info")]
    [SerializeField] private ResourceList _resourceType;

    [SerializeField] private string _displayName;

    [SerializeField] private Sprite _icon;

    [SerializeField] private string _description;

    public ResourceList resourceType => _resourceType;
    public string DisplayName => _displayName;
    public Sprite Icon => _icon;
    public string Description => _description;
}