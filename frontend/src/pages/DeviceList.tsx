import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { api } from '../api/client';
import StatusBadge from '../components/StatusBadge';
import type { Device } from '../types';

export default function DeviceList() {
  const [devices, setDevices] = useState<Device[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [polling, setPolling] = useState<string | null>(null);

  const fetchDevices = async () => {
    try {
      const data = await api.getDevices();
      setDevices(data);
      setError('');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load devices');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchDevices();
    const interval = setInterval(fetchDevices, 30000);
    return () => clearInterval(interval);
  }, []);

  const handlePoll = async (deviceId: string) => {
    setPolling(deviceId);
    try {
      const result = await api.pollDevice(deviceId);
      setDevices(prev =>
        prev.map(d =>
          d.device_id === deviceId
            ? { ...d, last_health: result.health }
            : d
        )
      );
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Poll failed');
    } finally {
      setPolling(null);
    }
  };

  const handleDelete = async (deviceId: string) => {
    if (!confirm(`Delete device ${deviceId}?`)) return;
    try {
      await api.deleteDevice(deviceId);
      setDevices(prev => prev.filter(d => d.device_id !== deviceId));
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Delete failed');
    }
  };

  if (loading) return <div className="loading">Loading devices...</div>;

  return (
    <div>
      <div className="page-header">
        <h2>Devices ({devices.length})</h2>
        <Link to="/devices/add">
          <button className="btn-primary">+ Add Device</button>
        </Link>
      </div>

      {error && <div className="error">{error}</div>}

      <table>
        <thead>
          <tr>
            <th>Name</th>
            <th>Host</th>
            <th>Vendor</th>
            <th>Reachable</th>
            <th>Cameras</th>
            <th>Disks</th>
            <th>Response</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {devices.map(d => (
            <tr key={d.device_id}>
              <td>
                <Link to={`/devices/${d.device_id}`}>{d.name}</Link>
              </td>
              <td>{d.host}:{d.port}</td>
              <td>{d.vendor}</td>
              <td>
                {d.last_health ? (
                  <StatusBadge status={d.last_health.reachable ? 'online' : 'offline'} />
                ) : (
                  <StatusBadge status="unknown" />
                )}
              </td>
              <td>
                {d.last_health
                  ? `${d.last_health.online_cameras}/${d.last_health.camera_count}`
                  : '\u2014'}
              </td>
              <td>
                {d.last_health ? (
                  <StatusBadge status={d.last_health.disk_ok ? 'ok' : 'error'} />
                ) : '\u2014'}
              </td>
              <td>
                {d.last_health
                  ? `${Math.round(d.last_health.response_time_ms)}ms`
                  : '\u2014'}
              </td>
              <td>
                <div className="actions">
                  <button
                    className="btn-primary"
                    onClick={() => handlePoll(d.device_id)}
                    disabled={polling === d.device_id}
                  >
                    {polling === d.device_id ? 'Polling...' : 'Poll'}
                  </button>
                  <button
                    className="btn-danger"
                    onClick={() => handleDelete(d.device_id)}
                  >
                    Delete
                  </button>
                </div>
              </td>
            </tr>
          ))}
          {devices.length === 0 && (
            <tr>
              <td colSpan={8} style={{ textAlign: 'center', padding: '2rem' }}>
                No devices yet. <Link to="/devices/add">Add one</Link>
              </td>
            </tr>
          )}
        </tbody>
      </table>
    </div>
  );
}
