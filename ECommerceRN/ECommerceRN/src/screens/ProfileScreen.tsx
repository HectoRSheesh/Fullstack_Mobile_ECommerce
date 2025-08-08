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
import { authService } from '../services/api';
import { storage } from '../utils/storage';
import { User } from '../types';

interface Props {
  onLogout: () => void;
}

const ProfileScreen: React.FC<Props> = ({ onLogout }) => {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);

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
        <ActivityIndicator size="large" color="#007AFF" />
        <Text style={styles.loadingText}>Profil yükleniyor...</Text>
      </View>
    );
  }

  return (
    <ScrollView style={styles.container}>
      {/* User Info Section */}
      <View style={styles.userSection}>
        <View style={styles.avatarContainer}>
          <Icon name="person-circle" size={80} color="#007AFF" />
        </View>
        
        <Text style={styles.userName}>
          {user?.firstName && user?.lastName 
            ? `${user.firstName} ${user.lastName}`
            : user?.username || 'Kullanıcı'
          }
        </Text>
        
        {user?.email && (
          <Text style={styles.userEmail}>{user.email}</Text>
        )}
      </View>

      {/* User Details */}
      <View style={styles.detailsSection}>
        <Text style={styles.sectionTitle}>Hesap Bilgileri</Text>
        
        <View style={styles.detailItem}>
          <Icon name="person-outline" size={20} color="#666" />
          <View style={styles.detailContent}>
            <Text style={styles.detailLabel}>Kullanıcı Adı</Text>
            <Text style={styles.detailValue}>{user?.username || '-'}</Text>
          </View>
        </View>

        <View style={styles.detailItem}>
          <Icon name="mail-outline" size={20} color="#666" />
          <View style={styles.detailContent}>
            <Text style={styles.detailLabel}>E-posta</Text>
            <Text style={styles.detailValue}>{user?.email || '-'}</Text>
          </View>
        </View>

        {user?.firstName && (
          <View style={styles.detailItem}>
            <Icon name="person-outline" size={20} color="#666" />
            <View style={styles.detailContent}>
              <Text style={styles.detailLabel}>Ad</Text>
              <Text style={styles.detailValue}>{user.firstName}</Text>
            </View>
          </View>
        )}

        {user?.lastName && (
          <View style={styles.detailItem}>
            <Icon name="person-outline" size={20} color="#666" />
            <View style={styles.detailContent}>
              <Text style={styles.detailLabel}>Soyad</Text>
              <Text style={styles.detailValue}>{user.lastName}</Text>
            </View>
          </View>
        )}

        <View style={styles.detailItem}>
          <Icon name="card-outline" size={20} color="#666" />
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
          <Icon name="bag-outline" size={20} color="#666" />
          <Text style={styles.menuText}>Siparişlerim</Text>
          <Icon name="chevron-forward" size={20} color="#ccc" />
        </TouchableOpacity>

        <TouchableOpacity style={styles.menuItem} onPress={() => Alert.alert('Bilgi', 'Bu özellik henüz mevcut değil.')}>
          <Icon name="heart-outline" size={20} color="#666" />
          <Text style={styles.menuText}>Favorilerim</Text>
          <Icon name="chevron-forward" size={20} color="#ccc" />
        </TouchableOpacity>

        <TouchableOpacity style={styles.menuItem} onPress={() => Alert.alert('Bilgi', 'Bu özellik henüz mevcut değil.')}>
          <Icon name="location-outline" size={20} color="#666" />
          <Text style={styles.menuText}>Adreslerim</Text>
          <Icon name="chevron-forward" size={20} color="#ccc" />
        </TouchableOpacity>

        <TouchableOpacity style={styles.menuItem} onPress={() => Alert.alert('Bilgi', 'Bu özellik henüz mevcut değil.')}>
          <Icon name="settings-outline" size={20} color="#666" />
          <Text style={styles.menuText}>Ayarlar</Text>
          <Icon name="chevron-forward" size={20} color="#ccc" />
        </TouchableOpacity>

        <TouchableOpacity style={styles.menuItem} onPress={() => Alert.alert('Bilgi', 'Bu özellik henüz mevcut değil.')}>
          <Icon name="help-circle-outline" size={20} color="#666" />
          <Text style={styles.menuText}>Yardım</Text>
          <Icon name="chevron-forward" size={20} color="#ccc" />
        </TouchableOpacity>
      </View>

      {/* Logout Button */}
      <View style={styles.logoutSection}>
        <TouchableOpacity style={styles.logoutButton} onPress={handleLogout}>
          <Icon name="log-out-outline" size={20} color="#F44336" />
          <Text style={styles.logoutText}>Çıkış Yap</Text>
        </TouchableOpacity>
      </View>
    </ScrollView>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#f5f5f5',
  },
  loadingContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
  },
  loadingText: {
    marginTop: 10,
    fontSize: 16,
    color: '#666',
  },
  userSection: {
    backgroundColor: 'white',
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
    color: '#333',
    marginBottom: 5,
  },
  userEmail: {
    fontSize: 16,
    color: '#666',
  },
  detailsSection: {
    backgroundColor: 'white',
    padding: 20,
    marginBottom: 20,
  },
  sectionTitle: {
    fontSize: 18,
    fontWeight: 'bold',
    color: '#333',
    marginBottom: 15,
  },
  detailItem: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingVertical: 12,
    borderBottomWidth: 1,
    borderBottomColor: '#f0f0f0',
  },
  detailContent: {
    marginLeft: 15,
    flex: 1,
  },
  detailLabel: {
    fontSize: 14,
    color: '#666',
    marginBottom: 2,
  },
  detailValue: {
    fontSize: 16,
    fontWeight: '600',
    color: '#333',
  },
  menuSection: {
    backgroundColor: 'white',
    padding: 20,
    marginBottom: 20,
  },
  menuItem: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingVertical: 15,
    borderBottomWidth: 1,
    borderBottomColor: '#f0f0f0',
  },
  menuText: {
    fontSize: 16,
    color: '#333',
    marginLeft: 15,
    flex: 1,
  },
  logoutSection: {
    backgroundColor: 'white',
    padding: 20,
    marginBottom: 30,
  },
  logoutButton: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    backgroundColor: '#FFF5F5',
    padding: 15,
    borderRadius: 10,
    borderWidth: 1,
    borderColor: '#F44336',
  },
  logoutText: {
    fontSize: 16,
    fontWeight: '600',
    color: '#F44336',
    marginLeft: 10,
  },
});

export default ProfileScreen;
