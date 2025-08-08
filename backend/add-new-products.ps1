# Yeni çeşitli ürünler eklemek için script
Write-Host "=== E-Commerce New Products Setup Script ===" -ForegroundColor Green

# Test kullanıcısı oluştur/login yap
$testUser = @{
    fullName = "Test User"
    email = "test@ecommerce.com"
    password = "Test123!"
    confirmPassword = "Test123!"
} | ConvertTo-Json

Write-Host "Creating/Login test user..." -ForegroundColor Yellow
try {
    $userResponse = Invoke-WebRequest -Uri "http://localhost:7038/api/auth/register" -Method POST -Body $testUser -ContentType "application/json"
    $userResult = $userResponse.Content | ConvertFrom-Json
    Write-Host "✓ User created successfully" -ForegroundColor Green
    $token = $userResult.token
}
catch {
    Write-Host "User exists, trying to login..." -ForegroundColor Yellow
    $loginData = @{
        email = "test@ecommerce.com"
        password = "Test123!"
    } | ConvertTo-Json
    
    try {
        $loginResponse = Invoke-WebRequest -Uri "http://localhost:7038/api/auth/login" -Method POST -Body $loginData -ContentType "application/json"
        $loginResult = $loginResponse.Content | ConvertFrom-Json
        $token = $loginResult.token
        Write-Host "✓ Logged in successfully" -ForegroundColor Green
    }
    catch {
        Write-Host "✗ Login failed: $($_.Exception.Message)" -ForegroundColor Red
        exit 1
    }
}

# Authorization header
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

# Ana kategorileri oluştur
Write-Host "`nCreating main categories..." -ForegroundColor Yellow
$mainCategories = @(
    @{
        name = "Electronics"
        description = "Electronic devices and gadgets"
        isActive = $true
    },
    @{
        name = "Clothing"
        description = "Fashion and clothing items"
        isActive = $true
    },
    @{
        name = "Home & Garden"
        description = "Home improvement and garden supplies"
        isActive = $true
    },
    @{
        name = "Sports & Outdoors"
        description = "Sports equipment and outdoor gear"
        isActive = $true
    },
    @{
        name = "Books & Media"
        description = "Books, movies, music and digital media"
        isActive = $true
    }
)

$createdCategories = @{}

foreach ($category in $mainCategories) {
    $categoryJson = $category | ConvertTo-Json
    Write-Host "Creating category: $($category.name)"
    
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:7038/api/categories" -Method POST -Body $categoryJson -Headers $headers
        $result = $response.Content | ConvertFrom-Json
        $createdCategories[$category.name] = $result.id
        Write-Host "✓ Category '$($category.name)' created with ID: $($result.id)" -ForegroundColor Green
    }
    catch {
        Write-Host "⚠ Category might exist: $($_.Exception.Message)" -ForegroundColor Yellow
        # Kategorileri al ve ID'yi bul
        try {
            $categoriesResponse = Invoke-WebRequest -Uri "http://localhost:7038/api/categories" -Method GET
            $categories = $categoriesResponse.Content | ConvertFrom-Json
            
            if ($categories.data) {
                $existingCategory = $categories.data | Where-Object { $_.name -eq $category.name }
            } else {
                $existingCategory = $categories | Where-Object { $_.name -eq $category.name }
            }
            
            if ($existingCategory) {
                $createdCategories[$category.name] = $existingCategory.id
                Write-Host "✓ Found existing category '$($category.name)' with ID: $($existingCategory.id)" -ForegroundColor Green
            }
        }
        catch {
            Write-Host "✗ Error getting categories: $($_.Exception.Message)" -ForegroundColor Red
        }
    }
}

