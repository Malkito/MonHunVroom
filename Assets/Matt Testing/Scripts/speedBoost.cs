using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Cinemachine;
using System.Collections;
using Unity.Netcode;

public class speedBoost : NetworkBehaviour
{

    [SerializeField] private float MaxBoost;
    [SerializeField] private Slider boostUi;
    [SerializeField] private float boostForce;
    [SerializeField] private float boostConsumtionRate;
    public float boostGainedOnPickup;
    public float currentboost;

    [SerializeField] TMP_Text speedBoostText;

    private Rigidbody playerRB;

    [SerializeField] private TrailRenderer[] Trials;
    [SerializeField] private ParticleSystem windParticles;
    [SerializeField] private CinemachineCamera cam;
    [SerializeField] private float speedFOV;
    [SerializeField] private float TrialSpeedSize;

    [SerializeField] private float FOVduration;

    private float TrialBaseSize;
    private float baseFOV;

    playerStats PlayerStats;

    public override void OnNetworkSpawn()
    {
        PlayerStats = GetComponent<playerStats>();
    }
    private void Awake()
    {
        playerRB = GetComponent<Rigidbody>();
    }
    void Start()
    {
        MaxBoost = 100 + (PlayerStats.currentSpecialBoost.Value * 20);

        boostUi.maxValue = MaxBoost;
        currentboost = 0;
        baseFOV = cam.Lens.FieldOfView;
        TrialBaseSize = Trials[0].widthMultiplier;
    }

    private void FixedUpdate()
    {


        boostUi.value = currentboost;

        int boostAmount = Mathf.RoundToInt(currentboost);
        speedBoostText.text = boostAmount.ToString();

        if (GameInput.instance.getSprintInput() && currentboost > 0)
        {
            activateSpeedBoost();
        }
        else
        {

            StopAllCoroutines();
            StartCoroutine(chagneFov(false));

            windParticles.Stop();

            DecreaseTrailClientRpc();

        }
    }



    private void activateSpeedBoost()
    {
        playerRB.AddForce(playerRB.transform.forward * (boostForce * (PlayerStats.currentSpecialBoost.Value / 5)), ForceMode.VelocityChange);
        currentboost -= boostConsumtionRate;

        StopAllCoroutines();
        StartCoroutine(chagneFov(true));

        windParticles.Play();

        IncreaseTrailServerRpc();
    }



    IEnumerator chagneFov(bool IsSpeedingUp)
    {
        float elapsedTime = 0;

        while (elapsedTime < FOVduration)
        {
            elapsedTime += Time.deltaTime;
            if (IsSpeedingUp)
            {
                cam.Lens.FieldOfView = Mathf.Lerp(cam.Lens.FieldOfView, speedFOV, elapsedTime / FOVduration);
            }
            else
            {
                cam.Lens.FieldOfView = Mathf.Lerp(cam.Lens.FieldOfView, baseFOV, elapsedTime / FOVduration);
            }
            yield return null;
        }
    }


    public bool AddBoost()
    {

        if (currentboost == MaxBoost) return false;

        currentboost += boostGainedOnPickup;
        if(currentboost > MaxBoost)
        {
            currentboost = MaxBoost;
            return true;
        }
        return true;

    }

    [ServerRpc]
    private void IncreaseTrailServerRpc(ServerRpcParams rpcParams = default)
    {
        IncreaseTrailClientRpc();
    }

    [ClientRpc]
    private void IncreaseTrailClientRpc()
    {
        foreach (TrailRenderer trial in Trials)
        {
            trial.widthMultiplier = TrialSpeedSize;
        }
    }


    [ServerRpc]
    private void DecreaseTrailServerRpc(ServerRpcParams rpcParams = default)
    {

    }

    [ClientRpc]
    private void DecreaseTrailClientRpc()
    {
        foreach (TrailRenderer trial in Trials)
        {
            trial.widthMultiplier = TrialBaseSize;
        }
    }

}
