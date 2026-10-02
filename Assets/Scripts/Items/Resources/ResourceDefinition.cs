using UnityEngine;

[CreateAssetMenu(fileName = "Resource", menuName = "Dungeon/Resource")]
public class ResourceDefinition : ScriptableObject
{
    public string id;
    public string displayName;
    public GameObject model;
}
