using UnityEngine;

[CreateAssetMenu(fileName = "Scroll", menuName = "Dungeon/Scroll")]
public class ScrollDefinition : ScriptableObject
{
    public string id;
    public string displayName;
    public GameObject model;
    public Effect[] effects;
}
