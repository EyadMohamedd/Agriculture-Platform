import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useFormik } from 'formik';
import * as Yup from 'yup';
import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
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

export default function CreateFarm() {
  const navigate = useNavigate();
  const [serverError, setServerError] = useState('');

  const formik = useFormik({
    initialValues: {
      name: '',
      cropType: '',
      location: { latitude: null, longitude: null, formattedAddress: '' }
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
        cropType: values.cropType || undefined,
        location: {
          latitude: values.location.latitude,
          longitude: values.location.longitude,
          formattedAddress: values.location.formattedAddress
        }
      };
      try {
        await farmsApi.create(payload);
        navigate('/farms');
      } catch (err) {
        setServerError(err.response?.data?.message || 'Failed to create farm');
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

  return (
    <Stack spacing={3}>
      <Typography variant="h4">Add New Farm</Typography>
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
                <InputLabel>Crop Type (optional)</InputLabel>
                <OutlinedInput label="Crop Type (optional)" {...formik.getFieldProps('cropType')} />
              </FormControl>
            </Grid>

            <Grid size={{ xs: 12 }}>
              <Typography variant="subtitle1" sx={{ mb: 1 }}>
                Location * — click on the map to select
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
                  {formik.isSubmitting ? 'Creating...' : 'Create Farm'}
                </Button>
              </Stack>
            </Grid>
          </Grid>
        </Box>
      </MainCard>
    </Stack>
  );
}
