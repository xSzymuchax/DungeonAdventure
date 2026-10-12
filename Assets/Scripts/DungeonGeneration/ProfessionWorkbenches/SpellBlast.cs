using System.Collections;
using UnityEngine;

public class SpellBlast : MonoBehaviour
{
    [SerializeField] float growTime = 0.5f;
    [SerializeField] float shrinkTime = 0.35f;
    [SerializeField] float fullTiles = 3f;

    float groundY;
    float startDiameter;
    bool grown;

    void Start()
    {
        groundY = transform.position.y;
        startDiameter = transform.localScale.x;
        StartCoroutine(Play());
    }

    public IEnumerator UntilGrown()
    {
        while (!grown)
            yield return null;
    }

    IEnumerator Play()
    {
        float end = Consts.TILE_SIZE * (fullTiles < 1f ? 1f : fullTiles);
        yield return Resize(startDiameter, end, growTime);
        grown = true;
        yield return Resize(end, startDiameter, shrinkTime);
        Destroy(gameObject);
    }

    IEnumerator Resize(float from, float to, float duration)
    {
        if (duration <= 0f)
        {
            Apply(to);
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            Apply(Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration)));
            yield return null;
        }

        Apply(to);
    }

    void Apply(float diameter)
    {
        transform.localScale = Vector3.one * diameter;
        Vector3 at = transform.position;
        at.y = groundY + diameter * 0.5f;
        transform.position = at;
    }
}
