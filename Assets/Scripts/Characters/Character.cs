using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour, IActor, IWalkable
{
    private double _energy;
    private Position2D _currentTarget = new();
    private List<Position2D> _currentPath = new();

    public double Energy { get => _energy; set { _energy = value; } }

    public double WalkCost => stats.CurrentWalkingCost;

    public MoveCostManager MoveCostManager => moveCostManager;

    public Position2D CurrentTarget => _currentTarget;

    public List<Position2D> CurrentPath => _currentPath;

    protected MoveCostManager moveCostManager;
    protected CharacterStats stats;

    private void Start()
    {
        InitMoveCosts();
        stats = GetComponent<CharacterStats>();
        Energy = 10;
    }

    protected virtual void InitMoveCosts() { }

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

    public MoveCostManager GetMoveCosts()
    {
        return moveCostManager;
    }

    public IEnumerator WalkingAnimation(Vector3 targetPosition)
    {
        Vector3 startPosition = transform.position;
        float duration = Consts.WALK_ANIMATION_TIME;
        float progress = 0f;

        while (progress < duration)
        {
            progress += Time.deltaTime;
            float completion = progress / duration;

            transform.position = Vector3.Lerp(startPosition, targetPosition, completion);

            yield return null;
        }

        transform.position = targetPosition;
    }

    public void SetTarget(Position2D target)
    {
        _currentTarget = target;
    }

    public void RecalculatePath(Position2D target)
    {
        _currentPath = GameController.Instance.RequestCalculatePath(this, target);
        _currentTarget = target;
    }
}
