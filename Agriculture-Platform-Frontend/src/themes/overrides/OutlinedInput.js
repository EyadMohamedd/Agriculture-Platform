export default function OutlinedInput(theme) {
  return {
    MuiOutlinedInput: {
      styleOverrides: {
        root: {
          '& .MuiOutlinedInput-notchedOutline': { borderColor: theme.vars.palette.grey[300] },
          '&.Mui-focused': { boxShadow: theme.vars.customShadows?.primary },
          '&.Mui-error': { '&.Mui-focused': { boxShadow: theme.vars.customShadows?.error } },
          '&.Mui-disabled': { '& .MuiOutlinedInput-notchedOutline': { borderColor: theme.vars.palette.grey[200] } }
        },
        input: { padding: '10.5px 14px 10.5px 12px' },
        inputSizeSmall: { padding: '7.5px 8px 7.5px 12px' },
        inputMultiline: { padding: 0 },
        notchedOutline: { borderRadius: 4 }
      }
    }
  };
}