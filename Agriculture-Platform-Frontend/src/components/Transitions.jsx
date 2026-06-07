import PropTypes from 'prop-types';
import { forwardRef } from 'react';
import Collapse from '@mui/material/Collapse';
import Fade from '@mui/material/Fade';
import Grow from '@mui/material/Grow';
import Slide from '@mui/material/Slide';
import Zoom from '@mui/material/Zoom';

const Transitions = forwardRef(({ children, type = 'Grow', position = 'top-left', in: inProp, ...others }, ref) => {
  let positionSX = { transformOrigin: '0 0 0' };
  if (position === 'top-right') positionSX = { transformOrigin: 'top right' };
  else if (position === 'top') positionSX = { transformOrigin: 'top' };
  else if (position === 'bottom-left') positionSX = { transformOrigin: 'bottom left' };
  else if (position === 'bottom-right') positionSX = { transformOrigin: 'bottom right' };
  else if (position === 'bottom') positionSX = { transformOrigin: 'bottom' };

  return (
    <div ref={ref}>
      {type === 'Grow' && (
        <Grow in={inProp} timeout={{ appear: 0, enter: 150, exit: 150 }} {...others}>
          <div style={positionSX}>{children}</div>
        </Grow>
      )}
      {type === 'Fade' && <Fade in={inProp} timeout={{ appear: 0, enter: 300, exit: 150 }} {...others}><div>{children}</div></Fade>}
      {type === 'Slide' && <Slide in={inProp} direction={position === 'top-right' ? 'left' : 'down'} timeout={{ appear: 0, enter: 150, exit: 150 }} {...others}><div>{children}</div></Slide>}
      {type === 'Zoom' && <Zoom in={inProp} timeout={{ appear: 0, enter: 150, exit: 150 }} {...others}><div>{children}</div></Zoom>}
      {type === 'Collapse' && <Collapse in={inProp} timeout={150} {...others}>{children}</Collapse>}
    </div>
  );
});

Transitions.displayName = 'Transitions';
Transitions.propTypes = {
  children: PropTypes.node, type: PropTypes.string, position: PropTypes.string, in: PropTypes.bool
};
export default Transitions;
