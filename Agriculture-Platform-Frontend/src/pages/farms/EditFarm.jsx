import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useFormik } from 'formik';
import * as Yup from 'yup';
import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import CircularProgress from '@mui/material/CircularProgress';
import FormControl from '@mui/material/FormControl';
import FormHelperText from '@mui/material/FormHelperText';
import Grid from '@mui/material/Grid';
import InputLabel from '@mui/material/InputLabel';
import OutlinedInput from '@mui/material/OutlinedInput';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import MainCard from 'src/components/MainCard';
import LocationPicker from 'src/components/LocationPicker';
import { farmsApi } from 'src/api/farms';

export default function EditFarm() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [farm, setFarm] = useState(null);
  const [loading, setLoading] = useState(true);
  const [serverError, setServerError] = useState('');

  useEffect(() => {
    farmsApi.getAll({ pageSize: 100 }).then((res) => {
      const found = (res.data.data.items || []).find((f) => f.id === id);
      setFarm(found || null);
    }).finally(() => setLoading(false));
  }, [id]);

  const formik = useFormik({
    enableReinitialize: true,
    initialValues: {
      name: farm?.name || '',
      cropType: farm?.cropType || '',
      location: {
        latitude: farm?.location?.latitude ?? null,
        longitude: farm?.location?.longitude ?? null,
        formattedAddress: farm?.location?.formattedAddress || ''
      }
    },
    validationSchema: Yup.object({
      name: Yup.string().required('Farm name is required'),
      location: Yup.object({
        latitude: Yup.number().typeError('Must be a number').nullable().required('Please pick a location on the map'),
        longitude: Yup.number().typeError('Must be a number').nullable().required('Please pick a location on the map'),
        formattedAddress: Yup.string().required('Please pick a location on the map')
      })
    }),
    onSubmit: async (values, { setSubmitting }) => {
      setServerError('');
      const payload = {
        name: values.name,
        cropType: values.cropType || '',
        location: {
          latitude: values.location.latitude,
          longitude: values.location.longitude,
          formattedAddress: values.location.formattedAddress
        }
      };
      try {
        await farmsApi.update(id, payload);
        navigate('/farms');
      } catch (err) {
        setServerError(err.response?.data?.message || 'Failed to update farm');
      } finally {
        setSubmitting(false);
      }
    }
  });

  const locationError = Boolean(
    formik.touched.location &&
    (formik.errors.location?.latitude || formik.errors.location?.formattedAddress)
  );
  const locationHelperText = locationError
    ? (formik.errors.location?.latitude || formik.errors.location?.formattedAddress)
    : '';

  if (loading) return <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}><CircularProgress /></Box>;

  return (
    <Stack spacing={3}>
      <Typography variant="h4">Edit Farm</Typography>
      {serverError && <Alert severity="error">{serverError}</Alert>}

      <MainCard title="Farm Details">
        <Box component="form" onSubmit={formik.handleSubmit}>
          <Grid container spacing={3}>
            <Grid size={{ xs: 12, sm: 6 }}>
              <FormControl fullWidth error={formik.touched.name && Boolean(formik.errors.name)}>
                <InputLabel>Farm Name *</InputLabel>
                <OutlinedInput label="Farm Name *" {...formik.getFieldProps('name')} />
                {formik.touched.name && formik.errors.name && (
                  <FormHelperText error>{formik.errors.name}</FormHelperText>
                )}
              </FormControl>
            </Grid>

            <Grid size={{ xs: 12, sm: 6 }}>
              <FormControl fullWidth>
                <InputLabel>Crop Type</InputLabel>
                <OutlinedInput label="Crop Type" {...formik.getFieldProps('cropType')} />
              </FormControl>
            </Grid>

            <Grid size={{ xs: 12 }}>
              <Typography variant="subtitle1" sx={{ mb: 1 }}>
                Location — click the map to update
              </Typography>
              <LocationPicker
                value={formik.values.location}
                onChange={(loc) => {
                  formik.setFieldValue('location', loc);
                  formik.setFieldTouched('location', true, false);
                }}
                error={locationError}
                helperText={locationHelperText}
              />
            </Grid>

            <Grid size={{ xs: 12 }}>
              <Stack direction="row" spacing={2} justifyContent="flex-end">
                <Button variant="outlined" onClick={() => navigate('/farms')}>Cancel</Button>
                <Button type="submit" variant="contained" disabled={formik.isSubmitting}>
                  {formik.isSubmitting ? 'Saving...' : 'Save Changes'}
                </Button>
              </Stack>
            </Grid>
          </Grid>
        </Box>
      </MainCard>
    </Stack>
  );
}
