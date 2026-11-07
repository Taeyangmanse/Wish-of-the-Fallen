using System;

namespace Project.Core.HexGrid
{
    public sealed class AmbushCalculator
    {
        private readonly Random _rng;

        public const int DefaultAmbushChancePerFogNode = 20;
        public const int MaxAmbushChanceCap = 90;

        public AmbushCalculator(Random rng)
        {
            _rng = rng ?? throw new ArgumentNullException(nameof(rng));
        }

        /// <summary>
        /// 미개척지 이동 시 기습 여부와 확률 데이터를 계산합니다.
        /// </summary>
        public (bool isAmbushed, int totalChance, int roll) CalculateAmbush(
            int pathDistance,
            int ambushChanceReduction)
        {
            int baseChance = pathDistance * DefaultAmbushChancePerFogNode;
            int totalChance = Math.Clamp(baseChance - ambushChanceReduction, 0, MaxAmbushChanceCap);

            if (totalChance <= 0)
            {
                return (false, 0, 0);
            }

            int roll = _rng.Next(1, 101);
            bool isAmbushed = roll <= totalChance;

            return (isAmbushed, totalChance, roll);
        }
    }
}