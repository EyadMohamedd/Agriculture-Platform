namespace AgriculturalMonitorSystem.Infrastructure.Simulation;

public record CsvSensorRow(
    double Temperature,
    double SoilPh,
    double SoilMoisture,
    double NpkN,
    double NpkP,
    double NpkK,
    double Rainfall);

public class CsvReadingProvider
{
    private readonly List<CsvSensorRow> _rows;
    private int _index;
    private readonly object _lock = new();

    public CsvReadingProvider(ILogger<CsvReadingProvider> logger)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "SeedData", "sensor_readings.csv");

        if (!File.Exists(path))
            throw new FileNotFoundException($"Sensor readings CSV not found at: {path}");

        _rows = File.ReadAllLines(path)
            .Skip(1)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(ParseRow)
            .ToList();

        logger.LogInformation("CsvReadingProvider loaded {Count} sensor readings from CSV", _rows.Count);
    }

    public CsvSensorRow GetNext()
    {
        lock (_lock)
        {
            var row = _rows[_index];
            _index = (_index + 1) % _rows.Count;
            return row;
        }
    }

    private static CsvSensorRow ParseRow(string line)
    {
        var p = line.Split(',');
        return new CsvSensorRow(
            Temperature:  double.Parse(p[1]),
            SoilPh:       double.Parse(p[2]),
            SoilMoisture: double.Parse(p[3]),
            NpkN:         double.Parse(p[4]),
            NpkP:         double.Parse(p[5]),
            NpkK:         double.Parse(p[6]),
            Rainfall:     double.Parse(p[7]));
    }
}
