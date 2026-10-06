using UnityEngine;

[CreateAssetMenu(fileName = "Food", menuName = "Dungeon/Food")]
public class FoodDefinition : ScriptableObject
{
    public string id;
    public string displayName;
    [TextArea(2, 5)] public string description;
    [Min(0f)] public float weight = 10f;
    public GameObject model;
    public float satiety;
    public float hydration;
    public float sanity;
    public int timeToEat = 1;
    public bool spoils;

    void OnValidate()
    {
        if (timeToEat < 1)
            timeToEat = 1;
        if (weight < 0f)
            weight = 0f;
    }
}
