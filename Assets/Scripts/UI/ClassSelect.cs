using System;
using UnityEngine;
using UnityEngine.UI;

public class ClassSelect : MonoBehaviour
{
    [SerializeField] Button szermierz;
    [SerializeField] Button kusznik;
    [SerializeField] Button mag;

    Action<PlayerClass> chosen;

    void Awake()
    {
        if (szermierz != null)
            szermierz.onClick.AddListener(() => Pick(PlayerClass.Szermierz));
        if (kusznik != null)
            kusznik.onClick.AddListener(() => Pick(PlayerClass.Kusznik));
        if (mag != null)
            mag.onClick.AddListener(() => Pick(PlayerClass.Mag));
    }

    public void Show(Action<PlayerClass> onChosen)
    {
        chosen = onChosen;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        chosen = null;
        gameObject.SetActive(false);
    }

    void Pick(PlayerClass kind)
    {
        Action<PlayerClass> pick = chosen;
        Hide();
        if (pick != null)
            pick(kind);
    }
}
