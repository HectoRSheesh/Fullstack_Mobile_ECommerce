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
  Image,
  ScrollView,
  SafeAreaView,
} from 'react-native';
import Icon from 'react-native-vector-icons/Ionicons';
import { useFocusEffect } from '@react-navigation/native';
import { cartService, orderService } from '../services/api';
import { storage } from '../utils/storage';
import { CartItem, CartSummary } from '../types';
import { useTheme } from '../contexts/ThemeContext';

const CartScreen: React.FC = () => {
  const { colors } = useTheme();
  const [cartItems, setCartItems] = useState<CartItem[]>([]);
  const [cartSummary, setCartSummary] = useState<CartSummary | null>(null);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [updating, setUpdating] = useState<number | null>(null);
  const [ordering, setOrdering] = useState(false);

  useEffect(() => {
    loadCartItems();
  }, []);

  // Sayfa her odaklandığında sepeti yenile
  useFocusEffect(
    React.useCallback(() => {
      console.log('🛒 Cart screen focused, refreshing cart...');
      loadCartItems();
    }, [])
  );

  const loadCartItems = async () => {
    try {
      console.log('🛒 Loading cart summary...');
      
      // Check if we have a token
      const token = await storage.getAuthToken();
      console.log('🛒 Auth token available:', token ? 'Yes' : 'No');
      if (token) {
        console.log('🛒 Token preview:', token.substring(0, 20) + '...');
      } else {
        console.error('🛒 No auth token found - user not logged in');
        Alert.alert('Giriş Gerekli', 'Sepeti görüntülemek için lütfen giriş yapın.');
        return;
      }
      
      const summary = await cartService.getCartSummary();
      console.log('🛒 Cart summary loaded successfully:', summary);
      setCartSummary(summary);
      setCartItems(summary.items);
    } catch (error) {
      console.error('Error loading cart items:', error);
      
      // More detailed error logging
      if (error instanceof Error) {
        console.error('Error message:', error.message);
        console.error('Error stack:', error.stack);
        Alert.alert('Hata', error.message);
      } else {
        Alert.alert('Hata', 'Sepet yüklenirken bilinmeyen bir hata oluştu');
      }
    } finally {
      setLoading(false);
    }
  };

  const onRefresh = async () => {
    setRefreshing(true);
    await loadCartItems();
    setRefreshing(false);
  };

  const updateQuantity = async (itemId: number, newQuantity: number) => {
    if (newQuantity <= 0) {
      removeItem(itemId);
      return;
    }

    setUpdating(itemId);
    try {
      const updatedSummary = await cartService.updateCartItem(itemId, newQuantity);
      setCartSummary(updatedSummary);
      setCartItems(updatedSummary.items);
    } catch (error) {
      console.error('Error updating cart item:', error);
      Alert.alert('Hata', 'Ürün güncellenirken bir hata oluştu.');
    } finally {
      setUpdating(null);
    }
  };

  const removeItem = async (itemId: number) => {
    Alert.alert(
      'Ürünü Kaldır',
      'Bu ürünü sepetten kaldırmak istediğinizden emin misiniz?',
      [
        { text: 'İptal', style: 'cancel' },
        {
          text: 'Kaldır',
          style: 'destructive',
          onPress: async () => {
            try {
              await cartService.removeFromCart(itemId);
              await loadCartItems();
            } catch (error) {
              console.error('Error removing cart item:', error);
              Alert.alert('Hata', 'Ürün kaldırılırken bir hata oluştu.');
            }
          },
        },
      ]
    );
  };

  const clearCart = async () => {
    Alert.alert(
      'Sepeti Temizle',
      'Sepetteki tüm ürünleri kaldırmak istediğinizden emin misiniz?',
      [
        { text: 'İptal', style: 'cancel' },
        {
          text: 'Temizle',
          style: 'destructive',
          onPress: async () => {
            try {
              await cartService.clearCart();
              await loadCartItems();
            } catch (error) {
              console.error('Error clearing cart:', error);
              Alert.alert('Hata', 'Sepet temizlenirken bir hata oluştu.');
            }
          },
        },
      ]
    );
  };

  const createOrder = async () => {
    if (cartItems.length === 0) {
      Alert.alert('Uyarı', 'Sepetiniz boş.');
      return;
    }

    setOrdering(true);
    try {
      await orderService.createOrderFromCart();
      Alert.alert(
        'Başarılı',
        'Siparişiniz oluşturuldu!',
        [{ text: 'Tamam', onPress: () => loadCartItems() }]
      );
    } catch (error) {
      console.error('Error creating order:', error);
      Alert.alert('Hata', 'Sipariş oluşturulurken bir hata oluştu.');
    } finally {
      setOrdering(false);
    }
  };

  const calculateTotal = () => {
    return cartSummary?.totalAmount || 0;
  };

  const renderCartItem = ({ item }: { item: CartItem }) => (
    <View style={[styles.cartItem, { backgroundColor: colors.card, borderColor: colors.border }]}>
      <Image
        source={{ uri: item.productImageUrl || 'https://via.placeholder.com/80' }}
        style={styles.productImage}
        resizeMode="cover"
      />
      
      <View style={styles.productInfo}>
        <Text style={[styles.productName, { color: colors.text }]} numberOfLines={2}>
          {item.productName || 'Ürün'}
        </Text>
        <Text style={[styles.productPrice, { color: colors.primary }]}>₺{item.productPrice.toFixed(2)}</Text>
      </View>

      <View style={styles.quantityContainer}>
        <TouchableOpacity
          style={[styles.quantityButton, updating === item.id && styles.quantityButtonDisabled, { borderColor: colors.border }]}
          onPress={() => updateQuantity(item.id, item.quantity - 1)}
          disabled={updating === item.id}
        >
          <Icon name="remove" size={16} color={updating === item.id ? colors.border : colors.text} />
        </TouchableOpacity>
        
        <Text style={[styles.quantityText, { color: colors.text }]}>{item.quantity}</Text>
        
        <TouchableOpacity
          style={[styles.quantityButton, updating === item.id && styles.quantityButtonDisabled, { borderColor: colors.border }]}
          onPress={() => updateQuantity(item.id, item.quantity + 1)}
          disabled={updating === item.id}
        >
          <Icon name="add" size={16} color={updating === item.id ? colors.border : colors.text} />
        </TouchableOpacity>
      </View>

      <TouchableOpacity
        style={styles.removeButton}
        onPress={() => removeItem(item.id)}
      >
        <Icon name="trash-outline" size={20} color="#F44336" />
      </TouchableOpacity>
    </View>
  );

  if (loading) {
    return (
      <SafeAreaView style={[styles.container, { backgroundColor: colors.background }]}>
        <View style={styles.loadingContainer}>
          <ActivityIndicator size="large" color={colors.primary} />
          <Text style={[styles.loadingText, { color: colors.text }]}>Sepet yükleniyor...</Text>
        </View>
      </SafeAreaView>
    );
  }

  if (cartItems.length === 0) {
    return (
      <SafeAreaView style={[styles.container, { backgroundColor: colors.background }]}>
        <View style={styles.emptyContainer}>
          <Icon name="bag-outline" size={80} color={colors.border} />
          <Text style={[styles.emptyText, { color: colors.text }]}>Sepetiniz boş</Text>
          <Text style={[styles.emptySubText, { color: colors.text }]}>Ürün eklemek için alışverişe başlayın</Text>
        </View>
      </SafeAreaView>
    );
  }

  return (
    <SafeAreaView style={[styles.container, { backgroundColor: colors.background }]}>
      {/* Header */}
      <View style={[styles.header, { backgroundColor: colors.card, borderBottomColor: colors.border }]}>
        <Text style={[styles.headerTitle, { color: colors.text }]}>Sepetim ({cartItems.length} ürün)</Text>
        <TouchableOpacity onPress={clearCart}>
          <Text style={[styles.clearText, { color: colors.primary }]}>Temizle</Text>
        </TouchableOpacity>
      </View>

      {/* Cart Items */}
      <FlatList
        data={cartItems}
        renderItem={renderCartItem}
        keyExtractor={(item) => item.id.toString()}
        contentContainerStyle={styles.cartList}
        refreshControl={
          <RefreshControl refreshing={refreshing} onRefresh={onRefresh} />
        }
      />

      {/* Footer */}
      <View style={[styles.footer, { backgroundColor: colors.card, borderTopColor: colors.border }]}>
        {cartSummary && (
          <View style={styles.summaryContainer}>
            <View style={styles.summaryRow}>
              <Text style={[styles.summaryLabel, { color: colors.text }]}>Ara Toplam:</Text>
              <Text style={[styles.summaryValue, { color: colors.text }]}>₺{cartSummary.totalAmount.toFixed(2)}</Text>
            </View>
            {cartSummary.shippingCost > 0 && (
              <View style={styles.summaryRow}>
                <Text style={[styles.summaryLabel, { color: colors.text }]}>Kargo:</Text>
                <Text style={[styles.summaryValue, { color: colors.text }]}>₺{cartSummary.shippingCost.toFixed(2)}</Text>
              </View>
            )}
            {cartSummary.taxAmount > 0 && (
              <View style={styles.summaryRow}>
                <Text style={[styles.summaryLabel, { color: colors.text }]}>KDV:</Text>
                <Text style={[styles.summaryValue, { color: colors.text }]}>₺{cartSummary.taxAmount.toFixed(2)}</Text>
              </View>
            )}
            <View style={[styles.summaryRow, styles.totalRow]}>
              <Text style={[styles.totalLabel, { color: colors.text }]}>Toplam:</Text>
              <Text style={[styles.totalPrice, { color: colors.primary }]}>₺{cartSummary.grandTotal.toFixed(2)}</Text>
            </View>
          </View>
        )}
        
        <TouchableOpacity
          style={[styles.orderButton, { backgroundColor: colors.primary }, ordering && styles.orderButtonDisabled]}
          onPress={createOrder}
          disabled={ordering}
        >
          {ordering ? (
            <ActivityIndicator color="white" />
          ) : (
            <>
              <Icon name="card" size={20} color="white" style={styles.orderIcon} />
              <Text style={styles.orderButtonText}>Sipariş Ver</Text>
            </>
          )}
        </TouchableOpacity>
      </View>
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
    color: '#666',
  },
  emptyContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    padding: 20,
  },
  emptyText: {
    fontSize: 20,
    fontWeight: 'bold',
    color: '#666',
    marginTop: 20,
    marginBottom: 10,
  },
  emptySubText: {
    fontSize: 16,
    color: '#999',
    textAlign: 'center',
  },
  header: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    padding: 20,
    backgroundColor: 'white',
    shadowColor: '#000',
    shadowOffset: {
      width: 0,
      height: 1,
    },
    shadowOpacity: 0.22,
    shadowRadius: 2.22,
    elevation: 3,
  },
  headerTitle: {
    fontSize: 18,
    fontWeight: 'bold',
    color: '#333',
  },
  clearText: {
    fontSize: 16,
    color: '#F44336',
    fontWeight: '600',
  },
  cartList: {
    padding: 15,
  },
  cartItem: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: 'white',
    padding: 15,
    borderRadius: 12,
    marginBottom: 15,
    shadowColor: '#000',
    shadowOffset: {
      width: 0,
      height: 1,
    },
    shadowOpacity: 0.22,
    shadowRadius: 2.22,
    elevation: 3,
  },
  productImage: {
    width: 60,
    height: 60,
    borderRadius: 8,
    marginRight: 12,
  },
  productInfo: {
    flex: 1,
    marginRight: 10,
  },
  productName: {
    fontSize: 16,
    fontWeight: '600',
    color: '#333',
    marginBottom: 4,
  },
  productPrice: {
    fontSize: 16,
    fontWeight: 'bold',
    color: '#007AFF',
  },
  quantityContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    borderWidth: 1,
    borderColor: '#ddd',
    borderRadius: 6,
    marginRight: 10,
  },
  quantityButton: {
    padding: 8,
    justifyContent: 'center',
    alignItems: 'center',
  },
  quantityButtonDisabled: {
    opacity: 0.5,
  },
  quantityText: {
    fontSize: 16,
    fontWeight: 'bold',
    marginHorizontal: 12,
    minWidth: 20,
    textAlign: 'center',
  },
  removeButton: {
    padding: 8,
  },
  footer: {
    backgroundColor: 'white',
    padding: 20,
    shadowColor: '#000',
    shadowOffset: {
      width: 0,
      height: -1,
    },
    shadowOpacity: 0.22,
    shadowRadius: 2.22,
    elevation: 3,
  },
  totalContainer: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 15,
  },
  summaryContainer: {
    marginBottom: 15,
  },
  summaryRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 8,
  },
  summaryLabel: {
    fontSize: 16,
    color: '#666',
  },
  summaryValue: {
    fontSize: 16,
    fontWeight: '600',
    color: '#333',
  },
  totalRow: {
    borderTopWidth: 1,
    borderTopColor: '#eee',
    paddingTop: 8,
    marginTop: 8,
  },
  totalLabel: {
    fontSize: 18,
    fontWeight: '600',
    color: '#333',
  },
  totalPrice: {
    fontSize: 24,
    fontWeight: 'bold',
    color: '#007AFF',
  },
  orderButton: {
    backgroundColor: '#007AFF',
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    padding: 15,
    borderRadius: 10,
  },
  orderButtonDisabled: {
    backgroundColor: '#ccc',
  },
  orderIcon: {
    marginRight: 10,
  },
  orderButtonText: {
    color: 'white',
    fontSize: 18,
    fontWeight: 'bold',
  },
});

export default CartScreen;
