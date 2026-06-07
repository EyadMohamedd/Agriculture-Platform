import { useState } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { useFormik } from 'formik';
import * as Yup from 'yup';
import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import FormControl from '@mui/material/FormControl';
import FormHelperText from '@mui/material/FormHelperText';
import IconButton from '@mui/material/IconButton';
import InputAdornment from '@mui/material/InputAdornment';
import InputLabel from '@mui/material/InputLabel';
import OutlinedInput from '@mui/material/OutlinedInput';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import { EyeOutlined, EyeInvisibleOutlined } from '@ant-design/icons';
import AnimateButton from 'src/components/AnimateButton';
import { authApi } from 'src/api/auth';

export default function ResetPassword() {
  const navigate = useNavigate();
  const { state } = useLocation();
  const [showPassword, setShowPassword] = useState(false);
  const [serverError, setServerError] = useState('');

  const resetToken = state?.resetToken || '';

  const formik = useFormik({
    initialValues: { token: resetToken, newPassword: '' },
    validationSchema: Yup.object({
      token: Yup.string().required('Reset token is required'),
      newPassword: Yup.string()
        .min(8, 'Minimum 8 characters')
        .matches(/[a-zA-Z]/, 'Must contain at least one letter')
        .matches(/[0-9]/, 'Must contain at least one number')
        .required('New password is required')
    }),
    onSubmit: async (values, { setSubmitting }) => {
      setServerError('');
      try {
        await authApi.resetPassword(values);
        navigate('/login', { state: { passwordReset: true } });
      } catch (err) {
        setServerError(err.response?.data?.message || 'Password reset failed.');
      } finally {
        setSubmitting(false);
      }
    }
  });

  return (
    <>
      <Stack spacing={1} sx={{ mb: 3 }}>
        <Typography variant="h4">Reset Password</Typography>
        <Typography color="text.secondary" variant="body2">Enter your new password below</Typography>
      </Stack>

      {serverError && <Alert severity="error" sx={{ mb: 2 }}>{serverError}</Alert>}

      <Box component="form" onSubmit={formik.handleSubmit}>
        <Stack spacing={2}>
          <FormControl fullWidth error={formik.touched.token && Boolean(formik.errors.token)}>
            <InputLabel htmlFor="token">Reset Token</InputLabel>
            <OutlinedInput id="token" label="Reset Token" {...formik.getFieldProps('token')} />
            {formik.touched.token && formik.errors.token && <FormHelperText error>{formik.errors.token}</FormHelperText>}
          </FormControl>

          <FormControl fullWidth error={formik.touched.newPassword && Boolean(formik.errors.newPassword)}>
            <InputLabel htmlFor="newPassword">New Password</InputLabel>
            <OutlinedInput
              id="newPassword"
              label="New Password"
              type={showPassword ? 'text' : 'password'}
              {...formik.getFieldProps('newPassword')}
              endAdornment={
                <InputAdornment position="end">
                  <IconButton onClick={() => setShowPassword(!showPassword)} edge="end">
                    {showPassword ? <EyeOutlined /> : <EyeInvisibleOutlined />}
                  </IconButton>
                </InputAdornment>
              }
            />
            {formik.touched.newPassword && formik.errors.newPassword && <FormHelperText error>{formik.errors.newPassword}</FormHelperText>}
          </FormControl>

          <AnimateButton>
            <Button fullWidth size="large" type="submit" variant="contained" disabled={formik.isSubmitting}>
              {formik.isSubmitting ? 'Resetting...' : 'Reset Password'}
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
