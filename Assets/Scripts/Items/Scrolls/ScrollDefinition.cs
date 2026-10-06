using UnityEngine;

[CreateAssetMenu(fileName = "Scroll", menuName = "Dungeon/Scroll")]
public class ScrollDefinition : ScriptableObject
{
    public string id;
    public string displayName;
    [Min(0f)] public float weight = 10f;
    public GameObject model;
    public Effect[] effects;

    void OnValidate()
    {
        if (weight < 0f)
            weight = 0f;
    }
}
