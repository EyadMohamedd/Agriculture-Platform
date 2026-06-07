import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useFormik } from 'formik';
import * as Yup from 'yup';
import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import FormControl from '@mui/material/FormControl';
import FormHelperText from '@mui/material/FormHelperText';
import InputLabel from '@mui/material/InputLabel';
import OutlinedInput from '@mui/material/OutlinedInput';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import AnimateButton from 'src/components/AnimateButton';
import { authApi } from 'src/api/auth';

export default function ForgotPassword() {
  const navigate = useNavigate();
  const [serverError, setServerError] = useState('');

  const formik = useFormik({
    initialValues: { email: '', farmRegistrationNumber: '', username: '' },
    validationSchema: Yup.object({
      email: Yup.string().email('Invalid email format').required('Email is required'),
      farmRegistrationNumber: Yup.string().required('Phone number is required'),
      username: Yup.string().required('Username is required')
    }),
    onSubmit: async (values, { setSubmitting }) => {
      setServerError('');
      try {
        const res = await authApi.forgotPassword(values);
        const { resetToken } = res.data.data;
        navigate('/reset-password', { state: { resetToken } });
      } catch (err) {
        setServerError(err.response?.data?.message || 'Verification failed. Please check your answers.');
      } finally {
        setSubmitting(false);
      }
    }
  });

  return (
    <>
      <Stack spacing={1} sx={{ mb: 3 }}>
        <Typography variant="h4">Forgot Password</Typography>
        <Typography color="text.secondary" variant="body2">
          Answer your security questions to reset your password
        </Typography>
      </Stack>

      <Alert severity="info" sx={{ mb: 2 }}>
        <Typography variant="body2">
          <strong>Security Question 1:</strong> What is your farm&apos;s registration number? (your registered phone number)<br />
          <strong>Security Question 2:</strong> What is your username? (your registered name)
        </Typography>
      </Alert>

      {serverError && <Alert severity="error" sx={{ mb: 2 }}>{serverError}</Alert>}

      <Box component="form" onSubmit={formik.handleSubmit}>
        <Stack spacing={2}>
          <FormControl fullWidth error={formik.touched.email && Boolean(formik.errors.email)}>
            <InputLabel htmlFor="email">Email Address</InputLabel>
            <OutlinedInput id="email" label="Email Address" type="email" {...formik.getFieldProps('email')} />
            {formik.touched.email && formik.errors.email && <FormHelperText error>{formik.errors.email}</FormHelperText>}
          </FormControl>

          <FormControl fullWidth error={formik.touched.farmRegistrationNumber && Boolean(formik.errors.farmRegistrationNumber)}>
            <InputLabel htmlFor="farmRegistrationNumber">Farm Registration Number (Phone)</InputLabel>
            <OutlinedInput id="farmRegistrationNumber" label="Farm Registration Number (Phone)" {...formik.getFieldProps('farmRegistrationNumber')} />
            {formik.touched.farmRegistrationNumber && formik.errors.farmRegistrationNumber && (
              <FormHelperText error>{formik.errors.farmRegistrationNumber}</FormHelperText>
            )}
          </FormControl>

          <FormControl fullWidth error={formik.touched.username && Boolean(formik.errors.username)}>
            <InputLabel htmlFor="username">Username (Registered Name)</InputLabel>
            <OutlinedInput id="username" label="Username (Registered Name)" {...formik.getFieldProps('username')} />
            {formik.touched.username && formik.errors.username && <FormHelperText error>{formik.errors.username}</FormHelperText>}
          </FormControl>

          <AnimateButton>
            <Button fullWidth size="large" type="submit" variant="contained" disabled={formik.isSubmitting}>
              {formik.isSubmitting ? 'Verifying...' : 'Verify & Get Reset Token'}
            </Button>
          </AnimateButton>

          <Stack direction="row" justifyContent="center">
            <Typography component={Link} to="/login" variant="body2" sx={{ color: 'primary.main', textDecoration: 'none' }}>
              Back to Sign In
            </Typography>
          </Stack>
        </Stack>
      </Box>
    </>
  );
}
