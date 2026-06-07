import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';

export default function AuthFooter() {
  return (
    <Stack direction="row" justifyContent="space-between" sx={{ px: 3, pb: 2 }}>
      <Typography variant="subtitle2" color="text.secondary">
        &copy; {new Date().getFullYear()} AgriMonitor
      </Typography>
      <Typography variant="subtitle2" color="text.secondary">
        Agricultural Monitoring System
      </Typography>
    </Stack>
  );
}
