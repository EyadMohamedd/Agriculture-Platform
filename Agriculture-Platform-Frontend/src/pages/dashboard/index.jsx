import { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Chip from '@mui/material/Chip';
import CircularProgress from '@mui/material/CircularProgress';
import Divider from '@mui/material/Divider';
import FormControl from '@mui/material/FormControl';
import Grid from '@mui/material/Grid';
import InputLabel from '@mui/material/InputLabel';
import MenuItem from '@mui/material/MenuItem';
import Select from '@mui/material/Select';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import {
  ThunderboltOutlined, DropboxOutlined, ExperimentOutlined,
  AlertOutlined, CloudOutlined, DashboardOutlined,
  BankOutlined, TeamOutlined, SafetyOutlined, RightOutlined
} from '@ant-design/icons';
import MainCard from 'src/components/MainCard';
import { farmsApi } from 'src/api/farms';
import { sensorsApi } from 'src/api/sensors';
import { alertsApi } from 'src/api/alerts';
import { adminApi } from 'src/api/admin';
import { useAuth } from 'src/hooks/useAuth';

const SEVERITY_COLOR = { Critical: 'error', High: 'warning', Medium: 'info' };

// ── Shared sensor card ────────────────────────────────────────────────────────
function SensorCard({ label, value, unit, icon, color = 'primary' }) {
  return (
    <MainCard contentSX={{ p: 2.5 }}>
      <Stack spacing={1}>
        <Stack direction="row" alignItems="center" justifyContent="space-between">
          <Typography variant="h6" color="text.secondary">{label}</Typography>
          <Box sx={{ color: `${color}.main`, fontSize: 24 }}>{icon}</Box>
        </Stack>
        <Typography variant="h3">
          {value !== null && value !== undefined
            ? `${typeof value === 'number' ? value.toFixed(1) : value}${unit}`
            : '—'}
        </Typography>
      </Stack>
    </MainCard>
  );
}

// ── Stat card for admin overview ──────────────────────────────────────────────
function StatCard({ label, count, icon, color, action, actionLabel }) {
  return (
    <MainCard contentSX={{ p: 2.5 }}>
      <Stack spacing={1.5}>
        <Stack direction="row" alignItems="center" justifyContent="space-between">
          <Typography variant="h6" color="text.secondary">{label}</Typography>
          <Box sx={{ color: `${color}.main`, fontSize: 24 }}>{icon}</Box>
        </Stack>
        <Typography variant="h2">{count ?? '—'}</Typography>
        {action && (
          <Button size="small" endIcon={<RightOutlined />} onClick={action} sx={{ alignSelf: 'flex-start', p: 0 }}>
            {actionLabel}
          </Button>
        )}
      </Stack>
    </MainCard>
  );
}

// ── Admin dashboard ───────────────────────────────────────────────────────────
function AdminDashboard() {
  const navigate = useNavigate();
  const [stats, setStats] = useState({ farms: null, users: null, ranges: null, sensors: null });
  const [recentFarmers, setRecentFarmers] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    Promise.all([
      farmsApi.getAll({ pageSize: 200, page: 1 }),
      adminApi.getUsers({ pageSize: 5, page: 1 }),
      adminApi.getValidationRanges()
    ]).then(([farmsRes, usersRes, rangesRes]) => {
      const farms = farmsRes.data.data?.items || [];
      setStats({
        farms: farmsRes.data.data?.totalCount ?? 0,
        users: usersRes.data.data?.totalCount ?? 0,
        ranges: (rangesRes.data.data || []).length,
        sensors: farms.reduce((sum, f) => sum + (f.sensorCount || 0), 0)
      });
      setRecentFarmers(usersRes.data.data?.items || []);
    }).finally(() => setLoading(false));
  }, []);

  if (loading) {
    return <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}><CircularProgress /></Box>;
  }

  return (
    <Stack spacing={3}>
      <Grid container spacing={2.5}>
        <Grid size={{ xs: 12, sm: 6, md: 4 }}>
          <StatCard label="Total Farms" count={stats.farms} icon={<BankOutlined />} color="primary"
            action={() => navigate('/farms')} actionLabel="View All Farms" />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 4 }}>
          <StatCard label="Registered Farmers" count={stats.users} icon={<TeamOutlined />} color="info"
            action={() => navigate('/admin/users')} actionLabel="Manage Users" />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 4 }}>
          <StatCard label="Validation Range Types" count={stats.ranges} icon={<SafetyOutlined />} color="warning"
            action={() => navigate('/admin/validation-ranges')} actionLabel="Manage Ranges" />
        </Grid>
      </Grid>

      <MainCard title="Recently Registered Farmers">
        {recentFarmers.length === 0 ? (
          <Typography color="text.secondary" sx={{ py: 2, textAlign: 'center' }}>No farmers registered yet.</Typography>
        ) : (
          <Stack divider={<Divider />} spacing={0}>
            {recentFarmers.map((user) => (
              <Stack
                key={user.id}
                direction={{ xs: 'column', sm: 'row' }}
                justifyContent="space-between"
                alignItems={{ xs: 'flex-start', sm: 'center' }}
                spacing={1}
                sx={{ py: 1.5 }}
              >
                <Stack direction="row" alignItems="center" spacing={1.5}>
                  <Box sx={{
                    width: 36, height: 36, borderRadius: '50%',
                    bgcolor: 'primary.lighter', color: 'primary.main',
                    display: 'flex', alignItems: 'center', justifyContent: 'center',
                    fontWeight: 600, fontSize: 14
                  }}>
                    {user.name?.charAt(0).toUpperCase()}
                  </Box>
                  <Stack>
                    <Typography variant="subtitle2">{user.name}</Typography>
                    <Typography variant="caption" color="text.secondary">{user.email}</Typography>
                  </Stack>
                </Stack>
                <Stack direction="row" alignItems="center" spacing={2}>
                  <Chip label={`${user.farmCount ?? 0} farm${user.farmCount !== 1 ? 's' : ''}`} size="small" variant="outlined" />
                  <Typography variant="caption" color="text.secondary">
                    {user.createdAt ? new Date(user.createdAt).toLocaleDateString() : '—'}
                  </Typography>
                </Stack>
              </Stack>
            ))}
          </Stack>
        )}
      </MainCard>
    </Stack>
  );
}

