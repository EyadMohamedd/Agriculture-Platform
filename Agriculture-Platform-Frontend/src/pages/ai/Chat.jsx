import { useState, useEffect } from 'react';
import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import CircularProgress from '@mui/material/CircularProgress';
import Divider from '@mui/material/Divider';
import FormControl from '@mui/material/FormControl';
import Grid from '@mui/material/Grid';

import InputLabel from '@mui/material/InputLabel';
import MenuItem from '@mui/material/MenuItem';
import OutlinedInput from '@mui/material/OutlinedInput';
import Paper from '@mui/material/Paper';

import Select from '@mui/material/Select';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import { RobotOutlined, SendOutlined } from '@ant-design/icons';
import MainCard from 'src/components/MainCard';
import { farmsApi } from 'src/api/farms';
import { aiApi } from 'src/api/ai';

export default function AiChat() {
  const [farms, setFarms] = useState([]);
  const [selectedFarm, setSelectedFarm] = useState('');
  const [question, setQuestion] = useState('');
  const [response, setResponse] = useState(null);
  const [loading, setLoading] = useState(false);
  const [farmsLoading, setFarmsLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    farmsApi.getAll({ pageSize: 100 }).then((res) => {
      const items = res.data.data.items || [];
      setFarms(items);
      if (items.length > 0) setSelectedFarm(items[0].id);
    }).catch(() => setError('Failed to load farms'))
      .finally(() => setFarmsLoading(false));
  }, []);

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!question.trim() || !selectedFarm) return;
    setLoading(true);
    setError('');
    setResponse(null);
    try {
      const res = await aiApi.ask({ farmId: selectedFarm, question: question.trim() });
      setResponse(res.data.data || res.data);
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to get AI response');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Stack spacing={3}>
      <Stack direction="row" alignItems="center" spacing={1}>
        <RobotOutlined style={{ fontSize: 24 }} />
        <Typography variant="h4">Ask AI Assistant</Typography>
      </Stack>

      {error && <Alert severity="error" onClose={() => setError('')}>{error}</Alert>}

      <MainCard title="Ask a Question">
        <Box component="form" onSubmit={handleSubmit}>
          <Grid container spacing={2}>
            <Grid size={{ xs: 12 }}>
              <FormControl fullWidth size="small" disabled={farmsLoading}>
                <InputLabel>Farm</InputLabel>
                <Select label="Farm" value={selectedFarm} onChange={(e) => setSelectedFarm(e.target.value)}>
                  {farms.map((f) => <MenuItem key={f.id} value={f.id}>{f.name}</MenuItem>)}
                </Select>
              </FormControl>
            </Grid>
            <Grid size={{ xs: 12 }}>
              <FormControl fullWidth>
                <InputLabel>Your Question</InputLabel>
                <OutlinedInput
                  label="Your Question"
                  multiline
                  rows={3}
                  value={question}
                  onChange={(e) => setQuestion(e.target.value)}
                  placeholder="Ask about your farm conditions, recommendations..."
                />
              </FormControl>
            </Grid>
            <Grid size={{ xs: 12 }}>
              <Button
                type="submit"
                variant="contained"
                endIcon={loading ? <CircularProgress size={18} color="inherit" /> : <SendOutlined />}
                disabled={loading || !question.trim() || !selectedFarm}
              >
                {loading ? 'Getting Answer...' : 'Ask AI'}
              </Button>
            </Grid>
          </Grid>
        </Box>
      </MainCard>

      {response && (
        <MainCard title="AI Response">
          <Stack spacing={2}>
            <Box>
              <Typography variant="subtitle2" color="text.secondary" gutterBottom>Your Question</Typography>
              <Paper variant="outlined" sx={{ p: 2, bgcolor: 'background.default' }}>
                <Typography>{response.question || question}</Typography>
              </Paper>
            </Box>
            <Divider />
            <Box>
              <Typography variant="subtitle2" color="text.secondary" gutterBottom>Answer</Typography>
              <Paper variant="outlined" sx={{ p: 2, bgcolor: 'primary.lighter' }}>
                <Typography
                  variant="body1"
                  sx={{ whiteSpace: 'pre-wrap', lineHeight: 1.8 }}
                >
                  {response.answer}
                </Typography>
              </Paper>
            </Box>

          </Stack>
        </MainCard>
      )}
    </Stack>
  );
}
