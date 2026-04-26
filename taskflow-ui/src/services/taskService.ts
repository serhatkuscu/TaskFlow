import { authService } from './authService';
import type { PagedResult, TaskItem, TaskStatus } from '@/types';

const API_URL = process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:5008';

function headers(): HeadersInit {
  return {
    'Content-Type': 'application/json',
    Authorization: `Bearer ${authService.getToken()}`,
  };
}

async function handleResponse<T>(res: Response): Promise<T> {
  if (res.status === 401) {
    authService.logout();
    window.location.href = '/login';
    throw new Error('Session expired. Please log in again.');
  }
  if (res.status === 204) return undefined as T;
  if (!res.ok) {
    const err = await res.json().catch(() => ({}));
    throw new Error(err.message ?? `Request failed (${res.status}).`);
  }
  return res.json();
}

export const taskService = {
  getAll(pageSize = 50): Promise<PagedResult> {
    return fetch(`${API_URL}/api/tasks?pageNumber=1&pageSize=${pageSize}`, {
      headers: headers(),
    }).then((r) => handleResponse<PagedResult>(r));
  },

  create(title: string, description?: string): Promise<TaskItem> {
    return fetch(`${API_URL}/api/tasks`, {
      method: 'POST',
      headers: headers(),
      body: JSON.stringify({ title, description }),
    }).then((r) => handleResponse<TaskItem>(r));
  },

  update(id: string, title: string, description?: string): Promise<TaskItem> {
    return fetch(`${API_URL}/api/tasks/${id}`, {
      method: 'PUT',
      headers: headers(),
      body: JSON.stringify({ title, description }),
    }).then((r) => handleResponse<TaskItem>(r));
  },

  updateStatus(id: string, status: TaskStatus): Promise<TaskItem> {
    return fetch(`${API_URL}/api/tasks/${id}/status`, {
      method: 'PATCH',
      headers: headers(),
      body: JSON.stringify({ status }),
    }).then((r) => handleResponse<TaskItem>(r));
  },

  delete(id: string): Promise<void> {
    return fetch(`${API_URL}/api/tasks/${id}`, {
      method: 'DELETE',
      headers: headers(),
    }).then((r) => handleResponse<void>(r));
  },
};
