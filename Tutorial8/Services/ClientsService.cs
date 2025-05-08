using Microsoft.Data.SqlClient;
using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public class NotFoundException : Exception;
public class TripFullException : Exception;
public class ClientsService : IClientsService
{
    private const string ConnectionString = "Server=localhost,1433;Database=master;User Id=sa;Password=Password1$;TrustServerCertificate=True";
    
    private readonly ITripsService _tripsService;

    public ClientsService(ITripsService tripsService)
    {
        _tripsService = tripsService;
    }
    
    public async Task<int> AddClientAsync(ClientDTO client)
    {
        var command = "INSERT INTO Client (FirstName, LastName, Email, Telephone, Pesel) VALUES (@FirstName, @LastName, @Email, @Telephone, @Pesel); SELECT SCOPE_IDENTITY();";
        await using var conn = new SqlConnection(ConnectionString);
        await using var cmd = new SqlCommand(command, conn);
        cmd.Parameters.AddWithValue("@FirstName", client.FirstName);
        cmd.Parameters.AddWithValue("@LastName", client.LastName);
        cmd.Parameters.AddWithValue("@Email", client.Email);
        cmd.Parameters.AddWithValue("@Telephone", client.Telephone);
        cmd.Parameters.AddWithValue("@Pesel", client.Pesel);
        await conn.OpenAsync();

        var id = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(id);
    }

    public async Task<bool> DoesRegistrationExist(int clientId, int tripId)
    {
        await using var conn = new SqlConnection(ConnectionString);
        await using var cmd = new SqlCommand("SELECT 1 FROM Client_Trip WHERE IdClient = @clientId AND IdTrip = @tripId", conn);
        cmd.Parameters.AddWithValue("@clientId", clientId);
        cmd.Parameters.AddWithValue("@tripId", tripId);

        await conn.OpenAsync();
        var obj = await cmd.ExecuteScalarAsync();

        return obj != null;
    }

    public async Task<bool> DeleteRegistration(int clientId, int tripId)
    {
        if (!await DoesRegistrationExist(clientId, tripId))
        {
            throw new NotFoundException();
        }
        await using var conn = new SqlConnection(ConnectionString);
        await using var cmd = new SqlCommand("DELETE FROM Client_Trip WHERE IdClient = @clientId AND IdTrip = @tripId", conn);
        cmd.Parameters.AddWithValue("@clientId", clientId);
        cmd.Parameters.AddWithValue("@tripId", tripId);

        await conn.OpenAsync();
        var rows = await cmd.ExecuteNonQueryAsync();

        return rows == 1;
    }

    public async Task<bool> RegisterClientToTrip(int clientId, int tripId)
    {
        if (!await DoesClientExist(clientId) || !await _tripsService.DoesTripExist(tripId))
        {
            throw new NotFoundException();
        }

        if (!await _tripsService.IsTripAvailable(tripId))
        {
            throw new TripFullException();
        }
        
        var command = "INSERT INTO Client_Trip (IdClient, IdTrip, RegisteredAt, PaymentDate) VALUES (@IdClient, @IdTrip, @RegisteredAt, NULL);SELECT SCOPE_IDENTITY();";
        await using var conn = new SqlConnection(ConnectionString);
        await using var cmd = new SqlCommand(command, conn);
        cmd.Parameters.AddWithValue("@IdClient", clientId);
        cmd.Parameters.AddWithValue("@IdTrip", tripId);
        cmd.Parameters.AddWithValue("@RegisteredAt", DateTime.Now.ToString("yyyyMMdd"));
        await conn.OpenAsync();
        try
        {
            var rows = await cmd.ExecuteNonQueryAsync();
            return rows == 1;
        }
        catch (SqlException)
        {
            return false;
        }
    }

    public async Task<bool> DoesClientExist(int id)
    {
        await using var conn = new SqlConnection(ConnectionString);
        await using var cmd = new SqlCommand("SELECT 1 FROM Client WHERE IdClient = @Id", conn);
        cmd.Parameters.AddWithValue("@Id", id);

        await conn.OpenAsync();
        var obj = await cmd.ExecuteScalarAsync();

        return obj != null;
    }
    
}