using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCharacter : Character, IHasPerception
{
    [SerializeField] string enemyId = "base_enemy";
    [SerializeField] OnHitEffect[] onHits;

    public OnHitEffect[] OnHits => onHits;

    public string EnemyId => string.IsNullOrEmpty(enemyId) ? "base_enemy" : enemyId;
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
