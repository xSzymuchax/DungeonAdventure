using UnityEngine;

[CreateAssetMenu(fileName = "Rune", menuName = "Dungeon/Rune")]
public class RuneDefinition : ScriptableObject
{
    public string id;
    public string displayName;
    public GameObject model;
    public int maxCharges = 1;
    public int throwDamage;
    public Effect[] effects;

    void OnValidate()
    {
        if (maxCharges < 1)
            maxCharges = 1;
    }
}
