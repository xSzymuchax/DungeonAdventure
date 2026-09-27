using UnityEngine;
using UnityEngine.UI;

public class QuickBar : MonoBehaviour
{
    public void WaitAction() {
        PlayerCharacter player = GameController.Instance.Player;
        if (player == null)
            return;

        PlayerController controller = player.GetComponent<PlayerController>();
        if (controller == null)
            return;

        controller.StartCoroutine(controller.PlayerWait());
    }
}
