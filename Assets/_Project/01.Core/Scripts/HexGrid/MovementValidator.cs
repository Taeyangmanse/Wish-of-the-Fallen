namespace Project.Core.HexGrid
{
    public static class MovementValidator
    {
        /// <summary>
        /// 이동에 필요한 기본 조건 및 'A* Pathfinding'을 통한 실제 도달 가능 여부를 검증합니다.
        /// </summary>
        public static MovementFailureReason Validate(
            HexGridModel gridModel,
            HexPathfinder pathfinder, // A* 탐색기 의존성 주입
            HexCoordinates startCoords,
            HexCoordinates targetCoords,
            in ExpeditionMovementContext context,
            out HexTileData startTile,
            out HexTileData targetTile)
        {
            startTile = default;
            targetTile = default;

            if (context.CurrentAP < context.CostAP)
                return MovementFailureReason.NotEnoughAP;

            if (!gridModel.TryGetTile(startCoords, out startTile) || !startTile.IsTraversable)
                return MovementFailureReason.InvalidStartTile;

            if (!gridModel.TryGetTile(targetCoords, out targetTile) || !targetTile.IsTraversable)
                return MovementFailureReason.TargetNotTraversable;

            bool isSupplyRoute = startTile.HasSupply && targetTile.HasSupply;
            int maxRange = isSupplyRoute ? context.MaxSupplyRange : context.MaxWildernessRange;

            // context.Faction은 원정대의 소속 세력(예: Faction.DemonKing)이라고 가정
            int actualDistance = pathfinder.FindPathDistance(
                startCoords,
                targetCoords,
                requireSupply: isSupplyRoute,
                targetFaction: context.Faction);

            // 경로를 찾지 못했거나(-1), 최대 사거리를 초과한 경우
            if (actualDistance == -1 || actualDistance > maxRange)
            {
                return MovementFailureReason.OutOfRange;
            }

            return MovementFailureReason.None;
        }
    }
}