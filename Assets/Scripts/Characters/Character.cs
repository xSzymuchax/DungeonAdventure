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

    public int Health { get; set; }
    public bool IsDead => Health <= 0;

    public ISkill BasicAttack
    {
        get
        {
            EnsureCombat();
            return basicAttack;
        }
    }

    public IReadOnlyList<ISkill> Skills
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
        Health = stats.MaxHealth;
        Mana = stats.MaxMana;
        Energy = 10;
        EnsureCombat();
    }

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
    }

    public void RemoveMana(double amount)
    {
        Mana -= amount;
        if (Mana < 0)
            Mana = 0;
    }

    public bool TakeDamage(int amount)
    {
        if (IsDead)
            return true;

        Health = Mathf.Max(0, Health - amount);
        Debug.Log(name + " took " + amount + " damage, HP=" + Health);
        if (!IsDead)
            OnDamaged();
        return IsDead;
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

    private void HideDestroyed()
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
