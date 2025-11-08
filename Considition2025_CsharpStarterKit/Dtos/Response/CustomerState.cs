namespace Considition2025_CsharpStarterKit.Dtos.Response;

public enum CustomerState
{
    Home,
    WaitingForCharger,
    Charging,
    DoneCharging,
    Traveling,
    TransitioningToNode,
    TransitioningToEdge,
    DestinationReached,
    FailedToCharge,
    RanOutOfJuice
}