using Microsoft.Data.SqlClient;
using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public class TripsService : ITripsService
{
    private const string ConnectionString = "Server=localhost,1433;Database=master;User Id=sa;Password=Password1$;TrustServerCertificate=True";
    
    public async Task<bool> DoesTripExist(int id)
    {
        await using var conn = new SqlConnection(ConnectionString);
        await using var cmd = new SqlCommand("SELECT 1 FROM Trip WHERE IdTrip = @Id", conn);
        cmd.Parameters.AddWithValue("@Id", id);

        await conn.OpenAsync();
        var result = await cmd.ExecuteScalarAsync();

        return result != null;
    }

    public async Task<bool> IsTripAvailable(int id)
    {
        await using var conn = new SqlConnection(ConnectionString);
        await using var cmd = new SqlCommand("SELECT COUNT(*) AS PARTICIPANTS, MaxPeople FROM Trip JOIN Client_Trip ON Trip.IdTrip = Client_Trip.IdTrip WHERE Trip.IdTrip = @Id GROUP BY MaxPeople", conn);
        cmd.Parameters.AddWithValue("@Id", id);

        await conn.OpenAsync();

        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return await IsEmpty(id);
            
        var participants = reader.GetInt32(reader.GetOrdinal("Participants"));
        var maxPeople = reader.GetInt32(reader.GetOrdinal("MaxPeople"));

        return participants < maxPeople;
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

    public async Task<bool> IsEmpty(int id)
    {
        await using var conn = new SqlConnection(ConnectionString);
        await using var cmd = new SqlCommand("SELECT MaxPeople FROM Trip WHERE IdTrip = @Id", conn);
        cmd.Parameters.AddWithValue("@Id", id);

        await conn.OpenAsync();

        var result = await cmd.ExecuteScalarAsync();
        return result != null;
    }

    private async Task<List<CountryDTO>> GetCountries(int tripId)
    {
        var countryCommand = """
                                SELECT Country.IdCountry, Country.Name
                                FROM Country_Trip JOIN Country ON Country_Trip.IdCountry = Country.IdCountry
                                WHERE IdTrip = @IdTrip
                             """;
        await using (var conn = new SqlConnection(ConnectionString))
        await using (var cmd2 = new SqlCommand(countryCommand, conn))
        {
            await conn.OpenAsync();
            cmd2.Parameters.AddWithValue("@IdTrip", tripId);
            await using (var reader = await cmd2.ExecuteReaderAsync())
            {
                List<CountryDTO> countries = [];
                while (await reader.ReadAsync())
                {
                    countries.Add(new CountryDTO
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("IdCountry")),
                        Name = reader.GetString(reader.GetOrdinal("Name"))
                    });
                }

                return countries;
            }
        }
    }

    public async Task<List<ClientTripDTO>> GetTripsByClientId(int id)
    {
        if (!await DoesClientExist(id))
        {
            throw new NotFoundException();
        }

        var clientTrips = new List<ClientTripDTO>();
        var command = "SELECT * FROM Trip JOIN Client_Trip ON Trip.IdTrip = Client_Trip.IdTrip WHERE IdClient = @idClient";

        await using (var conn = new SqlConnection(ConnectionString))
        await using (var cmd = new SqlCommand(command, conn)){
            await conn.OpenAsync();
            cmd.Parameters.AddWithValue("@idClient", id);
                
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var idOrdinal = reader.GetOrdinal("IdTrip");
                    clientTrips.Add(new ClientTripDTO()
                    {
                        trip = new TripDTO()
                        {
                            Id = reader.GetInt32(idOrdinal),
                            Name = reader.GetString(1),
                            Description = reader.GetString(reader.GetOrdinal("Description")),
                            DateFrom = reader.GetDateTime(reader.GetOrdinal("DateFrom")).ToString("G"),
                            DateTo = reader.GetDateTime(reader.GetOrdinal("DateTo")).ToString("G"),
                            MaxPeople = reader.GetInt32(reader.GetOrdinal("MaxPeople")),
                        },
                        RegisteredAt = reader.GetInt32(reader.GetOrdinal("RegisteredAt")),
                        PaymentDate = reader.IsDBNull(reader.GetOrdinal("PaymentDate")) ? null : reader.GetInt32(reader.GetOrdinal("PaymentDate")),

                    });
                }
            }
        }
        
        foreach (var clientTrip in clientTrips)
        {
            clientTrip.trip.Countries = await GetCountries(clientTrip.trip.Id);
        }

        return clientTrips;
    }
    public async Task<List<TripDTO>> GetTrips(int id=-1)
    {
        if (id != -1 && !await DoesTripExist(id))
        {
            throw new NotFoundException();
        }
        
        var trips = new List<TripDTO>();
        var command = id == -1 ? "SELECT * FROM Trip" : "SELECT * FROM Trip WHERE IdTrip = @id";
        await using (var conn = new SqlConnection(ConnectionString))
        await using (var cmd = new SqlCommand(command, conn))
        {
            await conn.OpenAsync();
            if (id != -1) cmd.Parameters.AddWithValue("@id", id);
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var idOrdinal = reader.GetOrdinal("IdTrip");
                    trips.Add(new TripDTO()
                    {
                        Id = reader.GetInt32(idOrdinal),
                        Name = reader.GetString(1),
                        Description = reader.GetString(reader.GetOrdinal("Description")),
                        DateFrom = reader.GetDateTime(reader.GetOrdinal("DateFrom")).ToString("G"),
                        DateTo = reader.GetDateTime(reader.GetOrdinal("DateTo")).ToString("G"),
                        MaxPeople = reader.GetInt32(reader.GetOrdinal("MaxPeople"))
                    });
                }
            }
        }
        
        foreach (var trip in trips)
        {
            trip.Countries = await GetCountries(trip.Id);
        }

        return trips;
    }
    
    
    
}