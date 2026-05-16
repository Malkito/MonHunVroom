using LordBreakerX.Health;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class TestMonsterHUD : NetworkBehaviour
{
    [SerializeField]
    private Image _fillImage;

    [SerializeField]
    private TMP_Text _healthText;

    private Transform _localPlayer;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        playerMovement[] players = GameObject.FindObjectsByType<playerMovement>(FindObjectsSortMode.None);
        foreach(playerMovement playerMovement in players)
        {
            if (playerMovement.IsOwner)
            {
                _localPlayer = playerMovement.transform;
                break;
            }
        }
    }

    private void Update()
    {
        if (_localPlayer != null)
        {
            transform.LookAt(_localPlayer, Vector3.up);
        }
    }

    public void OnHealthChanged(HealthInfo info)
    {
        _fillImage.fillAmount = info.CurrentHealth / info.Maxhealth;
        _healthText.text = $"{info.CurrentHealth} / {info.Maxhealth} HP";

        if (info.CurrentHealth > 0) ShowHud();
        else HideHud();
    }

    public void HideHud()
    {
        gameObject.SetActive(false);
    }

    public void ShowHud()
    {
        gameObject.SetActive(true);
    }
}
