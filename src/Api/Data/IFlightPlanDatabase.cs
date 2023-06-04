using Api.Models;

namespace Api.Data
{
    public interface IFlightPlanDatabase<T> : IDatabaseAdapter<T>
    {
        Task<TransactionResult> FileFlightPlan(FlightPlan flightPlan);
    }
}
