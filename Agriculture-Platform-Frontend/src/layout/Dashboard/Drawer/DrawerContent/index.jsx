import PropTypes from 'prop-types';
import Navigation from './Navigation';

export default function DrawerContent({ open }) {
  return <Navigation open={open} />;
}

DrawerContent.propTypes = { open: PropTypes.bool };
