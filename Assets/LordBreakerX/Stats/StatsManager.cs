using LordBreakerX.Utilities.Math;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace LordBreakerX.Stats
{
    public class StatsManager : StatsManager<StatHolder>
    {
        
    }

    public abstract class StatsManager<T> : NetworkBehaviour where T : StatHolder
    {
        [SerializeField]
        private NetworkManager _networkManager;

        [SerializeField]
        private T _statHolder;

        [SerializeField]
        private int _maxPlayers = 4;

        private Dictionary<string, float> _statRegistry = new Dictionary<string, float>();

        private NetworkVariable<int> _currentPlayerAmount = new NetworkVariable<int>(0);

        public int MaxPlayers { get => _maxPlayers; }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _currentPlayerAmount.OnValueChanged += OnPlayerAmountChanged;
            UpdateStats();
        }

        private void OnPlayerAmountChanged(int previousValue, int newValue)
        {
            UpdateStats();
        }

        protected virtual void OnEnable()
        {
            _networkManager.OnClientConnectedCallback += OnPlayerJoined;
            _networkManager.OnClientDisconnectCallback += OnPlayerQuited;
        }

        protected virtual void OnDisable()
        {
            _networkManager.OnClientConnectedCallback -= OnPlayerJoined;
            _networkManager.OnClientDisconnectCallback -= OnPlayerQuited;
        }

        private void OnPlayerJoined(ulong obj)
        {
            if (IsServer)
                _currentPlayerAmount.Value += 1;
            UpdateStats();
        }

        private void OnPlayerQuited(ulong obj)
        {
            if (IsServer)
                _currentPlayerAmount.Value -= 1;
            UpdateStats();
        }

        public void UpdateStats()
        {
            float percentage = PercentageUtility.Percentage(_currentPlayerAmount.Value, 0, _maxPlayers);

            foreach (Stat stat in _statHolder.GetStats())
            {
                float value = stat.GetValue(percentage);
                _statRegistry[stat.ID] = value;
            }
        }

        public float GetStat(string statID)
        {
            if (!_statRegistry.ContainsKey(statID)) 
            {
                UpdateStats();
            }

            if (_statRegistry.ContainsKey(statID)) return _statRegistry[statID];
            else
            {
                Debug.LogError($"The stat {statID} does not exist in the {_statHolder.name} stat holder!");
                return 0;
            }
        }
    }
}
