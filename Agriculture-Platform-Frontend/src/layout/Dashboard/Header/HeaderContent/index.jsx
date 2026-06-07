import Box from '@mui/material/Box';
import IconButton from '@mui/material/IconButton';
import Tooltip from '@mui/material/Tooltip';
import { BellOutlined } from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import { useAuth } from 'src/hooks/useAuth';
import Profile from './Profile';

export default function HeaderContent() {
  const navigate = useNavigate();
  const { isAdmin } = useAuth();
  return (
    <>
      <Box sx={{ width: '100%' }} />
      {!isAdmin && (
        <Tooltip title="Alerts">
          <IconButton onClick={() => navigate('/alerts')} size="large" sx={{ color: 'text.primary' }} aria-label="alerts">
            <BellOutlined />
          </IconButton>
        </Tooltip>
      )}
      <Profile />
    </>
  );
}
