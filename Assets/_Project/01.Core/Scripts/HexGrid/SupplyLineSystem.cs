using System;
using System.Collections.Generic;

namespace Project.Core.HexGrid
{
    public sealed class SupplyLineSystem
    {
        private readonly Queue<HexCoordinates> _bfsQueue;
        private readonly HashSet<HexCoordinates> _connectedSet;
        private readonly HexGridModel _gridModel;

        public SupplyLineSystem(HexGridModel gridModel, int initialCapacity = 256)
        {
            _gridModel = gridModel ?? throw new ArgumentNullException(nameof(gridModel));
            _bfsQueue = new Queue<HexCoordinates>(initialCapacity);
            _connectedSet = new HashSet<HexCoordinates>(initialCapacity);
        }

        /// <summary>
        /// [주차 결산용] 지정한 본성(HQ)으로부터 이어진 유효 보급망을 재계산하고 그리드 모델을 갱신합니다.
        /// </summary>
        /// <param name="hqCoordinates">본성 좌표</param>
        /// <param name="targetFaction">보급망을 계산할 세력 (기본값: 마왕군)</param>
        public void RecalculateSupplyLines(
            HexCoordinates hqCoordinates,
            Faction targetFaction = Faction.DemonKing)
        {
            // 1. 캐시 초기화 (GC Alloc = 0 bytes)
            _bfsQueue.Clear();
            _connectedSet.Clear();

            // 2. 본성 노드 유효성 검사 (점령 여부, 통행 가능 여부, 오염 여부 검증)
            if (!_gridModel.TryGetTile(hqCoordinates, out var hqTile) ||
                hqTile.Owner != targetFaction ||
                !hqTile.IsTraversable ||
                hqTile.IsCorrupted) // 본성이 적에게 함락되거나 오염/파괴되었다면 보급망 전면 붕괴
            {
                _gridModel.UpdateSupplyLines(_connectedSet);
                return;
            }

            // 3. BFS 탐색 시작
            _bfsQueue.Enqueue(hqCoordinates);
            _connectedSet.Add(hqCoordinates);

            while (_bfsQueue.Count > 0)
            {
                var current = _bfsQueue.Dequeue();

                for (int i = 0; i < 6; i++)
                {
                    var neighbor = current.GetNeighbor(i);

                    // [기획 룰 검증]
                    // 1) 그리드에 존재
                    // 2) 대상 세력 소유
                    // 3) 이동 가능 지형(IsTraversable)
                    // 4) 오염되지 않음(!IsCorrupted: 오염 지대는 보급선 단절)
                    // 5) 미방문 노드(_connectedSet.Add 성공 시 단일 해시 룩업으로 큐 삽입)
                    if (_gridModel.TryGetTile(neighbor, out var neighborTile) &&
                        neighborTile.Owner == targetFaction &&
                        neighborTile.IsTraversable &&
                        !neighborTile.IsCorrupted &&
                        _connectedSet.Add(neighbor))
                    {
                        _bfsQueue.Enqueue(neighbor);
                    }
                }
            }

            // 4. 모델 상태 동기화
            _gridModel.UpdateSupplyLines(_connectedSet);
        }
    }
}