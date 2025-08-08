import React from 'react';
import {
  View,
  Text,
  StyleSheet,
  TouchableOpacity,
  ScrollView,
  SafeAreaView,
} from 'react-native';
import Icon from 'react-native-vector-icons/Ionicons';
import { useTheme, ThemeMode } from '../contexts/ThemeContext';

const ThemeSettingsScreen: React.FC = () => {
  const { themeMode, setThemeMode, colors } = useTheme();

  const themeOptions: { mode: ThemeMode; title: string; description: string; icon: string }[] = [
    {
      mode: 'light',
      title: 'Açık Tema',
      description: 'Her zaman açık tema kullan',
      icon: 'sunny-outline',
    },
    {
      mode: 'dark',
      title: 'Koyu Tema',
      description: 'Her zaman koyu tema kullan',
      icon: 'moon-outline',
    },
    {
      mode: 'system',
      title: 'Sistem Ayarı',
      description: 'Cihaz ayarlarına göre otomatik',
      icon: 'phone-portrait-outline',
    },
  ];

  const styles = StyleSheet.create({
    container: {
      flex: 1,
      backgroundColor: colors.background,
    },
    content: {
      flex: 1,
      padding: 16,
    },
    sectionTitle: {
      fontSize: 16,
      fontWeight: '600',
      color: colors.text,
      marginBottom: 16,
      marginTop: 8,
    },
    optionContainer: {
      backgroundColor: colors.card,
      borderRadius: 12,
      marginBottom: 12,
      overflow: 'hidden',
    },
    option: {
      flexDirection: 'row',
      alignItems: 'center',
      padding: 16,
      borderBottomWidth: StyleSheet.hairlineWidth,
      borderBottomColor: colors.border,
    },
    lastOption: {
      borderBottomWidth: 0,
    },
    optionIcon: {
      width: 24,
      height: 24,
      marginRight: 16,
    },
    optionContent: {
      flex: 1,
    },
    optionTitle: {
      fontSize: 16,
      fontWeight: '500',
      color: colors.text,
      marginBottom: 2,
    },
    optionDescription: {
      fontSize: 14,
      color: colors.textSecondary,
    },
    checkIcon: {
      marginLeft: 8,
    },
    previewSection: {
      marginTop: 24,
    },
    previewContainer: {
      backgroundColor: colors.card,
      borderRadius: 12,
      padding: 16,
      marginTop: 8,
    },
    previewTitle: {
      fontSize: 16,
      fontWeight: '600',
      color: colors.text,
      marginBottom: 8,
    },
    previewItem: {
      flexDirection: 'row',
      alignItems: 'center',
      paddingVertical: 8,
    },
    previewText: {
      flex: 1,
      fontSize: 14,
      color: colors.textSecondary,
      marginLeft: 12,
    },
  });

  return (
    <SafeAreaView style={styles.container}>
      <ScrollView style={styles.container}>
        <View style={styles.content}>
          <Text style={styles.sectionTitle}>Tema Seçenekleri</Text>
          
          <View style={styles.optionContainer}>
            {themeOptions.map((option, index) => (
              <TouchableOpacity
                key={option.mode}
                style={[
                  styles.option,
                  index === themeOptions.length - 1 && styles.lastOption,
                ]}
                onPress={() => setThemeMode(option.mode)}
              >
                <Icon
                  name={option.icon}
                  size={24}
                  color={colors.primary}
                  style={styles.optionIcon}
                />
                <View style={styles.optionContent}>
                  <Text style={styles.optionTitle}>{option.title}</Text>
                  <Text style={styles.optionDescription}>{option.description}</Text>
                </View>
                {themeMode === option.mode && (
                  <Icon
                    name="checkmark-circle"
                    size={24}
                    color={colors.primary}
                    style={styles.checkIcon}
                  />
                )}
              </TouchableOpacity>
            ))}
          </View>

          <View style={styles.previewSection}>
            <Text style={styles.sectionTitle}>Önizleme</Text>
            <View style={styles.previewContainer}>
              <Text style={styles.previewTitle}>Örnek Kart</Text>
              <View style={styles.previewItem}>
                <Icon name="home-outline" size={20} color={colors.primary} />
                <Text style={styles.previewText}>Ana Sayfa</Text>
              </View>
              <View style={styles.previewItem}>
                <Icon name="grid-outline" size={20} color={colors.primary} />
                <Text style={styles.previewText}>Ürünler</Text>
              </View>
              <View style={styles.previewItem}>
                <Icon name="bag-outline" size={20} color={colors.primary} />
                <Text style={styles.previewText}>Sepet</Text>
              </View>
            </View>
          </View>
        </View>
      </ScrollView>
    </SafeAreaView>
  );
};

export default ThemeSettingsScreen;
