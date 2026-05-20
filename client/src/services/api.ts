const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:54736';

let isRefreshing = false;
let refreshPromise: Promise<boolean> | null = null;

async function attemptRefresh(): Promise<boolean> {
  if (isRefreshing && refreshPromise) {
    return refreshPromise;
  }

  isRefreshing = true;
  refreshPromise = fetch(`${API_BASE_URL}/api/auth/refresh`, {
    method: 'POST',
    credentials: 'include',
  }).then((res) => {
    isRefreshing = false;
    refreshPromise = null;
    return res.ok;
  }).catch(() => {
    isRefreshing = false;
    refreshPromise = null;
    return false;
  });

  return refreshPromise;
}

export async function apiFetch<T>(
  path: string,
  options: RequestInit = {}
): Promise<T> {
  const headers: HeadersInit = {
    'Content-Type': 'application/json',
    ...options.headers,
  };

  let response = await fetch(`${API_BASE_URL}${path}`, {
    ...options,
    headers,
    credentials: 'include',
  });

  // If 401, try to refresh the token and retry once
  if (response.status === 401) {
    const refreshed = await attemptRefresh();
    if (refreshed) {
      response = await fetch(`${API_BASE_URL}${path}`, {
        ...options,
        headers,
        credentials: 'include',
      });
    }
  }

  if (response.status === 401) {
    localStorage.removeItem('authenticated');
    window.location.href = '/login';
    throw new Error('Unauthorized');
  }

  if (response.status === 429) {
    throw { message: 'Too many requests. Please wait a moment and try again.' };
  }

  if (!response.ok) {
    const error = await response.json();
    throw error;
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return response.json();
}
