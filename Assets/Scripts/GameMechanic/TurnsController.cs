using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnsController
{
    private List<IActor> enemies;
    private IActor player;
    private double gameSpeed;

    public TurnsController(double gameSpeed) { this.gameSpeed = gameSpeed; }

    public void AddEnemy(IActor enemy)
    {
        enemies.Add(enemy);
    }

    public void RemoveEnemy(IActor enemy)
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
        enemies.ForEach(e => e.AddEnergy(gameSpeed));
    }

    public bool CanPerformAction(IActor actor)
    {
        if (actor.GetEnergy() > player.GetEnergy())
            return true;
        return false;
    }
}