// ── Farmer dashboard ──────────────────────────────────────────────────────────
function FarmerDashboard() {
  const [farms, setFarms] = useState([]);
  const [selectedFarm, setSelectedFarm] = useState('');
  const [readings, setReadings] = useState(null);
  const [alerts, setAlerts] = useState([]);
  const [loading, setLoading] = useState(false);
  const [farmsLoading, setFarmsLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    farmsApi.getAll({ pageSize: 100 }).then((res) => {
      const items = res.data.data.items || [];
      setFarms(items);
      if (items.length > 0) setSelectedFarm(items[0].id);
    }).catch(() => setError('Failed to load farms')).finally(() => setFarmsLoading(false));
  }, []);

  const loadFarmData = useCallback(async (farmId) => {
    if (!farmId) return;
    setLoading(true);
    setError('');
    try {
      const [readingsRes, alertsRes] = await Promise.all([
        sensorsApi.getLatest(farmId),
        alertsApi.getAll({ farmId, pageSize: 5, sortBy: 'timestamp', sortOrder: 'desc' })
      ]);
      setReadings(readingsRes.data.data.readings || {});
      setAlerts(alertsRes.data.data.items || []);
    } catch {
      setError('Failed to load farm data');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { loadFarmData(selectedFarm); }, [selectedFarm, loadFarmData]);

  const r = readings || {};
  const temp     = r.temperature?.value?.temperature ?? null;
  const ph       = r.ph?.value?.ph ?? null;
  const moisture = r.moisture?.value?.moisture ?? null;
  const npkN     = r.npk?.value?.n ?? null;
  const npkP     = r.npk?.value?.p ?? null;
  const npkK     = r.npk?.value?.k ?? null;
  const rainfall = r.rainfall?.value?.rainfall ?? null;

  return (
    <Stack spacing={3}>
      <Stack direction={{ xs: 'column', sm: 'row' }} justifyContent="space-between" alignItems={{ xs: 'flex-start', sm: 'center' }} spacing={2}>
        <Stack direction="row" alignItems="center" spacing={1}>
          <DashboardOutlined style={{ fontSize: 24 }} />
          <Typography variant="h4">Dashboard</Typography>
        </Stack>
        <FormControl sx={{ minWidth: 240 }} size="small" disabled={farmsLoading}>
          <InputLabel>Select Farm</InputLabel>
          <Select value={selectedFarm} label="Select Farm" onChange={(e) => setSelectedFarm(e.target.value)}>
            {farms.map((f) => <MenuItem key={f.id} value={f.id}>{f.name}</MenuItem>)}
          </Select>
        </FormControl>
      </Stack>

      {error && <Alert severity="error" onClose={() => setError('')}>{error}</Alert>}

      {farms.length === 0 && !farmsLoading && (
        <Alert severity="info">
          No farms yet.{' '}
          <Typography component="a" href="/farms/create" sx={{ color: 'primary.main' }}>
            Create your first farm
          </Typography>{' '}
          to get started.
        </Alert>
      )}

      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}><CircularProgress /></Box>
      ) : (
        <>
          <Typography variant="h5">Latest Sensor Readings</Typography>
          <Grid container spacing={2.5}>
            <Grid size={{ xs: 12, sm: 6, md: 4, lg: 2.4 }}>
              <SensorCard label="Temperature" value={temp} unit="°C" icon={<ThunderboltOutlined />} color="error" />
            </Grid>
            <Grid size={{ xs: 12, sm: 6, md: 4, lg: 2.4 }}>
              <SensorCard label="Soil pH" value={ph} unit="" icon={<ExperimentOutlined />} color="warning" />
            </Grid>
            <Grid size={{ xs: 12, sm: 6, md: 4, lg: 2.4 }}>
              <SensorCard label="Soil Moisture" value={moisture} unit="%" icon={<DropboxOutlined />} color="info" />
            </Grid>
            <Grid size={{ xs: 12, sm: 6, md: 4, lg: 2.4 }}>
              <SensorCard
                label="NPK (N/P/K)"
                value={r.npk ? `${(npkN ?? 0).toFixed(0)}/${(npkP ?? 0).toFixed(0)}/${(npkK ?? 0).toFixed(0)}` : null}
                unit=""
                icon={<ExperimentOutlined />}
                color="success"
              />
            </Grid>
            <Grid size={{ xs: 12, sm: 6, md: 4, lg: 2.4 }}>
              <SensorCard label="Rainfall" value={rainfall} unit=" mm" icon={<CloudOutlined />} color="primary" />
            </Grid>
          </Grid>

          <MainCard
            title="Recent Alerts"
            secondary={
              <Chip
                label={`${alerts.length} alerts`}
                size="small"
                color={alerts.some((a) => a.severity === 'Critical') ? 'error' : 'default'}
              />
            }
          >
            {alerts.length === 0 ? (
              <Typography color="text.secondary" sx={{ py: 2, textAlign: 'center' }}>No recent alerts</Typography>
            ) : (
              <Stack divider={<Divider />} spacing={0}>
                {alerts.map((alert) => (
                  <Stack
                    key={alert.id}
                    direction={{ xs: 'column', sm: 'row' }}
                    justifyContent="space-between"
                    alignItems={{ xs: 'flex-start', sm: 'center' }}
                    spacing={1}
                    sx={{ py: 1.5 }}
                  >
                    <Stack direction="row" alignItems="center" spacing={1}>
                      <AlertOutlined style={{
                        color: alert.severity === 'Critical' ? '#ff4d4f'
                          : alert.severity === 'High' ? '#faad14' : '#1890ff'
                      }} />
                      <Stack>
                        <Typography variant="subtitle2">{alert.message}</Typography>
                        <Typography variant="caption" color="text.secondary">
                          {new Date(alert.timestamp).toLocaleString()}
                        </Typography>
                      </Stack>
                    </Stack>
                    <Chip label={alert.severity} size="small" color={SEVERITY_COLOR[alert.severity] || 'default'} />
                  </Stack>
                ))}
              </Stack>
            )}
          </MainCard>

          {selectedFarm && farms.length > 0 && (() => {
            const farm = farms.find((f) => f.id === selectedFarm);
            if (!farm) return null;
            return (
              <MainCard title="Farm Information">
                <Grid container spacing={2}>
                  <Grid size={{ xs: 12, sm: 6 }}>
                    <Typography variant="subtitle2" color="text.secondary">Farm Name</Typography>
                    <Typography variant="body1">{farm.name}</Typography>
                  </Grid>
                  <Grid size={{ xs: 12, sm: 6 }}>
                    <Typography variant="subtitle2" color="text.secondary">Crop Type</Typography>
                    <Typography variant="body1">{farm.cropType || 'Not specified'}</Typography>
                  </Grid>
                  <Grid size={{ xs: 12, sm: 6 }}>
                    <Typography variant="subtitle2" color="text.secondary">Location</Typography>
                    <Typography variant="body1">{farm.location?.formattedAddress}</Typography>
                  </Grid>
                  <Grid size={{ xs: 12, sm: 6 }}>
                    <Typography variant="subtitle2" color="text.secondary">Sensors</Typography>
                    <Typography variant="body1">{farm.sensorCount} sensors active</Typography>
                  </Grid>
                </Grid>
              </MainCard>
            );
          })()}
        </>
      )}
    </Stack>
  );
}

// ── Root export ───────────────────────────────────────────────────────────────
export default function Dashboard() {
  const { isAdmin } = useAuth();
  return isAdmin ? <AdminDashboard /> : <FarmerDashboard />;
}