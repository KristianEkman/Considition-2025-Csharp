using Considition2025_CsharpStarterKit.Dtos.Response;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Considition2025_CsharpStarterKit
{
    internal class StationSchedule
    {
        private readonly Dictionary<string, List<(int startTick, int endTick)>> _reservations;
        private readonly Dictionary<string, int> _totalChargers;

        public StationSchedule(Dictionary<string, int> stationChargers)
        {
            StationIds = stationChargers.Keys.ToArray();
            _reservations = stationChargers.ToDictionary(kv => kv.Key, kv => new List<(int, int)>());
            _totalChargers = stationChargers;
        }

        /// <summary>
        /// The IDs of all managed stations.
        /// </summary>
        public string[] StationIds { get; }

        /// <summary>
        /// Returns how many chargers a station has in total.
        /// </summary>
        public int GetTotalChargers(string stationId)
        {
            if (!_totalChargers.ContainsKey(stationId))
                throw new ArgumentException($"Unknown station '{stationId}'.");
            return _totalChargers[stationId];
        }

        /// <summary>
        /// Returns the number of occupied chargers at a given tick.
        /// </summary>
        public int GetOccupiedCount(string stationId, int tick)
        {
            if (!_reservations.ContainsKey(stationId))
                throw new ArgumentException($"Unknown station '{stationId}'.");

            return _reservations[stationId].Count(r => tick >= r.startTick && tick < r.endTick);
        }

        /// <summary>
        /// Returns true if at least one charger is free at the given tick.
        /// </summary>
        public bool IsFree(string stationId, int tick)
        {
            return GetOccupiedCount(stationId, tick) < GetTotalChargers(stationId);
        }

        public bool IsFree(string stationId, int startTick, int endTick)
        {
            if (!_reservations.ContainsKey(stationId))
                throw new ArgumentException($"Unknown station '{stationId}'.");

            if (endTick <= startTick)
                throw new ArgumentException("endTick must be greater than startTick.");

            // Check if there’s at least one charger available at every tick in the interval.
            for (int t = startTick; t < endTick; t++)
            {
                if (!IsFree(stationId, t))
                    return false; // No slot free for entire duration
            }
            return true;
        }

        /// <summary>
        /// Attempts to reserve one charger slot at the station for the given interval.
        /// Returns false if no slots are available.
        /// </summary>
        public bool Reserve(string stationId, int startTick, int endTick)
        {
            if (!_reservations.ContainsKey(stationId))
                throw new ArgumentException($"Unknown station '{stationId}'.");

            if (endTick <= startTick)
                throw new ArgumentException("endTick must be greater than startTick.");

            // Check if there’s at least one charger available at every tick in the interval.
            for (int t = startTick; t < endTick; t++)
            {
                if (!IsFree(stationId, t))
                    return false; // No slot free for entire duration
            }

            _reservations[stationId].Add((startTick, endTick));
            return true;
        }

        /// <summary>
        /// Frees all reservations (useful between simulation runs).
        /// </summary>
        public void ClearAll()
        {
            foreach (var list in _reservations.Values)
                list.Clear();
        }
    }
}
