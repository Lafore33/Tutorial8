using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public interface ITripsService
{
    Task<List<ClientTripDTO>> GetTripsByClientId(int id);
    Task<bool> DoesClientExist(int id);
    Task<bool> IsEmpty(int id);
    Task<bool> IsTripAvailable(int id);
    Task<List<TripDTO>> GetTrips(int id=-1);
    Task<bool> DoesTripExist(int id);
}