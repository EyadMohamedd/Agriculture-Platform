import PropTypes from 'prop-types';
import Box from '@mui/material/Box';
import Logo from 'src/components/Logo';

export default function DrawerHeader({ open }) {
  return (
    <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: open ? 'flex-start' : 'center', px: 2, py: 1.5, minHeight: 60, overflow: 'hidden' }}>
      {open ? <Logo /> : null}
    </Box>
  );
}

DrawerHeader.propTypes = { open: PropTypes.bool };
