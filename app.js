// API Configuration
const API_BASE_URL = 'http://localhost:7038/api';

// Global State
let authToken = localStorage.getItem('authToken');
let currentUser = JSON.parse(localStorage.getItem('currentUser') || 'null');
let categories = [];
let products = [];
let cart = JSON.parse(localStorage.getItem('cart') || '[]');
let currentPage = 1;
let totalPages = 1;
let selectedCategoryId = null;
let searchQuery = '';

// Initialize Application
document.addEventListener('DOMContentLoaded', function() {
    console.log('DOM Content Loaded - Initializing app...');
    console.log('API Base URL:', API_BASE_URL);
    
    // API test'i yap
    testAPIConnection();
    
    initializeAuth();
    
    // API çağrılarını küçük bir gecikmeyle yap - DOM elementlerinin hazır olması için
    setTimeout(() => {
        loadCategories();
        loadProducts();
    }, 500);
    
    updateCartUI();
});

// API bağlantısını test et
async function testAPIConnection() {
    try {
        console.log('Testing API connection...');
        
        // Category test endpoint
        const categoryTest = await fetch(`${API_BASE_URL}/category/test`);
        console.log('Category test response:', await categoryTest.json());
        
        // Product test endpoint  
        const productTest = await fetch(`${API_BASE_URL}/product/test`);
        console.log('Product test response:', await productTest.json());
        
        // Auth test endpoint
        const authTest = await fetch(`${API_BASE_URL}/auth/test`);
        console.log('Auth test response:', await authTest.json());
        
    } catch (error) {
        console.error('API Test failed:', error);
        showAlert('API bağlantısı başarısız: ' + error.message, 'danger');
    }
}

// Register test fonksiyonu
async function testRegister() {
    const testData = {
        fullName: "Test User",
        username: "testuser",
        email: "test@test.com",
        password: "123456",
        confirmPassword: "123456",
        phone: "1234567890",
        address: "Test Address"
    };
    
    console.log('Testing register with:', testData);
    console.log('Request URL:', `${API_BASE_URL}/auth/register`);
    console.log('Request body:', JSON.stringify(testData));
    
    try {
        const response = await fetch(`${API_BASE_URL}/auth/register`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            },
            body: JSON.stringify(testData)
        });
        
        console.log('Test register response status:', response.status);
        console.log('Response headers:', response.headers);
        
        const responseText = await response.text();
        console.log('Test register response text:', responseText);
        
        try {
            const responseJson = JSON.parse(responseText);
            console.log('Test register response JSON:', responseJson);
        } catch (parseError) {
            console.log('Response is not valid JSON');
        }
        
        if (response.ok) {
            showAlert('Test kayıt başarılı!', 'success');
        } else {
            showAlert('Test kayıt başarısız: ' + responseText, 'danger');
        }
    } catch (error) {
        console.error('Test register error:', error);
        showAlert('Test register hatası: ' + error.message, 'danger');
    }
}

// Authentication Functions
function initializeAuth() {
    if (authToken && currentUser) {
        showUserMenu();
    } else {
        showAuthButtons();
    }
}

function showLogin() {
    const modal = new bootstrap.Modal(document.getElementById('loginModal'));
    modal.show();
}

function showRegister() {
    const modal = new bootstrap.Modal(document.getElementById('registerModal'));
    modal.show();
}

async function login() {
    const email = document.getElementById('loginEmail').value;
    const password = document.getElementById('loginPassword').value;

    if (!email || !password) {
        showAlert('Email ve şifre gerekli', 'warning');
        return;
    }

    try {
        const response = await fetch(`${API_BASE_URL}/auth/login`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ email, password })
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const data = await response.json();

        if (response.ok) {
            authToken = data.token;
            currentUser = data.user;
            localStorage.setItem('authToken', authToken);
            localStorage.setItem('currentUser', JSON.stringify(currentUser));
            
            showUserMenu();
            bootstrap.Modal.getInstance(document.getElementById('loginModal')).hide();
            showAlert('Başarıyla giriş yapıldı', 'success');
            
            // Clear form
            document.getElementById('loginForm').reset();
        } else {
            showAlert(data.message || 'Giriş başarısız', 'danger');
        }
    } catch (error) {
        console.error('Login error:', error);
        showAlert('API bağlantısı başarısız. Lütfen tekrar deneyin.', 'danger');
    }
}

