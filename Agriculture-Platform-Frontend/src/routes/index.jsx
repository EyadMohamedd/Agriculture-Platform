import { lazy } from 'react';
import { createBrowserRouter, Navigate } from 'react-router-dom';
import Loadable from 'src/components/Loadable';
import DashboardLayout from 'src/layout/Dashboard';
import AuthLayout from 'src/layout/Auth';
import { ProtectedRoute, PublicRoute } from './ProtectedRoute';

// Auth pages
const Login = Loadable(lazy(() => import('src/pages/auth/Login')));
const Register = Loadable(lazy(() => import('src/pages/auth/Register')));
const ForgotPassword = Loadable(lazy(() => import('src/pages/auth/ForgotPassword')));
const ResetPassword = Loadable(lazy(() => import('src/pages/auth/ResetPassword')));

// Dashboard
const Dashboard = Loadable(lazy(() => import('src/pages/dashboard')));

// Farms
const FarmsList = Loadable(lazy(() => import('src/pages/farms')));
const FarmDetail = Loadable(lazy(() => import('src/pages/farms/FarmDetail')));
const CreateFarm = Loadable(lazy(() => import('src/pages/farms/CreateFarm')));
const EditFarm = Loadable(lazy(() => import('src/pages/farms/EditFarm')));
const FarmValidationRanges = Loadable(lazy(() => import('src/pages/farms/ValidationRanges')));
const ValidationRangesSelector = Loadable(lazy(() => import('src/pages/farms/ValidationRangesSelector')));

// Sensors
const SensorReadings = Loadable(lazy(() => import('src/pages/sensors/Readings')));
const SensorStatistics = Loadable(lazy(() => import('src/pages/sensors/Statistics')));

// Alerts
const Alerts = Loadable(lazy(() => import('src/pages/alerts')));

// AI
const AiChat = Loadable(lazy(() => import('src/pages/ai/Chat')));
const AiHistory = Loadable(lazy(() => import('src/pages/ai/History')));

// Profile
const Profile = Loadable(lazy(() => import('src/pages/profile')));

// Admin
const AdminUsers = Loadable(lazy(() => import('src/pages/admin/Users')));
const AdminValidationRanges = Loadable(lazy(() => import('src/pages/admin/ValidationRanges')));

const router = createBrowserRouter([
  {
    path: '/',
    element: <ProtectedRoute><DashboardLayout /></ProtectedRoute>,
    children: [
      { index: true, element: <Navigate to="/dashboard" replace /> },
      { path: 'dashboard', element: <Dashboard /> },
      { path: 'farms', element: <FarmsList /> },
      { path: 'farms/create', element: <CreateFarm /> },
      { path: 'farms/validation-ranges', element: <ValidationRangesSelector /> },
      { path: 'farms/:id', element: <FarmDetail /> },
      { path: 'farms/:id/edit', element: <EditFarm /> },
      { path: 'farms/:id/validation-ranges', element: <FarmValidationRanges /> },
      { path: 'sensors/readings', element: <SensorReadings /> },
      { path: 'sensors/statistics', element: <SensorStatistics /> },
      { path: 'alerts', element: <ProtectedRoute farmerOnly><Alerts /></ProtectedRoute> },
      { path: 'ai/chat', element: <AiChat /> },
      { path: 'ai/history', element: <AiHistory /> },
      { path: 'profile', element: <Profile /> },
      { path: 'admin/users', element: <ProtectedRoute adminOnly><AdminUsers /></ProtectedRoute> },
      { path: 'admin/validation-ranges', element: <ProtectedRoute adminOnly><AdminValidationRanges /></ProtectedRoute> }
    ]
  },
  {
    element: <PublicRoute><AuthLayout /></PublicRoute>,
    children: [
      { path: 'login', element: <Login /> },
      { path: 'register', element: <Register /> },
      { path: 'forgot-password', element: <ForgotPassword /> },
      { path: 'reset-password', element: <ResetPassword /> }
    ]
  },
  { path: '*', element: <Navigate to="/dashboard" replace /> }
]);

export default router;
