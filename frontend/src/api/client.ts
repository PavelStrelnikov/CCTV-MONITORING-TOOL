import type { Device, DeviceCreate, DeviceUpdate, DeviceDetail, PollResult, Overview } from '../types';

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

  getOverview: () => request<Overview>('/overview'),
};