async function register() {
    const fullName = document.getElementById('registerFullName').value;
    const email = document.getElementById('registerEmail').value;
    const password = document.getElementById('registerPassword').value;
    const phone = document.getElementById('registerPhone').value;
    const address = document.getElementById('registerAddress').value;

    if (!fullName || !email || !password) {
        showAlert('Ad soyad, email ve şifre gerekli', 'warning');
        return;
    }

    const registerData = {
        fullName,
        email,
        password,
        phone: phone || null,
        address: address || null
    };

    console.log('Register data:', registerData);

    try {
        console.log('Sending register request to:', `${API_BASE_URL}/auth/register`);
        
        const response = await fetch(`${API_BASE_URL}/auth/register`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(registerData)
        });

        console.log('Register response status:', response.status);
        console.log('Register response headers:', response.headers);

        if (!response.ok) {
            const errorText = await response.text();
            console.error('Register API Error Response:', errorText);
            
            // JSON parse etmeye çalış
            let errorData;
            try {
                errorData = JSON.parse(errorText);
                console.error('Parsed error data:', errorData);
            } catch (e) {
                console.error('Could not parse error as JSON:', e);
                errorData = { message: errorText };
            }
            
            // Validation hatalarını göster
            if (errorData.errors) {
                const errorMessages = errorData.errors.map(err => 
                    `${err.Field}: ${err.Errors.join(', ')}`
                ).join('\n');
                showAlert(`Doğrulama hataları:\n${errorMessages}`, 'danger');
            } else {
                showAlert(errorData.message || 'Kayıt başarısız', 'danger');
            }
            return;
        }

        const data = await response.json();
        console.log('Register response data:', data);

        bootstrap.Modal.getInstance(document.getElementById('registerModal')).hide();
        showAlert('Kayıt başarılı! Şimdi giriş yapabilirsiniz.', 'success');
        
        // Clear form
        document.getElementById('registerForm').reset();
        
        // Show login modal
        setTimeout(() => {
            showLogin();
        }, 2000);
    } catch (error) {
        console.error('Register error:', error);
        console.error('Error stack:', error.stack);
        showAlert('Kayıt başarısız: ' + error.message, 'danger');
    }
}

function logout() {
    authToken = null;
    currentUser = null;
    localStorage.removeItem('authToken');
    localStorage.removeItem('currentUser');
    cart = [];
    localStorage.removeItem('cart');
    
    showAuthButtons();
    updateCartUI();
    showAlert('Başarıyla çıkış yapıldı', 'info');
}

function showUserMenu() {
    document.getElementById('authButtons').classList.add('d-none');
    document.getElementById('userMenu').classList.remove('d-none');
    document.getElementById('userName').textContent = currentUser.fullName;
}

function showAuthButtons() {
    document.getElementById('authButtons').classList.remove('d-none');
    document.getElementById('userMenu').classList.add('d-none');
}

// Category Functions
async function loadCategories() {
    try {
        console.log('Loading categories from:', `${API_BASE_URL}/category`);
        
        const response = await fetch(`${API_BASE_URL}/category`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
            }
        });
        
        console.log('Categories response status:', response.status);
        console.log('Categories response headers:', response.headers);
        
        if (!response.ok) {
            const errorText = await response.text();
            console.error('API Error Response:', errorText);
            throw new Error(`HTTP error! status: ${response.status}, message: ${errorText}`);
        }
        
        const data = await response.json();
        console.log('Categories data received:', data);

        categories = data || [];
        renderCategories();
        
    } catch (error) {
        console.error('Error loading categories:', error);
        console.error('Error stack:', error.stack);
        
        // Boş kategoriler ile devam et
        categories = [];
        renderCategories();
        showAlert('Kategoriler yüklenemedi. Lütfen konsolu kontrol edin.', 'warning');
    }
}

