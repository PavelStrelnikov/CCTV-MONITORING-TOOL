import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Link as RouterLink } from 'react-router-dom';
import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import CardContent from '@mui/material/CardContent';
import Typography from '@mui/material/Typography';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import ListItemText from '@mui/material/ListItemText';
import Chip from '@mui/material/Chip';
import Alert from '@mui/material/Alert';
import CircularProgress from '@mui/material/CircularProgress';
import Link from '@mui/material/Link';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import DevicesIcon from '@mui/icons-material/Router';
import VideocamIcon from '@mui/icons-material/Videocam';
import StorageIcon from '@mui/icons-material/Storage';
import FiberManualRecordIcon from '@mui/icons-material/FiberManualRecord';
import AccessTimeIcon from '@mui/icons-material/AccessTime';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import ErrorOutlineIcon from '@mui/icons-material/ErrorOutline';
import NotificationsActiveIcon from '@mui/icons-material/NotificationsActive';
import { api } from '../api/client.ts';
import { timeAgo } from '../utils/formatTime.ts';
import type { Overview, Alert as AlertType } from '../types.ts';

export default function Dashboard() {
  const { t } = useTranslation();
  const [overview, setOverview] = useState<Overview | null>(null);
  const [alerts, setAlerts] = useState<AlertType[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const fetchData = async () => {
    try {
      const [ov, al] = await Promise.all([
        api.getOverview(),
        api.getAlerts({ status: 'active' }),
      ]);
      setOverview(ov);
      setAlerts(al);
      setError('');
    } catch (err) {
      setError(err instanceof Error ? err.message : t('dashboard.failedLoad'));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
    const interval = setInterval(fetchData, 30000);
    return () => clearInterval(interval);
  }, []);

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" p={4}>
        <CircularProgress />
      </Box>
    );
  }

  if (error) {
    return <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>;
  }

  const ov = overview!;

  // Stat cards - row 1: devices & cameras
  const statCards = [
    {
      title: t('dashboard.devices'),
      value: `${ov.reachable_devices}/${ov.total_devices}`,
      subtitle: ov.unreachable_devices > 0 ? t('dashboard.offline', { count: ov.unreachable_devices }) : t('dashboard.allOnline'),
      accent: ov.unreachable_devices > 0 ? '#EF4444' : '#22C55E',
      icon: <DevicesIcon sx={{ fontSize: 28 }} />,
    },
    {
      title: t('dashboard.cameras'),
      value: `${ov.online_cameras}/${ov.total_cameras}`,
      subtitle: ov.offline_cameras > 0 ? t('dashboard.offline', { count: ov.offline_cameras }) : t('dashboard.allOnline'),
      accent: ov.offline_cameras > 0 ? '#F59E0B' : '#22C55E',
      icon: <VideocamIcon sx={{ fontSize: 28 }} />,
    },
    {
      title: t('dashboard.disks'),
      value: `${ov.disks_ok_count}/${ov.total_disks}`,
      subtitle: ov.disks_error_count > 0 ? t('dashboard.error', { count: ov.disks_error_count }) : t('dashboard.allOk'),
      accent: ov.disks_error_count > 0 ? '#EF4444' : '#22C55E',
      icon: <StorageIcon sx={{ fontSize: 28 }} />,
    },
    {
      title: t('dashboard.recording'),
      value: `${ov.recording_ok}/${ov.recording_total}`,
      subtitle: ov.recording_total > 0
        ? (ov.recording_ok < ov.recording_total ? t('dashboard.noRec', { count: ov.recording_total - ov.recording_ok }) : t('dashboard.allRecording'))
        : t('dashboard.noData'),
      accent: ov.recording_total > 0 && ov.recording_ok < ov.recording_total ? '#F59E0B' : '#22C55E',
      icon: <FiberManualRecordIcon sx={{ fontSize: 28 }} />,
    },
    {
      title: t('dashboard.timeSync'),
      value: ov.time_drift_issues > 0 ? `${ov.time_drift_issues}` : 'OK',
      subtitle: ov.time_drift_issues > 0 ? t('dashboard.devicesDrifted', { count: ov.time_drift_issues }) : t('dashboard.allSynced'),
      accent: ov.time_drift_issues > 0 ? '#F59E0B' : '#22C55E',
      icon: <AccessTimeIcon sx={{ fontSize: 28 }} />,
    },
    {
      title: t('dashboard.alerts'),
      value: alerts.length,
      subtitle: alerts.length > 0 ? t('dashboard.active') : t('dashboard.noAlerts'),
      accent: alerts.length > 0 ? '#F59E0B' : '#22C55E',
      icon: <NotificationsActiveIcon sx={{ fontSize: 28 }} />,
    },
  ];

  const severityColor = (sev: string) => {
    switch (sev.toLowerCase()) {
      case 'critical': return 'error';
      case 'warning': return 'warning';
      case 'info': return 'info';
      default: return 'default';
    }
  };

  // Sort devices: offline first, then by offline cameras desc
  const sortedDevices = [...ov.devices].sort((a, b) => {
    if (a.reachable !== b.reachable) return a.reachable ? 1 : -1;
    return b.offline_cameras - a.offline_cameras;
  });

  return (
    <Box>
      <Typography variant="h4" gutterBottom>
        {t('dashboard.title')}
      </Typography>

      {/* Stat cards */}
      <Box
        sx={{
          display: 'grid',
          gridTemplateColumns: { xs: 'repeat(2, 1fr)', sm: 'repeat(auto-fit, minmax(180px, 1fr))' },
          gap: 2,
          mb: 3,
        }}
      >
        {statCards.map((c) => (
          <Card
            key={c.title}
            sx={{ borderInlineStart: `4px solid ${c.accent}` }}
          >
            <CardContent sx={{ display: 'flex', alignItems: 'center', gap: 1.5, py: 1.5, '&:last-child': { pb: 1.5 } }}>
              <Box sx={{ color: c.accent }}>{c.icon}</Box>
              <Box>
                <Typography variant="caption" color="text.secondary" sx={{ textTransform: 'uppercase', letterSpacing: '0.05em' }}>
                  {c.title}
                </Typography>
                <Typography variant="h4" sx={{ color: c.accent, fontWeight: 700, lineHeight: 1.2 }}>
                  {c.value}
                </Typography>
                <Typography variant="caption" color="text.secondary">
                  {c.subtitle}
                </Typography>
              </Box>
            </CardContent>
          </Card>
        ))}
      </Box>

      {/* Device status table + Alerts side by side */}
      <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', md: '2fr 1fr' }, gap: 2 }}>
        {/* Device table */}
        <Card>
          <CardContent sx={{ p: 0, '&:last-child': { pb: 0 } }}>
            <Typography variant="h6" sx={{ px: 2, pt: 2, pb: 1 }}>
              {t('dashboard.devices')}
            </Typography>
            <Box sx={{ overflowX: 'auto' }}>
            <Table size="small" sx={{ minWidth: 600 }}>
              <TableHead>
                <TableRow>
                  <TableCell>{t('table.name')}</TableCell>
                  <TableCell align="center">{t('table.status')}</TableCell>
                  <TableCell align="center">{t('table.cameras')}</TableCell>
                  <TableCell align="center">{t('table.disks')}</TableCell>
                  <TableCell align="center">{t('table.recording')}</TableCell>
                  <TableCell align="center">{t('table.time')}</TableCell>
                  <TableCell align="right">{t('table.lastPoll')}</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {sortedDevices.map((dev) => {
                  const driftAbs = dev.time_drift != null ? Math.abs(dev.time_drift) : null;
                  let driftLabel = '';
                  if (dev.time_drift != null) {
                    if (driftAbs! < 5) driftLabel = 'OK';
                    else if (driftAbs! < 60) driftLabel = `${dev.time_drift > 0 ? '+' : ''}${dev.time_drift}s`;
                    else if (driftAbs! < 3600) driftLabel = `${Math.round(dev.time_drift / 60)}min`;
                    else driftLabel = `${(dev.time_drift / 3600).toFixed(1)}h`;
                  }
                  const driftColor = driftAbs == null ? undefined : driftAbs < 30 ? 'success' : driftAbs < 300 ? 'warning' : 'error';

                  const recMissing = dev.recording_total - dev.recording_ok;

                  return (
                    <TableRow key={dev.device_id} hover>
                      <TableCell>
                        <Link component={RouterLink} to={`/devices/${dev.device_id}`} underline="hover" color="inherit">
                          {dev.name}
                        </Link>
                      </TableCell>
                      <TableCell align="center">
                        <Chip
                          label={dev.reachable ? t('status.online') : t('status.offline')}
                          size="small"
                          color={dev.reachable ? 'success' : 'error'}
                          sx={{ minWidth: 65 }}
                        />
                      </TableCell>
                      <TableCell align="center">
                        {dev.camera_count > 0 ? (
                          <Typography variant="body2" color={dev.offline_cameras > 0 ? 'warning.main' : 'text.primary'}>
                            {dev.online_cameras}/{dev.camera_count}
                          </Typography>
                        ) : (
                          <Typography variant="caption" color="text.disabled">-</Typography>
                        )}
                      </TableCell>
                      <TableCell align="center">
                        {dev.disk_ok ? (
                          <CheckCircleIcon fontSize="small" color="success" />
                        ) : (
                          <ErrorOutlineIcon fontSize="small" color="error" />
                        )}
                      </TableCell>
                      <TableCell align="center">
                        {dev.recording_total > 0 ? (
                          <Typography variant="body2" color={recMissing > 0 ? 'warning.main' : 'success.main'}>
                            {dev.recording_ok}/{dev.recording_total}
                          </Typography>
                        ) : (
                          <Typography variant="caption" color="text.disabled">-</Typography>
                        )}
                      </TableCell>
                      <TableCell align="center">
                        {driftLabel ? (
                          <Chip label={driftLabel} size="small" color={driftColor as any} variant="outlined" sx={{ height: 22, fontSize: 11 }} />
                        ) : (
                          <Typography variant="caption" color="text.disabled">-</Typography>
                        )}
                      </TableCell>
                      <TableCell align="right">
                        <Typography variant="caption" color="text.secondary">
                          {dev.last_poll_at ? timeAgo(dev.last_poll_at) : t('time.never')}
                        </Typography>
                      </TableCell>
                    </TableRow>
                  );
                })}
                {sortedDevices.length === 0 && (
                  <TableRow>
                    <TableCell colSpan={7} align="center">
                      <Typography color="text.secondary" sx={{ py: 2 }}>{t('dashboard.noDevices')}</Typography>
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
            </Box>
          </CardContent>
        </Card>

        {/* Alerts */}
        <Card>
          <CardContent>
            <Typography variant="h6" gutterBottom>
              {t('dashboard.activeAlerts')}
            </Typography>
            {alerts.length === 0 ? (
              <Typography color="text.secondary">{t('dashboard.noActiveAlerts')}</Typography>
            ) : (
              <List disablePadding>
                {alerts.slice(0, 10).map((a) => (
                  <ListItem key={a.id} divider sx={{ borderColor: 'divider', px: 0 }}>
                    <ListItemText
                      primary={a.device_name ? `${a.device_name}: ${a.alert_type.replace(/_/g, ' ')}` : a.message}
                      secondary={timeAgo(a.created_at)}
                      primaryTypographyProps={{ variant: 'body2' }}
                    />
                    <Chip
                      label={a.severity}
                      size="small"
                      color={severityColor(a.severity) as 'error' | 'warning' | 'info' | 'default'}
                    />
                  </ListItem>
                ))}
              </List>
            )}
            {alerts.length > 10 && (
              <Box mt={1}>
                <Link component={RouterLink} to="/alerts">
                  {t('dashboard.viewAllAlerts', { count: alerts.length })}
                </Link>
              </Box>
            )}
          </CardContent>
        </Card>
      </Box>
    </Box>
  );
}
