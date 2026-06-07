import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Chip from '@mui/material/Chip';
import CircularProgress from '@mui/material/CircularProgress';
import Dialog from '@mui/material/Dialog';
import DialogActions from '@mui/material/DialogActions';
import DialogContent from '@mui/material/DialogContent';
import DialogContentText from '@mui/material/DialogContentText';
import DialogTitle from '@mui/material/DialogTitle';
import IconButton from '@mui/material/IconButton';
import Stack from '@mui/material/Stack';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TablePagination from '@mui/material/TablePagination';
import TableRow from '@mui/material/TableRow';
import Tooltip from '@mui/material/Tooltip';
import Typography from '@mui/material/Typography';
import { PlusOutlined, EditOutlined, DeleteOutlined, EyeOutlined, SafetyOutlined } from '@ant-design/icons';
import MainCard from 'src/components/MainCard';
import { farmsApi } from 'src/api/farms';
import { adminApi } from 'src/api/admin';
import { useAuth } from 'src/hooks/useAuth';

export default function FarmsList() {
  const navigate = useNavigate();
  const { isFarmer, isAdmin } = useAuth();
  const [farms, setFarms] = useState([]);
  const [ownerMap, setOwnerMap] = useState({});
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [page, setPage] = useState(0);
  const [totalCount, setTotalCount] = useState(0);
  const [pageSize] = useState(20);
  const [deleteDialog, setDeleteDialog] = useState({ open: false, farm: null });
  const [deleting, setDeleting] = useState(false);

  const loadFarms = () => {
    setLoading(true);
    const farmsReq = farmsApi.getAll({ page: page + 1, pageSize });
    const usersReq = isAdmin ? adminApi.getUsers({ pageSize: 200 }) : Promise.resolve(null);
    Promise.all([farmsReq, usersReq])
      .then(([farmsRes, usersRes]) => {
        setFarms(farmsRes.data.data.items || []);
        setTotalCount(farmsRes.data.data.totalCount || 0);
        if (usersRes) {
          const map = {};
          (usersRes.data.data.items || []).forEach((u) => { map[u.id] = u.name; });
          setOwnerMap(map);
        }
      })
      .catch(() => setError('Failed to load farms'))
      .finally(() => setLoading(false));
  };

  useEffect(() => { loadFarms(); }, [page]);

  const handleDelete = async () => {
    if (!deleteDialog.farm) return;
    setDeleting(true);
    try {
      await farmsApi.delete(deleteDialog.farm.id);
      setDeleteDialog({ open: false, farm: null });
      loadFarms();
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to delete farm');
    } finally {
      setDeleting(false);
    }
  };

  return (
    <Stack spacing={3}>
      <Stack direction={{ xs: 'column', sm: 'row' }} justifyContent="space-between" alignItems={{ xs: 'flex-start', sm: 'center' }} spacing={2}>
        <Typography variant="h4">{isFarmer ? 'My Farms' : 'All Farms'}</Typography>
        {isFarmer && (
          <Button variant="contained" startIcon={<PlusOutlined />} onClick={() => navigate('/farms/create')}>
            Add Farm
          </Button>
        )}
      </Stack>

      {error && <Alert severity="error" onClose={() => setError('')}>{error}</Alert>}

      <MainCard content={false}>
        {loading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}><CircularProgress /></Box>
        ) : farms.length === 0 ? (
          <Box sx={{ py: 8, textAlign: 'center' }}>
            <Typography color="text.secondary">No farms found.</Typography>
            {isFarmer && <Button sx={{ mt: 2 }} variant="outlined" onClick={() => navigate('/farms/create')}>Create First Farm</Button>}
          </Box>
        ) : (
          <>
            <TableContainer>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Farm Name</TableCell>
                    {isAdmin && <TableCell>Owner</TableCell>}
                    <TableCell>Location</TableCell>
                    <TableCell>Crop Type</TableCell>
                    <TableCell align="center">Sensors</TableCell>
                    <TableCell>Created</TableCell>
                    <TableCell align="center">Actions</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {farms.map((farm) => (
                    <TableRow key={farm.id} hover>
                      <TableCell>
                        <Typography variant="subtitle2">{farm.name}</Typography>
                      </TableCell>
                      {isAdmin && (
                        <TableCell>
                          <Typography variant="body2">{ownerMap[farm.userId] || '—'}</Typography>
                        </TableCell>
                      )}
                      <TableCell>{farm.location?.formattedAddress || `${farm.location?.latitude}, ${farm.location?.longitude}`}</TableCell>
                      <TableCell>
                        {farm.cropType ? <Chip label={farm.cropType} size="small" color="success" variant="outlined" /> : <Typography variant="body2" color="text.secondary">—</Typography>}
                      </TableCell>
                      <TableCell align="center">{farm.sensorCount}</TableCell>
                      <TableCell>{new Date(farm.createdAt).toLocaleDateString()}</TableCell>
                      <TableCell align="center">
                        <Stack direction="row" spacing={0.5} justifyContent="center">
                          <Tooltip title={isAdmin ? 'View Details' : 'View Dashboard'}>
                            <IconButton size="small" onClick={() => navigate(isAdmin ? `/farms/${farm.id}` : `/dashboard?farm=${farm.id}`)}><EyeOutlined /></IconButton>
                          </Tooltip>
                          {isFarmer && (
                            <>
                              <Tooltip title="Edit Farm">
                                <IconButton size="small" onClick={() => navigate(`/farms/${farm.id}/edit`)}><EditOutlined /></IconButton>
                              </Tooltip>
                              <Tooltip title="Validation Ranges">
                                <IconButton size="small" onClick={() => navigate(`/farms/${farm.id}/validation-ranges`)}><SafetyOutlined /></IconButton>
                              </Tooltip>
                              <Tooltip title="Delete Farm">
                                <IconButton size="small" color="error" onClick={() => setDeleteDialog({ open: true, farm })}><DeleteOutlined /></IconButton>
                              </Tooltip>
                            </>
                          )}
                        </Stack>
                      </TableCell>
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

      <Dialog open={deleteDialog.open} onClose={() => setDeleteDialog({ open: false, farm: null })}>
        <DialogTitle>Delete Farm</DialogTitle>
        <DialogContent>
          <DialogContentText>
            Are you sure you want to delete <strong>{deleteDialog.farm?.name}</strong>?
            This will permanently delete all sensors, readings, alerts, and AI conversations for this farm.
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteDialog({ open: false, farm: null })}>Cancel</Button>
          <Button onClick={handleDelete} color="error" variant="contained" disabled={deleting}>
            {deleting ? 'Deleting...' : 'Delete'}
          </Button>
        </DialogActions>
      </Dialog>
    </Stack>
  );
}
