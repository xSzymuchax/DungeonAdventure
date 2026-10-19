public class Kusznik : CharacterClass
{
    public override string ClassName => "Kusznik";
    public override int ClassId => (int)PlayerClass.Kusznik;
    public override int HealthPerLevel => 5;
    public override int ManaPerLevel => 5;
    public override float StrengthPerStep => 1f;
    public override float KnowledgePerStep => 1f;

    public override void GiveStartingGear(PlayerCharacter player, ItemCatalog catalog)
    {
        Equip(player, catalog, "armor_tier1");
        Equip(player, catalog, "bow_tier1");
        GiveArrows(player, catalog, 3);
    }
}
