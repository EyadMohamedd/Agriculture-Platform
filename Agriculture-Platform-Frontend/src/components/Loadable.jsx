import { Suspense } from 'react';
import LinearProgress from '@mui/material/LinearProgress';
import Box from '@mui/material/Box';

const LoadingFallback = () => (
  <Box sx={{ position: 'fixed', top: 0, left: 0, zIndex: 1301, width: '100%' }}>
    <LinearProgress color="primary" />
  </Box>
);

export default function Loadable(Component) {
  return function WrappedComponent(props) {
    return (
      <Suspense fallback={<LoadingFallback />}>
        <Component {...props} />
      </Suspense>
    );
  };
}
