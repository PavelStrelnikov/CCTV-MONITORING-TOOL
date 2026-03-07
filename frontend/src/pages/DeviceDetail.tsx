import { useEffect, useState } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { api } from '../api/client';
import StatusBadge from '../components/StatusBadge';
import type { DeviceDetail as DeviceDetailType, PollResult } from '../types';

export default function DeviceDetail() {
  const { deviceId } = useParams<{ deviceId: string }>();
  const navigate = useNavigate();
  const [detail, setDetail] = useState<DeviceDetailType | null>(null);
  const [pollResult, setPollResult] = useState<PollResult | null>(null);
  const [loading, setLoading] = useState(true);
  const [polling, setPolling] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    if (!deviceId) return;
    api.getDeviceDetail(deviceId)
      .then(setDetail)
      .catch(err => setError(err instanceof Error ? err.message : 'Failed to load'))
      .finally(() => setLoading(false));
  }, [deviceId]);

  const handlePoll = async () => {
    if (!deviceId) return;
    setPolling(true);
    setError('');
    try {
      const result = await api.pollDevice(deviceId);
      setPollResult(result);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Poll failed');
    } finally {
      setPolling(false);
    }
  };

  const handleDelete = async () => {
    if (!deviceId || !confirm('Delete this device?')) return;
    try {
      await api.deleteDevice(deviceId);
      navigate('/');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Delete failed');
    }
  };

  const formatBytes = (bytes: number) => {
    if (bytes === 0) return '0 B';
    const gb = bytes / (1024 * 1024 * 1024);
    if (gb >= 1) return `${gb.toFixed(1)} GB`;
    const mb = bytes / (1024 * 1024);
    return `${mb.toFixed(0)} MB`;
  };

  if (loading) return <div className="loading">Loading...</div>;
  if (!detail) return <div className="error">Device not found</div>;

  const health = pollResult?.health || detail.device.last_health;

  return (
    <div>
      <div className="page-header">
        <h2>{detail.device.name} ({detail.device.device_id})</h2>
        <div className="actions">
          <button className="btn-primary" onClick={handlePoll} disabled={polling}>
            {polling ? 'Polling...' : 'Poll Now'}
          </button>
          <Link to={`/devices/${deviceId}/edit`}>
            <button className="btn-secondary">Edit</button>
          </Link>
          <button className="btn-danger" onClick={handleDelete}>Delete</button>
        </div>
      </div>

      {error && <div className="error">{error}</div>}

      {health && (
        <div className="card">
          <h2>Health Summary</h2>
          <table>
            <tbody>
              <tr><td>Reachable</td><td><StatusBadge status={health.reachable ? 'online' : 'offline'} /></td></tr>
              <tr><td>Cameras</td><td>{health.online_cameras}/{health.camera_count} online</td></tr>
              <tr><td>Disks</td><td><StatusBadge status={health.disk_ok ? 'ok' : 'error'} /></td></tr>
              <tr><td>Response Time</td><td>{Math.round(health.response_time_ms)}ms</td></tr>
              <tr><td>Last Check</td><td>{new Date(health.checked_at).toLocaleString()}</td></tr>
            </tbody>
          </table>
        </div>
      )}

      {detail.cameras.length > 0 && (
        <div className="card">
          <h2>Cameras ({detail.cameras.length})</h2>
          <table>
            <thead>
              <tr>
                <th>Channel</th>
                <th>Name</th>
                <th>IP</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              {detail.cameras.map(c => (
                <tr key={c.channel_id}>
                  <td>{c.channel_id}</td>
                  <td>{c.channel_name || '\u2014'}</td>
                  <td>{c.ip_address || '\u2014'}</td>
                  <td><StatusBadge status={c.status} /></td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {detail.disks.length > 0 && (
        <div className="card">
          <h2>Disks ({detail.disks.length})</h2>
          <table>
            <thead>
              <tr>
                <th>Disk</th>
                <th>Capacity</th>
                <th>Free</th>
                <th>Health</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              {detail.disks.map(d => (
                <tr key={d.disk_id}>
                  <td>{d.disk_id}</td>
                  <td>{formatBytes(d.capacity_bytes)}</td>
                  <td>{formatBytes(d.free_bytes)}</td>
                  <td>{d.health_status}</td>
                  <td><StatusBadge status={d.status} /></td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {detail.alerts.length > 0 && (
        <div className="card">
          <h2>Active Alerts ({detail.alerts.length})</h2>
          <table>
            <thead>
              <tr>
                <th>Type</th>
                <th>Severity</th>
                <th>Message</th>
                <th>Since</th>
              </tr>
            </thead>
            <tbody>
              {detail.alerts.map(a => (
                <tr key={a.id}>
                  <td>{a.alert_type}</td>
                  <td><StatusBadge status={a.severity} /></td>
                  <td>{a.message}</td>
                  <td>{new Date(a.created_at).toLocaleString()}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      <div className="card">
        <h2>Connection Info</h2>
        <table>
          <tbody>
            <tr><td>Host</td><td>{detail.device.host}:{detail.device.port}</td></tr>
            <tr><td>Vendor</td><td>{detail.device.vendor}</td></tr>
            <tr><td>Active</td><td>{detail.device.is_active ? 'Yes' : 'No'}</td></tr>
          </tbody>
        </table>
      </div>
    </div>
  );
}
