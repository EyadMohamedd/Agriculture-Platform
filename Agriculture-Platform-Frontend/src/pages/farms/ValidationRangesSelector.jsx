import { useEffect, useState } from 'react';
import { useFormik } from 'formik';
import * as Yup from 'yup';
import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import CircularProgress from '@mui/material/CircularProgress';
import Dialog from '@mui/material/Dialog';
import DialogActions from '@mui/material/DialogActions';
import DialogContent from '@mui/material/DialogContent';
import DialogTitle from '@mui/material/DialogTitle';
import FormControl from '@mui/material/FormControl';
import FormHelperText from '@mui/material/FormHelperText';
import Grid from '@mui/material/Grid';
import IconButton from '@mui/material/IconButton';
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
import TableRow from '@mui/material/TableRow';
import Tooltip from '@mui/material/Tooltip';
import Typography from '@mui/material/Typography';
import { PlusOutlined, EditOutlined, DeleteOutlined, SafetyOutlined } from '@ant-design/icons';
import MainCard from 'src/components/MainCard';
import { farmsApi } from 'src/api/farms';

const SENSOR_TYPES = [
  { value: 'temperature', label: 'Temperature (°C)' },
  { value: 'ph', label: 'Soil pH' },
  { value: 'moisture', label: 'Soil Moisture (%)' },
  { value: 'npk_n', label: 'NPK Nitrogen' },
  { value: 'npk_p', label: 'NPK Phosphorus' },
  { value: 'npk_k', label: 'NPK Potassium' },
  { value: 'rainfall', label: 'Rainfall (mm)' }
];

const rangeSchema = Yup.object({
  sensorType: Yup.string().required('Sensor type is required'),
  criticalLow: Yup.number().typeError('Must be a number').required('Required'),
  warningLow: Yup.number().typeError('Must be a number').required('Required')
    .min(Yup.ref('criticalLow'), 'Must be ≥ Critical Low'),
  minNormal: Yup.number().typeError('Must be a number').required('Required')
    .min(Yup.ref('warningLow'), 'Must be ≥ Warning Low'),
  maxNormal: Yup.number().typeError('Must be a number').required('Required')
    .min(Yup.ref('minNormal'), 'Must be ≥ Min Normal'),
  warningHigh: Yup.number().typeError('Must be a number').required('Required')
    .min(Yup.ref('maxNormal'), 'Must be ≥ Max Normal'),
  criticalHigh: Yup.number().typeError('Must be a number').required('Required')
    .min(Yup.ref('warningHigh'), 'Must be ≥ Warning High')
});

const RANGE_FIELDS = [
  { key: 'criticalLow', label: 'Critical Low' },
  { key: 'warningLow', label: 'Warning Low' },
  { key: 'minNormal', label: 'Min Normal' },
  { key: 'maxNormal', label: 'Max Normal' },
  { key: 'warningHigh', label: 'Warning High' },
  { key: 'criticalHigh', label: 'Critical High' }
];

