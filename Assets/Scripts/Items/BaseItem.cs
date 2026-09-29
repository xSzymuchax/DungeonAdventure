using UnityEngine;

[CreateAssetMenu(fileName = "BaseItem", menuName = "Dungeon/Base Item")]
public class BaseItem : ScriptableObject
{
    public string id;
    public string displayName;
    public ItemKind kind;
    public EquipmentSlot slot;
    public GameObject model;
    public int tier = 1;
    public StatRequirement[] requirements;
    public BaseModifier[] modifiers;
    public bool thrownWeapon;
    public int throwDamage;
    public bool sharp;

    void OnValidate()
    {
        if (tier < 1)
            tier = 1;
    }
}

[System.Serializable]
public class BaseModifier
{
    public StatId stat;
    public float value = 1f;
}