function renderCategories() {
    const container = document.getElementById('categoriesList');
    
    if (categories.length === 0) {
        container.innerHTML = '<div class="p-3 text-muted">Kategori bulunamadı</div>';
        return;
    }

    const html = categories.map(category => `
        <a href="#" class="list-group-item list-group-item-action category-item ${selectedCategoryId === category.id ? 'active' : ''}" 
           onclick="selectCategory(${category.id}, '${category.name}')">
            <i class="fas fa-tag me-2"></i>${category.name}
            ${category.subcategories && category.subcategories.length > 0 ? 
                `<span class="badge bg-secondary ms-auto">${category.subcategories.length}</span>` : ''}
        </a>
        ${category.subcategories ? category.subcategories.map(sub => `
            <a href="#" class="list-group-item list-group-item-action category-item ps-4 ${selectedCategoryId === sub.id ? 'active' : ''}" 
               onclick="selectCategory(${sub.id}, '${sub.name}')">
                <i class="fas fa-chevron-right me-2"></i>${sub.name}
            </a>
        `).join('') : ''}
    `).join('');

    container.innerHTML = `
        <a href="#" class="list-group-item list-group-item-action category-item ${selectedCategoryId === null ? 'active' : ''}" 
           onclick="selectCategory(null, 'Tüm Ürünler')">
            <i class="fas fa-th me-2"></i>Tüm Ürünler
        </a>
        ${html}
    `;
}

function selectCategory(categoryId, categoryName) {
    console.log('selectCategory called with:', categoryId, categoryName);
    selectedCategoryId = categoryId;
    console.log('selectedCategoryId set to:', selectedCategoryId);
    currentPage = 1;
    renderCategories();
    loadProducts();
}

// Product Functions
async function loadProducts() {
    showLoading();
    
    const pageSize = document.getElementById('pageSizeSelect')?.value || 12;
    const sortBy = document.getElementById('sortSelect')?.value;
    
    let url = `${API_BASE_URL}/product?PageNumber=${currentPage}&PageSize=${pageSize}`;
    
    if (selectedCategoryId) {
        url += `&CategoryId=${selectedCategoryId}`;
    }
    
    if (searchQuery) {
        url += `&SearchTerm=${encodeURIComponent(searchQuery)}`;
    }
    
    if (sortBy) {
        url += `&SortBy=${sortBy}`;
    }

    try {
        console.log('Loading products from:', url);
        console.log('Selected category ID:', selectedCategoryId);
        
        const response = await fetch(url, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
            }
        });
        
        console.log('Products response status:', response.status);
        console.log('Products response headers:', response.headers);
        
        if (!response.ok) {
            const errorText = await response.text();
            console.error('API Error Response:', errorText);
            throw new Error(`HTTP error! status: ${response.status}, message: ${errorText}`);
        }
        
        const data = await response.json();
        console.log('Products data received:', data);
        console.log('Data structure:', {
            hasProducts: !!data.products,
            hasData: !!data.data,
            dataLength: data.data ? data.data.length : 'N/A',
            productsLength: data.products ? data.products.length : 'N/A',
            totalCount: data.totalCount
        });

        products = data.products || data.data || data || [];
        console.log('Final products array:', products);
        console.log('Products array length:', products.length);
        totalPages = data.totalPages || data.pageCount || 1;
        renderProducts();
        renderPagination();
        
    } catch (error) {
        console.error('Error loading products:', error);
        console.error('Error stack:', error.stack);
        
        // Boş ürünler ile devam et
        products = [];
        totalPages = 1;
        renderProducts();
        renderPagination();
        showError('Ürünler yüklenemedi. Konsolu kontrol edin: ' + error.message);
    }
}

