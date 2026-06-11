import { useState, useEffect, useRef } from 'react';
import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import CircularProgress from '@mui/material/CircularProgress';
import FormControl from '@mui/material/FormControl';
import Grid from '@mui/material/Grid';
import InputLabel from '@mui/material/InputLabel';
import MenuItem from '@mui/material/MenuItem';
import OutlinedInput from '@mui/material/OutlinedInput';
import Select from '@mui/material/Select';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import { LineChart } from '@mui/x-charts/LineChart';
import { SearchOutlined } from '@ant-design/icons';
import MainCard from 'src/components/MainCard';
import { farmsApi } from 'src/api/farms';
import { sensorsApi } from 'src/api/sensors';

const SENSOR_TYPES = [
  { value: 'temperature', label: 'Temperature (°C)' },
  { value: 'ph', label: 'Soil pH' },
  { value: 'moisture', label: 'Soil Moisture (%)' },
  { value: 'npk_n', label: 'NPK Nitrogen' },
  { value: 'npk_p', label: 'NPK Phosphorus' },
  { value: 'npk_k', label: 'NPK Potassium' },
  { value: 'rainfall', label: 'Rainfall (mm)' }
];

// Extract the numeric value from a SensorReadingDto for the selected sensor type
function extractValue(reading, sensorType) {
  switch (sensorType) {
    case 'temperature': return reading.temperature;
    case 'ph':          return reading.soilPh;
    case 'moisture':    return reading.soilMoisture;
    case 'npk_n':       return reading.npkN;
    case 'npk_p':       return reading.npkP;
    case 'npk_k':       return reading.npkK;
    case 'rainfall':    return reading.rainfall;
    default:            return null;
  }
}

function StatCard({ label, value, color }) {
  return (
    <MainCard contentSX={{ p: 2 }}>
      <Typography variant="h6" color="text.secondary">{label}</Typography>
      <Typography variant="h3" color={color || 'text.primary'} sx={{ mt: 0.5 }}>
        {value != null ? Number(value).toFixed(2) : '—'}
      </Typography>
    </MainCard>
  );
}

