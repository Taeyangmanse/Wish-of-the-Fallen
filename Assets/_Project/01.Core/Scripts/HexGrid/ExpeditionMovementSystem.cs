using System;

namespace Project.Core.HexGrid
{
    public sealed class ExpeditionMovementSystem
    {
        private readonly HexGridModel _gridModel;
        private readonly AmbushCalculator _ambushCalculator;
        private readonly HexPathfinder _pathfinder;

        public ExpeditionMovementSystem(HexGridModel gridModel, AmbushCalculator ambushCalculator)
        {
            _gridModel = gridModel ?? throw new ArgumentNullException(nameof(gridModel));
            _ambushCalculator = ambushCalculator ?? throw new ArgumentNullException(nameof(ambushCalculator));
            _pathfinder = new HexPathfinder(_gridModel);
        }

        /// <summary>
        /// 하위 Validator와 Calculator를 조율하여 최종 이동 결과를 산출합니다.
        /// </summary>
        public MovementExecutionResult TryMoveExpedition(
            HexCoordinates startCoords,
            HexCoordinates targetCoords,
            in ExpeditionMovementContext context)
        {
            // 1. 순수 규칙 검증 (Validator 위임)
            var validationResult = MovementValidator.Validate(
                _gridModel, _pathfinder, startCoords, targetCoords, in context,
                out var startTile, out var targetTile);

            if (validationResult != MovementFailureReason.None)
            {
                return MovementExecutionResult.Fail(validationResult);
            }

            // 2. 안전 보급로 이동 처리
            bool isSupplyRoute = startTile.HasSupply && targetTile.HasSupply;
            if (isSupplyRoute)
            {
                return MovementExecutionResult.Success(false, context.CostAP, 0, 0);
            }

            // 3. 야지 이동 및 기습 확률 계산 (Calculator 위임)
            if (!targetTile.IsExplored)
            {
                int distance = startCoords.DistanceTo(targetCoords);
                var (isAmbushed, chance, roll) = _ambushCalculator.CalculateAmbush(distance, context.AmbushChanceReduction);

                return MovementExecutionResult.Success(isAmbushed, context.CostAP, chance, roll);
            }

            // 기습 조건에 해당하지 않는 안전 야지 이동
            return MovementExecutionResult.Success(false, context.CostAP, 0, 0);
        }
    }
}