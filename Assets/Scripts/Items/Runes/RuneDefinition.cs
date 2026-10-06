using UnityEngine;

[CreateAssetMenu(fileName = "Rune", menuName = "Dungeon/Rune")]
public class RuneDefinition : ScriptableObject
{
    public string id;
    public string displayName;
    [TextArea(2, 5)] public string description;
    [Min(0f)] public float weight = 10f;
    public GameObject model;
    public int maxCharges = 1;
    public int throwDamage;
    public Effect[] effects;

    void OnValidate()
    {
        if (maxCharges < 1)
            maxCharges = 1;
        if (weight < 0f)
            weight = 0f;
    }
}