function RangeDialog({ open, editing, farmId, onClose, onSaved }) {
  const [serverError, setServerError] = useState('');
  const formik = useFormik({
    enableReinitialize: true,
    initialValues: {
      sensorType: editing?.sensorType || '',
      criticalLow: editing?.criticalLow ?? '',
      warningLow: editing?.warningLow ?? '',
      minNormal: editing?.minNormal ?? '',
      maxNormal: editing?.maxNormal ?? '',
      warningHigh: editing?.warningHigh ?? '',
      criticalHigh: editing?.criticalHigh ?? ''
    },
    validationSchema: rangeSchema,
    onSubmit: async (values, { setSubmitting }) => {
      setServerError('');
      try {
        await farmsApi.upsertValidationRange(farmId, {
          sensorType: values.sensorType,
          criticalLow: parseFloat(values.criticalLow),
          warningLow: parseFloat(values.warningLow),
          minNormal: parseFloat(values.minNormal),
          maxNormal: parseFloat(values.maxNormal),
          warningHigh: parseFloat(values.warningHigh),
          criticalHigh: parseFloat(values.criticalHigh)
        });
        onSaved();
      } catch (err) {
        setServerError(err.response?.data?.message || 'Failed to save range');
      } finally {
        setSubmitting(false);
      }
    }
  });

  const handleClose = () => { formik.resetForm(); setServerError(''); onClose(); };

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
      <DialogTitle>{editing ? 'Edit Validation Range' : 'Add Validation Range'}</DialogTitle>
      <Box component="form" onSubmit={formik.handleSubmit}>
        <DialogContent>
          {serverError && <Alert severity="error" sx={{ mb: 2 }}>{serverError}</Alert>}
          <Grid container spacing={2}>
            <Grid size={{ xs: 12 }}>
              <FormControl fullWidth error={formik.touched.sensorType && Boolean(formik.errors.sensorType)}>
                <InputLabel>Sensor Type</InputLabel>
                <Select label="Sensor Type" {...formik.getFieldProps('sensorType')} disabled={Boolean(editing)}>
                  {SENSOR_TYPES.map((s) => <MenuItem key={s.value} value={s.value}>{s.label}</MenuItem>)}
                </Select>
                {formik.touched.sensorType && formik.errors.sensorType && <FormHelperText error>{formik.errors.sensorType}</FormHelperText>}
              </FormControl>
            </Grid>
            {RANGE_FIELDS.map(({ key, label }) => (
              <Grid key={key} size={{ xs: 12, sm: 6 }}>
                <FormControl fullWidth error={formik.touched[key] && Boolean(formik.errors[key])}>
                  <InputLabel>{label}</InputLabel>
                  <OutlinedInput label={label} {...formik.getFieldProps(key)} type="number" />
                  {formik.touched[key] && formik.errors[key] && <FormHelperText error>{formik.errors[key]}</FormHelperText>}
                </FormControl>
              </Grid>
            ))}
            <Grid size={{ xs: 12 }}>
              <Typography variant="caption" color="text.secondary">
                Ordering: Critical Low ≤ Warning Low ≤ Min Normal ≤ Max Normal ≤ Warning High ≤ Critical High
              </Typography>
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleClose}>Cancel</Button>
          <Button type="submit" variant="contained" disabled={formik.isSubmitting}>
            {formik.isSubmitting ? 'Saving...' : 'Save'}
          </Button>
        </DialogActions>
      </Box>
    </Dialog>
  );
}

const sensorLabel = (type) => SENSOR_TYPES.find((s) => s.value === type)?.label || type;

