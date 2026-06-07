export default function TableCell(theme) {
  return {
    MuiTableCell: {
      styleOverrides: {
        root: { fontSize: '0.875rem', padding: '12px 8px', borderColor: theme.vars.palette.divider },
        head: { fontWeight: 600, paddingTop: 20, paddingBottom: 20 }
      }
    }
  };
}