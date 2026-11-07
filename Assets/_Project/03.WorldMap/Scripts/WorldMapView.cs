using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Project.Core.HexGrid; // Core 데이터 참조

namespace Project.WorldMap
{
    public class WorldMapView : MonoBehaviour
    {
        [SerializeField] private HexTileRenderer _tilePrefab;
        [SerializeField] private Transform _mapContainer;

        private HexGridModel _gridModel;
        private ExpeditionMovementSystem _movementSystem;
        private readonly Dictionary<HexCoordinates, HexTileRenderer> _renderers = new();

        public void BindModel(HexGridModel gridModel, ExpeditionMovementSystem movementSystem)
        {
            if (_gridModel != null)
                _gridModel.OnTileStateChanged -= HandleTileStateChanged;

            _gridModel = gridModel;
            _movementSystem = movementSystem;

            _gridModel.OnTileStateChanged += HandleTileStateChanged;
        }

        private void OnDestroy()
        {
            if (_gridModel != null)
                _gridModel.OnTileStateChanged -= HandleTileStateChanged;
        }

        private void HandleTileStateChanged(HexTileData updatedTileData)
        {
            if (_renderers.TryGetValue(updatedTileData.Coordinates, out var renderer))
            {
                renderer.RefreshView(updatedTileData);
            }
        }

        // ... GenerateVisualMap() 기존 로직 유지 ...

        public void OnTileClicked(HexCoordinates targetCoords)
    {
        HexCoordinates currentPos = GetCurrentExpeditionPosition();
        ExpeditionMovementContext context = GetCurrentContext();

        var result = _movementSystem.TryMoveExpedition(currentPos, targetCoords, in context);

        if (result.IsSuccess)
        {
            // 이동 애니메이션 연출을 위해 코루틴 실행
            StartCoroutine(ExecuteMovementRoutine(currentPos, targetCoords, result));
        }
        else
        {
            ShowWarningUI(result.FailureReason);
        }
    }

    private IEnumerator ExecuteMovementRoutine(
        HexCoordinates startPos,
        HexCoordinates targetPos,
        MovementExecutionResult result)
    {
        // TODO: A* 패스파인더에서 리스트(경로)를 받아와 순차적으로 걷는 애니메이션 대기
        // yield return _unitAnimator.WalkAlongPath(pathList);

        // 임시 1초 대기 (연출 시간)
        yield return new WaitForSeconds(1f);

        // 1. 애니메이션이 끝난 후 비로소 Core에 위치 이동 및 AP 차감 확정 (Commit)
        // 주의: 이 로직은 실제로는 TurnManager 등 상위 컨트롤러를 호출하는 형태가 좋습니다.
        CommitMovement(startPos, targetPos, result);

        // 2. 기습 전투 진입
        if (result.IsAmbushed)
        {
            TriggerAmbushEncounter(targetPos);
        }
    }

    // ... 기타 임시 헬퍼 메서드 ...
    private HexCoordinates GetCurrentExpeditionPosition() => new HexCoordinates(0, 0);
    private ExpeditionMovementContext GetCurrentContext() => new ExpeditionMovementContext(Faction.DemonKing, 10);
    private void CommitMovement(HexCoordinates start, HexCoordinates target, MovementExecutionResult result) { /* 로직 */ }
    private void ShowWarningUI(MovementFailureReason reason) { /* 로직 */ }
    private void TriggerAmbushEncounter(HexCoordinates coords) { /* 로직 */ }
}