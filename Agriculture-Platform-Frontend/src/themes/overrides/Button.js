export default function Button(theme) {
  const disabledStyle = { '&.Mui-disabled': { backgroundColor: theme.vars.palette.grey[200] } };
  return {
    MuiButton: {
      defaultProps: { disableElevation: true },
      styleOverrides: {
        root: { fontWeight: 500 },
        contained: { ...disabledStyle },
        outlined: {
          ...disabledStyle,
          '&.Mui-disabled': { backgroundColor: 'transparent' }
        }
      }
    }
  };
}