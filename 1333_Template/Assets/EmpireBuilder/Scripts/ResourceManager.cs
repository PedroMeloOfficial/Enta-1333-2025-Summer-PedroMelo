using System.Collections.Generic;
using Mono.Cecil;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    [Header("All Resource Types")]
    [Tooltip("List of ResourceTypeSO assets to initialize resource entries.")]
    [SerializeField] private List<ResourceType> _resourceTypeSOs = new();

    // Internal dictionary mapping each ResourceDataSO to its current count
    private Dictionary<ResourceType, int> _resources;
    // Lookup map from enum value to ResourceDataSO asset for enum-based methods
    private Dictionary<ResourceList, ResourceType> _enumLookup;

    [Header("Debug: Resource Dictionary")]
    [Tooltip("Read-only list of resources and their counts for debugging.")]
    [SerializeField] private List<string> _debugResourceList = new List<string>();
    public void Initialize()
    {
        _resources = new Dictionary<ResourceType, int>();
        _enumLookup = new Dictionary<ResourceList, ResourceType>();

        foreach (ResourceType data in _resourceTypeSOs)
        {
            if (data == null)
                continue;

            // Initialize resource count
            if (!_resources.ContainsKey(data))
                _resources[data] = 0;

            // Populate enum lookup
            var key = data.resourceType;
            if (!_enumLookup.ContainsKey(key))
                _enumLookup[key] = data;
        }

        UpdateDebugList();
    }

    public void AddResource(ResourceType data, int amount)
    {
        if (data == null)
            return;

        if (!_resources.ContainsKey(data))
            _resources[data] = 0;

        _resources[data] += amount;
        Debug.Log($"ResourceManager: Added {amount}x {data.DisplayName}. New total: {_resources[data]}");

        UpdateDebugList();
    }

    public void TryAddResource(ResourceList type, int amount)
    {
        if (_enumLookup.TryGetValue(type, out var data))
            AddResource(data, amount);
    }

    public bool SpendResource(ResourceType data, int amount)
    {
        if (data == null || !_resources.ContainsKey(data) || _resources[data] < amount)
            return false;

        _resources[data] -= amount;
        Debug.Log($"ResourceManager: Spent {amount}x {data.DisplayName}. Remaining: {_resources[data]}");

        UpdateDebugList();
        return true;
    }

    public bool TrySpendResource(ResourceList type, int amount)
    {
        if (_enumLookup.TryGetValue(type, out var data))
            return SpendResource(data, amount);
        return false;
    }

    public int GetResourceCount(ResourceType data)
    {
        if (data == null)
            return 0;

        return _resources.TryGetValue(data, out var count) ? count : 0;
    }

    public int TryGetResourceCount(ResourceList type)
    {
        if (_enumLookup.TryGetValue(type, out var data))
            return GetResourceCount(data);
        return 0;
    }

    public IReadOnlyDictionary<ResourceType, int> GetAllResources() => _resources;

    // Updates the debug list shown in the Inspector.
    private void UpdateDebugList()
    {
        _debugResourceList.Clear();
        foreach (var kv in _resources)
        {
            var name = kv.Key != null ? kv.Key.DisplayName : "Unknown";
            _debugResourceList.Add($"{name}: {kv.Value}");
        }
    }

    private void OnValidate()
    {
        if (_resources != null)
            UpdateDebugList();
    }
}