# Ürünleri ekle
Write-Host "`nAdding products..." -ForegroundColor Yellow
$products = @(
    # Electronics
    @{
        name = "iPhone 15 Pro"
        description = "Latest Apple iPhone with A17 Pro chip, titanium design, and advanced camera system"
        price = 999.99
        stockQuantity = 50
        categoryId = $createdCategories["Electronics"]
        sku = "IPH15PRO001"
        imageUrl = "/images/iphone-15-pro.jpg"
        isActive = $true
    },
    @{
        name = "Samsung Galaxy S24 Ultra"
        description = "Premium Android smartphone with S Pen, 200MP camera, and AI features"
        price = 1199.99
        stockQuantity = 35
        categoryId = $createdCategories["Electronics"]
        sku = "SGS24ULTRA001"
        imageUrl = "/images/galaxy-s24-ultra.jpg"
        isActive = $true
    },
    @{
        name = "MacBook Air M3"
        description = "Ultra-thin laptop with M3 chip, 13-inch Liquid Retina display, up to 18 hours battery"
        price = 1099.99
        stockQuantity = 25
        categoryId = $createdCategories["Electronics"]
        sku = "MBAM3001"
        imageUrl = "/images/macbook-air-m3.jpg"
        isActive = $true
    },
    @{
        name = "AirPods Pro (3rd Gen)"
        description = "Active noise cancellation, transparency mode, spatial audio"
        price = 249.99
        stockQuantity = 100
        categoryId = $createdCategories["Electronics"]
        sku = "AIRPRO3001"
        imageUrl = "/images/airpods-pro-3.jpg"
        isActive = $true
    },
    @{
        name = "Sony WH-1000XM5"
        description = "Industry-leading noise canceling wireless headphones"
        price = 399.99
        stockQuantity = 60
        categoryId = $createdCategories["Electronics"]
        sku = "SONYWH1000XM5"
        imageUrl = "/images/sony-wh1000xm5.jpg"
        isActive = $true
    },

    # Clothing
    @{
        name = "Nike Air Force 1"
        description = "Classic white leather sneakers, comfortable and durable"
        price = 110.00
        stockQuantity = 200
        categoryId = $createdCategories["Clothing"]
        sku = "NIKEAF1001"
        imageUrl = "/images/nike-air-force-1.jpg"
        isActive = $true
    },
    @{
        name = "Levi's 501 Original Jeans"
        description = "Classic straight-leg jeans, original fit, 100% cotton denim"
        price = 89.99
        stockQuantity = 150
        categoryId = $createdCategories["Clothing"]
        sku = "LEVIS501001"
        imageUrl = "/images/levis-501-jeans.jpg"
        isActive = $true
    },
    @{
        name = "Adidas Originals Hoodie"
        description = "Comfortable cotton blend hoodie with iconic 3-stripes design"
        price = 75.00
        categoryId = $createdCategories["Clothing"]
        stockQuantity = 120
        sku = "ADIHOODIE001"
        imageUrl = "/images/adidas-hoodie.jpg"
        isActive = $true
    },
    @{
        name = "Ray-Ban Aviator Sunglasses"
        description = "Classic aviator style sunglasses with UV protection"
        price = 195.00
        stockQuantity = 80
        categoryId = $createdCategories["Clothing"]
        sku = "RBAV001"
        imageUrl = "/images/rayban-aviator.jpg"
        isActive = $true
    },

    # Home & Garden
    @{
        name = "Dyson V15 Detect"
        description = "Powerful cordless vacuum with laser dust detection"
        price = 749.99
        stockQuantity = 30
        categoryId = $createdCategories["Home & Garden"]
        sku = "DYSONV15001"
        imageUrl = "/images/dyson-v15.jpg"
        isActive = $true
    },
    @{
        name = "Instant Pot Duo 7-in-1"
        description = "Electric pressure cooker, slow cooker, rice cooker, and more"
        price = 99.99
        stockQuantity = 75
        categoryId = $createdCategories["Home & Garden"]
        sku = "INSTPOT7001"
        imageUrl = "/images/instant-pot-duo.jpg"
        isActive = $true
    },
    @{
        name = "Philips Hue Smart Bulb Starter Kit"
        description = "Color-changing smart LED bulbs with app control"
        price = 199.99
        stockQuantity = 50
        categoryId = $createdCategories["Home & Garden"]
        sku = "PHILHUE001"
        imageUrl = "/images/philips-hue-kit.jpg"
        isActive = $true
    },

    # Sports & Outdoors
    @{
        name = "Yeti Rambler 20oz Tumbler"
        description = "Double-wall vacuum insulated stainless steel tumbler"
        price = 35.00
        stockQuantity = 200
        categoryId = $createdCategories["Sports & Outdoors"]
        sku = "YETI20OZ001"
        imageUrl = "/images/yeti-rambler-20oz.jpg"
        isActive = $true
    },
    @{
        name = "Patagonia Better Sweater Fleece"
        description = "Warm, comfortable fleece jacket made from recycled polyester"
        price = 139.00
        stockQuantity = 85
        categoryId = $createdCategories["Sports & Outdoors"]
        sku = "PATBSFLEECE001"
        imageUrl = "/images/patagonia-fleece.jpg"
        isActive = $true
    },
    @{
        name = "Coleman 4-Person Dome Tent"
        description = "Easy-to-setup dome tent perfect for family camping"
        price = 89.99
        stockQuantity = 40
        categoryId = $createdCategories["Sports & Outdoors"]
        sku = "COLEMAN4TENT001"
        imageUrl = "/images/coleman-tent.jpg"
        isActive = $true
    },

    # Books & Media
    @{
        name = "The Psychology of Money"
        description = "Best-selling book about the psychology behind financial decisions"
        price = 16.99
        stockQuantity = 300
        categoryId = $createdCategories["Books & Media"]
        sku = "PSYMONEY001"
        imageUrl = "/images/psychology-money-book.jpg"
        isActive = $true
    },
    @{
        name = "Atomic Habits"
        description = "Life-changing book about building good habits and breaking bad ones"
        price = 18.99
        stockQuantity = 250
        categoryId = $createdCategories["Books & Media"]
        sku = "ATOMHAB001"
        imageUrl = "/images/atomic-habits-book.jpg"
        isActive = $true
    },
    @{
        name = "Kindle Paperwhite (11th Gen)"
        description = "Waterproof e-reader with 6.8-inch display and adjustable warm light"
        price = 139.99
        stockQuantity = 60
        categoryId = $createdCategories["Books & Media"]
        sku = "KINDLEPW11001"
        imageUrl = "/images/kindle-paperwhite.jpg"
        isActive = $true
    }
)

