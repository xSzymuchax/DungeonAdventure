using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ProjectileStop
{
    OnObstacle,
    BeforeObstacle
}

public class ProjectileSystem
{
    Dungeon dungeon;

    public void Bind(Dungeon dungeon)
    {
        this.dungeon = dungeon;
    }

    public Position2D Impact(Position2D from, Position2D to, ProjectileStop stop)
    {
        if (dungeon == null)
            return to;

        List<Position2D> line = from.LineTo(to);
        for (int i = 1; i < line.Count; i++)
        {
            if (!dungeon.IsWall(line[i]))
                continue;

            return stop == ProjectileStop.OnObstacle ? line[i] : line[i - 1];
        }

        return to;
    }

    public IEnumerator Fly(GameObject view, Position2D from, Position2D to, float height, float arc, float minDuration)
    {
        if (view == null || dungeon == null)
            yield break;

        Vector3 start = dungeon.GetTileWorldPosition(from) + Vector3.up * height;
        Vector3 end = dungeon.GetTileWorldPosition(to) + Vector3.up * height;
        float duration = Mathf.Max(minDuration, Consts.WALK_ANIMATION_TIME * from.ChebyshevTo(to));
        float progress = 0f;
        view.transform.position = start;

        while (progress < duration)
        {
            progress += Time.deltaTime;
            float t = Mathf.Clamp01(progress / duration);
            Vector3 position = Vector3.Lerp(start, end, t);
            position.y += Mathf.Sin(t * Mathf.PI) * arc;
            view.transform.position = position;
            yield return null;
        }

        view.transform.position = end;
        Object.Destroy(view);
    }
}
