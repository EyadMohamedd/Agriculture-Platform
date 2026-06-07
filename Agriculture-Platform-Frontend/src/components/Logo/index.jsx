import { Link } from 'react-router-dom';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import { EnvironmentOutlined } from '@ant-design/icons';

export default function Logo({ to = '/' }) {
  return (
    <Box component={Link} to={to} sx={{ display: 'flex', alignItems: 'center', gap: 1, textDecoration: 'none' }}>
      <Box sx={{ color: 'primary.main', fontSize: 28, display: 'flex', alignItems: 'center' }}>
        <EnvironmentOutlined />
      </Box>
      <Typography variant="h5" sx={{ color: 'primary.main', fontWeight: 700, letterSpacing: 0.5 }}>
        AgriMonitor
      </Typography>
    </Box>
  );
}
