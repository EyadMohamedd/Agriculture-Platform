export default function TableHead(theme) {
  return {
    MuiTableHead: {
      styleOverrides: {
        root: { backgroundColor: theme.vars.palette.grey.A50, borderTop: `1px solid ${theme.vars.palette.divider}`, borderBottom: `2px solid ${theme.vars.palette.divider}` }
      }
    }
  };
}