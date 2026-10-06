using UnityEngine;

[CreateAssetMenu(fileName = "Resource", menuName = "Dungeon/Resource")]
public class ResourceDefinition : ScriptableObject
{
    public string id;
    public string displayName;
    [Min(0f)] public float weight = 10f;
    public GameObject model;

    void OnValidate()
    {
        if (weight < 0f)
            weight = 0f;
    }
}