function renderProducts() {
    const container = document.getElementById('productsGrid');
    
    if (products.length === 0) {
        container.innerHTML = `
            <div class="col-12">
                <div class="empty-state">
                    <i class="fas fa-box-open"></i>
                    <h5>Ürün bulunamadı</h5>
                    <p>Aradığınız kriterlere uygun ürün bulunmuyor.</p>
                </div>
            </div>
        `;
        return;
    }

    const html = products.map(product => `
        <div class="col-md-4 col-sm-6 mb-4">
            <div class="card product-card h-100">
                <div class="product-image">
                    <i class="fas fa-image"></i>
                </div>
                <div class="card-body d-flex flex-column">
                    <h6 class="card-title">${product.name}</h6>
                    <p class="card-text text-muted small">${product.description || 'Açıklama yok'}</p>
                    <div class="mt-auto">
                        <div class="d-flex justify-content-between align-items-center mb-2">
                            <span class="price">₺${product.price.toFixed(2)}</span>
                            <small class="stock-info ${getStockClass(product.stockQuantity)}">
                                ${getStockText(product.stockQuantity)}
                            </small>
                        </div>
                        <button class="btn btn-primary btn-sm w-100" 
                                onclick="addToCart(${product.id})" 
                                ${product.stockQuantity === 0 ? 'disabled' : ''}>
                            <i class="fas fa-cart-plus me-1"></i>
                            ${product.stockQuantity === 0 ? 'Stokta Yok' : 'Sepete Ekle'}
                        </button>
                    </div>
                </div>
            </div>
        </div>
    `).join('');

    container.innerHTML = html;
}

function getStockClass(stock) {
    if (stock === 0) return 'stock-out';
    if (stock < 10) return 'stock-low';
    return 'stock-available';
}

function getStockText(stock) {
    if (stock === 0) return 'Stokta Yok';
    if (stock < 10) return `Son ${stock} adet`;
    return 'Stokta Var';
}

function renderPagination() {
    const container = document.getElementById('pagination');
    
    if (totalPages <= 1) {
        container.innerHTML = '';
        return;
    }

    let html = '';
    
    // Previous button
    html += `
        <li class="page-item ${currentPage === 1 ? 'disabled' : ''}">
            <a class="page-link" href="#" onclick="changePage(${currentPage - 1})">Önceki</a>
        </li>
    `;
    
    // Page numbers
    for (let i = 1; i <= totalPages; i++) {
        if (i === 1 || i === totalPages || (i >= currentPage - 2 && i <= currentPage + 2)) {
            html += `
                <li class="page-item ${i === currentPage ? 'active' : ''}">
                    <a class="page-link" href="#" onclick="changePage(${i})">${i}</a>
                </li>
            `;
        } else if (i === currentPage - 3 || i === currentPage + 3) {
            html += '<li class="page-item disabled"><span class="page-link">...</span></li>';
        }
    }
    
    // Next button
    html += `
        <li class="page-item ${currentPage === totalPages ? 'disabled' : ''}">
            <a class="page-link" href="#" onclick="changePage(${currentPage + 1})">Sonraki</a>
        </li>
    `;

    container.innerHTML = html;
}

function changePage(page) {
    if (page >= 1 && page <= totalPages && page !== currentPage) {
        currentPage = page;
        loadProducts();
        window.scrollTo(0, 0);
    }
}

function searchProducts() {
    searchQuery = document.getElementById('searchInput').value.trim();
    currentPage = 1;
    loadProducts();
}

// Cart Functions
function addToCart(productId) {
    if (!authToken) {
        showAlert('Sepete eklemek için giriş yapmalısınız', 'warning');
        showLogin();
        return;
    }

    const product = products.find(p => p.id === productId);
    if (!product) return;

    const existingItem = cart.find(item => item.productId === productId);
    
    if (existingItem) {
        if (existingItem.quantity < product.stock) {
            existingItem.quantity++;
            showAlert(`${product.name} sepete eklendi`, 'success');
        } else {
            showAlert('Stok miktarını aştınız', 'warning');
            return;
        }
    } else {
        cart.push({
            productId: productId,
            productName: product.name,
            price: product.price,
            quantity: 1,
            maxStock: product.stock
        });
        showAlert(`${product.name} sepete eklendi`, 'success');
    }
    
    localStorage.setItem('cart', JSON.stringify(cart));
    updateCartUI();
}

