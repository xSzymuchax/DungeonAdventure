using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnsController
{
    private List<IEnemyController> enemies;
    private IActor player;
    private float gameSpeed;

    public TurnsController(float gameSpeed)
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

    public List<IEnemyController> GetEnemies()
    {
        return new List<IEnemyController>(enemies);
    }

    public void ClearEnemies()
    {
        enemies.Clear();
    }

    public bool TurnElapsed { get; private set; }

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
        TurnElapsed = false;
        yield return ResolveEnemies();

        while (player != null && player.Energy <= 0)
        {
            TurnElapsed = true;
            if (player is PlayerCharacter character && !character.IsDead)
            {
                character.ApplyTurnUpkeep();
                if (character.IsDead)
                {
                    yield return character.PlayDeathAnimation();
                    break;
                }
            }
            AddEnergyAll();
        }

        yield return null;
    }

    private IEnumerator ResolveEnemies()
    {
        while (true)
        {
            IEnemyController bestEnemy = GetMostEnergyEnemy();

            if (bestEnemy == null)
                break;

            if (bestEnemy.Actor is IDamagable damagable && damagable.IsDead)
            {
                RemoveEnemy(bestEnemy);
                continue;
            }

            if (bestEnemy.Actor.Energy > player.Energy && bestEnemy.Actor.HasEnergy)
                yield return bestEnemy.MakeMove();
            else
                break;
        }
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
