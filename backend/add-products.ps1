# Clothing urunlerini eklemek icin script

# Oncetest kullanicisi olustur
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
$mensCategory = $categories | Where-Object { $_.name -eq "Men's Clothing" }
$womensCategory = $categories | Where-Object { $_.name -eq "Women's Clothing" }
$kidsCategory = $categories | Where-Object { $_.name -eq "Kids' Clothing" }

Write-Host "Found categories:"
Write-Host "Men's: $($mensCategory.id)"
Write-Host "Women's: $($womensCategory.id)"
Write-Host "Kids': $($kidsCategory.id)"

# Urunleri ekle
$products = @(
    @{
        name = "Men's Classic T-Shirt"
        description = "100% cotton classic fit t-shirt"
        price = 29.99
        stockQuantity = 150
        categoryId = $mensCategory.id
        sku = "MTS001"
        imageUrl = "/images/mens-tshirt.jpg"
        isActive = $true
    },
    @{
        name = "Women's Floral Dress"
        description = "Beautiful floral pattern dress"
        price = 89.99
        stockQuantity = 75
        categoryId = $womensCategory.id
        sku = "WFD001"
        imageUrl = "/images/womens-dress.jpg"
        isActive = $true
    },
    @{
        name = "Kids Cartoon T-Shirt"
        description = "Fun cartoon themed t-shirt for kids"
        price = 19.99
        stockQuantity = 200
        categoryId = $kidsCategory.id
        sku = "KTS001"
        imageUrl = "/images/kids-tshirt.jpg"
        isActive = $true
    }
)

foreach ($product in $products) {
    $productJson = $product | ConvertTo-Json
    Write-Host "Adding product: $($product.name)"
    
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:7038/api/product" -Method POST -Body $productJson -Headers $headers
        $result = $response.Content | ConvertFrom-Json
        Write-Host "Product added successfully with ID: $($result.id)"
    }
    catch {
        Write-Host "Error adding product: $($_.Exception.Message)"
    }
}

Write-Host "Products setup completed!"
