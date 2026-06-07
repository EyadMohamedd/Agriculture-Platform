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
import DialogContentText from '@mui/material/DialogContentText';
import DialogTitle from '@mui/material/DialogTitle';
import FormControl from '@mui/material/FormControl';
import FormHelperText from '@mui/material/FormHelperText';
import Grid from '@mui/material/Grid';
import IconButton from '@mui/material/IconButton';
import InputAdornment from '@mui/material/InputAdornment';
import InputLabel from '@mui/material/InputLabel';
import OutlinedInput from '@mui/material/OutlinedInput';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import { EyeOutlined, EyeInvisibleOutlined, UserOutlined } from '@ant-design/icons';
import MainCard from 'src/components/MainCard';
import { usersApi } from 'src/api/users';
import { authApi } from 'src/api/auth';
import { useAuth } from 'src/hooks/useAuth';

function ProfileForm({ profile, onUpdated }) {
  const { updateUser } = useAuth();
  const [serverError, setServerError] = useState('');
  const [success, setSuccess] = useState('');

  const formik = useFormik({
    enableReinitialize: true,
    initialValues: { name: profile?.name || '', phone: profile?.phone || '' },
    validationSchema: Yup.object({
      name: Yup.string().required('Name is required'),
      phone: Yup.string().matches(/^\d{10,15}$/, 'Phone must be 10-15 digits').required('Phone is required')
    }),
    onSubmit: async (values, { setSubmitting }) => {
      setServerError(''); setSuccess('');
      try {
        const res = await usersApi.updateProfile(values);
        updateUser(res.data.data || res.data);
        setSuccess('Profile updated successfully');
        onUpdated();
      } catch (err) {
        setServerError(err.response?.data?.message || 'Failed to update profile');
      } finally {
        setSubmitting(false);
      }
    }
  });

  return (
    <Box component="form" onSubmit={formik.handleSubmit}>
      {serverError && <Alert severity="error" sx={{ mb: 2 }}>{serverError}</Alert>}
      {success && <Alert severity="success" sx={{ mb: 2 }}>{success}</Alert>}
      <Grid container spacing={2.5}>
        <Grid size={{ xs: 12, sm: 6 }}>
          <FormControl fullWidth error={formik.touched.name && Boolean(formik.errors.name)}>
            <InputLabel>Full Name</InputLabel>
            <OutlinedInput label="Full Name" {...formik.getFieldProps('name')} />
            {formik.touched.name && formik.errors.name && <FormHelperText error>{formik.errors.name}</FormHelperText>}
          </FormControl>
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <FormControl fullWidth error={formik.touched.phone && Boolean(formik.errors.phone)}>
            <InputLabel>Phone</InputLabel>
            <OutlinedInput label="Phone" {...formik.getFieldProps('phone')} />
            {formik.touched.phone && formik.errors.phone && <FormHelperText error>{formik.errors.phone}</FormHelperText>}
          </FormControl>
        </Grid>
        <Grid size={{ xs: 12 }}>
          <Button type="submit" variant="contained" disabled={formik.isSubmitting}>
            {formik.isSubmitting ? 'Saving...' : 'Save Changes'}
          </Button>
        </Grid>
      </Grid>
    </Box>
  );
}

