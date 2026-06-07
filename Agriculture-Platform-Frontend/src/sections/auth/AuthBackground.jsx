import Box from '@mui/material/Box';

export default function AuthBackground() {
  return (
    <Box
      sx={{
        position: 'fixed', zIndex: -1, bottom: 0, right: 0, width: '100%', height: '100%',
        background: (theme) => `linear-gradient(135deg, ${theme.vars.palette.primary.lighter} 0%, ${theme.vars.palette.background.default} 60%, ${theme.vars.palette.success.lighter} 100%)`
      }}
    />
  );
}
