import PropTypes from 'prop-types';
import { Navigate } from 'react-router-dom';
import { useAuth } from 'src/hooks/useAuth';

export function ProtectedRoute({ children, adminOnly = false, farmerOnly = false }) {
  const { isAuthenticated, isAdmin } = useAuth();
  if (!isAuthenticated) return <Navigate to="/login" replace />;
  if (adminOnly && !isAdmin) return <Navigate to="/dashboard" replace />;
  if (farmerOnly && isAdmin) return <Navigate to="/dashboard" replace />;
  return children;
}

export function PublicRoute({ children }) {
  const { isAuthenticated } = useAuth();
  if (isAuthenticated) return <Navigate to="/dashboard" replace />;
  return children;
}

ProtectedRoute.propTypes = { children: PropTypes.node, adminOnly: PropTypes.bool, farmerOnly: PropTypes.bool };
PublicRoute.propTypes = { children: PropTypes.node };
