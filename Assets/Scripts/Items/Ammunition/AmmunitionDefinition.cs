using UnityEngine;

[CreateAssetMenu(fileName = "Ammunition", menuName = "Dungeon/Ammunition")]
public class AmmunitionDefinition : ScriptableObject
{
    public string id;
    public string displayName;
    [Min(0f)] public float weight = 10f;
    public GameObject model;
    public int damage = 1;
    public int maxDurability = 10;
    public WeaponKind launcher;

    void OnValidate()
    {
        if (damage < 0)
            damage = 0;
        if (maxDurability < 1)
            maxDurability = Consts.DEFAULT_DURABILITY;
        if (weight < 0f)
            weight = 0f;
    }
}
