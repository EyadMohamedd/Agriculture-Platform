import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useAuth } from 'src/hooks/useAuth';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Chip from '@mui/material/Chip';
import CircularProgress from '@mui/material/CircularProgress';
import Grid from '@mui/material/Grid';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import { EditOutlined, LeftOutlined, SafetyOutlined, DashboardOutlined } from '@ant-design/icons';
import MainCard from 'src/components/MainCard';
import { farmsApi } from 'src/api/farms';

export default function FarmDetail() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { isAdmin } = useAuth();
  const [farm, setFarm] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    farmsApi.getAll({ pageSize: 100 }).then((res) => {
      const found = (res.data.data.items || []).find((f) => f.id === id);
      setFarm(found || null);
    }).finally(() => setLoading(false));
  }, [id]);

  if (loading) return <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}><CircularProgress /></Box>;
  if (!farm) return <Typography color="error">Farm not found.</Typography>;

  return (
    <Stack spacing={3}>
      <Stack direction="row" alignItems="center" spacing={2}>
        <Button startIcon={<LeftOutlined />} onClick={() => navigate('/farms')} variant="outlined" size="small">Back</Button>
        <Typography variant="h4">{farm.name}</Typography>
      </Stack>

      <MainCard title="Farm Details">
        <Grid container spacing={2.5}>
          <Grid size={{ xs: 12, sm: 6 }}>
            <Typography variant="subtitle2" color="text.secondary">Farm Name</Typography>
            <Typography variant="body1" sx={{ mt: 0.5 }}>{farm.name}</Typography>
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <Typography variant="subtitle2" color="text.secondary">Crop Type</Typography>
            <Box sx={{ mt: 0.5 }}>
              {farm.cropType
                ? <Chip label={farm.cropType} size="small" color="success" variant="outlined" />
                : <Typography variant="body2" color="text.secondary">Not specified</Typography>}
            </Box>
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <Typography variant="subtitle2" color="text.secondary">Formatted Address</Typography>
            <Typography variant="body1" sx={{ mt: 0.5 }}>{farm.location?.formattedAddress || '—'}</Typography>
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <Typography variant="subtitle2" color="text.secondary">Coordinates</Typography>
            <Typography variant="body1" sx={{ mt: 0.5 }}>
              {farm.location?.latitude != null ? `${farm.location.latitude}, ${farm.location.longitude}` : '—'}
            </Typography>
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <Typography variant="subtitle2" color="text.secondary">Active Sensors</Typography>
            <Typography variant="body1" sx={{ mt: 0.5 }}>{farm.sensorCount ?? 0} sensors</Typography>
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <Typography variant="subtitle2" color="text.secondary">Created</Typography>
            <Typography variant="body1" sx={{ mt: 0.5 }}>{new Date(farm.createdAt).toLocaleDateString()}</Typography>
          </Grid>
          <Grid size={{ xs: 12 }}>
            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} sx={{ mt: 1 }}>
              {!isAdmin && (
                <Button variant="contained" startIcon={<EditOutlined />} onClick={() => navigate(`/farms/${id}/edit`)}>
                  Edit Farm
                </Button>
              )}
              <Button variant="outlined" startIcon={<SafetyOutlined />} onClick={() => navigate(`/farms/${id}/validation-ranges`)}>
                Validation Ranges
              </Button>
              {!isAdmin && (
                <Button variant="outlined" startIcon={<DashboardOutlined />} onClick={() => navigate(`/dashboard?farm=${id}`)}>
                  View Dashboard
                </Button>
              )}
            </Stack>
          </Grid>
        </Grid>
      </MainCard>
    </Stack>
  );
}
