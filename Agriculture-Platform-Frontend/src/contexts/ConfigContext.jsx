import { createContext, useContext } from 'react';
import PropTypes from 'prop-types';
import { defaultConfig } from 'src/config';
import { useLocalStorage } from 'src/hooks/useLocalStorage';

export const ConfigContext = createContext(null);

export function ConfigProvider({ children }) {
  const [state, setState] = useLocalStorage('agri-config', defaultConfig);

  const setField = (field, value) => setState((prev) => ({ ...prev, [field]: value }));
  const resetState = () => setState(defaultConfig);

  return (
    <ConfigContext.Provider value={{ state, setState, setField, resetState }}>
      {children}
    </ConfigContext.Provider>
  );
}

ConfigProvider.propTypes = { children: PropTypes.node };

export function useConfig() {
  const context = useContext(ConfigContext);
  if (!context) throw new Error('useConfig must be used within ConfigProvider');
  return context;
}