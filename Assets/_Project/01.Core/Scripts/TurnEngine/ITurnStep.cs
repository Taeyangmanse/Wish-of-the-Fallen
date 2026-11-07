namespace Project.Core.TurnEngine
{
    // 각 결산 단계를 독립적으로 캡슐화하는 인터페이스
    public interface ITurnStep
    {
        int StepOrder { get; }
        string StepName { get; }
        void Execute(TurnResolutionContext context);
    }

    // 주차 결산 시 상태 데이터를 전달하는 컨텍스트 구조체
    public class TurnResolutionContext
    {
        public int CurrentWeek;
        public int AvailableAP;
        public bool IsSupplyLineIntact;
        public bool IsWorldRotPurged; // 소원으로 부패 제거 여부
        // 추가 상태 필드 확장
    }
}