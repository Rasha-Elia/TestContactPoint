using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Nominatim.API.Geocoders;
using Nominatim.API.Interfaces;
using Nominatim.API.Models;
using Nominatim.API.Web;

class Program
{
    static async Task Main(string[] args)
    {
        // Setup dependency injection
        var services = new ServiceCollection();
        services.AddHttpClient();
        services.AddTransient<INominatimWebInterface, NominatimWebInterface>();
        services.AddTransient<ReverseGeocoder>();

        var serviceProvider = services.BuildServiceProvider();

        string wktPoint = "POINT(4.378257028885023 50.80874321535354)"; // Example: Times Square, New York
        var (streetName, postalCode, city) = await GetAddressFromWktPoint(wktPoint, serviceProvider);

        Console.WriteLine($"Street Name: {streetName}");
        Console.WriteLine($"Postal Code: {postalCode}");
        Console.WriteLine($"City: {city}");
    }

    static async Task<(string streetName, string postalCode, string city)> GetAddressFromWktPoint(string wktPoint, ServiceProvider serviceProvider)
    {
        // Extract coordinates from WKT Point
        var coordinates = wktPoint.Replace("POINT(", "").Replace(")", "").Split(' ');
        double longitude = double.Parse(coordinates[0]);
        double latitude = double.Parse(coordinates[1]);

        // Get ReverseGeocoder from service provider
        var reverseGeocoder = serviceProvider.GetRequiredService<ReverseGeocoder>();

        var result = await reverseGeocoder.ReverseGeocode(new ReverseGeocodeRequest(){Latitude = latitude,Longitude = longitude});

        if (result != null)
        {
            return (result.Address?.Road ?? "Not found",
                result.Address?.PostCode ?? "Not found",
                result.Address?.City ?? result.Address?.Town ?? "Not found");
        }
        else
        {
            return ("Not found", "Not found", "Not found");
        }
    }
}