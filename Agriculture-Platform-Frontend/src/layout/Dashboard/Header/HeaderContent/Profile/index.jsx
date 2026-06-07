import { useState, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import Avatar from '@mui/material/Avatar';
import Box from '@mui/material/Box';
import ButtonBase from '@mui/material/ButtonBase';
import ClickAwayListener from '@mui/material/ClickAwayListener';
import Divider from '@mui/material/Divider';
import List from '@mui/material/List';
import ListItemButton from '@mui/material/ListItemButton';
import ListItemIcon from '@mui/material/ListItemIcon';
import ListItemText from '@mui/material/ListItemText';
import Paper from '@mui/material/Paper';
import Popper from '@mui/material/Popper';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import Transitions from 'src/components/Transitions';
import { useAuth } from 'src/hooks/useAuth';
import { authApi } from 'src/api/auth';
import { UserOutlined, LogoutOutlined } from '@ant-design/icons';

export default function Profile() {
  const navigate = useNavigate();
  const { user, logout } = useAuth();
  const anchorRef = useRef(null);
  const [open, setOpen] = useState(false);

  const handleToggle = () => setOpen((prevOpen) => !prevOpen);
  const handleClose = (event) => {
    if (anchorRef.current && anchorRef.current.contains(event.target)) return;
    setOpen(false);
  };

  const handleLogout = async () => {
    try { await authApi.logout(); } catch { /* stateless logout */ }
    logout();
    navigate('/login');
  };

  const initials = user?.name ? user.name.split(' ').map((n) => n[0]).join('').slice(0, 2).toUpperCase() : 'U';

  return (
    <Box sx={{ flexShrink: 0, ml: 0.75 }}>
      <ButtonBase
        sx={(theme) => ({
          p: 0.25, bgcolor: open ? theme.vars.palette.grey[200] : 'transparent',
          borderRadius: 1, '&:hover': { bgcolor: theme.vars.palette.grey[100] }
        })}
        aria-label="open profile"
        ref={anchorRef}
        aria-controls={open ? 'profile-grow' : undefined}
        aria-haspopup="true"
        onClick={handleToggle}
      >
        <Stack direction="row" spacing={2} alignItems="center" sx={{ p: 0.5 }}>
          <Avatar sx={{ width: 32, height: 32, bgcolor: 'primary.main', fontSize: 14 }}>{initials}</Avatar>
          <Typography variant="subtitle1" sx={{ color: 'text.primary' }}>{user?.name}</Typography>
        </Stack>
      </ButtonBase>
      <Popper
        placement="bottom-end"
        open={open}
        anchorEl={anchorRef.current}
        role={undefined}
        transition
        disablePortal
        modifiers={[{ name: 'offset', options: { offset: [0, 9] } }]}
      >
        {({ TransitionProps }) => (
          <Transitions type="Grow" position="top-right" in={open} {...TransitionProps}>
            <Paper sx={(theme) => ({ boxShadow: theme.vars.customShadows?.z1, width: 290, minWidth: 240, maxWidth: 290 })}>
              <ClickAwayListener onClickAway={handleClose}>
                <Box sx={{ pt: 2, pb: 1 }}>
                  <Stack direction="row" spacing={1.25} sx={{ px: 2, pb: 1.5 }}>
                    <Avatar sx={{ width: 40, height: 40, bgcolor: 'primary.main' }}>{initials}</Avatar>
                    <Stack>
                      <Typography variant="h6">{user?.name}</Typography>
                      <Typography variant="body2" color="text.secondary">{user?.role}</Typography>
                    </Stack>
                  </Stack>
                  <Divider />
                  <List component="nav" sx={{ px: 1, py: 0, '& .MuiListItemIcon-root': { minWidth: 32 } }}>
                    <ListItemButton onClick={() => { navigate('/profile'); setOpen(false); }}>
                      <ListItemIcon><UserOutlined /></ListItemIcon>
                      <ListItemText primary="Profile" />
                    </ListItemButton>
                    <ListItemButton onClick={handleLogout}>
                      <ListItemIcon><LogoutOutlined /></ListItemIcon>
                      <ListItemText primary="Logout" />
                    </ListItemButton>
                  </List>
                </Box>
              </ClickAwayListener>
            </Paper>
          </Transitions>
        )}
      </Popper>
    </Box>
  );
}
