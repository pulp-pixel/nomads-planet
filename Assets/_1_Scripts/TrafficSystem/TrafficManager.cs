using System;
using UnityEngine;
using NomadsPlanet.Utils;
using Unity.VisualScripting;

namespace NomadsPlanet
{
    // 사실 얘가 주도적으로 하는게... 너무 없네
    public class TrafficManager : MonoBehaviour
    {
        public static TrafficManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                return;
            }

            Instance = this;
        }

        public static TrafficType GetTrafficType(string carTag)
        {
            return carTag switch
            {
                "L" => TrafficType.Left,
                "R" => TrafficType.Right,
                "LR" => TrafficType.Left | TrafficType.Right,
                "LF" => TrafficType.Left | TrafficType.Forward,
                "RF" => TrafficType.Right | TrafficType.Forward,
                _ => TrafficType.Left | TrafficType.Right | TrafficType.Forward
            };
        }

        public static LaneType GetLaneType(string name)
        {
            return name.Contains("1") ? LaneType.First : LaneType.Second;
        }
    }
}