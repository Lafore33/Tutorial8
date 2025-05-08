using Microsoft.AspNetCore.Mvc;
using Tutorial8.Models.DTOs;
using Tutorial8.Services;

namespace Tutorial8.Controllers;

/// <summary>
/// Controller for managing client-related operations including registration to trips.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class ClientsController : ControllerBase
{
    private readonly IClientsService _clientsService;
    private readonly ITripsService _tripsService;

    public ClientsController(IClientsService clientsService, ITripsService tripsService)
    {
        _clientsService = clientsService;
        _tripsService = tripsService;
    }

    /// <summary>
    /// Registers a new client.
    /// </summary>
    /// <param name="client">Client data in JSON body.</param>
    /// <returns>201 Created with client data if valid; 400 BadRequest if model state is invalid.</returns>
    [HttpPost]
    public async Task<IActionResult> PostClient([FromBody] ClientDTO client)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        var newId = await _clientsService.AddClientAsync(client);
        client.Id = newId;

        return CreatedAtAction(nameof(GetTrip), new { id = newId }, client);
    }

    /// <summary>
    /// Retrieves all trips associated with a given client.
    /// </summary>
    /// <param name="id">Client ID.</param>
    /// <returns>200 OK with list of trips, 404 Not Found if client doesn't exist.</returns>
    [HttpGet("{id}/trips")]
    public async Task<IActionResult> GetTrip(int id)
    {
        try
        {
            var trips = await _tripsService.GetTripsByClientId(id);
            return Ok(trips.Count == 0 ? [] : trips);
        }
        catch (NotFoundException)
        {
            return NotFound($"Client with ID {id} doesn't exist");
        }
    }

    /// <summary>
    /// Registers a client to a trip.
    /// </summary>
    /// <param name="id">Client ID.</param>
    /// <param name="tripId">Trip ID.</param>
    /// <returns>
    /// 200 OK if registration succeeded, 
    /// 404 Not Found if client or trip doesn't exist, 
    /// 400 BadRequest if trip is full or already assigned.
    /// </returns>
    [HttpPut("{id}/trips/{tripId}")]
    public async Task<IActionResult> AddTripToClient(int id, int tripId)
    {
        try
        {
            var isAdded = await _clientsService.RegisterClientToTrip(id, tripId);
            return isAdded ? Ok($"Client with ID {id} registered to a Trip with ID {tripId}") : BadRequest($"Client with ID {id} is already assigned to a Trip with ID {tripId}");
        }
        catch (NotFoundException)
        {
            return NotFound($"Client with ID {id} or Trip with ID {tripId} doesn't exist");
        }
        catch (TripFullException)
        {
            return BadRequest($"The trip with ID {tripId} is already full");
        }
    }

    /// <summary>
    /// Deletes the registration of a client from a trip.
    /// </summary>
    /// <param name="id">Client ID.</param>
    /// <param name="tripId">Trip ID.</param>
    /// <returns>
    /// 200 OK if deletion succeeded, 
    /// 404 Not Found if registration doesn't exist, 
    /// 500 Internal Server Error if deletion fails unexpectedly.
    /// </returns>
    [HttpDelete("{id}/trips/{tripId}")]
    public async Task<IActionResult> DeleteRegistration(int id, int tripId)
    {
        try
        {
            var isDeleted = await _clientsService.DeleteRegistration(id, tripId);
            return isDeleted ? Ok() : StatusCode(500);
        }
        catch (NotFoundException)
        {
            return NotFound($"Client with ID {id} is not registered to Trip with ID {tripId}");
        }
    }
}
