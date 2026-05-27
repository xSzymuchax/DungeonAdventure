using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MovementSystem : MonoBehaviour
{
    public Dungeon dungeonFloor;
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

    public void SetTarget(IWalkable walker, Position2D target)
    {
        walker.CurrentTarget = target;
    }

    public List<Position2D> RecalculatePath(IWalkable walker, Position2D target)
    {
        List<Position2D> result = new();
        result = AStar.FindClosestPath(dungeonFloor.GetTileInfos(), dungeonFloor.FindActorPosition(walker), target, walker.MoveCostManager, MovementDirections.EIGHT);
        result.RemoveAt(0);
        walker.CurrentPath = result;
        return result;
    }

    public IEnumerator Walk(IWalkable walker, Position2D tile)
    {
        Position2D startPosition = walker.Position;
        IAction action = new MoveAction(walker, tile, dungeonFloor);
        yield return StartCoroutine(action.PerformAction());
        yield return StartCoroutine(WalkingAnimation(walker, startPosition, tile));
    }
}
