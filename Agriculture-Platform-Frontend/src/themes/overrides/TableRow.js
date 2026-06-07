export default function TableRow() {
  return {
    MuiTableRow: {
      styleOverrides: {
        root: { '&:last-of-type td, &:last-of-type th': { borderBottom: 0 } }
      }
    }
  };
}