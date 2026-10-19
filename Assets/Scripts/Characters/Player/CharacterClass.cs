using UnityEngine;

public abstract class CharacterClass : MonoBehaviour
{
    public abstract string ClassName { get; }
    public abstract int ClassId { get; }
    public abstract int HealthPerLevel { get; }
    public abstract int ManaPerLevel { get; }
    public abstract float StrengthPerStep { get; }
    public abstract float KnowledgePerStep { get; }

    public abstract void GiveStartingGear(PlayerCharacter player, ItemCatalog catalog);

    public static CharacterClass Attach(GameObject host, int classId)
    {
        if (host == null)
            return null;

        System.Type kind = TypeOf(classId);
        CharacterClass current = host.GetComponent<CharacterClass>();
        if (current != null && current.GetType() == kind)
            return current;

        if (current != null)
            DestroyImmediate(current);

        return (CharacterClass)host.AddComponent(kind);
    }

    public static System.Type TypeOf(int classId)
    {
        if (classId == (int)PlayerClass.Szermierz)
            return typeof(Szermierz);
        if (classId == (int)PlayerClass.Mag)
            return typeof(Mag);
        return typeof(Kusznik);
    }

    protected static void Equip(PlayerCharacter player, ItemCatalog catalog, string id)
    {
        if (player == null || player.Inventory == null || catalog == null || !catalog.TryGetEquipment(id, out BaseItem definition))
            return;

        EquipmentItem item = ItemGenerator.GenerateEquipment(definition, catalog.statRolls, false);
        player.Inventory.PutEquipped(item);
    }

    protected static void GiveArrows(PlayerCharacter player, ItemCatalog catalog, int count)
    {
        if (count < 1 || player == null || player.Inventory == null || catalog == null || !catalog.TryGetAmmunition("arrow", out AmmunitionDefinition definition))
            return;

        Ammunition arrows = ItemGenerator.GenerateAmmunition(definition);
        if (arrows == null)
            return;

        arrows.Count = count;
        int piece = arrows.MaxDurability < 1 ? 1 : arrows.MaxDurability;
        arrows.Durability = count * piece;
        player.Inventory.TryAdd(arrows);
    }
}
