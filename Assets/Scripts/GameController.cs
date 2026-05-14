using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController Instance;
    public Transform playerHolder;
    public GameObject player;
    public Camera playerCamera;

    void Start()
    {
        Instance = this;
        GameObject p = Instantiate(player, playerHolder);
        playerCamera.GetComponent<CameraController>().SetTarget(p.transform);
        p.GetComponent<PlayerController>().SetRaySource(playerCamera.transform);
    }

    // TODO - fix it after more logic is added
    public bool IsPlayerTurn()
    {
        return true;
    }
}
