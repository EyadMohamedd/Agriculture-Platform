import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';

export default function Footer() {
  return (
    <Box sx={{ py: 1.5, px: 3, borderTop: (theme) => `1px solid ${theme.vars.palette.divider}`, display: 'flex', justifyContent: 'space-between' }}>
      <Typography variant="caption" color="text.secondary">
        &copy; {new Date().getFullYear()} AgriMonitor
      </Typography>
      <Typography variant="caption" color="text.secondary">
        Agricultural Monitoring System
      </Typography>
    </Box>
  );
}