function ChangePasswordForm() {
  const [showCurrent, setShowCurrent] = useState(false);
  const [showNew, setShowNew] = useState(false);
  const [serverError, setServerError] = useState('');
  const [success, setSuccess] = useState('');

  const formik = useFormik({
    initialValues: { currentPassword: '', newPassword: '' },
    validationSchema: Yup.object({
      currentPassword: Yup.string().required('Current password is required'),
      newPassword: Yup.string()
        .min(8, 'Minimum 8 characters')
        .matches(/[a-zA-Z]/, 'Must contain at least one letter')
        .matches(/[0-9]/, 'Must contain at least one number')
        .required('New password is required')
    }),
    onSubmit: async (values, { setSubmitting, resetForm }) => {
      setServerError(''); setSuccess('');
      try {
        await usersApi.changePassword(values);
        setSuccess('Password changed successfully');
        resetForm();
      } catch (err) {
        setServerError(err.response?.data?.message || 'Failed to change password');
      } finally {
        setSubmitting(false);
      }
    }
  });

  return (
    <Box component="form" onSubmit={formik.handleSubmit}>
      {serverError && <Alert severity="error" sx={{ mb: 2 }}>{serverError}</Alert>}
      {success && <Alert severity="success" sx={{ mb: 2 }}>{success}</Alert>}
      <Grid container spacing={2.5}>
        {[
          { field: 'currentPassword', label: 'Current Password', show: showCurrent, setShow: setShowCurrent },
          { field: 'newPassword', label: 'New Password', show: showNew, setShow: setShowNew }
        ].map(({ field, label, show, setShow }) => (
          <Grid key={field} size={{ xs: 12, sm: 6 }}>
            <FormControl fullWidth error={formik.touched[field] && Boolean(formik.errors[field])}>
              <InputLabel>{label}</InputLabel>
              <OutlinedInput
                label={label}
                type={show ? 'text' : 'password'}
                {...formik.getFieldProps(field)}
                endAdornment={
                  <InputAdornment position="end">
                    <IconButton onClick={() => setShow(!show)} edge="end">
                      {show ? <EyeOutlined /> : <EyeInvisibleOutlined />}
                    </IconButton>
                  </InputAdornment>
                }
              />
              {formik.touched[field] && formik.errors[field] && <FormHelperText error>{formik.errors[field]}</FormHelperText>}
            </FormControl>
          </Grid>
        ))}
        <Grid size={{ xs: 12 }}>
          <Button type="submit" variant="contained" disabled={formik.isSubmitting}>
            {formik.isSubmitting ? 'Changing...' : 'Change Password'}
          </Button>
        </Grid>
      </Grid>
    </Box>
  );
}

export default function Profile() {
  const { logout } = useAuth();
  const [profile, setProfile] = useState(null);
  const [loading, setLoading] = useState(true);
  const [deleteOpen, setDeleteOpen] = useState(false);
  const [deleting, setDeleting] = useState(false);
  const [deleteError, setDeleteError] = useState('');

  const loadProfile = () => {
    usersApi.getProfile().then((res) => setProfile(res.data.data || res.data))
      .catch(() => {}).finally(() => setLoading(false));
  };

  useEffect(() => { loadProfile(); }, []);

  const handleDeleteAccount = async () => {
    setDeleting(true);
    setDeleteError('');
    try {
      await authApi.deleteAccount();
      logout();
    } catch (err) {
      setDeleteError(err.response?.data?.message || 'Failed to delete account');
      setDeleting(false);
    }
  };

  if (loading) return <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}><CircularProgress /></Box>;

  return (
    <Stack spacing={3}>
      <Stack direction="row" alignItems="center" spacing={1}>
        <UserOutlined style={{ fontSize: 24 }} />
        <Typography variant="h4">Profile</Typography>
      </Stack>

      <MainCard title="Account Info">
        <Grid container spacing={2}>
          <Grid size={{ xs: 12, sm: 6 }}>
            <Typography variant="subtitle2" color="text.secondary">Email</Typography>
            <Typography variant="body1" sx={{ mt: 0.5 }}>{profile?.email || '—'}</Typography>
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <Typography variant="subtitle2" color="text.secondary">Role</Typography>
            <Typography variant="body1" sx={{ mt: 0.5 }}>{profile?.role || '—'}</Typography>
          </Grid>
        </Grid>
      </MainCard>

      <MainCard title="Edit Profile">
        <ProfileForm profile={profile} onUpdated={loadProfile} />
      </MainCard>

      <MainCard title="Change Password">
        <ChangePasswordForm />
      </MainCard>

      <MainCard title="Danger Zone">
        <Stack spacing={1}>
          <Typography variant="body2" color="text.secondary">
            Permanently delete your account and all associated data. This action cannot be undone.
          </Typography>
          <Box>
            <Button variant="outlined" color="error" onClick={() => setDeleteOpen(true)}>
              Delete Account
            </Button>
          </Box>
        </Stack>
      </MainCard>

      <Dialog open={deleteOpen} onClose={() => setDeleteOpen(false)}>
        <DialogTitle>Delete Account</DialogTitle>
        <DialogContent>
          {deleteError && <Alert severity="error" sx={{ mb: 2 }}>{deleteError}</Alert>}
          <DialogContentText>
            Are you sure you want to permanently delete your account? All your farms, sensors, and data will be removed.
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteOpen(false)}>Cancel</Button>
          <Button onClick={handleDeleteAccount} color="error" variant="contained" disabled={deleting}>
            {deleting ? 'Deleting...' : 'Delete My Account'}
          </Button>
        </DialogActions>
      </Dialog>
    </Stack>
  );
}
