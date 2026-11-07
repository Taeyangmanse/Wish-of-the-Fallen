using System;
using System.Collections.Generic;

namespace Project.Core.HexGrid
{
    /// <summary>
    /// 후퇴 실패 원인을 세분화한 열거형 (상위 전투/원정 매니저의 페널티 처리용)
    /// </summary>
    public enum RetreatFailureReason
    {
        None,
        InvalidStartNode,     // 출발 노드가 그리드에 존재하지 않음
        CompletelySurrounded, // 적이나 통행 불가 지형에 의해 완전히 포위됨
        NoReachableNode       // 이동 가능한 유효 반경 내 타일이 전무함
    }

    public sealed class RetreatCalculator
    {
        private readonly Queue<(HexCoordinates Coords, int Distance)> _bfsQueue;
        private readonly HashSet<HexCoordinates> _visited;

        // 탐색 후보군 분리 (거리 및 위험도 기반)
        private readonly List<HexCoordinates> _optimalCandidates;  // 안전 보급선 연결 타일 (정규 거리)
        private readonly List<HexCoordinates> _dangerCandidates;   // 보급 끊김/적지 등 억까 타일 (정규 거리)
        private readonly List<HexCoordinates> _fallbackCandidates; // 거리 미달이지만 도달 가능한 탈출구

        public RetreatCalculator(int initialCapacity = 64)
        {
            _bfsQueue = new Queue<(HexCoordinates, int)>(initialCapacity);
            _visited = new HashSet<HexCoordinates>(initialCapacity);
            _optimalCandidates = new List<HexCoordinates>(initialCapacity);
            _dangerCandidates = new List<HexCoordinates>(initialCapacity);
            _fallbackCandidates = new List<HexCoordinates>(initialCapacity);
        }

        /// <summary>
        /// 퇴각 경로를 계산합니다. 완전히 고립되어 후퇴할 수 없는 경우 false와 실패 사유를 반환합니다.
        /// </summary>
        public bool TryCalculateRetreat(
            HexCoordinates startCoords,
            Faction retreatingFaction,
            IReadOnlyDictionary<HexCoordinates, HexNode> grid,
            Random rng,
            out HexCoordinates destination,
            out RetreatFailureReason failureReason,
            int minDistance = 3,
            int maxDistance = 4)
        {
            destination = startCoords;
            failureReason = RetreatFailureReason.None;

            // 1. 방어적 가드 클로저
            if (grid == null) throw new ArgumentNullException(nameof(grid));
            if (rng == null) throw new ArgumentNullException(nameof(rng));
            if (minDistance < 1 || maxDistance < minDistance)
            {
                throw new ArgumentException($"유효하지 않은 거리 설정: min={minDistance}, max={maxDistance}");
            }

            // 2. 출발 노드 유효성 검사
            if (!grid.TryGetValue(startCoords, out var startNode))
            {
                failureReason = RetreatFailureReason.InvalidStartNode;
                return false;
            }

            // 3. 인접 6방향 퇴각로 확보 여부 1차 검사 (완전 포위/고립 조기 판정)
            bool hasImmediateExit = false;
            for (int i = 0; i < 6; i++)
            {
                var neighborCoords = startCoords.GetNeighbor(i);
                if (grid.TryGetValue(neighborCoords, out var neighborNode) && IsTraversable(neighborNode))
                {
                    hasImmediateExit = true;
                    break;
                }
            }

            if (!hasImmediateExit)
            {
                failureReason = RetreatFailureReason.CompletelySurrounded;
                return false; // 포위/사방 지형 차단으로 퇴각 불가
            }

            // 4. 탐색 컬렉션 초기화
            _bfsQueue.Clear();
            _visited.Clear();
            _optimalCandidates.Clear();
            _dangerCandidates.Clear();
            _fallbackCandidates.Clear();

            _bfsQueue.Enqueue((startCoords, 0));
            _visited.Add(startCoords);

            // 5. BFS 경로 탐색
            while (_bfsQueue.Count > 0)
            {
                var (currentCoords, currentDist) = _bfsQueue.Dequeue();

                // 출발지를 제외한 유효 노드들을 카테고리별로 분류
                if (currentDist > 0 && grid.TryGetValue(currentCoords, out var currentNode))
                {
                    if (currentDist >= minDistance)
                    {
                        // 기획 의도 반영: 안전 타일과 위험(보급 단절/적지) 타일을 분리하여 수집
                        if (currentNode.CurrentFaction == retreatingFaction && currentNode.IsSupplyConnected)
                        {
                            _optimalCandidates.Add(currentCoords);
                        }
                        else
                        {
                            _dangerCandidates.Add(currentCoords); // 억까 허용 타일
                        }
                    }
                    else if (currentDist > 0)
                    {
                        _fallbackCandidates.Add(currentCoords);
                    }
                }

                // 최대 이동 거리 한계 도달 시 이웃 확장 차단
                if (currentDist >= maxDistance) continue;

                int nextDist = currentDist + 1;

                for (int i = 0; i < 6; i++)
                {
                    var neighborCoords = currentCoords.GetNeighbor(i);

                    // 벽이나 통행 불가 지형이 아닌 노드라면 억까/위험 구역도 통과 및 방문 허용
                    if (grid.TryGetValue(neighborCoords, out var neighborNode) &&
                        IsTraversable(neighborNode) &&
                        _visited.Add(neighborCoords))
                    {
                        _bfsQueue.Enqueue((neighborCoords, nextDist));
                    }
                }
            }

            // 6. 결과 선택 (기획 밸런스에 맞춘 난수 추출)

            // 1순위: 충분한 거리(minDistance 이상)가 확보된 안전 구역
            if (_optimalCandidates.Count > 0)
            {
                destination = _optimalCandidates[rng.Next(_optimalCandidates.Count)];
                return true;
            }

            // 2순위: 억까 발생! 안전하지 않은 타일(보급 단절, 적지 등)로 퇴각
            if (_dangerCandidates.Count > 0)
            {
                destination = _dangerCandidates[rng.Next(_dangerCandidates.Count)];
                return true;
            }

            // 3순위: 거리가 3칸 미만이지만 최소한의 퇴각로가 열려 있는 협소 지형
            if (_fallbackCandidates.Count > 0)
            {
                destination = _fallbackCandidates[rng.Next(_fallbackCandidates.Count)];
                return true;
            }

            // 사방이 막혀 유의미한 이동 노드를 찾지 못함
            failureReason = RetreatFailureReason.NoReachableNode;
            return false;
        }

        /// <summary>
        /// 단순 지형 통행 불가(바위, 절벽, 심연 등) 여부만 체크하는 규칙.
        /// 적군 점령지나 보급 단절 지역이어도 지형 자체만 뚫려 있다면 억까 퇴각용으로 통과 가능합니다.
        /// </summary>
        private static bool IsTraversable(HexNode node)
        {
            // 예: 통행 불가능한 장애물 타일 속성이 있다면 배제 (HexNode에 해당 프로퍼티가 있다고 가정)
            // return !node.IsImpassable;
            return node != null;
        }
    }
}