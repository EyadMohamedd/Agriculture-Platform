import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
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
import { useAuth } from 'src/hooks/useAuth';

export default function Login() {
  const navigate = useNavigate();
  const { login } = useAuth();
  const [showPassword, setShowPassword] = useState(false);
  const [serverError, setServerError] = useState('');

  const formik = useFormik({
    initialValues: { email: '', password: '' },
    validationSchema: Yup.object({
      email: Yup.string().email('Invalid email format').required('Email is required'),
      password: Yup.string().required('Password is required')
    }),
    onSubmit: async (values, { setSubmitting }) => {
      setServerError('');
      try {
        const res = await authApi.login(values);
        const { token, userId, name, email, role, expiresAt } = res.data.data;
        login(token, { id: userId, name, email, role, expiresAt });
        navigate('/dashboard');
      } catch (err) {
        setServerError(err.response?.data?.message || 'Login failed. Please try again.');
      } finally {
        setSubmitting(false);
      }
    }
  });

  return (
    <>
      <Stack spacing={1} sx={{ mb: 3 }}>
        <Typography variant="h4">Sign In</Typography>
        <Stack direction="row" spacing={0.5} alignItems="center">
          <Typography color="text.secondary">Don&apos;t have an account?</Typography>
          <Typography component={Link} to="/register" variant="body1" sx={{ color: 'primary.main', textDecoration: 'none', fontWeight: 500 }}>
            Create Account
          </Typography>
        </Stack>
      </Stack>

      {serverError && <Alert severity="error" sx={{ mb: 2 }}>{serverError}</Alert>}

      <Box component="form" onSubmit={formik.handleSubmit}>
        <Stack spacing={2}>
          <FormControl fullWidth error={formik.touched.email && Boolean(formik.errors.email)}>
            <InputLabel htmlFor="email">Email Address</InputLabel>
            <OutlinedInput
              id="email"
              label="Email Address"
              type="email"
              {...formik.getFieldProps('email')}
            />
            {formik.touched.email && formik.errors.email && (
              <FormHelperText error>{formik.errors.email}</FormHelperText>
            )}
          </FormControl>

          <FormControl fullWidth error={formik.touched.password && Boolean(formik.errors.password)}>
            <InputLabel htmlFor="password">Password</InputLabel>
            <OutlinedInput
              id="password"
              label="Password"
              type={showPassword ? 'text' : 'password'}
              {...formik.getFieldProps('password')}
              endAdornment={
                <InputAdornment position="end">
                  <IconButton onClick={() => setShowPassword(!showPassword)} edge="end">
                    {showPassword ? <EyeOutlined /> : <EyeInvisibleOutlined />}
                  </IconButton>
                </InputAdornment>
              }
            />
            {formik.touched.password && formik.errors.password && (
              <FormHelperText error>{formik.errors.password}</FormHelperText>
            )}
          </FormControl>

          <Stack direction="row" justifyContent="flex-end">
            <Typography
              component={Link}
              to="/forgot-password"
              variant="body2"
              sx={{ color: 'primary.main', textDecoration: 'none' }}
            >
              Forgot Password?
            </Typography>
          </Stack>

          <AnimateButton>
            <Button
              fullWidth
              size="large"
              type="submit"
              variant="contained"
              disabled={formik.isSubmitting}
            >
              {formik.isSubmitting ? 'Signing in...' : 'Sign In'}
            </Button>
          </AnimateButton>
        </Stack>
      </Box>
    </>
  );
}
