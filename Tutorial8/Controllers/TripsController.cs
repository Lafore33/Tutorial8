using Microsoft.AspNetCore.Mvc;
using Tutorial8.Services;

namespace Tutorial8.Controllers
{
    /// <summary>
    /// Controller responsible for retrieving trip data.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TripsController : ControllerBase
    {
        private readonly ITripsService _tripsService;

        public TripsController(ITripsService tripsService)
        {
            _tripsService = tripsService;
        }

        /// <summary>
        /// Retrieves all available trips.
        /// </summary>
        /// <returns>200 OK with a list of all trips.</returns>
        [HttpGet]
        public async Task<IActionResult> GetTrips()
        {
            var trips = await _tripsService.GetTrips();
            return Ok(trips);
        }

        /// <summary>
        /// Retrieves detailed information about a specific trip by its ID.
        /// </summary>
        /// <param name="id">The ID of the trip to retrieve.</param>
        /// <returns>
        /// 200 OK with trip details if found;
        /// 404 Not Found if the trip ID does not exist.
        /// </returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTrip(int id)
        {
            try
            {
                var trip = await _tripsService.GetTrips(id);
                return Ok(trip);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }
    }
}