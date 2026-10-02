using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillEffect", menuName = "Dungeon/Effect/Skill")]
public class SkillEffect : Effect
{
    public Skill skill;
    public string label = "Zaklęcie";

    public override string Label => string.IsNullOrEmpty(label) ? name : label;
    public override bool NeedsTarget => true;
    public override int Range => skill != null ? skill.Range : Consts.RANGED_ATTACK_RANGE;

    public override IEnumerator Apply(PlayerCharacter player, Position2D target)
    {
        if (player == null || skill == null)
            yield break;

        yield return skill.Use(player, target);
    }
}
