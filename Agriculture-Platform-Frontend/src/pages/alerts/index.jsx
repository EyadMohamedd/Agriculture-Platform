import { useState, useEffect } from 'react';
import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Chip from '@mui/material/Chip';
import CircularProgress from '@mui/material/CircularProgress';
import FormControl from '@mui/material/FormControl';
import Grid from '@mui/material/Grid';
import InputLabel from '@mui/material/InputLabel';
import MenuItem from '@mui/material/MenuItem';
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
import { AlertOutlined } from '@ant-design/icons';
import MainCard from 'src/components/MainCard';
import { farmsApi } from 'src/api/farms';
import { alertsApi } from 'src/api/alerts';

const SEVERITY_COLOR = { Critical: 'error', High: 'warning', Medium: 'info', Low: 'default' };
const SEVERITY_OPTIONS = [
  { value: '', label: 'All Severities' },
  { value: 'Critical', label: 'Critical' },
  { value: 'High', label: 'High' },
  { value: 'Medium', label: 'Medium' },
  { value: 'Low', label: 'Low' }
];

const SENSOR_LABEL_MAP = {
  temperature: 'Temperature',
  ph: 'Soil pH',
  moisture: 'Soil Moisture',
  npk_n: 'NPK Nitrogen',
  npk_p: 'NPK Phosphorus',
  npk_k: 'NPK Potassium',
  rainfall: 'Rainfall'
};

// Parse sensor type from alert type string, e.g. "temperature_critical_high" → "Temperature"
function parseSensorLabel(type) {
  if (!type) return '—';
  for (const [key, label] of Object.entries(SENSOR_LABEL_MAP)) {
    if (type.startsWith(key + '_') || type === key) return label;
  }
  return type.split('_')[0];
}

// Parse threshold label from alert type, e.g. "temperature_critical_high" → "Critical High"
const THRESHOLD_SUFFIX_MAP = {
  'critical_high': 'Critical High',
  'critical_low':  'Critical Low',
  'warning_high':  'Warning High',
  'warning_low':   'Warning Low',
  'above_normal':  'Above Normal',
  'below_normal':  'Below Normal'
};

function parseThreshold(type) {
  if (!type) return '—';
  for (const [suffix, label] of Object.entries(THRESHOLD_SUFFIX_MAP)) {
    if (type.endsWith('_' + suffix)) return label;
  }
  return '—';
}

export default function AlertsPage() {
  const [farms, setFarms] = useState([]);
  const [selectedFarm, setSelectedFarm] = useState('');
  const [severity, setSeverity] = useState('');
  const [alerts, setAlerts] = useState([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(0);
  const [pageSize] = useState(20);
  const [loading, setLoading] = useState(true);
  const [farmsLoading, setFarmsLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    farmsApi.getAll({ pageSize: 100 }).then((res) => {
      setFarms(res.data.data.items || []);
    }).catch(() => {}).finally(() => setFarmsLoading(false));
  }, []);

  const loadAlerts = () => {
    setLoading(true);
    setError('');
    const params = { page: page + 1, pageSize, sortBy: 'timestamp', sortOrder: 'desc' };
    if (selectedFarm) params.farmId = selectedFarm;
    if (severity) params.severity = severity;
    alertsApi.getAll(params)
      .then((res) => {
        setAlerts(res.data.data.items || []);
        setTotalCount(res.data.data.totalCount || 0);
      })
      .catch(() => setError('Failed to load alerts'))
      .finally(() => setLoading(false));
  };

  useEffect(() => { loadAlerts(); }, [page, selectedFarm, severity]);

  const criticalCount = alerts.filter((a) => a.severity === 'Critical').length;

  return (
    <Stack spacing={3}>
      <Stack direction={{ xs: 'column', sm: 'row' }} justifyContent="space-between" alignItems={{ xs: 'flex-start', sm: 'center' }} spacing={2}>
        <Stack direction="row" alignItems="center" spacing={1}>
          <AlertOutlined style={{ fontSize: 24 }} />
          <Typography variant="h4">Alerts</Typography>
        </Stack>
        {criticalCount > 0 && (
          <Chip label={`${criticalCount} Critical`} color="error" size="small" />
        )}
      </Stack>

      {error && <Alert severity="error" onClose={() => setError('')}>{error}</Alert>}

      <MainCard title="Filters">
        <Grid container spacing={2}>
          <Grid size={{ xs: 12, sm: 6 }}>
            <FormControl fullWidth size="small" disabled={farmsLoading}>
              <InputLabel shrink>Farm</InputLabel>
              <Select label="Farm" notched displayEmpty value={selectedFarm} onChange={(e) => { setSelectedFarm(e.target.value); setPage(0); }}>
                <MenuItem value="">All Farms</MenuItem>
                {farms.map((f) => <MenuItem key={f.id} value={f.id}>{f.name}</MenuItem>)}
              </Select>
            </FormControl>
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <FormControl fullWidth size="small">
              <InputLabel shrink>Severity</InputLabel>
              <Select label="Severity" notched displayEmpty value={severity} onChange={(e) => { setSeverity(e.target.value); setPage(0); }}>
                {SEVERITY_OPTIONS.map((s) => <MenuItem key={s.value} value={s.value}>{s.label}</MenuItem>)}
              </Select>
            </FormControl>
          </Grid>
        </Grid>
      </MainCard>

      <MainCard content={false}>
        {loading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}><CircularProgress /></Box>
        ) : alerts.length === 0 ? (
          <Box sx={{ py: 8, textAlign: 'center' }}>
            <Typography color="text.secondary">No alerts found.</Typography>
          </Box>
        ) : (
          <>
            <TableContainer>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Message</TableCell>
                    <TableCell>Severity</TableCell>
                    <TableCell>Sensor</TableCell>
                    <TableCell>Threshold</TableCell>
                    <TableCell>Timestamp</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {alerts.map((alert) => (
                    <TableRow key={alert.id} hover>
                      <TableCell>
                        <Stack direction="row" alignItems="center" spacing={1}>
                          <AlertOutlined style={{
                            color: alert.severity === 'Critical' ? '#ff4d4f'
                              : alert.severity === 'High' ? '#faad14'
                              : alert.severity === 'Medium' ? '#1890ff' : '#8c8c8c'
                          }} />
                          <Typography variant="body2">{alert.message}</Typography>
                        </Stack>
                      </TableCell>
                      <TableCell>
                        <Chip label={alert.severity} size="small" color={SEVERITY_COLOR[alert.severity] || 'default'} />
                      </TableCell>
                      <TableCell>{parseSensorLabel(alert.type)}</TableCell>
                      <TableCell>{parseThreshold(alert.type)}</TableCell>
                      <TableCell>{new Date(alert.timestamp).toLocaleString()}</TableCell>
                    </TableRow>
                  ))}
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
