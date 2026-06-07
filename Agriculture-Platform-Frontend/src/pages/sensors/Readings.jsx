import { useState, useEffect } from 'react';
import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Chip from '@mui/material/Chip';
import CircularProgress from '@mui/material/CircularProgress';
import FormControl from '@mui/material/FormControl';
import Grid from '@mui/material/Grid';
import InputLabel from '@mui/material/InputLabel';
import MenuItem from '@mui/material/MenuItem';
import OutlinedInput from '@mui/material/OutlinedInput';
import Select from '@mui/material/Select';
import Stack from '@mui/material/Stack';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TablePagination from '@mui/material/TablePagination';
import TableRow from '@mui/material/TableRow';
import Typography from '@mui/material/Typography';
import Button from '@mui/material/Button';
import { SearchOutlined } from '@ant-design/icons';
import MainCard from 'src/components/MainCard';
import { farmsApi } from 'src/api/farms';
import { sensorsApi } from 'src/api/sensors';

// API returns flat fields: temperature, soilPh, soilMoisture, npkN, npkP, npkK, rainfall
// Derive sensor type from whichever field is non-null
function getSensorType(r) {
  if (r.temperature != null) return 'Temperature';
  if (r.soilPh != null) return 'Soil pH';
  if (r.soilMoisture != null) return 'Soil Moisture';
  if (r.npkN != null || r.npkP != null || r.npkK != null) return 'NPK';
  if (r.rainfall != null) return 'Rainfall';
  return '—';
}

function getDisplayValue(r) {
  if (r.temperature != null) return { val: r.temperature.toFixed(2), unit: '°C' };
  if (r.soilPh != null) return { val: r.soilPh.toFixed(2), unit: '' };
  if (r.soilMoisture != null) return { val: r.soilMoisture.toFixed(2), unit: '%' };
  if (r.npkN != null || r.npkP != null || r.npkK != null) {
    return { val: `N:${(r.npkN ?? 0).toFixed(0)} P:${(r.npkP ?? 0).toFixed(0)} K:${(r.npkK ?? 0).toFixed(0)}`, unit: '' };
  }
  if (r.rainfall != null) return { val: r.rainfall.toFixed(2), unit: 'mm' };
  return { val: '—', unit: '' };
}

const SENSOR_TYPES = [
  { value: '', label: 'All Types' },
  { value: 'temperature', label: 'Temperature' },
  { value: 'ph', label: 'Soil pH' },
  { value: 'moisture', label: 'Soil Moisture' },
  { value: 'npk_n', label: 'NPK Nitrogen' },
  { value: 'npk_p', label: 'NPK Phosphorus' },
  { value: 'npk_k', label: 'NPK Potassium' },
  { value: 'rainfall', label: 'Rainfall' }
];

const TYPE_COLOR = { Temperature: 'error', 'Soil pH': 'warning', 'Soil Moisture': 'info', NPK: 'success', Rainfall: 'primary' };

export default function SensorReadings() {
  const [farms, setFarms] = useState([]);
  const [selectedFarm, setSelectedFarm] = useState('');
  const [sensorType, setSensorType] = useState('');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [readings, setReadings] = useState([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(0);
  const [pageSize] = useState(20);
  const [loading, setLoading] = useState(false);
  const [farmsLoading, setFarmsLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    farmsApi.getAll({ pageSize: 100 }).then((res) => {
      const items = res.data.data.items || [];
      setFarms(items);
      if (items.length > 0) setSelectedFarm(items[0].id);
    }).catch(() => setError('Failed to load farms'))
      .finally(() => setFarmsLoading(false));
  }, []);

  const loadReadings = () => {
    if (!selectedFarm) return;
    setLoading(true);
    setError('');
    const params = { farmId: selectedFarm, page: page + 1, pageSize };
    if (sensorType) params.sensorType = sensorType;
    if (startDate) params.startDate = startDate;
    if (endDate) params.endDate = endDate;
    sensorsApi.getReadings(params)
      .then((res) => {
        setReadings(res.data.data.items || []);
        setTotalCount(res.data.data.totalCount || 0);
      })
      .catch(() => setError('Failed to load sensor readings'))
      .finally(() => setLoading(false));
  };

  useEffect(() => { loadReadings(); }, [page, selectedFarm]);

  const handleSearch = () => { setPage(0); loadReadings(); };

  return (
    <Stack spacing={3}>
      <Typography variant="h4">Sensor Readings</Typography>
      {error && <Alert severity="error" onClose={() => setError('')}>{error}</Alert>}

      <MainCard title="Filters">
        <Grid container spacing={2} alignItems="flex-end">
          <Grid size={{ xs: 12, sm: 6, md: 3 }}>
            <FormControl fullWidth size="small" disabled={farmsLoading}>
              <InputLabel>Farm</InputLabel>
              <Select label="Farm" value={selectedFarm} onChange={(e) => { setSelectedFarm(e.target.value); setPage(0); }}>
                {farms.map((f) => <MenuItem key={f.id} value={f.id}>{f.name}</MenuItem>)}
              </Select>
            </FormControl>
          </Grid>
          <Grid size={{ xs: 12, sm: 6, md: 3 }}>
            <FormControl fullWidth size="small">
              <InputLabel shrink>Sensor Type</InputLabel>
              <Select
                label="Sensor Type"
                notched
                displayEmpty
                value={sensorType}
                onChange={(e) => setSensorType(e.target.value)}
              >
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
            <Button fullWidth variant="contained" startIcon={<SearchOutlined />} onClick={handleSearch}>
              Search
            </Button>
          </Grid>
        </Grid>
      </MainCard>

      <MainCard content={false}>
        {loading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}><CircularProgress /></Box>
        ) : readings.length === 0 ? (
          <Box sx={{ py: 8, textAlign: 'center' }}>
            <Typography color="text.secondary">No readings found for the selected filters.</Typography>
          </Box>
        ) : (
          <>
            <TableContainer>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Sensor Type</TableCell>
                    <TableCell align="right">Value</TableCell>
                    <TableCell align="right">Unit</TableCell>
                    <TableCell>Timestamp</TableCell>
                    <TableCell align="center">Anomaly</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {readings.map((r) => {
                    const type = getSensorType(r);
                    const { val, unit } = getDisplayValue(r);
                    return (
                      <TableRow key={r.id} hover>
                        <TableCell>
                          <Chip label={type} size="small" color={TYPE_COLOR[type] || 'default'} variant="outlined" />
                        </TableCell>
                        <TableCell align="right">
                          <Typography variant="body2" fontWeight={500}>{val}</Typography>
                        </TableCell>
                        <TableCell align="right">
                          <Typography variant="body2" color="text.secondary">{unit}</Typography>
                        </TableCell>
                        <TableCell>{new Date(r.timestamp).toLocaleString()}</TableCell>
                        <TableCell align="center">
                          {r.isAnomaly ? <Chip label="Anomaly" size="small" color="error" /> : null}
                        </TableCell>
                      </TableRow>
                    );
                  })}
                </TableBody>
              </Table>
            </TableContainer>
            <TablePagination
              component="div"
              count={totalCount}
              page={page}
              onPageChange={(_, p) => setPage(p)}
              rowsPerPage={pageSize}
              rowsPerPageOptions={[pageSize]}
            />
          </>
        )}
      </MainCard>
    </Stack>
  );
}
