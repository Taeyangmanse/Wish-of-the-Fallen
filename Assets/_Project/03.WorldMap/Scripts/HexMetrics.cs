using UnityEngine;
using Project.Core.HexGrid;

namespace Project.WorldMap
{
    /// <summary>
    /// 순수 C# 로직 좌표(Core)와 유니티 렌더링 좌표(WorldMap)를 상호 변환하는 유틸리티
    /// </summary>
    public static class HexMetrics
    {
        public const float OuterRadius = 1f; // 타일 중심에서 꼭짓점까지의 거리
        public const float InnerRadius = OuterRadius * 0.866025404f; // sqrt(3)/2 (중심에서 변까지의 거리)

        /// <summary>
        /// (Pointy-Top 육각 타일 기준) 논리 좌표를 유니티 World Position으로 변환
        /// </summary>
        public static Vector3 CoordinatesToPosition(HexCoordinates coords)
        {
            // 입방 좌표계/축 좌표계(Q=X, R=Z)를 2D 평면(X, Y)으로 변환
            float x = (coords.Q + coords.R * 0.5f) * (InnerRadius * 2f);
            float y = coords.R * (OuterRadius * 1.5f);

            return new Vector3(x, y, 0f);
        }
    }
}