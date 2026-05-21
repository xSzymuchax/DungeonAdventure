using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour, IActor
{
    public double Energy { get; set; }
    protected MoveCostManager moveCostManager;

    private void Start()
    {
        InitMoveCosts();
    }

    protected virtual void InitMoveCosts() { }

    public double GetEnergy()
    {
        return Energy;
    }
    public void AddEnergy(double amount)
    {
        Energy += amount;
    }

    public void RemoveEnergy(double amount)
    {
        Energy -= amount;
    }

    public MoveCostManager GetMoveCosts()
    {
        return moveCostManager;
    }

    public IEnumerator WalkingAnimation(Vector3 targetPosition)
    {
        Vector3 startPosition = transform.position;
        float duration = Consts.WALK_ANIMATION_TIME;
        float progress = 0f;

        while (progress < duration)
        {
            progress += Time.deltaTime;
            float completion = progress / duration;

            transform.position = Vector3.Lerp(startPosition, targetPosition, completion);

            yield return null;
        }

        transform.position = targetPosition;
    }
}
