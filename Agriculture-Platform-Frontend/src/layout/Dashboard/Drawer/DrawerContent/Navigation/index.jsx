import PropTypes from 'prop-types';
import Box from '@mui/material/Box';
import NavGroup from './NavGroup';
import { useMenuItems } from 'src/menu-items';

export default function Navigation({ open }) {
  const items = useMenuItems();
  return (
    <Box sx={{ pt: 1 }}>
      {items.map((group) => <NavGroup key={group.id} item={group} open={open} />)}
    </Box>
  );
}

Navigation.propTypes = { open: PropTypes.bool };
