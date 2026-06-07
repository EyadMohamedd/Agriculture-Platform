import PropTypes from 'prop-types';
import Box from '@mui/material/Box';
import Divider from '@mui/material/Divider';
import List from '@mui/material/List';
import Typography from '@mui/material/Typography';
import NavItem from './NavItem';

export default function NavGroup({ item, open }) {
  return (
    <List subheader={
      item.title && open ? (
        <Box sx={{ pl: 3, mb: 0.5, mt: 1 }}>
          <Typography variant="subtitle2" color="text.secondary" textTransform="uppercase" letterSpacing="0.5px">
            {item.title}
          </Typography>
        </Box>
      ) : (
        open ? null : <Divider sx={{ my: 0.5 }} />
      )
    }>
      {item.children?.map((menuItem) => (
        <NavItem key={menuItem.id} item={menuItem} open={open} />
      ))}
    </List>
  );
}

NavGroup.propTypes = { item: PropTypes.object, open: PropTypes.bool };
