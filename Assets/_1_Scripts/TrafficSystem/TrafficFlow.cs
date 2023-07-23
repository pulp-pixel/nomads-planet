using System.Collections;
using UnityEngine;
using NomadsPlanet.Utils;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;
using LightType = NomadsPlanet.Utils.LightType;

namespace NomadsPlanet
{
    // 현재 어떤 신호를 갖고 있는지 알려준다.
    // 좌회전이나 우회전만 가능한 차선에서는, 2차선 모두 이용이 가능하다.
    public class TrafficFlow : MonoBehaviour
    {
        // "해당 차선에서 갈 수 있는 곳들 목록"
        [SerializeField, RequiredListLength(2)]
        private Transform[] leftCarTargets = new Transform[2]; // 좌회전, 직진

        private readonly Transform[] _leftWayPoint = new Transform[2]; // 왼쪽 좌회전 경유지

        [SerializeField, RequiredListLength(2)]
        private Transform[] rightCarTargets = new Transform[2]; // 우회전, 직진

        private readonly Transform[] _rightWayPoint = new Transform[2]; // 우회전 경유지

        // "현재 차선에서 차량이 위치할 수 있는 곳들 목록"
        private List<CarDetector> LeftCarDetectors { get; set; }
        private List<CarDetector> RightCarDetectors { get; set; }

        [ShowInInspector] private readonly List<CarHandler> _insideCars = new(14);

        // 이 아래는 클래스 성질을 나타내기 위함. 볼 필요 x
        private LightType _curLightType;
        private TrafficType _thisTrafficType;
        private LightController _lightController;

        private void Awake() => _Init();

        private void Update()
        {
            if (_curLightType is LightType.Red or LightType.Yellow)
            {
                return;
            }

            _OnLeftCarsUpdate();
            _OnRightCarsUpdate();
        }

        // 차량이 들어왔다면,
        private void OnCarEnter(CarHandler car)
        {
            if (_insideCars.Contains(car))
            {
                return;
            }

            _insideCars.Add(car);

            // 맨 앞에서부터 탐색하고, 빈 곳이 있으면 거기로 보내
            // 한번 정해지면, 거기로만가
            for (int i = 0; i < LeftCarDetectors.Count; i++)
            {
                if (Random.value < .5f)
                {
                    if (LeftCarDetectors[i].TargetCat == CarHandler.NullCar)
                    {
                        LeftCarDetectors[i].TargetCat = car;

                        if (i + 2 < LeftCarDetectors.Count)
                        {
                            car.MoveViaWaypoint(LeftCarDetectors[i].transform.position, new[]
                                {
                                    LeftCarDetectors[i + 2].transform.position,
                                    LeftCarDetectors[i + 1].transform.position,
                                },
                                false
                            );
                        }
                        else
                        {
                            car.MoveToTarget(LeftCarDetectors[i].transform.position, false);
                        }

                        break;
                    }
                }
                else
                {
                    if (RightCarDetectors[i].TargetCat == CarHandler.NullCar)
                    {
                        RightCarDetectors[i].TargetCat = car;

                        if (i + 2 < RightCarDetectors.Count)
                        {
                            car.MoveViaWaypoint(RightCarDetectors[i].transform.position, new[]
                                {
                                    RightCarDetectors[i + 2].transform.position,
                                    RightCarDetectors[i + 1].transform.position,
                                },
                                false
                            );
                        }
                        else
                        {
                            car.MoveToTarget(RightCarDetectors[i].transform.position,
                                false
                            );
                        }

                        break;
                    }
                }
            }
        }

        private void _OnLeftCarsUpdate()
        {
            // 맨 앞에 놈은 아예 다른 곳으로 가게 하기
            bool isLeftOnCar = LeftCarDetectors[0].TargetCat != CarHandler.NullCar &&
                               LeftCarDetectors[0].CarOnThisPoint() &&
                               _insideCars.Contains(LeftCarDetectors[0].TargetCat);

            if (!isLeftOnCar)
            {
                return;
            }

            bool isLeft = _thisTrafficType.HasFlag(TrafficType.Left) || _thisTrafficType.HasFlag(TrafficType.Right);
            bool isForward = _thisTrafficType.HasFlag(TrafficType.Forward);

            StartCoroutine(DelayedRemove(LeftCarDetectors[0].TargetCat));
            MoveToOtherLane(LeftCarDetectors, leftCarTargets, _leftWayPoint, isLeft, isForward);
        }

