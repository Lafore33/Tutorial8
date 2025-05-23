﻿namespace Tutorial8.Models.DTOs;

public class TripDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string DateFrom { get; set; }
    public string DateTo { get; set; }
    public int MaxPeople { get; set; }
    public List<CountryDTO> Countries { get; set; }
}

public class ClientTripDTO
{
    public TripDTO trip { get; set; }
    public int RegisteredAt { get; set; }
    public int? PaymentDate { get; set; }
}

public class CountryDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
}