export default function ValidationRangesSelector() {
  const [farms, setFarms] = useState([]);
  const [selectedFarm, setSelectedFarm] = useState('');
  const [ranges, setRanges] = useState([]);
  const [farmsLoading, setFarmsLoading] = useState(true);
  const [rangesLoading, setRangesLoading] = useState(false);
  const [error, setError] = useState('');
  const [dialog, setDialog] = useState({ open: false, editing: null });
  const [deleteDialog, setDeleteDialog] = useState({ open: false, range: null });
  const [deleting, setDeleting] = useState(false);

  useEffect(() => {
    farmsApi.getAll({ pageSize: 100 }).then((res) => {
      const items = res.data.data.items || [];
      setFarms(items);
      if (items.length > 0) setSelectedFarm(items[0].id);
    }).catch(() => setError('Failed to load farms'))
      .finally(() => setFarmsLoading(false));
  }, []);

  const loadRanges = () => {
    if (!selectedFarm) return;
    setRangesLoading(true);
    farmsApi.getValidationRanges(selectedFarm)
      .then((res) => setRanges(res.data.data || []))
      .catch(() => setError('Failed to load validation ranges'))
      .finally(() => setRangesLoading(false));
  };

  useEffect(() => { if (selectedFarm) loadRanges(); }, [selectedFarm]);

  const handleSaved = () => { setDialog({ open: false, editing: null }); loadRanges(); };

  const handleDelete = async () => {
    if (!deleteDialog.range) return;
    setDeleting(true);
    try {
      await farmsApi.deleteValidationRange(selectedFarm, deleteDialog.range.id);
      setDeleteDialog({ open: false, range: null });
      loadRanges();
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to delete range');
    } finally {
      setDeleting(false);
    }
  };

  const farmName = farms.find((f) => f.id === selectedFarm)?.name || '';

  return (
    <Stack spacing={3}>
      <Stack direction="row" alignItems="center" spacing={1}>
        <SafetyOutlined style={{ fontSize: 24 }} />
        <Stack>
          <Typography variant="h4">Validation Ranges</Typography>
          <Typography variant="body2" color="text.secondary">Override system thresholds per farm</Typography>
        </Stack>
      </Stack>

      {error && <Alert severity="error" onClose={() => setError('')}>{error}</Alert>}

      <MainCard title="Select Farm">
        <FormControl size="small" sx={{ minWidth: 300 }} disabled={farmsLoading}>
          <InputLabel>Farm</InputLabel>
          <Select label="Farm" value={selectedFarm} onChange={(e) => setSelectedFarm(e.target.value)}>
            {farms.map((f) => <MenuItem key={f.id} value={f.id}>{f.name}</MenuItem>)}
          </Select>
        </FormControl>
      </MainCard>

      {selectedFarm && (
        <MainCard
          title={`Thresholds — ${farmName}`}
          secondary={
            <Button variant="contained" size="small" startIcon={<PlusOutlined />} onClick={() => setDialog({ open: true, editing: null })}>
              Add Range
            </Button>
          }
        >
          {rangesLoading ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}><CircularProgress /></Box>
          ) : ranges.length === 0 ? (
            <Box sx={{ py: 4, textAlign: 'center' }}>
              <Typography color="text.secondary">No custom ranges — system defaults apply.</Typography>
              <Button sx={{ mt: 2 }} variant="outlined" startIcon={<PlusOutlined />} onClick={() => setDialog({ open: true, editing: null })}>
                Add First Range
              </Button>
            </Box>
          ) : (
            <TableContainer>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Sensor Type</TableCell>
                    <TableCell align="right">Critical Low</TableCell>
                    <TableCell align="right">Warning Low</TableCell>
                    <TableCell align="right">Min Normal</TableCell>
                    <TableCell align="right">Max Normal</TableCell>
                    <TableCell align="right">Warning High</TableCell>
                    <TableCell align="right">Critical High</TableCell>
                    <TableCell align="center">Actions</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {ranges.map((range) => (
                    <TableRow key={range.id} hover>
                      <TableCell><Typography variant="subtitle2">{sensorLabel(range.sensorType)}</Typography></TableCell>
                      <TableCell align="right">{range.criticalLow}</TableCell>
                      <TableCell align="right">{range.warningLow}</TableCell>
                      <TableCell align="right">{range.minNormal}</TableCell>
                      <TableCell align="right">{range.maxNormal}</TableCell>
                      <TableCell align="right">{range.warningHigh}</TableCell>
                      <TableCell align="right">{range.criticalHigh}</TableCell>
                      <TableCell align="center">
                        <Stack direction="row" spacing={0.5} justifyContent="center">
                          <Tooltip title="Edit">
                            <IconButton size="small" onClick={() => setDialog({ open: true, editing: range })}><EditOutlined /></IconButton>
                          </Tooltip>
                          <Tooltip title="Delete">
                            <IconButton size="small" color="error" onClick={() => setDeleteDialog({ open: true, range })}><DeleteOutlined /></IconButton>
                          </Tooltip>
                        </Stack>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          )}
        </MainCard>
      )}

      <RangeDialog
        open={dialog.open}
        editing={dialog.editing}
        farmId={selectedFarm}
        onClose={() => setDialog({ open: false, editing: null })}
        onSaved={handleSaved}
      />

      <Dialog open={deleteDialog.open} onClose={() => setDeleteDialog({ open: false, range: null })}>
        <DialogTitle>Delete Validation Range</DialogTitle>
        <DialogContent>
          <Typography>
            Delete the {sensorLabel(deleteDialog.range?.sensorType)} range? System default will apply instead.
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteDialog({ open: false, range: null })}>Cancel</Button>
          <Button onClick={handleDelete} color="error" variant="contained" disabled={deleting}>
            {deleting ? 'Deleting...' : 'Delete'}
          </Button>
        </DialogActions>
      </Dialog>
    </Stack>
  );
}
