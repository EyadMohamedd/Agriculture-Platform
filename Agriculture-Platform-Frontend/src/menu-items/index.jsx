import {
  DashboardOutlined, BankOutlined, BarChartOutlined, AlertOutlined,
  RobotOutlined, UserOutlined, HistoryOutlined,
  ThunderboltOutlined, TeamOutlined, SafetyOutlined
} from '@ant-design/icons';
import { useAuth } from 'src/hooks/useAuth';

const farmerMenuItems = [
  {
    id: 'main', title: 'Main', type: 'group',
    children: [
      { id: 'dashboard', title: 'Dashboard', type: 'item', url: '/dashboard', icon: <DashboardOutlined /> },
      { id: 'farms', title: 'My Farms', type: 'item', url: '/farms', icon: <BankOutlined /> },
      { id: 'validation-ranges', title: 'Validation Ranges', type: 'item', url: '/farms/validation-ranges', icon: <SafetyOutlined /> }
    ]
  },
  {
    id: 'monitoring', title: 'Monitoring', type: 'group',
    children: [
      { id: 'readings', title: 'Sensor Readings', type: 'item', url: '/sensors/readings', icon: <ThunderboltOutlined /> },
      { id: 'statistics', title: 'Statistics', type: 'item', url: '/sensors/statistics', icon: <BarChartOutlined /> },
      { id: 'alerts', title: 'Alerts', type: 'item', url: '/alerts', icon: <AlertOutlined /> }
    ]
  },
  {
    id: 'ai', title: 'AI Assistant', type: 'group',
    children: [
      { id: 'ai-chat', title: 'Ask AI', type: 'item', url: '/ai/chat', icon: <RobotOutlined /> },
      { id: 'ai-history', title: 'Conversation History', type: 'item', url: '/ai/history', icon: <HistoryOutlined /> }
    ]
  },
  {
    id: 'account', title: 'Account', type: 'group',
    children: [
      { id: 'profile', title: 'Profile', type: 'item', url: '/profile', icon: <UserOutlined /> }
    ]
  }
];

const adminMenuItems = [
  {
    id: 'main', title: 'Main', type: 'group',
    children: [
      { id: 'dashboard', title: 'Dashboard', type: 'item', url: '/dashboard', icon: <DashboardOutlined /> },
      { id: 'farms', title: 'All Farms', type: 'item', url: '/farms', icon: <BankOutlined /> }
    ]
  },
  {
    id: 'admin', title: 'Administration', type: 'group',
    children: [
      { id: 'admin-users', title: 'User Management', type: 'item', url: '/admin/users', icon: <TeamOutlined /> },
      { id: 'admin-ranges', title: 'Validation Ranges', type: 'item', url: '/admin/validation-ranges', icon: <SafetyOutlined /> }
    ]
  },
  {
    id: 'account', title: 'Account', type: 'group',
    children: [
      { id: 'profile', title: 'Profile', type: 'item', url: '/profile', icon: <UserOutlined /> }
    ]
  }
];

export function useMenuItems() {
  const { isAdmin } = useAuth();
  return isAdmin ? adminMenuItems : farmerMenuItems;
}
