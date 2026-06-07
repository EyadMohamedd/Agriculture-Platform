import PropTypes from 'prop-types';
import Box from '@mui/material/Box';
import Chip from '@mui/material/Chip';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import { RiseOutlined, FallOutlined } from '@ant-design/icons';
import MainCard from 'src/components/MainCard';

export default function AnalyticCard({ color = 'primary', title, count, percentage, isLoss, icon, extra }) {
  return (
    <MainCard contentSX={{ p: 2.25 }}>
      <Stack spacing={0.5}>
        <Typography variant="h6" color="text.secondary">{title}</Typography>
        <Stack direction="row" alignItems="center" justifyContent="space-between">
          <Typography variant="h4" color="inherit">{count}</Typography>
          {icon && (
            <Box sx={{ color: `${color}.main`, fontSize: 32 }}>{icon}</Box>
          )}
        </Stack>
        {(percentage !== undefined || extra) && (
          <Stack direction="row" alignItems="center" spacing={1} flexWrap="wrap">
            {percentage !== undefined && (
              <Chip
                variant="combined"
                color={isLoss ? 'error' : 'success'}
                icon={isLoss ? <FallOutlined style={{ fontSize: 16, color: 'inherit' }} /> : <RiseOutlined style={{ fontSize: 16, color: 'inherit' }} />}
                label={`${percentage}%`}
                sx={{ pl: 0.5 }}
                size="small"
              />
            )}
            {extra && (
              <Typography variant="caption" color="text.secondary">{extra}</Typography>
            )}
          </Stack>
        )}
      </Stack>
    </MainCard>
  );
}

AnalyticCard.propTypes = {
  color: PropTypes.string, title: PropTypes.string, count: PropTypes.oneOfType([PropTypes.string, PropTypes.number]),
  percentage: PropTypes.number, isLoss: PropTypes.bool, icon: PropTypes.node, extra: PropTypes.string
};
