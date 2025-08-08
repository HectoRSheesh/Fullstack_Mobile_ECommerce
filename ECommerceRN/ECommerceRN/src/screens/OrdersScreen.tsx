import React, { useState, useEffect } from 'react';
import {
  View,
  Text,
  FlatList,
  StyleSheet,
  TouchableOpacity,
  ActivityIndicator,
  Alert,
  RefreshControl,
  SafeAreaView,
} from 'react-native';
import Icon from 'react-native-vector-icons/Ionicons';
import { useNavigation, useFocusEffect } from '@react-navigation/native';
import { StackNavigationProp } from '@react-navigation/stack';
import { orderService } from '../services/api';
import { storage } from '../utils/storage';
import { Order } from '../types';
import { MainStackParamList } from '../navigation/AppNavigator';
import { useTheme } from '../contexts/ThemeContext';

type OrdersScreenNavigationProp = StackNavigationProp<MainStackParamList>;

const OrdersScreen: React.FC = () => {
  const { colors } = useTheme();
  const navigation = useNavigation<OrdersScreenNavigationProp>();
  const [orders, setOrders] = useState<Order[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);

  useEffect(() => {
    loadOrders();
  }, []);

  // Sayfa her odaklandığında siparişleri yenile
  useFocusEffect(
    React.useCallback(() => {
      console.log('📋 Orders screen focused, refreshing orders...');
      loadOrders();
    }, [])
  );

  const loadOrders = async () => {
    try {
      console.log('📋 Loading orders...');
      
      // Check if we have a token
      const token = await storage.getAuthToken();
      if (!token) {
        console.error('📋 No auth token found');
        Alert.alert('Giriş Gerekli', 'Siparişlerinizi görüntülemek için lütfen giriş yapın.');
        return;
      }
      
      const ordersData = await orderService.getSimpleOrderList();
      console.log('📋 Orders loaded successfully:', ordersData);
      setOrders(ordersData);
    } catch (error) {
      console.error('Error loading orders:', error);
      
      if (error instanceof Error) {
        console.error('Error message:', error.message);
        Alert.alert('Hata', error.message);
      } else {
        Alert.alert('Hata', 'Siparişler yüklenirken bilinmeyen bir hata oluştu');
      }
    } finally {
      setLoading(false);
    }
  };

  const onRefresh = async () => {
    setRefreshing(true);
    await loadOrders();
    setRefreshing(false);
  };

  const getStatusColor = (status: string) => {
    switch (status.toLowerCase()) {
      case 'pending':
      case 'beklemede':
        return '#FF9500';
      case 'processing':
      case 'hazırlanıyor':
        return '#007AFF';
      case 'shipped':
      case 'kargoya verildi':
        return '#34C759';
      case 'delivered':
      case 'teslim edildi':
        return '#00C851';
      case 'cancelled':
      case 'iptal edildi':
        return '#FF3B30';
      default:
        return '#666';
    }
  };

  const getStatusText = (status: string) => {
    switch (status.toLowerCase()) {
      case 'pending':
        return 'Beklemede';
      case 'processing':
        return 'Hazırlanıyor';
      case 'shipped':
        return 'Kargoya Verildi';
      case 'delivered':
        return 'Teslim Edildi';
      case 'cancelled':
        return 'İptal Edildi';
      default:
        return status;
    }
  };

  const renderOrder = ({ item }: { item: Order }) => (
    <TouchableOpacity
      style={[styles.orderCard, { backgroundColor: colors.card, borderColor: colors.border }]}
      onPress={() => {
        // Order detail sayfasına yönlendir
        console.log('Order pressed:', item.id);
      }}
    >
      <View style={styles.orderHeader}>
        <View>
          <Text style={[styles.orderNumber, { color: colors.text }]}>#{item.orderNumber}</Text>
          <Text style={[styles.orderDate, { color: colors.text, opacity: 0.7 }]}>
            {new Date(item.orderDate).toLocaleDateString('tr-TR')}
          </Text>
        </View>
        <View style={[styles.statusBadge, { backgroundColor: getStatusColor(item.status) }]}>
          <Text style={styles.statusText}>{getStatusText(item.status)}</Text>
        </View>
      </View>
      
      <View style={styles.orderInfo}>
        <Text style={[styles.itemCount, { color: colors.text, opacity: 0.8 }]}>{item.totalItems} ürün</Text>
        <Text style={[styles.orderTotal, { color: colors.primary }]}>₺{item.totalAmount.toFixed(2)}</Text>
      </View>
      
      <View style={styles.orderFooter}>
        <Text style={[styles.shippingAddress, { color: colors.text, opacity: 0.7 }]} numberOfLines={1}>
          {item.shippingAddress}, {item.shippingCity}
        </Text>
        <Icon name="chevron-forward" size={16} color={colors.text} />
      </View>
    </TouchableOpacity>
  );

  if (loading) {
    return (
      <SafeAreaView style={[styles.container, { backgroundColor: colors.background }]}>
        <View style={styles.loadingContainer}>
          <ActivityIndicator size="large" color={colors.primary} />
          <Text style={[styles.loadingText, { color: colors.text }]}>Siparişler yükleniyor...</Text>
        </View>
      </SafeAreaView>
    );
  }

  if (orders.length === 0) {
    return (
      <SafeAreaView style={[styles.container, { backgroundColor: colors.background }]}>
        <View style={styles.emptyContainer}>
          <Icon name="receipt-outline" size={80} color={colors.border} />
          <Text style={[styles.emptyText, { color: colors.text }]}>Henüz siparişiniz yok</Text>
          <Text style={[styles.emptySubText, { color: colors.text, opacity: 0.7 }]}>İlk siparişinizi vermek için alışverişe başlayın</Text>
        </View>
      </SafeAreaView>
    );
  }

  return (
    <SafeAreaView style={[styles.container, { backgroundColor: colors.background }]}>
      <View style={[styles.header, { backgroundColor: colors.card, borderBottomColor: colors.border }]}>
        <Text style={[styles.headerTitle, { color: colors.text }]}>Siparişlerim</Text>
        <Text style={[styles.headerSubtitle, { color: colors.text, opacity: 0.7 }]}>{orders.length} sipariş</Text>
      </View>
      
      <FlatList
        data={orders}
        renderItem={renderOrder}
        keyExtractor={(item) => item.id.toString()}
        contentContainerStyle={styles.ordersList}
        refreshControl={
          <RefreshControl refreshing={refreshing} onRefresh={onRefresh} tintColor={colors.primary} />
        }
      />
    </SafeAreaView>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
  },
  loadingContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
  },
  loadingText: {
    marginTop: 10,
    fontSize: 16,
  },
  emptyContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    padding: 20,
  },
  emptyText: {
    fontSize: 20,
    fontWeight: '600',
    marginTop: 20,
  },
  emptySubText: {
    fontSize: 16,
    textAlign: 'center',
    marginTop: 8,
  },
  header: {
    padding: 20,
    borderBottomWidth: 1,
  },
  headerTitle: {
    fontSize: 24,
    fontWeight: 'bold',
  },
  headerSubtitle: {
    fontSize: 16,
    marginTop: 4,
  },
  ordersList: {
    padding: 15,
  },
  orderCard: {
    borderRadius: 12,
    padding: 15,
    marginBottom: 15,
    borderWidth: 1,
    shadowColor: '#000',
    shadowOffset: {
      width: 0,
      height: 2,
    },
    shadowOpacity: 0.1,
    shadowRadius: 4,
    elevation: 3,
  },
  orderHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'flex-start',
    marginBottom: 12,
  },
  orderNumber: {
    fontSize: 18,
    fontWeight: 'bold',
  },
  orderDate: {
    fontSize: 14,
    marginTop: 2,
  },
  statusBadge: {
    paddingHorizontal: 12,
    paddingVertical: 6,
    borderRadius: 15,
  },
  statusText: {
    fontSize: 12,
    fontWeight: '600',
    color: 'white',
  },
  orderInfo: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 12,
  },
  itemCount: {
    fontSize: 16,
  },
  orderTotal: {
    fontSize: 20,
    fontWeight: 'bold',
  },
  orderFooter: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    paddingTop: 12,
    borderTopWidth: 1,
    borderTopColor: '#eee',
  },
  shippingAddress: {
    fontSize: 14,
    flex: 1,
    marginRight: 10,
  },
});

export default OrdersScreen;