function removeFromCart(productId) {
    cart = cart.filter(item => item.productId !== productId);
    localStorage.setItem('cart', JSON.stringify(cart));
    updateCartUI();
    renderCartItems();
}

function updateCartQuantity(productId, newQuantity) {
    const item = cart.find(item => item.productId === productId);
    if (!item) return;

    if (newQuantity <= 0) {
        removeFromCart(productId);
        return;
    }

    if (newQuantity > item.maxStock) {
        showAlert('Stok miktarını aştınız', 'warning');
        return;
    }

    item.quantity = newQuantity;
    localStorage.setItem('cart', JSON.stringify(cart));
    updateCartUI();
    renderCartItems();
}

function updateCartUI() {
    const cartCount = cart.reduce((total, item) => total + item.quantity, 0);
    document.getElementById('cartCount').textContent = cartCount;
    
    if (cartCount === 0) {
        document.getElementById('cartCount').classList.add('d-none');
    } else {
        document.getElementById('cartCount').classList.remove('d-none');
    }
}

function toggleCart() {
    const modal = new bootstrap.Modal(document.getElementById('cartModal'));
    renderCartItems();
    modal.show();
}

function renderCartItems() {
    const container = document.getElementById('cartItems');
    const totalElement = document.getElementById('cartTotal');
    
    if (cart.length === 0) {
        container.innerHTML = `
            <div class="empty-state">
                <i class="fas fa-shopping-cart"></i>
                <h6>Sepetiniz boş</h6>
                <p>Alışverişe başlamak için ürün ekleyin</p>
            </div>
        `;
        totalElement.textContent = '₺0.00';
        return;
    }

    const html = cart.map(item => `
        <div class="cart-item">
            <div class="d-flex justify-content-between align-items-center">
                <div class="flex-grow-1">
                    <h6 class="mb-1">${item.productName}</h6>
                    <span class="text-muted">₺${item.price.toFixed(2)}</span>
                </div>
                <div class="quantity-controls">
                    <button class="btn btn-outline-secondary btn-sm" onclick="updateCartQuantity(${item.productId}, ${item.quantity - 1})">
                        <i class="fas fa-minus"></i>
                    </button>
                    <span class="mx-2">${item.quantity}</span>
                    <button class="btn btn-outline-secondary btn-sm" onclick="updateCartQuantity(${item.productId}, ${item.quantity + 1})">
                        <i class="fas fa-plus"></i>
                    </button>
                    <button class="btn btn-outline-danger btn-sm ms-2" onclick="removeFromCart(${item.productId})">
                        <i class="fas fa-trash"></i>
                    </button>
                </div>
            </div>
            <div class="text-end mt-2">
                <strong>₺${(item.price * item.quantity).toFixed(2)}</strong>
            </div>
        </div>
    `).join('');

    container.innerHTML = html;
    
    const total = cart.reduce((sum, item) => sum + (item.price * item.quantity), 0);
    totalElement.textContent = `₺${total.toFixed(2)}`;
}

