using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCharacter : Character, IHasPerception
{
    [SerializeField] string enemyId = "base_enemy";
    [SerializeField] string displayName;
    [SerializeField] string description;
    [SerializeField] OnHitEffect[] onHits;
    [SerializeField] int experience = 15;

    public OnHitEffect[] OnHits => onHits;
    public int Experience => experience < 0 ? 0 : experience;

    public string EnemyId => string.IsNullOrEmpty(enemyId) ? "base_enemy" : enemyId;
    public string Title => string.IsNullOrEmpty(displayName) ? EnemyId : displayName;

    public string Description()
    {
        return description ?? "";
    }

    public Sprite Picture()
    {
        return Portrait.Of(this);
    }
    public EnemyStats EnemyStats => GetStats() as EnemyStats;

    void Awake()
    {
        SetStatsProvider(new StatsProvider(GetComponent<EnemyStats>()));
    }

    public int ViewRange => EnemyStats.ViewRange;
    public int WakeUpRange => EnemyStats.WakeUpRange;
    public int AttackRange => EnemyStats.AttackRange;
    public int DetectionRange => EnemyStats.DetectionRange;

    protected override void OnDamaged()
    {
        EnemyController controller = GetComponent<EnemyController>();
        controller?.OnDamaged();
    }
}
