export default function Chip(theme) {
  return {
    MuiChip: {
      styleOverrides: {
        root: { borderRadius: 4, '&:focus': { boxShadow: 'none' } },
        sizeLarge: { fontSize: '1rem', height: 40 },
        light: {
          '&.MuiChip-colorDefault': { backgroundColor: theme.vars.palette.secondary.lighter, color: theme.vars.palette.secondary.dark },
          '&.MuiChip-colorPrimary': { backgroundColor: theme.vars.palette.primary.lighter, color: theme.vars.palette.primary.dark },
          '&.MuiChip-colorSuccess': { backgroundColor: theme.vars.palette.success.lighter, color: theme.vars.palette.success.dark },
          '&.MuiChip-colorError': { backgroundColor: theme.vars.palette.error.lighter, color: theme.vars.palette.error.dark },
          '&.MuiChip-colorWarning': { backgroundColor: theme.vars.palette.warning.lighter, color: theme.vars.palette.warning.dark }
        }
      }
    }
  };
}