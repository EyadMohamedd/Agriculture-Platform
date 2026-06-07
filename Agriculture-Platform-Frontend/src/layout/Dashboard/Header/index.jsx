import PropTypes from 'prop-types';
import Toolbar from '@mui/material/Toolbar';
import IconButton from '@mui/material/IconButton';
import { MenuFoldOutlined, MenuUnfoldOutlined } from '@ant-design/icons';
import AppBarStyled from './AppBarStyled';
import HeaderContent from './HeaderContent';

export default function Header({ open, handleDrawerToggle }) {
  return (
    <AppBarStyled position="fixed" open={open} elevation={0} sx={(theme) => ({ borderBottom: `1px solid ${theme.vars.palette.divider}`, bgcolor: 'background.paper' })}>
      <Toolbar>
        <IconButton
          disableRipple
          aria-label="open/close drawer"
          onClick={handleDrawerToggle}
          edge="start"
          sx={{ color: 'text.primary', mr: 2 }}
        >
          {open ? <MenuFoldOutlined /> : <MenuUnfoldOutlined />}
        </IconButton>
        <HeaderContent />
      </Toolbar>
    </AppBarStyled>
  );
}

Header.propTypes = { open: PropTypes.bool, handleDrawerToggle: PropTypes.func };
