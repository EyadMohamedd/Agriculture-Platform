import { useState, useEffect } from 'react';
import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import CircularProgress from '@mui/material/CircularProgress';
import Collapse from '@mui/material/Collapse';
import Divider from '@mui/material/Divider';
import FormControl from '@mui/material/FormControl';
import IconButton from '@mui/material/IconButton';
import InputLabel from '@mui/material/InputLabel';
import MenuItem from '@mui/material/MenuItem';
import Paper from '@mui/material/Paper';
import Select from '@mui/material/Select';
import Stack from '@mui/material/Stack';
import TablePagination from '@mui/material/TablePagination';
import Typography from '@mui/material/Typography';
import { DownOutlined, UpOutlined, HistoryOutlined } from '@ant-design/icons';
import MainCard from 'src/components/MainCard';
import { farmsApi } from 'src/api/farms';
import { aiApi } from 'src/api/ai';

function ConversationItem({ conversation }) {
  const [open, setOpen] = useState(false);

  return (
    <Paper variant="outlined" sx={{ overflow: 'hidden' }}>
      <Stack
        direction="row"
        alignItems="center"
        justifyContent="space-between"
        sx={{ p: 2, cursor: 'pointer', '&:hover': { bgcolor: 'action.hover' } }}
        onClick={() => setOpen(!open)}
      >
        <Stack spacing={0.5} sx={{ flex: 1, mr: 2 }}>
          <Typography variant="subtitle2" noWrap>{conversation.question}</Typography>
          <Typography variant="caption" color="text.secondary">
            {new Date(conversation.createdAt || conversation.timestamp).toLocaleString()}
          </Typography>
        </Stack>
        <IconButton size="small">{open ? <UpOutlined /> : <DownOutlined />}</IconButton>
      </Stack>
      <Collapse in={open}>
        <Divider />
        <Box sx={{ p: 2 }}>
          <Typography variant="subtitle2" color="text.secondary" gutterBottom>Question</Typography>
          <Typography variant="body2" sx={{ mb: 2 }}>{conversation.question}</Typography>
          <Typography variant="subtitle2" color="text.secondary" gutterBottom>Answer</Typography>
          <Typography variant="body2" sx={{ whiteSpace: 'pre-wrap', lineHeight: 1.8 }}>{conversation.answer}</Typography>
        </Box>
      </Collapse>
    </Paper>
  );
}

export default function AiHistory() {
  const [farms, setFarms] = useState([]);
  const [selectedFarm, setSelectedFarm] = useState('');
  const [allConversations, setAllConversations] = useState([]);
  const [page, setPage] = useState(0);
  const [pageSize] = useState(10);
  const [loading, setLoading] = useState(false);
  const [farmsLoading, setFarmsLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    farmsApi.getAll({ pageSize: 100 }).then((res) => {
      setFarms(res.data.data.items || []);
    }).catch(() => {}).finally(() => setFarmsLoading(false));
  }, []);

  useEffect(() => {
    setLoading(true);
    setError('');
    aiApi.getConversations({ page: 1, pageSize: 200 })
      .then((res) => setAllConversations(res.data.data.items || []))
      .catch(() => setError('Failed to load conversation history'))
      .finally(() => setLoading(false));
  }, []);

  const filtered = selectedFarm
    ? allConversations.filter((c) => c.farmId === selectedFarm)
    : allConversations;

  const totalCount = filtered.length;
  const conversations = filtered.slice(page * pageSize, page * pageSize + pageSize);

  return (
    <Stack spacing={3}>
      <Stack direction="row" alignItems="center" spacing={1}>
        <HistoryOutlined style={{ fontSize: 24 }} />
        <Typography variant="h4">Conversation History</Typography>
      </Stack>

      {error && <Alert severity="error" onClose={() => setError('')}>{error}</Alert>}

      <MainCard title="Filter by Farm">
        <FormControl size="small" sx={{ minWidth: 280 }} disabled={farmsLoading}>
          <InputLabel shrink>Farm</InputLabel>
          <Select label="Farm" value={selectedFarm} onChange={(e) => { setSelectedFarm(e.target.value); setPage(0); }} displayEmpty notched>
            <MenuItem value="">All Farms</MenuItem>
            {farms.map((f) => <MenuItem key={f.id} value={f.id}>{f.name}</MenuItem>)}
          </Select>
        </FormControl>
      </MainCard>

      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}><CircularProgress /></Box>
      ) : conversations.length === 0 ? (
        <MainCard>
          <Box sx={{ py: 6, textAlign: 'center' }}>
            <Typography color="text.secondary">No conversations found.</Typography>
          </Box>
        </MainCard>
      ) : (
        <Stack spacing={1.5}>
          {conversations.map((conv) => (
            <ConversationItem key={conv.id} conversation={conv} />
          ))}
          <TablePagination
            component="div"
            count={totalCount}
            page={page}
            onPageChange={(_, p) => setPage(p)}
            rowsPerPage={pageSize}
            rowsPerPageOptions={[pageSize]}
          />
        </Stack>
      )}
    </Stack>
  );
}
