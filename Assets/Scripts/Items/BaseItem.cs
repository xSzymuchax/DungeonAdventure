using UnityEngine;

[CreateAssetMenu(fileName = "BaseItem", menuName = "Dungeon/Base Item")]
public class BaseItem : ScriptableObject
{
    public string id;
    public string displayName;
    public EquipmentSlot slot;
    public GameObject model;
    public int tier = 1;
    public int maxDurability = 10;
    public StatRequirement[] requirements;
    public BaseModifier[] modifiers;
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
    }
}

[System.Serializable]
public class BaseModifier
{
    public StatId stat;
    public float value = 1f;
}
