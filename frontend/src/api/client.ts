import type { Device, DeviceCreate, DeviceUpdate, DeviceDetail, PollResult, Overview, Alert, HealthLogEntry, PollLogEntry, SystemSettings, Tag, Folder, FolderTree } from '../types';

const BASE = '/api';

async function request<T>(path: string, options?: RequestInit): Promise<T> {
  const response = await fetch(`${BASE}${path}`, {
    headers: { 'Content-Type': 'application/json' },
    ...options,
  });
  if (!response.ok) {
    const body = await response.json().catch(() => ({}));
    throw new Error(body.detail || `HTTP ${response.status}`);
  }
  if (response.status === 204) return undefined as T;
  return response.json();
}

export const api = {
  getDevices: () => request<Device[]>('/devices'),

  createDevice: (data: DeviceCreate) =>
    request<Device>('/devices', {
      method: 'POST',
      body: JSON.stringify(data),
    }),

  updateDevice: (deviceId: string, data: DeviceUpdate) =>
    request<Device>(`/devices/${deviceId}`, {
      method: 'PATCH',
      body: JSON.stringify(data),
    }),

  deleteDevice: (deviceId: string) =>
    request<void>(`/devices/${deviceId}`, { method: 'DELETE' }),

  getDeviceDetail: (deviceId: string) =>
    request<DeviceDetail>(`/devices/${deviceId}`),

  pollDevice: (deviceId: string) =>
    request<PollResult>(`/devices/${deviceId}/poll`, { method: 'POST' }),

  getCredentials: (deviceId: string) =>
    request<{ username: string; password: string }>(`/devices/${deviceId}/credentials`),

  getOverview: () => request<Overview>('/overview'),

  // Tags
  getTags: () => request<Tag[]>('/tags'),
  addTag: (deviceId: string, tag: string) =>
    request<{ tag: string }>(`/devices/${deviceId}/tags`, {
      method: 'POST',
      body: JSON.stringify({ tag }),
    }),
  removeTag: (deviceId: string, tag: string) =>
    request<void>(`/devices/${deviceId}/tags/${tag}`, { method: 'DELETE' }),
  updateTag: (tagName: string, data: { name?: string; color?: string }) =>
    request<Tag>(`/tags/${encodeURIComponent(tagName)}`, {
      method: 'PATCH',
      body: JSON.stringify(data),
    }),
  deleteTag: (tagName: string) =>
    request<void>(`/tags/${encodeURIComponent(tagName)}`, { method: 'DELETE' }),

  // History
  getDeviceHistory: (deviceId: string, hours = 24) =>
    request<HealthLogEntry[]>(`/devices/${deviceId}/history?hours=${hours}`),

  // Alerts
  getAlerts: (params?: { status?: string; device_id?: string }) => {
    const qs = params
      ? '?' + new URLSearchParams(
          Object.entries(params).filter(([, v]) => v != null) as [string, string][]
        ).toString()
      : '';
    return request<Alert[]>(`/alerts${qs}`);
  },

  // Poll logs (cross-device)
  getPollLogs: (hours = 24, limit = 500) =>
    request<PollLogEntry[]>(`/poll-logs?hours=${hours}&limit=${limit}`),

  // System settings
  getSettings: () => request<SystemSettings>('/settings'),
  updateSettings: (data: Partial<SystemSettings>) =>
    request<SystemSettings>('/settings', {
      method: 'PUT',
      body: JSON.stringify(data),
    }),

  // Ignored channels
  getIgnoredChannels: (deviceId: string) =>
    request<string[]>(`/devices/${deviceId}/ignored-channels`),
  setIgnoredChannels: (deviceId: string, channels: string[]) =>
    request<string[]>(`/devices/${deviceId}/ignored-channels`, {
      method: 'PUT',
      body: JSON.stringify(channels),
    }),

  // Time sync
  syncDeviceTime: (deviceId: string) =>
    request<{ success: boolean; time_set: string; timezone: string; status_code: number }>(
      `/devices/${deviceId}/sync-time`, { method: 'POST' },
    ),

  // Network info
  getDeviceNetwork: (deviceId: string) =>
    request<{ interfaces: { id: string; ip: string | null; mask: string | null; gateway: string | null; mac: string | null }[]; ports: { protocol: string; port: number; enabled: boolean }[] }>(
      `/devices/${deviceId}/network`,
    ),

  // Snapshot URL (timestamp param for cache-busting on refresh)
  getSnapshotUrl: (deviceId: string, channelId: string) =>
    `${BASE}/devices/${deviceId}/snapshot/${channelId}?t=${Math.floor(Date.now() / 30000)}`,

  // Folders
  getFolders: () => request<FolderTree[]>('/folders'),
  createFolder: (data: { name: string; parent_id?: number | null; color?: string | null; icon?: string | null }) =>
    request<Folder>('/folders', { method: 'POST', body: JSON.stringify(data) }),
  updateFolder: (folderId: number, data: { name?: string; parent_id?: number | null; sort_order?: number; color?: string | null; icon?: string | null }) =>
    request<Folder>(`/folders/${folderId}`, { method: 'PATCH', body: JSON.stringify(data) }),
  deleteFolder: (folderId: number) =>
    request<void>(`/folders/${folderId}`, { method: 'DELETE' }),
};
