import React, { useState, useEffect } from 'react';
import {
  View,
  Text,
  ScrollView,
  StyleSheet,
  TouchableOpacity,
  ActivityIndicator,
  Alert,
  Image,
  Dimensions,
} from 'react-native';
import Icon from 'react-native-vector-icons/Ionicons';
import { StackNavigationProp } from '@react-navigation/stack';
import { RouteProp } from '@react-navigation/native';
import { productService, cartService } from '../services/api';
import { storage } from '../utils/storage';
import { Product } from '../types';
import { MainStackParamList } from '../navigation/AppNavigator';

const { width } = Dimensions.get('window');

type ProductDetailScreenNavigationProp = StackNavigationProp<MainStackParamList, 'ProductDetail'>;
type ProductDetailScreenRouteProp = RouteProp<MainStackParamList, 'ProductDetail'>;

interface Props {
  navigation: ProductDetailScreenNavigationProp;
  route: ProductDetailScreenRouteProp;
}

const ProductDetailScreen: React.FC<Props> = ({ navigation, route }) => {
  const { productId } = route.params;
  const [product, setProduct] = useState<Product | null>(null);
  const [loading, setLoading] = useState(true);
  const [addingToCart, setAddingToCart] = useState(false);
  const [quantity, setQuantity] = useState(1);

  useEffect(() => {
    loadProduct();
  }, [productId]);

  const loadProduct = async () => {
    try {
      const productData = await productService.getProductById(productId);
      setProduct(productData);
    } catch (error) {
      console.error('Error loading product:', error);
      Alert.alert('Hata', 'Ürün bilgileri yüklenirken bir hata oluştu.');
    } finally {
      setLoading(false);
    }
  };

  const handleAddToCart = async () => {
    if (!product) return;

    setAddingToCart(true);
    try {
      console.log('🛒 Adding product to cart:', { productId: product.id, productName: product.name, quantity });
      
      // Check authentication before adding to cart
      const token = await storage.getAuthToken();
      console.log('🛒 Auth token available in ProductDetail:', token ? 'Yes' : 'No');
      
      await cartService.addToCart({ productId: product.id, quantity });
      console.log('🛒 Product added to cart successfully');
      
      Alert.alert(
        'Başarılı', 
        'Ürün sepete eklendi!',
        [
          { text: 'Alışverişe Devam', style: 'cancel' },
          { 
            text: 'Sepete Git', 
            onPress: () => {
              // MainTabs'a dön ve Cart tab'ını aç
              navigation.navigate('MainTabs');
              // Tab navigator için global state yöneticisi kullanmak gerekebilir
              // Şimdilik sadece ana sayfaya dönelim
            },
            style: 'default'
          }
        ]
      );
    } catch (error) {
      console.error('🛒 Error adding to cart:', error);
      if (error instanceof Error) {
        console.error('🛒 Error message:', error.message);
      }
      Alert.alert('Hata', 'Ürün sepete eklenirken bir hata oluştu: ' + (error instanceof Error ? error.message : 'Bilinmeyen hata'));
    } finally {
      setAddingToCart(false);
    }
  };

  const incrementQuantity = () => {
    if (product && quantity < product.stockQuantity) {
      setQuantity(quantity + 1);
    }
  };

  const decrementQuantity = () => {
    if (quantity > 1) {
      setQuantity(quantity - 1);
    }
  };

  if (loading) {
    return (
      <View style={styles.loadingContainer}>
        <ActivityIndicator size="large" color="#007AFF" />
        <Text style={styles.loadingText}>Ürün yükleniyor...</Text>
      </View>
    );
  }

  if (!product) {
    return (
      <View style={styles.errorContainer}>
        <Text style={styles.errorText}>Ürün bulunamadı</Text>
        <TouchableOpacity style={styles.backButton} onPress={() => navigation.goBack()}>
          <Text style={styles.backButtonText}>Geri Dön</Text>
        </TouchableOpacity>
      </View>
    );
  }

  return (
    <View style={styles.container}>
      <ScrollView style={styles.content}>
        {/* Product Image */}
        <Image
          source={{ uri: product.imageUrl || 'https://via.placeholder.com/300' }}
          style={styles.productImage}
          resizeMode="cover"
        />

        {/* Product Info */}
        <View style={styles.productInfo}>
          <Text style={styles.productName}>{product.name}</Text>
          <Text style={styles.productPrice}>₺{product.price.toFixed(2)}</Text>
          
          {product.categoryName && (
            <View style={styles.categoryContainer}>
              <Text style={styles.categoryLabel}>Kategori: </Text>
              <Text style={styles.categoryName}>{product.categoryName}</Text>
            </View>
          )}

          <Text style={styles.stockInfo}>
            Stok: {product.stockQuantity} adet
          </Text>

          <Text style={styles.descriptionTitle}>Açıklama</Text>
          <Text style={styles.productDescription}>{product.description || 'Açıklama mevcut değil'}</Text>

          {/* Quantity Selector */}
          <View style={styles.quantityContainer}>
            <Text style={styles.quantityLabel}>Adet:</Text>
            <View style={styles.quantitySelector}>
              <TouchableOpacity
                style={[styles.quantityButton, quantity <= 1 && styles.quantityButtonDisabled]}
                onPress={decrementQuantity}
                disabled={quantity <= 1}
              >
                <Icon name="remove" size={20} color={quantity <= 1 ? "#ccc" : "#333"} />
              </TouchableOpacity>
              <Text style={styles.quantityText}>{quantity}</Text>
              <TouchableOpacity
                style={[styles.quantityButton, quantity >= product.stockQuantity && styles.quantityButtonDisabled]}
                onPress={incrementQuantity}
                disabled={quantity >= product.stockQuantity}
              >
                <Icon name="add" size={20} color={quantity >= product.stockQuantity ? "#ccc" : "#333"} />
              </TouchableOpacity>
            </View>
          </View>
        </View>
      </ScrollView>

      {/* Add to Cart Button */}
      <View style={styles.footer}>
        <TouchableOpacity
          style={[styles.addToCartButton, (addingToCart || product.stockQuantity === 0) && styles.addToCartButtonDisabled]}
          onPress={handleAddToCart}
          disabled={addingToCart || product.stockQuantity === 0}
        >
          {addingToCart ? (
            <ActivityIndicator color="white" />
          ) : (
            <>
              <Icon name="bag-add" size={20} color="white" style={styles.addToCartIcon} />
              <Text style={styles.addToCartText}>
                {product.stockQuantity === 0 ? 'Stokta Yok' : `Sepete Ekle - ₺${(product.price * quantity).toFixed(2)}`}
              </Text>
            </>
          )}
        </TouchableOpacity>
      </View>
    </View>
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
  errorContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    padding: 20,
  },
  errorText: {
    fontSize: 18,
    color: '#666',
    marginBottom: 20,
  },
  backButton: {
    backgroundColor: '#007AFF',
    paddingHorizontal: 20,
    paddingVertical: 10,
    borderRadius: 8,
  },
  backButtonText: {
    color: 'white',
    fontSize: 16,
    fontWeight: '600',
  },
  header: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    padding: 15,
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
  backIconButton: {
    padding: 5,
  },
  headerTitle: {
    fontSize: 18,
    fontWeight: 'bold',
    color: '#333',
  },
  placeholder: {
    width: 34,
  },
  content: {
    flex: 1,
  },
  productImage: {
    width: width,
    height: width * 0.8,
  },
  productInfo: {
    backgroundColor: 'white',
    padding: 20,
    borderTopLeftRadius: 20,
    borderTopRightRadius: 20,
    marginTop: -20,
  },
  productName: {
    fontSize: 24,
    fontWeight: 'bold',
    color: '#333',
    marginBottom: 10,
  },
  productPrice: {
    fontSize: 28,
    fontWeight: 'bold',
    color: '#007AFF',
    marginBottom: 15,
  },
  categoryContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    marginBottom: 10,
  },
  categoryLabel: {
    fontSize: 16,
    color: '#666',
  },
  categoryName: {
    fontSize: 16,
    fontWeight: '600',
    color: '#007AFF',
  },
  stockInfo: {
    fontSize: 16,
    color: '#4CAF50',
    marginBottom: 20,
  },
  descriptionTitle: {
    fontSize: 18,
    fontWeight: 'bold',
    color: '#333',
    marginBottom: 10,
  },
  productDescription: {
    fontSize: 16,
    color: '#666',
    lineHeight: 24,
    marginBottom: 20,
  },
  quantityContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    marginBottom: 20,
  },
  quantityLabel: {
    fontSize: 16,
    fontWeight: '600',
    color: '#333',
    marginRight: 15,
  },
  quantitySelector: {
    flexDirection: 'row',
    alignItems: 'center',
    borderWidth: 1,
    borderColor: '#ddd',
    borderRadius: 8,
  },
  quantityButton: {
    padding: 10,
    justifyContent: 'center',
    alignItems: 'center',
  },
  quantityButtonDisabled: {
    opacity: 0.5,
  },
  quantityText: {
    fontSize: 18,
    fontWeight: 'bold',
    marginHorizontal: 20,
    minWidth: 30,
    textAlign: 'center',
  },
  footer: {
    padding: 20,
    backgroundColor: 'white',
    shadowColor: '#000',
    shadowOffset: {
      width: 0,
      height: -1,
    },
    shadowOpacity: 0.22,
    shadowRadius: 2.22,
    elevation: 3,
  },
  addToCartButton: {
    backgroundColor: '#007AFF',
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    padding: 15,
    borderRadius: 10,
  },
  addToCartButtonDisabled: {
    backgroundColor: '#ccc',
  },
  addToCartIcon: {
    marginRight: 10,
  },
  addToCartText: {
    color: 'white',
    fontSize: 18,
    fontWeight: 'bold',
  },
});

export default ProductDetailScreen;