        private void _OnRightCarsUpdate()
        {
            bool isRightOnCar = RightCarDetectors[0].TargetCat != CarHandler.NullCar &&
                                RightCarDetectors[0].CarOnThisPoint() &&
                                _insideCars.Contains(RightCarDetectors[0].TargetCat);

            if (!isRightOnCar)
            {
                return;
            }

            bool isRight = _thisTrafficType.HasFlag(TrafficType.Right) ||
                           _thisTrafficType.HasFlag(TrafficType.Left);
            bool isForward = _thisTrafficType.HasFlag(TrafficType.Forward);

            StartCoroutine(DelayedRemove(RightCarDetectors[0].TargetCat));
            MoveToOtherLane(RightCarDetectors, rightCarTargets, _rightWayPoint, isRight, isForward);
        }

        private static void MoveToOtherLane(
            IReadOnlyList<CarDetector> carDetectors, IReadOnlyList<Transform> carTargets,
            IReadOnlyList<Transform> wayPoints, bool isCurved, bool isForward)
        {
            switch (isCurved)
            {
                case true when isForward:
                {
                    // 직진까지 가능한 경우, Curve나 직진 중 하나를 골라서 갈 수 있도록 한다.
                    if (Random.value < .5f)
                    {
                        carDetectors[0].TargetCat.MoveViaWaypoint(carTargets[0].position,
                            new[] { wayPoints[0].position, wayPoints[1].position },
                            true
                        );
                    }
                    else
                    {
                        carDetectors[0].TargetCat.MoveToTarget(carTargets[1].position, true);
                    }

                    break;
                }
                case true:
                    // 커브만 가능한 경우, 커브를 돌 수 있도록 한다.
                    carDetectors[0].TargetCat.MoveViaWaypoint(carTargets[0].position,
                        new[] { wayPoints[0].position, wayPoints[1].position },
                        true
                    );
                    break;
                default:
                {
                    // 직진만 가능한 경우, 직진시켜준다.
                    carDetectors[0].TargetCat.MoveToTarget(carTargets[1].position, true);
                    break;
                }
            }

            carDetectors[0].TargetCat = CarHandler.NullCar;

            // 뒤에 대기하고 있는 나머지 차들을 재배열 시켜준다.
            for (int i = 1; i < carDetectors.Count; i++)
            {
                if (carDetectors[i].TargetCat == CarHandler.NullCar)
                {
                    continue;
                }

                // 각 차의 이동 딜레이는 0.5초
                carDetectors[i].TargetCat.MoveToTarget(
                    carDetectors[i - 1].transform.position, true, i * .3f
                );
                carDetectors[i - 1].TargetCat = carDetectors[i].TargetCat;
                carDetectors[i].TargetCat = CarHandler.NullCar;
            }
        }

        private void _Init()
        {
            _thisTrafficType = TrafficManager.GetTrafficType(tag);
            _lightController = transform.GetChildFromName<LightController>("TrafficLight");

            var leftParents = transform.GetChildFromName<Transform>("1");
            var rightParents = transform.GetChildFromName<Transform>("2");

            _leftWayPoint[0] = leftParents.GetChildFromName<Transform>("waypoint") ?? transform;
            _rightWayPoint[0] = rightParents.GetChildFromName<Transform>("waypoint") ?? transform;
            _leftWayPoint[1] = leftParents.GetChildFromName<Transform>("waypoint (1)") ?? transform;
            _rightWayPoint[1] = rightParents.GetChildFromName<Transform>("waypoint (1)") ?? transform;

            LeftCarDetectors = new(7);
            RightCarDetectors = new(7);

            for (int i = 0; i < leftParents.childCount; i++)
            {
                bool isDetector = leftParents.GetChild(i).TryGetComponent<CarDetector>(out var detector);
                if (!isDetector)
                {
                    continue;
                }

                detector.InitSetup(LaneType.First, OnCarEnter);
                LeftCarDetectors.Add(detector);
            }

            for (int i = 0; i < rightParents.childCount; i++)
            {
                bool isDetector = rightParents.GetChild(i).TryGetComponent<CarDetector>(out var detector);
                if (!isDetector)
                {
                    continue;
                }

                detector.InitSetup(LaneType.Second, OnCarEnter);
                RightCarDetectors.Add(detector);
            }
        }


        // 외부에서 쓸 애들
        public List<Transform> GetTargetValues()
        {
            List<Transform> targets = new List<Transform>(8);
            for (int i = 3; i < LeftCarDetectors.Count; i++)
            {
                targets.Add(LeftCarDetectors[i].GetComponent<Transform>());
            }

            for (int i = 3; i < RightCarDetectors.Count; i++)
            {
                targets.Add(RightCarDetectors[i].GetComponent<Transform>());
            }

            return targets;
        }

        // LightController에서 횡단보도 타입을 정할 수 있도록 해준다.
        public void SetLightAction(LightType type)
        {
            _curLightType = type;
            _lightController.SetTrafficSign(type);
        }

        private IEnumerator DelayedRemove(CarHandler car)
        {
            yield return new WaitForSeconds(5f);

            if (_insideCars.Contains(car))
            {
                _insideCars.Remove(car);
            }
        }
    }
}