import React, { useState, useEffect } from 'react';
import { StatusBar, View, Text, ActivityIndicator } from 'react-native';
import { SafeAreaProvider } from 'react-native-safe-area-context';
import { GestureHandlerRootView } from 'react-native-gesture-handler';
import AppNavigator from './src/navigation/AppNavigator';
import { storage } from './src/utils/storage';
import { ThemeProvider } from './src/contexts/ThemeContext';

export default function App() {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    checkAuthState();
    
    // 5 saniye sonra force loading false yap
    const timeout = setTimeout(() => {
      console.log('⏰ Force loading timeout - setting loading to false');
      setIsLoading(false);
    }, 5000);
    
    return () => clearTimeout(timeout);
  }, []);

  const checkAuthState = async () => {
    try {
      console.log('🔐 Checking auth state...');
      const token = await storage.getAuthToken();
      console.log('🔐 Token found:', token ? 'Yes' : 'No');
      
      if (token) {
        console.log('🔐 Token preview:', token.substring(0, 30) + '...');
        // Token varsa, geçerliliğini kontrol et
        try {
          const response = await fetch(`http://10.0.2.2:7038/api/auth/profile`, {
            headers: {
              'Authorization': `Bearer ${token}`,
              'Content-Type': 'application/json'
            }
          });
          
          if (response.ok) {
            console.log('🔐 Token is valid, user authenticated');
            setIsAuthenticated(true);
          } else {
            console.log('🔐 Token is invalid, clearing auth state');
            await storage.removeAuthToken();
            setIsAuthenticated(false);
          }
        } catch (profileError) {
          console.error('🔐 Error validating token:', profileError);
          await storage.removeAuthToken();
          setIsAuthenticated(false);
        }
      } else {
        console.log('🔐 No token found, user not authenticated');
        setIsAuthenticated(false);
      }
    } catch (error) {
      console.error('🔐 Error checking auth state:', error);
      setIsAuthenticated(false);
    } finally {
      console.log('🔐 Auth check completed, loading false');
      setIsLoading(false);
    }
  };

  const handleLogin = () => {
    setIsAuthenticated(true);
  };

  const handleLogout = () => {
    setIsAuthenticated(false);
  };

  if (isLoading) {
    console.log('🔄 App still loading...');
    return (
      <SafeAreaProvider>
        <View style={{ flex: 1, justifyContent: 'center', alignItems: 'center', backgroundColor: '#f5f5f5' }}>
          <ActivityIndicator size="large" color="#007AFF" />
          <Text style={{ marginTop: 16, fontSize: 16, color: '#666' }}>Loading E-Commerce App...</Text>
        </View>
      </SafeAreaProvider>
    );
  }

  console.log('🚀 App rendering, authenticated:', isAuthenticated);

  return (
    <ThemeProvider>
      <SafeAreaProvider>
        <GestureHandlerRootView style={{ flex: 1 }}>
          <StatusBar barStyle="dark-content" />
          <AppNavigator 
            isAuthenticated={isAuthenticated}
            onLogin={handleLogin}
            onLogout={handleLogout}
          />
        </GestureHandlerRootView>
      </SafeAreaProvider>
    </ThemeProvider>
  );
}
