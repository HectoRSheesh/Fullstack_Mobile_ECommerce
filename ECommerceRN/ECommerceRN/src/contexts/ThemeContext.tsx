import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import { Appearance, ColorSchemeName } from 'react-native';
import AsyncStorage from '@react-native-async-storage/async-storage';

export type ThemeMode = 'light' | 'dark' | 'system';

interface ThemeContextType {
  themeMode: ThemeMode;
  currentTheme: 'light' | 'dark';
  setThemeMode: (mode: ThemeMode) => void;
  colors: typeof lightColors;
}

const lightColors = {
  primary: '#007AFF',
  primaryDark: '#0056CC',
  secondary: '#5856D6',
  background: '#FFFFFF',
  surface: '#F2F2F7',
  card: '#FFFFFF',
  text: '#000000',
  textSecondary: '#3C3C43',
  textTertiary: '#8E8E93',
  border: '#C6C6C8',
  borderLight: '#E5E5EA',
  success: '#34C759',
  warning: '#FF9500',
  error: '#FF3B30',
  info: '#007AFF',
  shadow: '#000000',
  overlay: 'rgba(0, 0, 0, 0.3)',
  tabBarActive: '#007AFF',
  tabBarInactive: '#8E8E93',
  placeholderText: '#C7C7CD',
  searchBackground: '#F2F2F7',
};

const darkColors = {
  primary: '#0A84FF',
  primaryDark: '#0969DA',
  secondary: '#5E5CE6',
  background: '#000000',
  surface: '#1C1C1E',
  card: '#2C2C2E',
  text: '#FFFFFF',
  textSecondary: '#EBEBF5',
  textTertiary: '#8E8E93',
  border: '#3A3A3C',
  borderLight: '#2C2C2E',
  success: '#30D158',
  warning: '#FF9F0A',
  error: '#FF453A',
  info: '#0A84FF',
  shadow: '#000000',
  overlay: 'rgba(0, 0, 0, 0.5)',
  tabBarActive: '#0A84FF',
  tabBarInactive: '#8E8E93',
  placeholderText: '#48484A',
  searchBackground: '#1C1C1E',
};

const ThemeContext = createContext<ThemeContextType | undefined>(undefined);

interface ThemeProviderProps {
  children: ReactNode;
}

export const ThemeProvider: React.FC<ThemeProviderProps> = ({ children }) => {
  const [themeMode, setThemeModeState] = useState<ThemeMode>('system');
  const [systemTheme, setSystemTheme] = useState<ColorSchemeName>(
    Appearance.getColorScheme()
  );

  // Mevcut tema (light veya dark)
  const currentTheme = 
    themeMode === 'system' 
      ? (systemTheme === 'dark' ? 'dark' : 'light')
      : themeMode;

  // Mevcut renk paleti
  const colors = currentTheme === 'dark' ? darkColors : lightColors;

  useEffect(() => {
    loadThemeMode();
    
    // Sistem tema değişikliklerini dinle
    const subscription = Appearance.addChangeListener(({ colorScheme }) => {
      setSystemTheme(colorScheme);
    });

    return () => subscription?.remove();
  }, []);

  const loadThemeMode = async () => {
    try {
      const savedTheme = await AsyncStorage.getItem('themeMode');
      if (savedTheme && ['light', 'dark', 'system'].includes(savedTheme)) {
        setThemeModeState(savedTheme as ThemeMode);
      }
    } catch (error) {
      console.log('Theme yüklenirken hata:', error);
    }
  };

  const setThemeMode = async (mode: ThemeMode) => {
    try {
      await AsyncStorage.setItem('themeMode', mode);
      setThemeModeState(mode);
    } catch (error) {
      console.log('Theme kaydedilirken hata:', error);
    }
  };

  return (
    <ThemeContext.Provider value={{
      themeMode,
      currentTheme,
      setThemeMode,
      colors,
    }}>
      {children}
    </ThemeContext.Provider>
  );
};

export const useTheme = (): ThemeContextType => {
  const context = useContext(ThemeContext);
  if (!context) {
    throw new Error('useTheme hook must be used within a ThemeProvider');
  }
  return context;
};
