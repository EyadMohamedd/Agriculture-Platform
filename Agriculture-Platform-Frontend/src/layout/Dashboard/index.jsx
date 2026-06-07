import { useState } from 'react';
import { Outlet } from 'react-router-dom';
import Box from '@mui/material/Box';
import Toolbar from '@mui/material/Toolbar';
import useMediaQuery from '@mui/material/useMediaQuery';
import Header from './Header';
import MainDrawer from './Drawer';
import Footer from './Footer';
import { DRAWER_WIDTH, MINI_DRAWER_WIDTH } from 'src/config';

export default function DashboardLayout() {
  const matchDownLG = useMediaQuery((theme) => theme.breakpoints.down('lg'));
  const [open, setOpen] = useState(!matchDownLG);

  const handleDrawerToggle = () => setOpen((prev) => !prev);

  return (
    <Box sx={{ display: 'flex', width: '100%', minHeight: '100vh' }}>
      <Header open={open} handleDrawerToggle={handleDrawerToggle} />
      <MainDrawer open={open} handleDrawerToggle={handleDrawerToggle} />
      <Box
        component="main"
        sx={{
          width: `calc(100% - ${open ? DRAWER_WIDTH : MINI_DRAWER_WIDTH}px)`,
          flexGrow: 1,
          display: 'flex',
          flexDirection: 'column',
          transition: (theme) => theme.transitions.create(['width', 'margin'], {
            easing: theme.transitions.easing.sharp,
            duration: open ? theme.transitions.duration.enteringScreen : theme.transitions.duration.leavingScreen
          })
        }}
      >
        <Toolbar />
        <Box sx={{ flexGrow: 1, p: { xs: 2, sm: 3 } }}>
          <Outlet />
        </Box>
        <Footer />
      </Box>
    </Box>
  );
}
