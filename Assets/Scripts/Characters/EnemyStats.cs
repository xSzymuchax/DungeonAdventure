using UnityEngine;

public class EnemyStats : CharacterStats
{
    [Header("Combat")]
    [SerializeField] int baseAttackRange = 1;

    [Header("Perception")]
    [SerializeField] int baseWakeUpRange = 3;
    [SerializeField] int baseDetectionRange = 5;

    public int AttackRange { get; private set; }
    public int WakeUpRange { get; private set; }
    public int DetectionRange { get; private set; }

    public override void Recalculate()
    {
        base.Recalculate();
        AttackRange = baseAttackRange;
        WakeUpRange = baseWakeUpRange;
        DetectionRange = baseDetectionRange;
    }

    public void ApplySavedRanges(int attackRange, int wakeUpRange, int detectionRange)
    {
        AttackRange = attackRange;
        WakeUpRange = wakeUpRange;
        DetectionRange = detectionRange;
    }
}
