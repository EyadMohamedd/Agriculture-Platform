import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { RouterProvider } from 'react-router-dom';
import ThemeCustomization from 'src/themes';
import { ConfigProvider } from 'src/contexts/ConfigContext';
import { AuthProvider } from 'src/contexts/AuthContext';
import router from 'src/routes';

createRoot(document.getElementById('root')).render(
  <StrictMode>
    <ConfigProvider>
      <AuthProvider>
        <ThemeCustomization>
          <RouterProvider router={router} />
        </ThemeCustomization>
      </AuthProvider>
    </ConfigProvider>
  </StrictMode>
);
