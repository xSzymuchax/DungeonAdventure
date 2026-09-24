using UnityEngine;

[CreateAssetMenu(fileName = "EnemyCatalog", menuName = "Dungeon/Enemy Catalog")]
public class EnemyCatalog : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        public string id;
        public GameObject prefab;
    }

    [SerializeField] Entry[] enemies;

    public bool TryGet(string id, out GameObject prefab)
    {
        if (enemies != null)
        {
            for (int i = 0; i < enemies.Length; i++)
            {
                Entry entry = enemies[i];
                if (entry != null && entry.id == id && entry.prefab != null)
                {
                    prefab = entry.prefab;
                    return true;
                }
            }
        }

        prefab = null;
        return false;
    }
}
