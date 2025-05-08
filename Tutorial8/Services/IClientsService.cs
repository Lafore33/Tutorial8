using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public interface IClientsService
{

    Task<bool> DoesClientExist(int id);
    
    Task<int> AddClientAsync(ClientDTO client);

    Task<bool> RegisterClientToTrip(int clientId, int tripId);

    Task<bool> DeleteRegistration(int clientId, int tripId);

    Task<bool> DoesRegistrationExist(int clientId, int tripId);
}