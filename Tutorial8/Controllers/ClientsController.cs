using Microsoft.AspNetCore.Mvc;
using Tutorial8.Models.DTOs;
using Tutorial8.Services;

namespace Tutorial8.Controllers;

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

    [HttpGet("{id}/trips")]
    public async Task<IActionResult> GetTrip(int id)
    {
        try
        {
            var trips = await _tripsService.GetTripsByClientId(id);
            return Ok(trips.Count == 0 ? [] : trips);
        }
        catch (NotFoundException e)
        {
            return NotFound($"Client with ID {id} doesn't exist");
        }
    }

    [HttpPut("{id}/trips/{tripId}")]
    public async Task<IActionResult> AddTripToClient(int id, int tripId)
    {
        try
        {
            var isAdded = await _clientsService.RegisterClientToTrip(id, tripId);
            return isAdded ? Ok($"Client with ID {id} registered to a Trip with ID {tripId}") : BadRequest($"Client with ID {id} is already assigned to a Trip with ID {tripId}");
        }
        catch (NotFoundException e)
        {
            return NotFound($"Client with ID {id} or Trip with ID {tripId} doesn't exist");
        }
        catch (TripFullException e)
        {
            return BadRequest($"The trip with ID {tripId} is already full");
        }
    }

    
    [HttpDelete("{id}/trips/{tripId}")]
    public async Task<IActionResult> DeleteRegistration(int id, int tripId)
    {
        try
        {
            var isDeleted = await _clientsService.DeleteRegistration(id, tripId);
            return isDeleted ? Ok() : StatusCode(500);
        }
        catch (NotFoundException e)
        {            
            return NotFound($"Client with ID {id} is not registered to Trip with ID {tripId}");
        }
    }

}        