async function checkout() {
    if (!authToken) {
        showAlert('Sipariş vermek için giriş yapmalısınız', 'warning');
        return;
    }

    if (cart.length === 0) {
        showAlert('Sepetiniz boş', 'warning');
        return;
    }

    try {
        // Backend'in beklediği formata uygun veri hazırla
        const checkoutData = {
            ShippingAddress: "Varsayılan teslimat adresi", // Bu dinamik olabilir
            ShippingCity: "İstanbul",
            PaymentMethod: "Kapıda Ödeme",
            OrderItems: cart.map(item => ({
                ProductId: item.productId,
                Quantity: item.quantity
            }))
        };

        console.log('Checkout data being sent:', checkoutData);

        const response = await fetch(`${API_BASE_URL}/order/create-from-cart`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${authToken}`
            },
            body: JSON.stringify(checkoutData)
        });

        const data = await response.json();

        if (response.ok) {
            cart = [];
            localStorage.removeItem('cart');
            updateCartUI();
            bootstrap.Modal.getInstance(document.getElementById('cartModal')).hide();
            showAlert('Siparişiniz başarıyla oluşturuldu!', 'success');
        } else {
            showAlert(data.message || 'Sipariş oluşturulamadı', 'danger');
        }
    } catch (error) {
        console.error('Checkout error:', error);
        showAlert('Sipariş oluşturulamadı. API bağlantısını kontrol edin.', 'danger');
    }
}

// Utility Functions
function showLoading() {
    document.getElementById('productsGrid').innerHTML = `
        <div class="col-12">
            <div class="loading">
                <i class="fas fa-spinner fa-spin fa-2x"></i>
                <p>Yükleniyor...</p>
            </div>
        </div>
    `;
}

function showError(message) {
    document.getElementById('productsGrid').innerHTML = `
        <div class="col-12">
            <div class="empty-state">
                <i class="fas fa-exclamation-triangle"></i>
                <h5>Hata</h5>
                <p>${message}</p>
            </div>
        </div>
    `;
}

function showAlert(message, type = 'info') {
    const alertContainer = document.getElementById('alertContainer');
    const alertId = 'alert-' + Date.now();
    
    const alertHtml = `
        <div id="${alertId}" class="alert alert-${type} alert-dismissible fade show" role="alert">
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `;
    
    alertContainer.innerHTML = alertHtml;
    
    // Auto dismiss after 5 seconds
    setTimeout(() => {
        const alertElement = document.getElementById(alertId);
        if (alertElement) {
            const alert = new bootstrap.Alert(alertElement);
            alert.close();
        }
    }, 5000);
}

// Search on Enter key
document.addEventListener('DOMContentLoaded', function() {
    const searchInput = document.getElementById('searchInput');
    if (searchInput) {
        searchInput.addEventListener('keypress', function(e) {
            if (e.key === 'Enter') {
                searchProducts();
            }
        });
    }
});

// Order Management Functions
async function loadUserOrders() {
    if (!authToken) {
        showAlert('Siparişleri görüntülemek için giriş yapmalısınız', 'warning');
        return;
    }

    try {
        console.log('Loading user orders...');
        
        const response = await fetch(`${API_BASE_URL}/order`, {
            headers: {
                'Authorization': `Bearer ${authToken}`
            }
        });

        if (response.ok) {
            const orders = await response.json();
            console.log('Orders loaded:', orders);
            displayOrders(orders);
        } else {
            const error = await response.json();
            showAlert(error.message || 'Siparişler yüklenemedi', 'danger');
        }
    } catch (error) {
        console.error('Load orders error:', error);
        showAlert('Siparişler yüklenemedi. API bağlantısını kontrol edin.', 'danger');
    }
}

function displayOrders(orders) {
    const ordersList = document.getElementById('ordersList');
    
    if (!ordersList) {
        console.warn('Orders list element not found');
        return;
    }

    if (orders.length === 0) {
        ordersList.innerHTML = `
            <div class="text-center p-4">
                <h5>Henüz siparişiniz bulunmamaktadır</h5>
                <p class="text-muted">İlk siparişinizi vermek için alışverişe başlayın!</p>
            </div>
        `;
        return;
    }

    ordersList.innerHTML = orders.map(order => `
        <div class="card mb-3">
            <div class="card-header d-flex justify-content-between align-items-center">
                <div>
                    <strong>Sipariş #${order.orderNumber}</strong>
                    <span class="badge bg-${getOrderStatusColor(order.status)} ms-2">${getOrderStatusText(order.status)}</span>
                </div>
                <small class="text-muted">${new Date(order.orderDate).toLocaleDateString('tr-TR')}</small>
            </div>
            <div class="card-body">
                <div class="row">
                    <div class="col-md-8">
                        <h6>Sipariş Detayları:</h6>
                        <ul class="list-unstyled">
                            ${order.items.map(item => `
                                <li class="mb-1">
                                    ${item.productName} x ${item.quantity} = ₺${item.totalPrice.toFixed(2)}
                                </li>
                            `).join('')}
                        </ul>
                        <p class="mb-1"><strong>Teslimat Adresi:</strong> ${order.shippingAddress}, ${order.shippingCity}</p>
                        <p class="mb-0"><strong>Ödeme Yöntemi:</strong> ${order.paymentMethod || 'Kapıda Ödeme'}</p>
                    </div>
                    <div class="col-md-4 text-end">
                        <p class="mb-1">Ara Toplam: ₺${(order.totalAmount - order.shippingCost - order.taxAmount).toFixed(2)}</p>
                        <p class="mb-1">Kargo: ₺${order.shippingCost.toFixed(2)}</p>
                        <p class="mb-1">KDV: ₺${order.taxAmount.toFixed(2)}</p>
                        <h5 class="text-primary">Toplam: ₺${order.totalAmount.toFixed(2)}</h5>
                        ${order.canBeCancelled ? `
                            <button class="btn btn-outline-danger btn-sm mt-2" onclick="cancelOrder(${order.id})">
                                Siparişi İptal Et
                            </button>
                        ` : ''}
                    </div>
                </div>
            </div>
        </div>
    `).join('');
}

function getOrderStatusColor(status) {
    switch (status.toLowerCase()) {
        case 'pending': return 'warning';
        case 'confirmed': return 'info';
        case 'processing': return 'primary';
        case 'shipped': return 'secondary';
        case 'delivered': return 'success';
        case 'cancelled': return 'danger';
        default: return 'secondary';
    }
}

function getOrderStatusText(status) {
    switch (status.toLowerCase()) {
        case 'pending': return 'Beklemede';
        case 'confirmed': return 'Onaylandı';
        case 'processing': return 'Hazırlanıyor';
        case 'shipped': return 'Kargoya Verildi';
        case 'delivered': return 'Teslim Edildi';
        case 'cancelled': return 'İptal Edildi';
        default: return status;
    }
}

async function cancelOrder(orderId) {
    if (!confirm('Bu siparişi iptal etmek istediğinizden emin misiniz?')) {
        return;
    }

    try {
        const response = await fetch(`${API_BASE_URL}/order/${orderId}`, {
            method: 'DELETE',
            headers: {
                'Authorization': `Bearer ${authToken}`
            }
        });

        if (response.ok) {
            showAlert('Sipariş başarıyla iptal edildi', 'success');
            loadUserOrders(); // Listeyi yenile
        } else {
            const error = await response.json();
            showAlert(error.message || 'Sipariş iptal edilemedi', 'danger');
        }
    } catch (error) {
        console.error('Cancel order error:', error);
        showAlert('Sipariş iptal edilemedi. API bağlantısını kontrol edin.', 'danger');
    }
}

// Siparişlerim sayfasını göster
function showOrdersPage() {
    if (!authToken) {
        showAlert('Siparişleri görüntülemek için giriş yapmalısınız', 'warning');
        return;
    }
    
    const mainContent = document.querySelector('.container.mt-4');
    if (mainContent) {
        mainContent.innerHTML = `
            <div class="row">
                <div class="col-12">
                    <div class="d-flex justify-content-between align-items-center mb-4">
                        <h2>Siparişlerim</h2>
                        <button class="btn btn-secondary" onclick="location.reload()">Ana Sayfaya Dön</button>
                    </div>
                    <div id="ordersList">
                        <div class="text-center">
                            <div class="spinner-border" role="status">
                                <span class="visually-hidden">Yükleniyor...</span>
                            </div>
                            <p class="mt-2">Siparişler yükleniyor...</p>
                        </div>
                    </div>
                </div>
            </div>
        `;
        
        loadUserOrders();
    }
}
