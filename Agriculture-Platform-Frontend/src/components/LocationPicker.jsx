import { useState, useCallback, useEffect } from 'react';
import { MapContainer, TileLayer, Marker, useMapEvents, useMap } from 'react-leaflet';
import L from 'leaflet';
import 'leaflet/dist/leaflet.css';
import Box from '@mui/material/Box';
import CircularProgress from '@mui/material/CircularProgress';
import FormControl from '@mui/material/FormControl';
import FormHelperText from '@mui/material/FormHelperText';
import Grid from '@mui/material/Grid';
import InputLabel from '@mui/material/InputLabel';
import OutlinedInput from '@mui/material/OutlinedInput';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';

// Fix Leaflet's broken default marker icons when bundled with Vite
delete L.Icon.Default.prototype._getIconUrl;
L.Icon.Default.mergeOptions({
  iconRetinaUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon-2x.png',
  iconUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png',
  shadowUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png'
});

function ClickHandler({ onPick }) {
  useMapEvents({ click: (e) => onPick(e.latlng.lat, e.latlng.lng) });
  return null;
}

function RecenterMap({ position }) {
  const map = useMap();
  useEffect(() => {
    if (position) map.setView(position, map.getZoom());
  }, [position, map]);
  return null;
}

export default function LocationPicker({ value, onChange, error, helperText }) {
  const [loading, setLoading] = useState(false);

  const hasLocation = value?.latitude != null && value?.longitude != null;
  const markerPos = hasLocation ? [value.latitude, value.longitude] : null;
  const defaultCenter = markerPos || [20, 0];
  const defaultZoom = markerPos ? 13 : 2;

  const handlePick = useCallback(async (lat, lng) => {
    setLoading(true);
    let formattedAddress = `${lat.toFixed(6)}, ${lng.toFixed(6)}`;
    try {
      const res = await fetch(
        `https://nominatim.openstreetmap.org/reverse?lat=${lat}&lon=${lng}&format=json`,
        { headers: { 'Accept-Language': 'en' } }
      );
      const data = await res.json();
      if (data.display_name) formattedAddress = data.display_name;
    } catch {
      // keep coordinate fallback
    } finally {
      setLoading(false);
    }
    onChange({
      latitude: parseFloat(lat.toFixed(6)),
      longitude: parseFloat(lng.toFixed(6)),
      formattedAddress
    });
  }, [onChange]);

  return (
    <Stack spacing={2}>
      <Box
        sx={{
          height: 400,
          width: '100%',
          borderRadius: 1,
          overflow: 'hidden',
          border: '1px solid',
          borderColor: error ? 'error.main' : 'divider'
        }}
      >
        <MapContainer center={defaultCenter} zoom={defaultZoom} style={{ height: '100%', width: '100%' }}>
          <TileLayer
            attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
            url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
          />
          <ClickHandler onPick={handlePick} />
          {markerPos && (
            <>
              <Marker position={markerPos} />
              <RecenterMap position={markerPos} />
            </>
          )}
        </MapContainer>
      </Box>

      {loading ? (
        <Stack direction="row" alignItems="center" spacing={1}>
          <CircularProgress size={14} />
          <Typography variant="caption" color="text.secondary">Fetching address...</Typography>
        </Stack>
      ) : !hasLocation ? (
        <Typography variant="caption" color={error ? 'error' : 'text.secondary'}>
          Click anywhere on the map to pick your farm location
        </Typography>
      ) : null}

      {helperText && !loading && (
        <FormHelperText error={error}>{helperText}</FormHelperText>
      )}

      <Grid container spacing={2}>
        <Grid size={{ xs: 12, sm: 6 }}>
          <FormControl fullWidth>
            <InputLabel shrink>Latitude</InputLabel>
            <OutlinedInput
              label="Latitude"
              notched
              value={value?.latitude ?? ''}
              inputProps={{ readOnly: true, style: { color: 'inherit' } }}
              sx={{ bgcolor: 'action.hover' }}
            />
          </FormControl>
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <FormControl fullWidth>
            <InputLabel shrink>Longitude</InputLabel>
            <OutlinedInput
              label="Longitude"
              notched
              value={value?.longitude ?? ''}
              inputProps={{ readOnly: true, style: { color: 'inherit' } }}
              sx={{ bgcolor: 'action.hover' }}
            />
          </FormControl>
        </Grid>
        <Grid size={{ xs: 12 }}>
          <FormControl fullWidth>
            <InputLabel shrink>Formatted Address</InputLabel>
            <OutlinedInput
              label="Formatted Address"
              notched
              value={value?.formattedAddress ?? ''}
              inputProps={{ readOnly: true, style: { color: 'inherit' } }}
              sx={{ bgcolor: 'action.hover' }}
            />
          </FormControl>
        </Grid>
      </Grid>
    </Stack>
  );
}
