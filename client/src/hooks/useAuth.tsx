import { createContext, useContext, useState, useEffect, useCallback, ReactNode } from 'react';
import { LoginRequest } from '../types';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:54736';

interface AuthContextType {
  isAuthenticated: boolean;
  login: (credentials: LoginRequest) => Promise<void>;
  register: (credentials: LoginRequest) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [isAuthenticated, setIsAuthenticated] = useState<boolean>(() => {
    // Check if we have a session marker (not the token itself — that's in httpOnly cookie)
    return localStorage.getItem('authenticated') === 'true';
  });

  const login = async (credentials: LoginRequest) => {
    const response = await fetch(`${API_BASE_URL}/api/auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify(credentials),
    });

    if (!response.ok) {
      const error = await response.json();
      throw error;
    }

    localStorage.setItem('authenticated', 'true');
    setIsAuthenticated(true);
  };

  const register = async (credentials: LoginRequest) => {
    const response = await fetch(`${API_BASE_URL}/api/auth/register`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify(credentials),
    });

    if (!response.ok) {
      const error = await response.json();
      throw error;
    }

    localStorage.setItem('authenticated', 'true');
    setIsAuthenticated(true);
  };

  const logout = useCallback(async () => {
    try {
      await fetch(`${API_BASE_URL}/api/auth/logout`, {
        method: 'POST',
        credentials: 'include',
      });
    } catch {
      // Best effort logout
    }
    localStorage.removeItem('authenticated');
    setIsAuthenticated(false);
  }, []);

  const refresh = useCallback(async (): Promise<boolean> => {
    try {
      const response = await fetch(`${API_BASE_URL}/api/auth/refresh`, {
        method: 'POST',
        credentials: 'include',
      });
      if (response.ok) {
        localStorage.setItem('authenticated', 'true');
        return true;
      }
    } catch {
      // Refresh failed
    }
    return false;
  }, []);

  // Set up a periodic token refresh (every 12 minutes for a 15-min access token)
  useEffect(() => {
    if (!isAuthenticated) return;

    const interval = setInterval(async () => {
      const success = await refresh();
      if (!success) {
        localStorage.removeItem('authenticated');
        setIsAuthenticated(false);
      }
    }, 12 * 60 * 1000);

    return () => clearInterval(interval);
  }, [isAuthenticated, refresh]);

  return (
    <AuthContext.Provider value={{ isAuthenticated, login, register, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}