export default function SensorStatistics() {
  const [farms, setFarms] = useState([]);
  const [selectedFarm, setSelectedFarm] = useState('');
  const [sensorType, setSensorType] = useState('temperature');
  const [appliedSensorType, setAppliedSensorType] = useState('temperature');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [stats, setStats] = useState(null);
  const [chartReadings, setChartReadings] = useState([]);
  const [loading, setLoading] = useState(false);
  const [farmsLoading, setFarmsLoading] = useState(true);
  const [error, setError] = useState('');
  const initialLoad = useRef(false);

  useEffect(() => {
    farmsApi.getAll({ pageSize: 100 }).then((res) => {
      const items = res.data.data.items || [];
      setFarms(items);
      if (items.length > 0) setSelectedFarm(items[0].id);
    }).catch(() => setError('Failed to load farms'))
      .finally(() => setFarmsLoading(false));
  }, []);

  const loadStats = (farmId, type, start, end) => {
    const farm = farmId ?? selectedFarm;
    const st = type ?? sensorType;
    if (!farm) return;
    setLoading(true);
    setError('');
    setStats(null);
    setChartReadings([]);

    const params = { sensorType: st };
    if (start ?? startDate) params.startDate = start ?? startDate;
    if (end ?? endDate) params.endDate = end ?? endDate;

    const readingsParams = { farmId: farm, sensorType: st, pageSize: 500, page: 1 };
    if (start ?? startDate) readingsParams.startDate = start ?? startDate;
    if (end ?? endDate) readingsParams.endDate = end ?? endDate;

    Promise.all([
      sensorsApi.getStatistics(farm, params),
      sensorsApi.getReadings(readingsParams)
    ]).then(([statsRes, readingsRes]) => {
      setStats(statsRes.data.data || null);
      const items = (readingsRes.data.data?.items || []).slice().reverse();
      setChartReadings(items);
      setAppliedSensorType(st);
    }).catch(() => setError('Failed to load statistics'))
      .finally(() => setLoading(false));
  };

  useEffect(() => {
    if (selectedFarm && !initialLoad.current) {
      initialLoad.current = true;
      loadStats(selectedFarm, sensorType, startDate, endDate);
    }
  }, [selectedFarm]);

  const handleApply = () => loadStats(selectedFarm, sensorType, startDate, endDate);

  const xLabels = chartReadings.map((r) =>
    new Date(r.timestamp).toLocaleString([], { month: 'numeric', day: 'numeric', hour: '2-digit', minute: '2-digit' })
  );
  const yValues = chartReadings.map((r) => extractValue(r, appliedSensorType) ?? null);
  const hasChartData = yValues.some((v) => v != null);

  const currentLabel = SENSOR_TYPES.find((t) => t.value === appliedSensorType)?.label ?? appliedSensorType;

  return (
    <Stack spacing={3}>
      <Typography variant="h4">Sensor Statistics</Typography>
      {error && <Alert severity="error" onClose={() => setError('')}>{error}</Alert>}

      <MainCard title="Filters">
        <Grid container spacing={2} alignItems="flex-end">
          <Grid size={{ xs: 12, sm: 6, md: 3 }}>
            <FormControl fullWidth size="small" disabled={farmsLoading}>
              <InputLabel>Farm</InputLabel>
              <Select label="Farm" value={selectedFarm} onChange={(e) => setSelectedFarm(e.target.value)}>
                {farms.map((f) => <MenuItem key={f.id} value={f.id}>{f.name}</MenuItem>)}
              </Select>
            </FormControl>
          </Grid>
          <Grid size={{ xs: 12, sm: 6, md: 3 }}>
            <FormControl fullWidth size="small">
              <InputLabel>Sensor Type</InputLabel>
              <Select label="Sensor Type" value={sensorType} onChange={(e) => setSensorType(e.target.value)}>
                {SENSOR_TYPES.map((t) => <MenuItem key={t.value} value={t.value}>{t.label}</MenuItem>)}
              </Select>
            </FormControl>
          </Grid>
          <Grid size={{ xs: 12, sm: 6, md: 2.5 }}>
            <FormControl fullWidth size="small">
              <InputLabel shrink>Start Date</InputLabel>
              <OutlinedInput label="Start Date" type="date" notched value={startDate} onChange={(e) => setStartDate(e.target.value)} />
            </FormControl>
          </Grid>
          <Grid size={{ xs: 12, sm: 6, md: 2.5 }}>
            <FormControl fullWidth size="small">
              <InputLabel shrink>End Date</InputLabel>
              <OutlinedInput label="End Date" type="date" notched value={endDate} onChange={(e) => setEndDate(e.target.value)} />
            </FormControl>
          </Grid>
          <Grid size={{ xs: 12, md: 1 }}>
            <Button fullWidth variant="contained" startIcon={<SearchOutlined />} onClick={handleApply}>
              Apply
            </Button>
          </Grid>
        </Grid>
      </MainCard>

      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}><CircularProgress /></Box>
      ) : stats ? (
        <>
          <Grid container spacing={2.5}>
            <Grid size={{ xs: 12, sm: 4 }}>
              <StatCard label={`Minimum (${stats.count ?? 0} readings)`} value={stats.min} color="info.main" />
            </Grid>
            <Grid size={{ xs: 12, sm: 4 }}>
              <StatCard label="Average" value={stats.avg} color="primary.main" />
            </Grid>
            <Grid size={{ xs: 12, sm: 4 }}>
              <StatCard label="Maximum" value={stats.max} color="error.main" />
            </Grid>
          </Grid>

          {hasChartData && (
            <MainCard title={`${currentLabel} Over Time`}>
              <Box sx={{ width: '100%', height: 320 }}>
                <LineChart
                  xAxis={[{ scaleType: 'point', data: xLabels }]}
                  series={[{ data: yValues, label: currentLabel, area: true, connectNulls: true }]}
                  height={300}
                />
              </Box>
            </MainCard>
          )}
        </>
      ) : (
        <MainCard>
          <Box sx={{ py: 6, textAlign: 'center' }}>
            <Typography color="text.secondary">Select a farm and apply filters to view statistics.</Typography>
          </Box>
        </MainCard>
      )}
    </Stack>
  );
}
