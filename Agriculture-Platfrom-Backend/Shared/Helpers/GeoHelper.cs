namespace AgriculturalMonitorSystem.Shared.Helpers;

public static class GeoHelper
{
    /// <summary>Haversine formula — returns distance in kilometres.</summary>
    public static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371;
        var dLat = ToRad(lat2 - lat1);
        var dLon = ToRad(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    public static bool IsValidLatitude(double lat) => lat is >= -90 and <= 90;
    public static bool IsValidLongitude(double lon) => lon is >= -180 and <= 180;

    private static double ToRad(double deg) => deg * Math.PI / 180;
}
