using System;
using UnityEngine;
using NomadsPlanet.Utils;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using Sirenix.Utilities;
using LightType = NomadsPlanet.Utils.LightType;

namespace NomadsPlanet
{
    // 현재 어떤 신호를 갖고 있는지 알려준다.
    // 좌회전이나 우회전만 가능한 차선에서는, 2차선 모두 이용이 가능하다.
    public class TrafficFlow : MonoBehaviour
    {
        [InfoBox("해당 차선에서 갈 수 있는 곳들 목록")]
        [SerializeField, RequiredListLength(2)]
        private Transform[] leftCarTargets = new Transform[2];

        [SerializeField, RequiredListLength(2)]
        private Transform[] rightCarTargets = new Transform[2];

        [InfoBox("현재 차선에서 차량이 위치할 수 있는 곳들 목록")]
        [ShowInInspector, ReadOnly]
        public List<Transform> LeftCarPoints { get; private set; } // 0번 인덱스가 가장 앞에 위치함

        private List<bool> _leftCarPlaced = new();

        [ShowInInspector, ReadOnly]
        public List<Transform> RightCarPoints { get; private set; }

        private List<bool> _rightCarPlaced = new();
        
        public TrafficType TrafficType { get; private set; }
        public LightType CurLightType { get; private set; }
        
        private LightController _lightController;
        private CarDetector _carDetector;

        private void Awake()
        {
            _InitGetters();
        }
        
        // Index 0번에 도착했을 때 발생시킬 액션도 있어야해
        private void Update()
        {
            if (_carDetector.GetCarLength() == 0)
            {
                return;
            }

            if (_carDetector.GetCarOnPosition(LeftCarPoints[0]))
            {
                Debug.Log("왼쪽 가장 앞 차선에 위치함");
            }
            
            if (_carDetector.GetCarOnPosition(rightCarTargets[0]))
            {
                Debug.Log("오른쪽 가장 앞 차선에 위치함");
            }
        }

        // LightController에서 횡단보도 타입을 정할 수 있도록 해준다.
        public void SetLightAction(LightType type)
        {
            CurLightType = type;
            _lightController.SetTrafficSign(type);
            
            // 신호가 바꼈을 때 할 액션이 있을까?
        }
        

        /* CAR STATE를 정해보자. (현재 신호등의 색상에 따라 내용 정리를 해야한다.)
         * 1. 처음에 들어왔을 때, 위치할 곳을 정해준다. (50% 확률로 왼쪽, 오른쪽 차선 구분),
         *    차선 정해주는 것부터 해서 전체 이동은... 여기서 할듯?
         *    대신 이동 관련 내용은 `CarHandler`에 넣어야함
         * 2. 맨 앞에 있는 애 또한 분기점이 있을 시, 50% 확률로 갈 곳 정하기
         * 3. 노란불은 멈추는 불. 무리하게 건너진 말고, 보이면 멈추되, 앞 2개까지는 건널 수 있게 하기
         * 4. 
         */
        private void OnCarEnterEvent(List<CarHandler> insideCars)
        {
            // 각 차 순서대로 위치를 정해준다.
            // Position 
        }
        private void OnCarExitEvent(List<CarHandler> insideCars)
        {
            
        }
        

        // 여기서 필요한 멤버들을 초기화해준다.
        private void _InitGetters()
        {
            // 차량 탐지 부분 초기화 (이벤트 메소드 두개를 넘겨준다.)
            _carDetector = GetComponent<CarDetector>();
            _carDetector.InitSetup(OnCarEnterEvent, OnCarExitEvent);
            
            TrafficType = TrafficManager.GetTrafficType(tag);
            _lightController = transform.GetChild(0).GetComponent<LightController>();
            var leftParents = transform.GetChild(1);
            var rightParents = transform.GetChild(2);

            LeftCarPoints = new List<Transform>();
            RightCarPoints = new List<Transform>();
            for (int i = 0; i < leftParents.childCount; i++)
            {
                LeftCarPoints.Add(leftParents.GetChild(i));
                _leftCarPlaced.Add(false);
            }

            for (int i = 0; i < rightParents.childCount; i++)
            {
                RightCarPoints.Add(rightParents.GetChild(i));
                _rightCarPlaced.Add(false);
            }
        }
    }
}