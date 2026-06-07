import PropTypes from 'prop-types';
import { Link, useLocation } from 'react-router-dom';
import Box from '@mui/material/Box';
import ListItemButton from '@mui/material/ListItemButton';
import ListItemIcon from '@mui/material/ListItemIcon';
import ListItemText from '@mui/material/ListItemText';
import Typography from '@mui/material/Typography';
import Tooltip from '@mui/material/Tooltip';

export default function NavItem({ item, open }) {
  const { pathname } = useLocation();
  const isSelected = pathname === item.url || pathname.startsWith(item.url + '/');

  const button = (
    <ListItemButton
      component={Link}
      to={item.url}
      selected={isSelected}
      sx={{
        mx: 1, my: 0.25, borderRadius: 1,
        pl: open ? 2 : 1.25,
        '&.Mui-selected': { bgcolor: 'primary.lighter', color: 'primary.main', '& .MuiListItemIcon-root': { color: 'primary.main' } },
        '&:hover': { bgcolor: 'primary.lighter', color: 'primary.main' }
      }}
    >
      {item.icon && (
        <ListItemIcon sx={{ minWidth: open ? 36 : 'auto', color: isSelected ? 'primary.main' : 'text.secondary', fontSize: 18 }}>
          {item.icon}
        </ListItemIcon>
      )}
      {open && <ListItemText primary={<Typography variant="body1" noWrap>{item.title}</Typography>} />}
    </ListItemButton>
  );

  return open ? button : <Tooltip title={item.title} placement="right">{button}</Tooltip>;
}

NavItem.propTypes = { item: PropTypes.object, open: PropTypes.bool };
