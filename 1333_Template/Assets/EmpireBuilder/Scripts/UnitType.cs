using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitType", menuName = "Game/Unit Type")]
public class UnitType : ScriptableObject
{
    [SerializeField] private int width = 1;
    [SerializeField] private int height = 1;

    [SerializeField] private int maxHp = 1;
    [SerializeField] private float moveSpeed = 1;
    [SerializeField] private int damage = 1;
    [SerializeField] private int defense = 1;
    [SerializeField] private AttackType attackType = AttackType.Melee;
    [SerializeField] private int range = 1;
    [SerializeField] private GameObject prefab;
    [Header("Team Materials")]
    [SerializeField] private Material[] teamMaterials = null;

    public int Width => width;
    public int Height => height;
    public int MaxHp => maxHp;
    public float MoveSpeed => moveSpeed;
    public int Damage => damage;
    public int Defense => defense;

    public GameObject Prefab => prefab;

    public Material GetTeamMaterial(Team team)
    {
        int index = (int)team;
        if (teamMaterials != null && index >= 0 && index < teamMaterials.Length)
        {
            return teamMaterials[index];
        }
        return null;
    }
}