$successCount = 0
$totalProducts = $products.Count

foreach ($product in $products) {
    if ($product.categoryId -eq $null) {
        Write-Host "⚠ Skipping product '$($product.name)' - no category ID" -ForegroundColor Yellow
        continue
    }
    
    $productJson = $product | ConvertTo-Json
    Write-Host "Adding product: $($product.name) (Price: $$$($product.price))"
    
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:7038/api/products" -Method POST -Body $productJson -Headers $headers
        $result = $response.Content | ConvertFrom-Json
        Write-Host "✓ Product '$($product.name)' added successfully with ID: $($result.id)" -ForegroundColor Green
        $successCount++
    }
    catch {
        Write-Host "✗ Error adding product '$($product.name)': $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host "`n=== SETUP COMPLETED ===" -ForegroundColor Green
Write-Host "Successfully added $successCount out of $totalProducts products" -ForegroundColor Cyan
Write-Host "Categories created: $($createdCategories.Count)" -ForegroundColor Cyan

if ($createdCategories.Count -gt 0) {
    Write-Host "`nCreated categories:" -ForegroundColor Yellow
    $createdCategories.GetEnumerator() | ForEach-Object {
        Write-Host "  - $($_.Key): ID $($_.Value)" -ForegroundColor White
    }
}

Write-Host "`nYou can now test the products in your e-commerce application!" -ForegroundColor Green
