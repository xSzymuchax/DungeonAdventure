using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour, IActor, IWalkable, IHasRepresentation, IFighter, ISkillCaster, IDamagable, ITokenHost
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

    public double Energy { get => _energy; set { _energy = value; } }
    public double Mana { get; set; } = 20;

    public double WalkCost => stats.CurrentWalkingCost;

    public MoveCostManager MoveCostManager => moveCostManager;

    public Position2D CurrentTarget { get => _currentTarget; set { _currentTarget = value; } }

    public List<Position2D> CurrentPath { get => _currentPath; set { _currentPath = value; } }

    public Position2D Position { get => _currentPosition; set { _currentPosition = value; } }

    public GameObject Representation => _myRepresentation;

    public Position2D LastPosition { get => _lastPosition; set { _lastPosition = value; } }

    public int Health { get; set; } = 20;

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

    protected MoveCostManager moveCostManager;
    protected CharacterStats stats;

    private void Start()
    {
        InitMoveCosts();
        stats = GetComponent<CharacterStats>();
        _myRepresentation = gameObject;
        Energy = 10;
        EnsureCombat();
    }

    protected virtual void InitMoveCosts() { }

    protected void EnsureCombat()
    {
        if (basicAttack != null)
            return;

        if (stats == null)
            stats = GetComponent<CharacterStats>();

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
    }

    public void RemoveMana(double amount)
    {
        Mana -= amount;
        if (Mana < 0)
            Mana = 0;
    }

    public bool TakeDamage(int amount)
    {
        Health = Mathf.Max(0, Health - amount);
        Debug.Log(name + " took " + amount + " damage, HP=" + Health);
        return Health <= 0;
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
            if (tokens[i].IsExpired)
                tokens.RemoveAt(i);
        }
    }

    public MoveCostManager GetMoveCosts()
    {
        return moveCostManager;
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
