using UnityEngine;

[CreateAssetMenu(fileName = "Ammunition", menuName = "Dungeon/Ammunition")]
public class AmmunitionDefinition : ScriptableObject
{
    public string id;
    public string displayName;
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
    }
}
