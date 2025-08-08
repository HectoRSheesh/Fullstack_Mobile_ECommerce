import React from 'react';
import { NavigationContainer } from '@react-navigation/native';
import { createStackNavigator } from '@react-navigation/stack';
import { createBottomTabNavigator } from '@react-navigation/bottom-tabs';
import Icon from 'react-native-vector-icons/Ionicons';

// Screens
import LoginScreen from '../screens/LoginScreen';
import RegisterScreen from '../screens/RegisterScreen';
import HomeScreen from '../screens/HomeScreen';
import ProductsScreen from '../screens/ProductsScreen';
import ProductDetailScreen from '../screens/ProductDetailScreen';
import CartScreen from '../screens/CartScreen';
import ProfileScreen from '../screens/ProfileScreen';
import OrdersScreen from '../screens/OrdersScreen';

// Types
export type RootStackParamList = {
  Auth: undefined;
  Main: undefined;
  Login: undefined;
  Register: undefined;
};

export type MainTabParamList = {
  Home: undefined;
  Products: undefined;
  Cart: undefined;
  Orders: undefined;
  Profile: undefined;
};

export type MainStackParamList = {
  MainTabs: undefined;
  ProductDetail: { productId: number };
};

const Stack = createStackNavigator<RootStackParamList>();
const Tab = createBottomTabNavigator<MainTabParamList>();
const MainStack = createStackNavigator<MainStackParamList>();

// Auth Stack
const AuthStack = ({ onLogin }: { onLogin: () => void }) => (
  <Stack.Navigator screenOptions={{ headerShown: false }}>
    <Stack.Screen name="Login">
      {(props) => <LoginScreen {...props} onLogin={onLogin} />}
    </Stack.Screen>
    <Stack.Screen name="Register" component={RegisterScreen} />
  </Stack.Navigator>
);

// Main Tab Navigator
const MainTabs = ({ onLogout }: { onLogout: () => void }) => (
  <Tab.Navigator
    screenOptions={({ route }) => ({
      tabBarIcon: ({ focused, color, size }) => {
        let iconName;

        if (route.name === 'Home') {
          iconName = focused ? 'home' : 'home-outline';
        } else if (route.name === 'Products') {
          iconName = focused ? 'grid' : 'grid-outline';
        } else if (route.name === 'Cart') {
          iconName = focused ? 'bag' : 'bag-outline';
        } else if (route.name === 'Orders') {
          iconName = focused ? 'receipt' : 'receipt-outline';
        } else if (route.name === 'Profile') {
          iconName = focused ? 'person' : 'person-outline';
        }

        return <Icon name={iconName!} size={size} color={color} />;
      },
      tabBarActiveTintColor: '#007AFF',
      tabBarInactiveTintColor: 'gray',
      headerShown: false,
    })}
  >
    <Tab.Screen 
      name="Home" 
      component={HomeScreen}
      options={{ title: 'Ana Sayfa' }}
    />
    <Tab.Screen 
      name="Products" 
      component={ProductsScreen}
      options={{ title: 'Ürünler' }}
    />
    <Tab.Screen 
      name="Cart" 
      component={CartScreen}
      options={{ title: 'Sepet' }}
    />
    <Tab.Screen 
      name="Orders" 
      component={OrdersScreen}
      options={{ title: 'Siparişler' }}
    />
    <Tab.Screen 
      name="Profile"
      options={{ title: 'Profil' }}
    >
      {(props) => <ProfileScreen {...props} onLogout={onLogout} />}
    </Tab.Screen>
  </Tab.Navigator>
);

// Main Stack Navigator (contains tabs and other screens)
const MainStackNavigator = ({ onLogout }: { onLogout: () => void }) => (
  <MainStack.Navigator>
    <MainStack.Screen 
      name="MainTabs" 
      options={{ headerShown: false }}
    >
      {(props) => <MainTabs {...props} onLogout={onLogout} />}
    </MainStack.Screen>
    <MainStack.Screen 
      name="ProductDetail" 
      component={ProductDetailScreen}
      options={{
        title: 'Ürün Detayı',
        headerStyle: {
          backgroundColor: '#007AFF',
        },
        headerTintColor: '#fff',
        headerTitleStyle: {
          fontWeight: 'bold',
        },
      }}
    />
  </MainStack.Navigator>
);

// Root Navigator
interface AppNavigatorProps {
  isAuthenticated: boolean;
  onLogin: () => void;
  onLogout: () => void;
}

const AppNavigator: React.FC<AppNavigatorProps> = ({ 
  isAuthenticated, 
  onLogin, 
  onLogout 
}) => {
  return (
    <NavigationContainer>
      {isAuthenticated ? (
        <MainStackNavigator onLogout={onLogout} />
      ) : (
        <AuthStack onLogin={onLogin} />
      )}
    </NavigationContainer>
  );
};

export default AppNavigator;
