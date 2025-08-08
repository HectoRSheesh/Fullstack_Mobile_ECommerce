# Clothing ürünlerini eklemek için script

# Önce test kullanıcısı oluştur
$testUser = @{
    fullName = "Test User"
    email = "test@clothing.com"
    password = "Test123!"
    confirmPassword = "Test123!"
} | ConvertTo-Json

Write-Host "Creating test user..."
try {
    $userResponse = Invoke-WebRequest -Uri "http://localhost:7038/api/auth/register" -Method POST -Body $testUser -ContentType "application/json"
    $userResult = $userResponse.Content | ConvertFrom-Json
    Write-Host "User created successfully"
    $token = $userResult.token
}
catch {
    Write-Host "User might already exist, trying to login..."
    # Login with existing user
    $loginData = @{
        email = "test@clothing.com"
        password = "Test123!"
    } | ConvertTo-Json
    
    $loginResponse = Invoke-WebRequest -Uri "http://localhost:7038/api/auth/login" -Method POST -Body $loginData -ContentType "application/json"
    $loginResult = $loginResponse.Content | ConvertFrom-Json
    $token = $loginResult.token
    Write-Host "Logged in successfully"
}

# Authorization header
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

# Kategorileri al
$categoriesResponse = Invoke-WebRequest -Uri "http://localhost:7038/api/category" -Method GET
$categories = $categoriesResponse.Content | ConvertFrom-Json
$clothingCategories = $categories | Where-Object { $_.name -like '*Clothing*' -and $_.parentCategoryName }

Write-Host "Found clothing categories:"
$clothingCategories | Format-Table id, name

# Ürünleri ekle
$products = @(
    @{
        name = "Men's Classic T-Shirt"
        description = "100% cotton classic fit t-shirt. Comfortable and durable for everyday wear."
        price = 29.99
        stockQuantity = 150
        categoryId = ($clothingCategories | Where-Object { $_.name -eq "Men's Clothing" }).id
        sku = "MTS001"
        imageUrl = "/images/mens-tshirt-classic.jpg"
        isActive = $true
    },
    @{
        name = "Men's Denim Jeans"
        description = "Classic blue denim jeans with regular fit. Perfect for casual and semi-formal occasions."
        price = 79.99
        stockQuantity = 80
        categoryId = ($clothingCategories | Where-Object { $_.name -eq "Men's Clothing" }).id
        sku = "MJN001"
        imageUrl = "/images/mens-jeans-denim.jpg"
        isActive = $true
    },
    @{
        name = "Women's Floral Dress"
        description = "Beautiful floral pattern dress made from lightweight fabric. Perfect for summer occasions."
        price = 89.99
        stockQuantity = 75
        categoryId = ($clothingCategories | Where-Object { $_.name -eq "Women's Clothing" }).id
        sku = "WFD001"
        imageUrl = "/images/womens-floral-dress.jpg"
        isActive = $true
    },
    @{
        name = "Women's Skinny Jeans"
        description = "High-waisted skinny jeans with stretch fabric for comfort and style."
        price = 79.99
        stockQuantity = 120
        categoryId = ($clothingCategories | Where-Object { $_.name -eq "Women's Clothing" }).id
        sku = "WSJ001"
        imageUrl = "/images/womens-skinny-jeans.jpg"
        isActive = $true
    },
    @{
        name = "Kids' Cartoon T-Shirt"
        description = "Fun cartoon-themed t-shirt made from soft cotton. Perfect for active kids."
        price = 19.99
        stockQuantity = 200
        categoryId = ($clothingCategories | Where-Object { $_.name -eq "Kids' Clothing" }).id
        sku = "KTS001"
        imageUrl = "/images/kids-cartoon-tshirt.jpg"
        isActive = $true
    },
    @{
        name = "Kids' Denim Overalls"
        description = "Classic denim overalls for kids. Durable and comfortable for play time."
        price = 49.99
        stockQuantity = 80
        categoryId = ($clothingCategories | Where-Object { $_.name -eq "Kids' Clothing" }).id
        sku = "KDO001"
        imageUrl = "/images/kids-denim-overalls.jpg"
        isActive = $true
    }
)

foreach ($product in $products) {
    $productJson = $product | ConvertTo-Json
    Write-Host "Adding product: $($product.name)"
    
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:7038/api/product" -Method POST -Body $productJson -Headers $headers
        $result = $response.Content | ConvertFrom-Json
        Write-Host "✓ Product added successfully with ID: $($result.id)"
    }
    catch {
        Write-Host "✗ Error adding product: $($_.Exception.Message)"
    }
}

Write-Host "Clothing products setup completed!"
