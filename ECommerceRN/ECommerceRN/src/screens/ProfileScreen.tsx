import React, { useState, useEffect } from 'react';
import {
  View,
  Text,
  StyleSheet,
  TouchableOpacity,
  ActivityIndicator,
  Alert,
  ScrollView,
} from 'react-native';
import Icon from 'react-native-vector-icons/Ionicons';
import { useNavigation } from '@react-navigation/native';
import { StackNavigationProp } from '@react-navigation/stack';
import { authService } from '../services/api';
import { storage } from '../utils/storage';
import { User } from '../types';
import { useTheme } from '../contexts/ThemeContext';
import { MainStackParamList } from '../navigation/AppNavigator';

interface Props {
  onLogout: () => void;
}

type ProfileScreenNavigationProp = StackNavigationProp<MainStackParamList>;

const ProfileScreen: React.FC<Props> = ({ onLogout }) => {
  const navigation = useNavigation<ProfileScreenNavigationProp>();
  const { colors } = useTheme();
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);

  const styles = StyleSheet.create({
    container: {
      flex: 1,
      backgroundColor: colors.background,
    },
    loadingContainer: {
      flex: 1,
      justifyContent: 'center',
      alignItems: 'center',
      backgroundColor: colors.background,
    },
    loadingText: {
      marginTop: 10,
      fontSize: 16,
      color: colors.textSecondary,
    },
    userSection: {
      backgroundColor: colors.card,
      alignItems: 'center',
      padding: 30,
      marginBottom: 20,
    },
    avatarContainer: {
      marginBottom: 15,
    },
    userName: {
      fontSize: 24,
      fontWeight: 'bold',
      color: colors.text,
      marginBottom: 5,
    },
    userEmail: {
      fontSize: 16,
      color: colors.textSecondary,
    },
    detailsSection: {
      backgroundColor: colors.card,
      padding: 20,
      marginBottom: 20,
    },
    sectionTitle: {
      fontSize: 18,
      fontWeight: 'bold',
      color: colors.text,
      marginBottom: 15,
    },
    detailItem: {
      flexDirection: 'row',
      alignItems: 'center',
      paddingVertical: 12,
      borderBottomWidth: 1,
      borderBottomColor: colors.borderLight,
    },
    detailContent: {
      marginLeft: 15,
      flex: 1,
    },
    detailLabel: {
      fontSize: 14,
      color: colors.textSecondary,
      marginBottom: 2,
    },
    detailValue: {
      fontSize: 16,
      fontWeight: '600',
      color: colors.text,
    },
    menuSection: {
      backgroundColor: colors.card,
      padding: 20,
      marginBottom: 20,
    },
    menuItem: {
      flexDirection: 'row',
      alignItems: 'center',
      paddingVertical: 15,
      borderBottomWidth: 1,
      borderBottomColor: colors.borderLight,
    },
    menuText: {
      fontSize: 16,
      color: colors.text,
      marginLeft: 15,
      flex: 1,
    },
    logoutSection: {
      backgroundColor: colors.card,
      padding: 20,
      marginBottom: 30,
    },
    logoutButton: {
      flexDirection: 'row',
      alignItems: 'center',
      justifyContent: 'center',
      backgroundColor: colors.surface,
      padding: 15,
      borderRadius: 10,
      borderWidth: 1,
      borderColor: colors.error,
    },
    logoutText: {
      fontSize: 16,
      fontWeight: '600',
      color: colors.error,
      marginLeft: 10,
    },
  });

  useEffect(() => {
    loadUserData();
  }, []);

  const loadUserData = async () => {
    try {
      console.log('👤 Loading user data...');
      
      // Önce local storage'dan kullanıcı verilerini al
      const userData = await storage.getUserData();
      if (userData) {
        console.log('👤 Local user data found:', userData);
        setUser(userData);
      }

      // Token kontrolü
      const token = await storage.getAuthToken();
      console.log('👤 Auth token available:', token ? 'Yes' : 'No');

      // Sonra güncel verileri API'den al
      console.log('👤 Fetching current user from API...');
      const currentUser = await authService.getCurrentUser();
      console.log('👤 Current user data received:', currentUser);
      
      setUser(currentUser);
      await storage.setUserData(currentUser);
    } catch (error) {
      console.error('👤 Error loading user data:', error);
      
      if (error instanceof Error) {
        console.error('👤 Error message:', error.message);
      }
      
      // Local storage'dan veri al
      const userData = await storage.getUserData();
      if (userData) {
        console.log('👤 Fallback to local user data:', userData);
        setUser(userData);
      } else {
        console.error('👤 No local user data available');
        Alert.alert('Hata', 'Kullanıcı bilgileri yüklenemedi. Lütfen yeniden giriş yapın.');
      }
    } finally {
      setLoading(false);
    }
  };

  const handleLogout = () => {
    Alert.alert(
      'Çıkış Yap',
      'Oturumu sonlandırmak istediğinizden emin misiniz?',
      [
        { text: 'İptal', style: 'cancel' },
        {
          text: 'Çıkış Yap',
          style: 'destructive',
          onPress: async () => {
            try {
              await storage.clearAll();
              onLogout();
            } catch (error) {
              console.error('Error during logout:', error);
              onLogout();
            }
          },
        },
      ]
    );
  };

  if (loading) {
    return (
      <View style={styles.loadingContainer}>
        <ActivityIndicator size="large" color={colors.primary} />
        <Text style={styles.loadingText}>Profil yükleniyor...</Text>
      </View>
    );
  }

  return (
    <ScrollView style={styles.container}>
      {/* User Info Section */}
      <View style={styles.userSection}>
        <View style={styles.avatarContainer}>
          <Icon name="person-circle" size={80} color={colors.primary} />
        </View>
        
        <Text style={styles.userName}>
          {user?.fullName || 'Kullanıcı'}
        </Text>
        
        {user?.email && (
          <Text style={styles.userEmail}>{user.email}</Text>
        )}
      </View>

      {/* User Details */}
      <View style={styles.detailsSection}>
        <Text style={styles.sectionTitle}>Hesap Bilgileri</Text>
        
        <View style={styles.detailItem}>
          <Icon name="person-outline" size={20} color={colors.textTertiary} />
          <View style={styles.detailContent}>
            <Text style={styles.detailLabel}>Ad Soyad</Text>
            <Text style={styles.detailValue}>{user?.fullName || '-'}</Text>
          </View>
        </View>

        <View style={styles.detailItem}>
          <Icon name="mail-outline" size={20} color={colors.textTertiary} />
          <View style={styles.detailContent}>
            <Text style={styles.detailLabel}>E-posta</Text>
            <Text style={styles.detailValue}>{user?.email || '-'}</Text>
          </View>
        </View>

        <View style={styles.detailItem}>
          <Icon name="card-outline" size={20} color={colors.textTertiary} />
          <View style={styles.detailContent}>
            <Text style={styles.detailLabel}>Kullanıcı ID</Text>
            <Text style={styles.detailValue}>{user?.id || '-'}</Text>
          </View>
        </View>
      </View>

      {/* Menu Items */}
      <View style={styles.menuSection}>
        <Text style={styles.sectionTitle}>Ayarlar</Text>
        
        <TouchableOpacity style={styles.menuItem} onPress={() => Alert.alert('Bilgi', 'Bu özellik henüz mevcut değil.')}>
          <Icon name="bag-outline" size={20} color={colors.textTertiary} />
          <Text style={styles.menuText}>Siparişlerim</Text>
          <Icon name="chevron-forward" size={20} color={colors.textTertiary} />
        </TouchableOpacity>

        <TouchableOpacity style={styles.menuItem} onPress={() => Alert.alert('Bilgi', 'Bu özellik henüz mevcut değil.')}>
          <Icon name="heart-outline" size={20} color={colors.textTertiary} />
          <Text style={styles.menuText}>Favorilerim</Text>
          <Icon name="chevron-forward" size={20} color={colors.textTertiary} />
        </TouchableOpacity>

        <TouchableOpacity style={styles.menuItem} onPress={() => Alert.alert('Bilgi', 'Bu özellik henüz mevcut değil.')}>
          <Icon name="location-outline" size={20} color={colors.textTertiary} />
          <Text style={styles.menuText}>Adreslerim</Text>
          <Icon name="chevron-forward" size={20} color={colors.textTertiary} />
        </TouchableOpacity>

        <TouchableOpacity style={styles.menuItem} onPress={() => navigation.navigate('ThemeSettings')}>
          <Icon name="color-palette-outline" size={20} color={colors.textTertiary} />
          <Text style={styles.menuText}>Tema Ayarları</Text>
          <Icon name="chevron-forward" size={20} color={colors.textTertiary} />
        </TouchableOpacity>

        <TouchableOpacity style={styles.menuItem} onPress={() => Alert.alert('Bilgi', 'Bu özellik henüz mevcut değil.')}>
          <Icon name="settings-outline" size={20} color={colors.textTertiary} />
          <Text style={styles.menuText}>Ayarlar</Text>
          <Icon name="chevron-forward" size={20} color={colors.textTertiary} />
        </TouchableOpacity>

        <TouchableOpacity style={styles.menuItem} onPress={() => Alert.alert('Bilgi', 'Bu özellik henüz mevcut değil.')}>
          <Icon name="help-circle-outline" size={20} color={colors.textTertiary} />
          <Text style={styles.menuText}>Yardım</Text>
          <Icon name="chevron-forward" size={20} color={colors.textTertiary} />
        </TouchableOpacity>
      </View>

      {/* Logout Button */}
      <View style={styles.logoutSection}>
        <TouchableOpacity style={styles.logoutButton} onPress={handleLogout}>
          <Icon name="log-out-outline" size={20} color={colors.error} />
          <Text style={styles.logoutText}>Çıkış Yap</Text>
        </TouchableOpacity>
      </View>
    </ScrollView>
  );
};

export default ProfileScreen;
