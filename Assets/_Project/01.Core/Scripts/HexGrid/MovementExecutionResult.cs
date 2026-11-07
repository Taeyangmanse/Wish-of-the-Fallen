namespace Project.Core.HexGrid
{
    /// <summary>
    /// 이동 시도에 따른 결과 데이터 모델
    /// </summary>
    public readonly struct MovementExecutionResult
    {
        public readonly bool IsSuccess;
        public readonly bool IsAmbushed;
        public readonly int ConsumedAP;
        public readonly int AmbushChance;
        public readonly int AmbushRoll;
        public readonly MovementFailureReason FailureReason;

        private MovementExecutionResult(
            bool isSuccess,
            bool isAmbushed,
            int consumedAP,
            int ambushChance,
            int ambushRoll,
            MovementFailureReason failureReason)
        {
            IsSuccess = isSuccess;
            IsAmbushed = isAmbushed;
            ConsumedAP = consumedAP;
            AmbushChance = ambushChance;
            AmbushRoll = ambushRoll;
            FailureReason = failureReason;
        }

        // 성공 팩토리 메서드
        public static MovementExecutionResult Success(bool isAmbushed, int consumedAP, int ambushChance, int ambushRoll)
            => new MovementExecutionResult(true, isAmbushed, consumedAP, ambushChance, ambushRoll, MovementFailureReason.None);

        // 실패 팩토리 메서드
        public static MovementExecutionResult Fail(MovementFailureReason reason)
            => new MovementExecutionResult(false, false, 0, 0, 0, reason);
    }
}