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

const passwordSchema = Yup.string()
  .min(8, 'Minimum 8 characters')
  .matches(/[a-zA-Z]/, 'Must contain at least one letter')
  .matches(/[0-9]/, 'Must contain at least one number')
  .required('Password is required');

export default function Register() {
  const navigate = useNavigate();
  const [showPassword, setShowPassword] = useState(false);
  const [serverError, setServerError] = useState('');

  const formik = useFormik({
    initialValues: { name: '', email: '', phone: '', password: '' },
    validationSchema: Yup.object({
      name: Yup.string().required('Name is required'),
      email: Yup.string().email('Invalid email format').required('Email is required'),
      phone: Yup.string().matches(/^\+?[0-9]{10,15}$/, 'Phone must be 10-15 digits').required('Phone is required'),
      password: passwordSchema
    }),
    onSubmit: async (values, { setSubmitting }) => {
      setServerError('');
      try {
        await authApi.register(values);
        navigate('/login', { state: { registered: true } });
      } catch (err) {
        setServerError(err.response?.data?.message || 'Registration failed. Please try again.');
      } finally {
        setSubmitting(false);
      }
    }
  });

  return (
    <>
      <Stack spacing={1} sx={{ mb: 3 }}>
        <Typography variant="h4">Create Account</Typography>
        <Stack direction="row" spacing={0.5} alignItems="center">
          <Typography color="text.secondary">Already have an account?</Typography>
          <Typography component={Link} to="/login" variant="body1" sx={{ color: 'primary.main', textDecoration: 'none', fontWeight: 500 }}>
            Sign In
          </Typography>
        </Stack>
      </Stack>

      {serverError && <Alert severity="error" sx={{ mb: 2 }}>{serverError}</Alert>}

      <Box component="form" onSubmit={formik.handleSubmit}>
        <Stack spacing={2}>
          <FormControl fullWidth error={formik.touched.name && Boolean(formik.errors.name)}>
            <InputLabel htmlFor="name">Full Name</InputLabel>
            <OutlinedInput id="name" label="Full Name" {...formik.getFieldProps('name')} />
            {formik.touched.name && formik.errors.name && <FormHelperText error>{formik.errors.name}</FormHelperText>}
          </FormControl>

          <FormControl fullWidth error={formik.touched.email && Boolean(formik.errors.email)}>
            <InputLabel htmlFor="email">Email Address</InputLabel>
            <OutlinedInput id="email" label="Email Address" type="email" {...formik.getFieldProps('email')} />
            {formik.touched.email && formik.errors.email && <FormHelperText error>{formik.errors.email}</FormHelperText>}
          </FormControl>

          <FormControl fullWidth error={formik.touched.phone && Boolean(formik.errors.phone)}>
            <InputLabel htmlFor="phone">Phone Number</InputLabel>
            <OutlinedInput id="phone" label="Phone Number" placeholder="+966501234567" {...formik.getFieldProps('phone')} />
            {formik.touched.phone && formik.errors.phone && <FormHelperText error>{formik.errors.phone}</FormHelperText>}
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
            {formik.touched.password && formik.errors.password && <FormHelperText error>{formik.errors.password}</FormHelperText>}
            <FormHelperText>Min 8 chars, at least 1 letter and 1 number</FormHelperText>
          </FormControl>

          <AnimateButton>
            <Button fullWidth size="large" type="submit" variant="contained" disabled={formik.isSubmitting}>
              {formik.isSubmitting ? 'Creating account...' : 'Create Account'}
            </Button>
          </AnimateButton>
        </Stack>
      </Box>
    </>
  );
}
