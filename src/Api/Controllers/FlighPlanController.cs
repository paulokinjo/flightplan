using Api.Data;
using Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Controllers
{
    [Authorize]
    [Route("api/v1/flightplan")]
    [ApiController]
    public class FlighPlanController : ControllerBase
    {
        private readonly IFlightPlanDatabase<FlightPlan> flightPlanDatabase;

        public FlighPlanController(IFlightPlanDatabase<FlightPlan> flightPlanDatabase) => 
            this.flightPlanDatabase = flightPlanDatabase;


        [HttpGet]
        [SwaggerResponse(StatusCodes.Status204NoContent, "No flight plans have been filed with this system")]
        public async Task<IActionResult> FlightPlanList()
        {
            var flightPlanList = await flightPlanDatabase.GetAll();
            if (flightPlanList.Count == 0)
            {
                return NoContent();
            }

            return Ok(flightPlanList);
        }

        [HttpGet]
        [Route("{flightPlanId}")]
        public async Task<IActionResult> GetFlightPlanById(string flightPlanId)
        {
            var flightPlan = await flightPlanDatabase.GetById(flightPlanId);
            if (flightPlan.Id != flightPlanId) 
            {
                return NotFound();
            }

            return Ok(flightPlan);
        }

        /// <summary>
        /// Files a new flight plan with the system
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/v1/flightplan/file
        ///     {
        ///         "flight_plan_id": "string",
        ///         "aircraft_identification": "string",
        ///         "aircraft_type": "string",
        ///         "airspeed": 0,
        ///         "altitude": 0,
        ///         "flight_type": "string",
        ///         "fuel_hours": 0,
        ///         "fuel_minutes": 0,
        ///         "departure_time": "2023-06-04T06:37:09.356Z",
        ///         "estimated_arrival_time": "2023-06-04T06:37:09.357Z",
        ///         "departing_airport": "string",
        ///         "arrival_airport": "string",
        ///         "route": "string",
        ///         "remarks": "string",
        ///         "number_onboards": 0
        ///     }
        /// </remarks>
        /// <param name="flightPlan">The flight plan data to tbe filed.</param>
        /// <response code="400">There is a problem with the flight plan data received by this system</response>
        /// <response code="500">The flight plan is valid but this system cannot process it</response>
        /// <returns></returns>
        [HttpPost]
        [Route("file")]
        public async Task<IActionResult> FileFlightPlan(FlightPlan flightPlan)
        {
            var transactionResult = await flightPlanDatabase.FileFlightPlan(flightPlan);
            return transactionResult switch
            {
                TransactionResult.Success => Ok(),
                TransactionResult.BadRequest => BadRequest(),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }

        [HttpPut]
        public async Task<IActionResult> UpdateFlightPlan(FlightPlan flightPlan)
        {
            var updateResult = await flightPlanDatabase.UpdateById(flightPlan.Id, flightPlan);
            return updateResult switch
            {
                TransactionResult.Success => Ok(),
                TransactionResult.NotFound => NotFound(),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }

        [HttpDelete]
        [Route("{flightPlanId}")]
        public async Task<IActionResult> DeleteFlightPlan(string flightPlanId)
        {
            Thread.Sleep(15000);
            var resultSuccess = await flightPlanDatabase.DeleteById(flightPlanId);
            if (resultSuccess)
            {
                return NoContent();
            }

            Thread.Sleep(20000);
            return NotFound();
        }

        [HttpGet]
        [Route("airport/departure/{flightPlanId}")]
        public async Task<IActionResult> GetFlightPlanDepartureAirport(string flightPlanId)
        {
            var flightPlan = await flightPlanDatabase.GetById(flightPlanId);
            if (flightPlan.Id != flightPlanId)
            {
                return NotFound();
            }

            return Ok(flightPlan.DepartureAirport);
        }

        [HttpGet]
        [Route("route/{flightPlanId}")]
        public async Task<IActionResult> GetFlightPlanRoute(string flightPlanId)
        {
            var flightPlan = await flightPlanDatabase.GetById(flightPlanId);
            if (flightPlan.Id != flightPlanId)
            {
                return NotFound();
            }

            return Ok(flightPlan.Route);
        }

        [HttpGet]
        [Route("time/enroute/{flightPlanId}")]
        public async Task<IActionResult> GetFlightPlanTimeEnroute(string flightPlanId)
        {
            var flightPlan = await flightPlanDatabase.GetById(flightPlanId);
            if (flightPlan.Id != flightPlanId)
            {
                return NotFound();
            }

            var estimatedTimeEnroute = flightPlan.ArrivalTime - flightPlan.DepartureTime;
            return Ok(estimatedTimeEnroute);
        }
    }
}
