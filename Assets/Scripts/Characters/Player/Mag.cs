public class Mag : CharacterClass
{
    public override string ClassName => "Mag";
    public override int ClassId => (int)PlayerClass.Mag;
    public override int HealthPerLevel => 3;
    public override int ManaPerLevel => 7;
    public override float StrengthPerStep => 0.5f;
    public override float KnowledgePerStep => 1.5f;

    public override void GiveStartingGear(PlayerCharacter player, ItemCatalog catalog)
    {
        Equip(player, catalog, "armor_tier1");
        Equip(player, catalog, "staff_tier1");
    }
}
