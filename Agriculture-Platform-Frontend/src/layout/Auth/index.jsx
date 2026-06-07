import { Outlet } from 'react-router-dom';
import Box from '@mui/material/Box';
import Grid from '@mui/material/Grid';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import AuthCard from 'src/sections/auth/AuthCard';
import AuthBackground from 'src/sections/auth/AuthBackground';
import AuthFooter from 'src/components/cards/AuthFooter';
import Logo from 'src/components/Logo';

export default function AuthLayout() {
  return (
    <Box sx={{ minHeight: '100vh' }}>
      <AuthBackground />
      <Stack sx={{ minHeight: '100vh', justifyContent: 'flex-end' }}>
        <Box sx={{ px: 3, mt: 3 }}>
          <Logo to="/" />
        </Box>
        <Box>
          <Grid
            container
            justifyContent="center"
            alignItems="center"
            sx={{ minHeight: { xs: 'calc(100vh - 210px)', sm: 'calc(100vh - 134px)', md: 'calc(100vh - 132px)' } }}
          >
            <Grid>
              <AuthCard>
                <Outlet />
              </AuthCard>
            </Grid>
          </Grid>
        </Box>
        <AuthFooter />
      </Stack>
    </Box>
  );
}
