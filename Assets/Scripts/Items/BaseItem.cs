using UnityEngine;

[CreateAssetMenu(fileName = "BaseItem", menuName = "Dungeon/Base Item")]
public class BaseItem : ScriptableObject
{
    public string id;
    public string displayName;
    [Min(0f)] public float weight = 10f;
    public EquipmentSlot slot;
    public GameObject model;
    public int tier = 1;
    public int maxDurability = 10;
    public StatRequirement[] requirements;
    public BaseModifier[] modifiers;
    [TextArea(2, 5)] public string description;
    public bool thrownWeapon;
    public int throwDamage;
    public bool sharp;
    public WeaponKind weaponKind;

    void OnValidate()
    {
        if (tier < 1)
            tier = 1;
        if (maxDurability < 1)
            maxDurability = Consts.DEFAULT_DURABILITY;
        if (weight < 0f)
            weight = 0f;
    }
}

[System.Serializable]
public class BaseModifier
{
    public StatId stat;
    public float value = 1f;
}
