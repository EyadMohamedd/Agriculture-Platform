import PropTypes from 'prop-types';

export default function AnimateButton({ children, type = 'scale' }) {
  if (type === 'scale') {
    return (
      <div style={{ display: 'inline-flex', transition: 'transform 0.2s', cursor: 'pointer' }}>
        {children}
      </div>
    );
  }
  return <>{children}</>;
}

AnimateButton.propTypes = { children: PropTypes.node, type: PropTypes.string };
