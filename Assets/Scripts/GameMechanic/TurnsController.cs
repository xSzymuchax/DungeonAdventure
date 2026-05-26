using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnsController
{
    private List<IEnemyController> enemies;
    private IActor player;
    private double gameSpeed;

    public TurnsController(double gameSpeed)
    {
        this.gameSpeed = gameSpeed;
        enemies = new();
    }

    public void AddEnemy(IEnemyController enemy)
    {
        enemies.Add(enemy);
    }

    public void RemoveEnemy(IEnemyController enemy)
    {
        enemies.Remove(enemy);
    }

    public void SetPlayer(IActor player)
    {
        this.player = player;
    }
    public bool IsPlayerReady()
    {
        return player.HasEnergy;
    }
    public void AddEnergyAll()
    {
        player.AddEnergy(gameSpeed);
        enemies.ForEach(e => e.Actor.AddEnergy(gameSpeed));
    }

    public IEnumerator EvaluateTurn()
    {
        while (true)
        {
            IEnemyController bestEnemy = GetMostEnergyEnemy();

            if (bestEnemy != null && bestEnemy.Actor.Energy > player.Energy && bestEnemy.Actor.HasEnergy)
            {
                yield return bestEnemy.MakeMove();
            }
                
            else
                break;
        }

        if (player.Energy <= 0)
            AddEnergyAll();

        yield return null;
    }

    private IEnemyController GetMostEnergyEnemy()
    {
        IEnemyController best=null;
        if (enemies.Count > 0)
            best = enemies[0];
        else
            return best;

        foreach (IEnemyController enemyController in enemies)
        {
            if (enemyController.Actor.Energy > best.Actor.Energy)
                best = enemyController;
        }

        return best;
    }
}
