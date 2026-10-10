using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSheet : MonoBehaviour
{
    const string BonusColor = "#1565C0";

    [SerializeField] GameObject skillsPanel;
    [SerializeField] GameObject statsPanel;
    [SerializeField] Image skillsButton;
    [SerializeField] Image statsButton;
    [SerializeField] TMP_Text statsText;

    PlayerCharacter player;

    static readonly StatId[] Rest =
    {
        StatId.Damage,
        StatId.Defense,
        StatId.Block,
        StatId.CriticalChance,
        StatId.Dodge,
        StatId.CounterDodge,
        StatId.MagicAmplify,
        StatId.MagicResistance,
        StatId.FireResistance,
        StatId.ColdResistance,
        StatId.PoisonResistance,
        StatId.ElectricityResistance,
        StatId.BleedingResistance,
        StatId.HungerResistance,
        StatId.WalkCost,
        StatId.AttackCost,
        StatId.ViewRange,
        StatId.MaxSatiety,
        StatId.MaxHydration,
        StatId.MaxSanity,
        StatId.HealthRegen,
        StatId.ManaRegen,
        StatId.SatietyBurn,
        StatId.HydrationBurn,
        StatId.SanityBurn,
        StatId.ArrowDamage,
        StatId.BoltDamage,
        StatId.DartDamage
    };

    void Awake()
    {
        Wire(skillsButton, ShowSkills);
        Wire(statsButton, ShowStats);
        ShowStats();
    }

    void OnEnable()
    {
        StartCoroutine(BindWhenReady());
    }

    void OnDisable()
    {
        if (GameController.Instance != null)
            GameController.Instance.PlayerReady -= BindPlayer;
        Unbind();
    }

    IEnumerator BindWhenReady()
    {
        while (GameController.Instance == null)
            yield return null;

        GameController.Instance.PlayerReady += BindPlayer;
        if (GameController.Instance.Player != null)
            BindPlayer(GameController.Instance.Player);
    }

    void BindPlayer(PlayerCharacter current)
    {
        if (player == current)
        {
            Refresh();
            return;
        }

        Unbind();
        player = current;
        if (player == null)
            return;

        player.HealthChanged += Refresh;
        player.ManaChanged += Refresh;
        player.SatietyChanged += Refresh;
        player.HydrationChanged += Refresh;
        player.SanityChanged += Refresh;
        if (player.Inventory != null)
            player.Inventory.Changed += Refresh;
        Refresh();
    }

    void Unbind()
    {
        if (player == null)
            return;

        player.HealthChanged -= Refresh;
        player.ManaChanged -= Refresh;
        player.SatietyChanged -= Refresh;
        player.HydrationChanged -= Refresh;
        player.SanityChanged -= Refresh;
        if (player.Inventory != null)
            player.Inventory.Changed -= Refresh;
        player = null;
    }

    void ShowSkills()
    {
        if (statsPanel != null)
            statsPanel.SetActive(false);
        if (skillsPanel != null)
            skillsPanel.SetActive(true);
    }

    void ShowStats()
    {
        if (skillsPanel != null)
            skillsPanel.SetActive(false);
        if (statsPanel != null)
            statsPanel.SetActive(true);
        Refresh();
    }

    void Refresh()
    {
        if (statsText == null)
            return;

        PlayerStats stats = player != null ? player.PlayerStats : null;
        if (player == null || stats == null)
        {
            statsText.text = "";
            return;
        }

        StringBuilder text = new StringBuilder();
        text.Append("<b>");
        text.Append(Line(StatLabel.Name(StatId.Strength), stats.Strength, Gear(StatId.Strength)));
        text.Append('\n');
        text.Append(Line(StatLabel.Name(StatId.Knowledge), stats.Knowledge, Gear(StatId.Knowledge)));
        text.Append('\n');
        text.Append(Line(StatLabel.Name(StatId.MaxHealth), player.Health, stats.MaxHealth, Gear(StatId.MaxHealth)));
        text.Append('\n');
        text.Append(Line(StatLabel.Name(StatId.MaxMana), player.Mana, stats.MaxMana, Gear(StatId.MaxMana)));
        text.Append("</b>\n\n");

        for (int i = 0; i < Rest.Length; i++)
        {
            StatId stat = Rest[i];
            float bonus = Gear(stat);
            if (stat == StatId.MaxSatiety)
                text.Append(Line(StatLabel.Name(stat), player.Satiety, stats.MaxSatiety, bonus));
            else if (stat == StatId.MaxHydration)
                text.Append(Line(StatLabel.Name(stat), player.Hydration, stats.MaxHydration, bonus));
            else if (stat == StatId.MaxSanity)
                text.Append(Line(StatLabel.Name(stat), player.Sanity, stats.MaxSanity, bonus));
            else if (stat == StatId.ArrowDamage || stat == StatId.BoltDamage || stat == StatId.DartDamage)
                text.Append(Line(StatLabel.Name(stat), bonus, bonus));
            else
            {
                float total = ItemRequirements.Value(stats, stat);
                text.Append(Line(StatLabel.Name(stat, total), total, bonus));
            }
            if (i < Rest.Length - 1)
                text.Append('\n');
        }

        statsText.text = text.ToString();
    }

    float Gear(StatId stat)
    {
        if (player == null || player.Inventory == null)
            return 0f;

        float sum = 0f;
        foreach (EquipmentItem equipment in player.Inventory.EquippedGear())
            sum += StatsProvider.Total(equipment, stat);
        return sum;
    }

    static string Line(string label, float total, float bonus)
    {
        return label + " " + Num(total) + Bonus(bonus);
    }

    static string Line(string label, float current, float max, float bonus)
    {
        return label + " " + Num(current) + "/" + Num(max) + Bonus(bonus);
    }

    static string Bonus(float bonus)
    {
        if (Mathf.Abs(bonus) < 0.001f)
            return "";
        string sign = bonus > 0f ? "+" : "";
        return " <color=" + BonusColor + ">(" + sign + Num(bonus) + ")</color>";
    }

    static string Num(float value)
    {
        return value.ToString("0.##");
    }

    static void Wire(Image image, UnityEngine.Events.UnityAction action)
    {
        if (image == null)
            return;

        Button button = image.GetComponent<Button>();
        if (button == null)
            button = image.gameObject.AddComponent<Button>();

        button.onClick.RemoveListener(action);
        button.onClick.AddListener(action);
    }
}
