using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace CupOHappiness.UI
{
    public sealed class ReadyRoster : MonoBehaviour
    {
        [SerializeField] private readyCheck source;
        [SerializeField] private ReadyPlayerRow rowPrefab;
        [SerializeField] private Transform container;
        private readonly Dictionary<ulong, ReadyPlayerRow> rows = new();
        private NetworkList<ReadyEntry> subscribed;

        private void OnEnable()
        {
            if (!source) return;
            subscribed = source.Roster;
            subscribed.OnListChanged += OnListChanged;
            Refresh(Snapshot(subscribed));
        }

        private void OnDisable()
        {
            if (subscribed != null) subscribed.OnListChanged -= OnListChanged;
            subscribed = null;
        }

        private void OnListChanged(NetworkListEvent<ReadyEntry> change) => Refresh(Snapshot(subscribed));

        private static List<ReadyEntry> Snapshot(NetworkList<ReadyEntry> list)
        {
            var copy = new List<ReadyEntry>(list.Count);
            for (int i = 0; i < list.Count; i++) copy.Add(list[i]);
            return copy;
        }

        public int RowCount => rows.Count;

        public void Refresh(IReadOnlyList<ReadyEntry> entries)
        {
            var ordered = new List<ReadyEntry>(entries);
            ordered.Sort((a, b) => Rank(a.clientId).CompareTo(Rank(b.clientId)));

            var present = new HashSet<ulong>();
            foreach (var entry in ordered) present.Add(entry.clientId);
            foreach (ulong id in new List<ulong>(rows.Keys))
            {
                if (present.Contains(id)) continue;
                DestroyRow(rows[id].gameObject);
                rows.Remove(id);
            }

            Transform parent = container ? container : transform;
            for (int i = 0; i < ordered.Count; i++)
            {
                var entry = ordered[i];
                if (!rows.TryGetValue(entry.clientId, out var row))
                {
                    row = Instantiate(rowPrefab, parent);
                    rows.Add(entry.clientId, row);
                }
                row.transform.SetSiblingIndex(i);
                row.Bind(entry.name.ToString(), entry.ready);
            }
        }

        // Host (server client id) first, then by client id.
        private static ulong Rank(ulong clientId) => clientId == NetworkManager.ServerClientId ? 0UL : clientId + 1UL;

        private static void DestroyRow(GameObject row)
        {
            if (Application.isPlaying) Destroy(row);
            else DestroyImmediate(row);
        }
    }
}
