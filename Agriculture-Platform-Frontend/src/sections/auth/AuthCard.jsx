import PropTypes from 'prop-types';
import Box from '@mui/material/Box';
import MainCard from 'src/components/MainCard';

export default function AuthCard({ children }) {
  return (
    <MainCard
      sx={{ maxWidth: { xs: 400, lg: 475 }, margin: { xs: 2.5, md: 3 } }}
      content={false}
      border={false}
      boxShadow
    >
      <Box sx={{ p: { xs: 2, sm: 3, md: 4, xl: 5 } }}>
        {children}
      </Box>
    </MainCard>
  );
}

AuthCard.propTypes = { children: PropTypes.node };
