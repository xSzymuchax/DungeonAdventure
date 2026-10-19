public class Szermierz : CharacterClass
{
    public override string ClassName => "Szermierz";
    public override int ClassId => (int)PlayerClass.Szermierz;
    public override int HealthPerLevel => 7;
    public override int ManaPerLevel => 3;
    public override float StrengthPerStep => 1.5f;
    public override float KnowledgePerStep => 0.5f;

    public override void GiveStartingGear(PlayerCharacter player, ItemCatalog catalog)
    {
        Equip(player, catalog, "armor_tier1");
        Equip(player, catalog, "sword_tier1");
    }
}
