using UnityEngine;
using Project.Core.HexGrid; // 코어 데이터 참조

namespace Project.WorldMap // View 계층 네임스페이스 분리
{
    public class HexTileRenderer : MonoBehaviour
    {
        [Header("Sprite Renderers")]
        [SerializeField] private SpriteRenderer _baseRenderer;
        [SerializeField] private SpriteRenderer _fogRenderer;
        [SerializeField] private SpriteRenderer _supplyLineRenderer;

        [Header("Faction Colors")]
        [SerializeField] private Color _colorDemonKing = new Color(0.6f, 0f, 0.8f);
        [SerializeField] private Color _colorEmpire = new Color(0.8f, 0.1f, 0.1f);
        [SerializeField] private Color _colorNeutral = Color.white;

        private HexCoordinates _myCoords;

        public HexCoordinates Coordinates => _myCoords;

        public void Initialize(HexCoordinates coords, Vector3 worldPosition)
        {
            _myCoords = coords;
            transform.position = worldPosition;
        }

        /// <summary>
        /// Core 모델의 데이터를 읽어 타일의 시각적 상태를 갱신합니다.
        /// </summary>
        public void RefreshView(HexTileData tileData)
        {
            // 1. 확정된 Faction 열거형에 따른 타일 색상 갱신
            switch (tileData.Owner)
            {
                case Faction.DemonKing:
                    _baseRenderer.color = _colorDemonKing;
                    break;
                case Faction.Empire:
                    _baseRenderer.color = _colorEmpire;
                    break;
                default:
                    _baseRenderer.color = _colorNeutral;
                    break;
            }

            // 2. 전장의 안개(Fog of War) 처리
            if (_fogRenderer != null)
            {
                _fogRenderer.enabled = !tileData.IsExplored;
            }

            // 3. 비트 플래그 기반 보급선 활성화 시각화
            if (_supplyLineRenderer != null)
            {
                // IsSupplyConnected가 아닌 비트 캡슐화 프로퍼티 HasSupply 사용
                bool showSupply = (tileData.Owner == Faction.DemonKing) && tileData.HasSupply;
                _supplyLineRenderer.enabled = showSupply;
            }
        }
    }
}