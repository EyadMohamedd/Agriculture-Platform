import { createContext, useContext, useState, useCallback } from 'react';
import PropTypes from 'prop-types';

export const AuthContext = createContext(null);

const TOKEN_KEY = 'agri_token';
const USER_KEY = 'agri_user';

function loadFromStorage() {
  try {
    const token = localStorage.getItem(TOKEN_KEY);
    const user = JSON.parse(localStorage.getItem(USER_KEY) || 'null');
    return { token, user };
  } catch {
    return { token: null, user: null };
  }
}

export function AuthProvider({ children }) {
  const stored = loadFromStorage();
  const [token, setToken] = useState(stored.token);
  const [user, setUser] = useState(stored.user);

  const login = useCallback((tokenValue, userData) => {
    localStorage.setItem(TOKEN_KEY, tokenValue);
    localStorage.setItem(USER_KEY, JSON.stringify(userData));
    setToken(tokenValue);
    setUser(userData);
  }, []);

  const logout = useCallback(() => {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    setToken(null);
    setUser(null);
  }, []);

  const updateUser = useCallback((userData) => {
    localStorage.setItem(USER_KEY, JSON.stringify(userData));
    setUser(userData);
  }, []);

  const isAuthenticated = Boolean(token);
  const isAdmin = user?.role === 'Admin';
  const isFarmer = user?.role === 'Farmer';

  return (
    <AuthContext.Provider value={{ token, user, isAuthenticated, isAdmin, isFarmer, login, logout, updateUser }}>
      {children}
    </AuthContext.Provider>
  );
}

AuthProvider.propTypes = { children: PropTypes.node };

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) throw new Error('useAuth must be used within AuthProvider');
  return context;
}
