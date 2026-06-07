import { useState, useEffect } from 'react';
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
import OutlinedInput from '@mui/material/OutlinedInput';
import Stack from '@mui/material/Stack';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import Tooltip from '@mui/material/Tooltip';
import Typography from '@mui/material/Typography';
import { EditOutlined, SafetyOutlined } from '@ant-design/icons';
import MainCard from 'src/components/MainCard';
import { adminApi } from 'src/api/admin';

const SENSOR_LABEL_MAP = {
  temperature: 'Temperature (°C)',
  ph: 'Soil pH',
  moisture: 'Soil Moisture (%)',
  npk_n: 'NPK Nitrogen',
  npk_p: 'NPK Phosphorus',
  npk_k: 'NPK Potassium',
  rainfall: 'Rainfall (mm)'
};

const sensorLabel = (type) => SENSOR_LABEL_MAP[type] || type;

const RANGE_FIELDS = [
  { key: 'criticalLow', label: 'Critical Low' },
  { key: 'warningLow', label: 'Warning Low' },
  { key: 'minNormal', label: 'Min Normal' },
  { key: 'maxNormal', label: 'Max Normal' },
  { key: 'warningHigh', label: 'Warning High' },
  { key: 'criticalHigh', label: 'Critical High' }
];

const rangeSchema = Yup.object({
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

function EditDialog({ open, range, onClose, onSaved }) {
  const [serverError, setServerError] = useState('');

  const formik = useFormik({
    enableReinitialize: true,
    initialValues: {
      criticalLow: range?.criticalLow ?? '',
      warningLow: range?.warningLow ?? '',
      minNormal: range?.minNormal ?? '',
      maxNormal: range?.maxNormal ?? '',
      warningHigh: range?.warningHigh ?? '',
      criticalHigh: range?.criticalHigh ?? ''
    },
    validationSchema: rangeSchema,
    onSubmit: async (values, { setSubmitting }) => {
      setServerError('');
      try {
        await adminApi.updateValidationRange(range.id, {
          criticalLow: parseFloat(values.criticalLow),
          warningLow: parseFloat(values.warningLow),
          minNormal: parseFloat(values.minNormal),
          maxNormal: parseFloat(values.maxNormal),
          warningHigh: parseFloat(values.warningHigh),
          criticalHigh: parseFloat(values.criticalHigh)
        });
        onSaved();
      } catch (err) {
        setServerError(err.response?.data?.message || 'Failed to update range');
      } finally {
        setSubmitting(false);
      }
    }
  });

  const handleClose = () => { formik.resetForm(); setServerError(''); onClose(); };

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
      <DialogTitle>Edit {sensorLabel(range?.sensorType)} Range</DialogTitle>
      <Box component="form" onSubmit={formik.handleSubmit}>
        <DialogContent>
          {serverError && <Alert severity="error" sx={{ mb: 2 }}>{serverError}</Alert>}
          <Grid container spacing={2}>
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

export default function AdminValidationRanges() {
  const [ranges, setRanges] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [editDialog, setEditDialog] = useState({ open: false, range: null });

  const load = () => {
    setLoading(true);
    adminApi.getValidationRanges()
      .then((res) => setRanges(res.data.data || []))
      .catch(() => setError('Failed to load validation ranges'))
      .finally(() => setLoading(false));
  };

  useEffect(() => { load(); }, []);

  const handleSaved = () => { setEditDialog({ open: false, range: null }); load(); };

  return (
    <Stack spacing={3}>
      <Stack direction="row" alignItems="center" spacing={1}>
        <SafetyOutlined style={{ fontSize: 24 }} />
        <Stack>
          <Typography variant="h4">System Validation Ranges</Typography>
          <Typography variant="body2" color="text.secondary">Default thresholds applied to all farms</Typography>
        </Stack>
      </Stack>

      {error && <Alert severity="error" onClose={() => setError('')}>{error}</Alert>}

      <MainCard content={false}>
        {loading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}><CircularProgress /></Box>
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
                      <Tooltip title="Edit Range">
                        <IconButton size="small" onClick={() => setEditDialog({ open: true, range })}>
                          <EditOutlined />
                        </IconButton>
                      </Tooltip>
                    </TableCell>
                  </TableRow>
                ))}
                {ranges.length === 0 && (
                  <TableRow>
                    <TableCell colSpan={8} align="center">
                      <Typography color="text.secondary" sx={{ py: 4 }}>No validation ranges configured.</Typography>
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
          </TableContainer>
        )}
      </MainCard>

      <EditDialog
        open={editDialog.open}
        range={editDialog.range}
        onClose={() => setEditDialog({ open: false, range: null })}
        onSaved={handleSaved}
      />
    </Stack>
  );
}
