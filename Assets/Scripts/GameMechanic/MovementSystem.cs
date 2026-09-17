using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MovementSystem : MonoBehaviour
{
    public Dungeon dungeonFloor;
    private readonly List<(IWalkable walker, Position2D from, Position2D to)> pendingWalkAnimations = new();

    public IEnumerator WalkingAnimation(IWalkable walker, Position2D from, Position2D to)
    {
        Vector3 startPosition = new Vector3(from.x * Consts.TILE_SIZE + 0.5f* Consts.TILE_SIZE, 0, from.y * Consts.TILE_SIZE + 0.5f * Consts.TILE_SIZE);
        Vector3 targetPosition = new Vector3(to.x * Consts.TILE_SIZE + 0.5f * Consts.TILE_SIZE, 0, to.y * Consts.TILE_SIZE + 0.5f * Consts.TILE_SIZE);
        float duration = Consts.WALK_ANIMATION_TIME;
        float progress = 0f;

        Transform representation = ((IHasRepresentation)walker).Representation.transform;
        Debug.Log(representation.name);
        while (progress < duration)
        {
            progress += Time.deltaTime;
            float completion = progress / duration;

            representation.position = Vector3.Lerp(startPosition, targetPosition, completion);

            yield return null;
        }

        representation.position = targetPosition;
    }

    public IEnumerator PlayPendingAnimations()
    {
        while (pendingWalkAnimations.Count > 0)
        {
            List<(IWalkable walker, Position2D from, Position2D to)> thisRound = new();
            List<(IWalkable walker, Position2D from, Position2D to)> remaining = new();
            HashSet<IWalkable> used = new();

            foreach (var pending in pendingWalkAnimations)
            {
                if (used.Add(pending.walker))
                    thisRound.Add(pending);
                else
                    remaining.Add(pending);
            }

            pendingWalkAnimations.Clear();
            pendingWalkAnimations.AddRange(remaining);

            List<Coroutine> playing = new();
            foreach (var anim in thisRound)
                playing.Add(StartCoroutine(WalkingAnimation(anim.walker, anim.from, anim.to)));

            foreach (var coroutine in playing)
                yield return coroutine;
        }
    }

    public void SetTarget(IWalkable walker, Position2D target)
    {
        walker.CurrentTarget = target;
    }

    public List<Position2D> RecalculatePath(IWalkable walker, Position2D target)
    {
        List<Position2D> result = new();
        result = AStar.FindClosestPath(dungeonFloor.GetTileInfos(), dungeonFloor.FindActorPosition(walker), target, walker.MoveCostManager, MovementDirections.EIGHT);
        if (result.Count == 0)
        {
            walker.CurrentPath = result;
            return result;
        }
        result.RemoveAt(0);
        walker.CurrentPath = result;
        return result;
    }

    public IEnumerator Walk(IWalkable walker, Position2D tile)
    {
        Position2D startPosition = walker.Position;
        walker.LastPosition = walker.Position;
        IAction action = new MoveAction(walker, tile, dungeonFloor);
        yield return StartCoroutine(action.PerformAction());

        if (walker.Position.x == tile.x && walker.Position.y == tile.y)
            pendingWalkAnimations.Add((walker, startPosition, tile));
    }
}
