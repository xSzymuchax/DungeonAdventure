using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour, IActor, IWalkable, IHasRepresentation, IFighter, ISkillCaster, IDamagable, ITokenHost, IHasStats
{
    private float _energy;
    private Position2D _currentPosition = new();
    private Position2D _lastPosition = new();
    private Position2D _currentTarget = new();
    private List<Position2D> _currentPath = new();
    private GameObject _myRepresentation;
    private readonly List<IToken> tokens = new();
    private readonly List<Item> lodged = new();
    private readonly List<GameObject> lodgedViews = new();
    protected readonly List<ISkill> skills = new();
    private ISkill basicAttack;
    private bool deathStarted;
    private bool vitalsRestored;

    public float Energy { get => _energy; set { _energy = value; } }
    public float Mana { get; set; }

    public float WalkCost => GetStats().WalkCost;

    public MoveCostManager MoveCostManager => GetStats().MoveCostManager;

    public CharacterStats Stats => GetStats();

    public Position2D CurrentTarget { get => _currentTarget; set { _currentTarget = value; } }

    public List<Position2D> CurrentPath { get => _currentPath; set { _currentPath = value; } }

    public Position2D Position { get => _currentPosition; set { _currentPosition = value; } }

    public GameObject Representation => _myRepresentation;

    public Position2D LastPosition { get => _lastPosition; set { _lastPosition = value; } }

    public event System.Action HealthChanged;
    public event System.Action ManaChanged;

    public float Health { get; private set; }
    public float MaxHealth => GetStats() != null ? GetStats().MaxHealth : 0;
    public bool IsDead => Health <= 0;

    public ISkill BasicAttack
    {
        get
        {
            EnsureCombat();
            return basicAttack;
        }
    }

    public List<ISkill> Skills
    {
        get
        {
            EnsureCombat();
            return skills;
        }
    }

    protected CharacterStats stats;

    private void Start()
    {
        stats = GetStats();
        _myRepresentation = gameObject;
        EnsureCombat();
        if (!vitalsRestored)
        {
            Health = stats.MaxHealth;
            Mana = stats.MaxMana;
            Energy = 10;
        }
        OnStarted();
    }

    protected virtual void OnStarted() { }

    protected void EnsureCombat()
    {
        if (basicAttack != null)
            return;

        GetStats();
        PopulateSkills();
    }

    protected virtual void PopulateSkills()
    {
        BasicMeleeAttack melee = ScriptableObject.CreateInstance<BasicMeleeAttack>();
        basicAttack = melee;
        skills.Add(basicAttack);
    }

    public CharacterStats GetStats()
    {
        if (stats == null)
            stats = GetComponent<CharacterStats>();
        return stats;
    }

    public float GetEnergy()
    {
        return Energy;
    }
    public void AddEnergy(float amount)
    {
        Energy += amount;
    }

    public void RemoveEnergy(float amount)
    {
        Energy -= amount;
    }

    public void AddMana(float amount)
    {
        Mana += amount;
        float maxMana = GetStats().MaxMana;
        if (Mana > maxMana)
            Mana = maxMana;
        ManaChanged?.Invoke();
    }

    public void RemoveMana(float amount)
    {
        Mana -= amount;
        if (Mana < 0)
            Mana = 0;
        ManaChanged?.Invoke();
    }

    public bool TakeDamage(float amount)
    {
        return TakeDamage(amount, DamageType.Physical, true);
    }

    public bool TakeDamage(float amount, bool fromHit)
    {
        return TakeDamage(amount, DamageType.Physical, fromHit);
    }

    public bool TakeDamage(float amount, DamageType type)
    {
        return TakeDamage(amount, type, true);
    }

    public bool TakeDamage(float amount, DamageType type, bool fromHit)
    {
        if (IsDead)
            return true;

        amount = Mitigate(amount, type, fromHit);
        if (amount <= 0f)
            return false;

        Health = Mathf.Max(0f, Health - amount);
        string kind = DamageTypes.Label(type);
        if (DamageTypes.IsMagical(type))
            kind += ", magiczne";
        Debug.Log(name + " took " + amount + " " + kind + " damage, HP=" + Health);
        HealthChanged?.Invoke();
        if (!IsDead && fromHit)
            OnDamaged();
        return IsDead;
    }

    float Mitigate(float amount, DamageType type, bool fromHit)
    {
        if (amount <= 0f)
            return 0f;

        CharacterStats stats = GetStats();
        if (stats != null && stats.Resistance(type) <= -1)
            amount *= 2f;

        if (DamageTypes.IsUnavoidable(type))
            return amount;

        if (!DamageTypes.IsMagical(type))
        {
            if (!fromHit || stats == null)
                return amount;
            float physical = amount - stats.Defense;
            if (physical <= 0f || stats.Block <= 0)
                return physical;
            if (Random.value * 100f < stats.Block)
                physical *= 0.5f;
            return physical;
        }

        if (stats != null && stats.Resistance(type) >= 1)
            return 0f;

        float percent = stats != null ? Mathf.Clamp(stats.MagicResistance, 0f, 100f) : 0f;
        float kept = amount * (100f - percent) / 100f;
        return Mathf.Ceil(kept);
    }

    public bool RollDodge(Character attacker, DamageType type)
    {
        if (type != DamageType.Physical)
            return false;

        CharacterStats stats = GetStats();
        if (stats == null || stats.Dodge <= 0)
            return false;

        int counter = 0;
        if (attacker != null && attacker.GetStats() != null)
            counter = attacker.GetStats().CounterDodge;
        int chance = Mathf.Max(0, stats.Dodge - counter);
        if (chance <= 0 || Random.value * 100f >= chance)
            return false;

        Debug.Log(name + " dodged");
        return true;
    }

    public void Heal(float amount)
    {
        if (IsDead || amount <= 0)
            return;

        float maxHealth = GetStats().MaxHealth;
        float next = Mathf.Min(maxHealth, Health + amount);
        if (next == Health)
            return;

        Health = next;
        HealthChanged?.Invoke();
    }

    protected virtual void OnDamaged() { }

    public IEnumerator PlayDeathAnimation()
    {
        if (deathStarted || !IsDead)
            yield break;

        deathStarted = true;
        yield return AnimateDeath();
        DropLodged();
        HideDestroyed();
        GameController.Instance.NotifyDestroyed(this);
    }

    protected virtual IEnumerator AnimateDeath()
    {
        yield return null;
    }

    public void HideDestroyed()
    {
        if (Representation == null)
            return;

        Transform visual = Representation.transform.Find(Consts.GRAPHIC_REPRESENTATION_IN_ACTOR_NAME);
        if (visual != null)
            visual.gameObject.SetActive(false);
        else
            Representation.SetActive(false);
    }

    public void AddToken(IToken token)
    {
        if (token == null)
            return;
        tokens.Add(token);
    }

    public IReadOnlyList<IToken> ActiveTokens => tokens;
    public IReadOnlyList<Item> LodgedItems => lodged;

    public void Lodge(Item item, GameObject view)
    {
        if (item == null)
            return;

        lodged.Add(item);
        lodgedViews.Add(view);
        if (view == null)
            return;

        view.transform.SetParent(transform, true);
    }

    void DropLodged()
    {
        if (GameController.Instance == null)
            return;

        for (int i = 0; i < lodged.Count; i++)
            GameController.Instance.ReleaseLodgedItem(lodged[i], lodgedViews[i], Position);

        lodged.Clear();
        lodgedViews.Clear();
    }

    public void ReplaceTokens(IReadOnlyList<IToken> restored)
    {
        tokens.Clear();
        if (restored == null)
            return;

        for (int i = 0; i < restored.Count; i++)
        {
            if (restored[i] != null)
                tokens.Add(restored[i]);
        }
    }

    public void RestoreVitals(float health, float mana, float energy)
    {
        stats = GetStats();
        _myRepresentation = gameObject;
        EnsureCombat();
        Health = health;
        Mana = mana;
        Energy = energy;
        vitalsRestored = true;
        HealthChanged?.Invoke();
        ManaChanged?.Invoke();
    }

    public void TickTokens()
    {
        for (int i = tokens.Count - 1; i >= 0; i--)
        {
            tokens[i].Tick(this);
            if (IsDead)
                break;
            if (tokens[i].IsExpired)
                tokens.RemoveAt(i);
        }
    }

    public MoveCostManager GetMoveCosts()
    {
        return GetStats().MoveCostManager;
    }

    public void SetTarget(Position2D target)
    {
        _currentTarget = target;
    }

    public void RecalculatePath(Position2D target)
    {
        GameController.Instance.RequestCalculatePath(this, target);
    }
}
