using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour, IActor, IWalkable, IHasRepresentation, IFighter, ISkillCaster, IDamagable, ITokenHost, IHasStats
{
    private double _energy;
    private Position2D _currentPosition = new();
    private Position2D _lastPosition = new();
    private Position2D _currentTarget = new();
    private List<Position2D> _currentPath = new();
    private GameObject _myRepresentation;
    private readonly List<IToken> tokens = new();
    protected readonly List<ISkill> skills = new();
    private ISkill basicAttack;
    private bool deathStarted;
    private bool vitalsRestored;

    public double Energy { get => _energy; set { _energy = value; } }
    public double Mana { get; set; }

    public double WalkCost => GetStats().WalkCost;

    public MoveCostManager MoveCostManager => GetStats().MoveCostManager;

    public CharacterStats Stats => GetStats();

    public Position2D CurrentTarget { get => _currentTarget; set { _currentTarget = value; } }

    public List<Position2D> CurrentPath { get => _currentPath; set { _currentPath = value; } }

    public Position2D Position { get => _currentPosition; set { _currentPosition = value; } }

    public GameObject Representation => _myRepresentation;

    public Position2D LastPosition { get => _lastPosition; set { _lastPosition = value; } }

    public event System.Action HealthChanged;
    public event System.Action ManaChanged;

    public int Health { get; private set; }
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

    public double GetEnergy()
    {
        return Energy;
    }
    public void AddEnergy(double amount)
    {
        Energy += amount;
    }

    public void RemoveEnergy(double amount)
    {
        Energy -= amount;
    }

    public void AddMana(double amount)
    {
        Mana += amount;
        double maxMana = GetStats().MaxMana;
        if (Mana > maxMana)
            Mana = maxMana;
        ManaChanged?.Invoke();
    }

    public void RemoveMana(double amount)
    {
        Mana -= amount;
        if (Mana < 0)
            Mana = 0;
        ManaChanged?.Invoke();
    }

    public bool TakeDamage(int amount)
    {
        if (IsDead)
            return true;

        Health = Mathf.Max(0, Health - amount);
        Debug.Log(name + " took " + amount + " damage, HP=" + Health);
        HealthChanged?.Invoke();
        if (!IsDead)
            OnDamaged();
        return IsDead;
    }

    public void Heal(int amount)
    {
        if (IsDead || amount <= 0)
            return;

        int maxHealth = GetStats().MaxHealth;
        int next = Mathf.Min(maxHealth, Health + amount);
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

    public void RestoreVitals(int health, double mana, double energy)
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
