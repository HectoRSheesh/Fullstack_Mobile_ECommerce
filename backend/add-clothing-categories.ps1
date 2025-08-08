# E-commerce API Test Scripts
# Clothing kategorisi ve ürünlerini eklemek için PowerShell script'leri

# Clothing ana kategorisini ekle
$clothingCategory = @{
    name = "Clothing"
    description = "Fashion and clothing items"
    slug = "clothing"
    sortOrder = 10
    isActive = $true
} | ConvertTo-Json

Write-Host "Adding Clothing category..."
try {
    $response = Invoke-WebRequest -Uri "http://localhost:7038/api/category" -Method POST -Body $clothingCategory -ContentType "application/json"
    $categoryResult = $response.Content | ConvertFrom-Json
    Write-Host "Clothing category added with ID: $($categoryResult.id)"
    $clothingCategoryId = $categoryResult.id
}
catch {
    Write-Host "Category might already exist or error occurred: $($_.Exception.Message)"
    # Try to get existing category
    $existingCategories = Invoke-WebRequest -Uri "http://localhost:7038/api/category" -Method GET
    $categories = $existingCategories.Content | ConvertFrom-Json
    $clothingCat = $categories | Where-Object { $_.name -eq "Clothing" }
    if ($clothingCat) {
        $clothingCategoryId = $clothingCat.id
        Write-Host "Found existing Clothing category with ID: $clothingCategoryId"
    }
}

# Alt kategorileri ekle
$subCategories = @(
    @{ name = "Men's Clothing"; description = "Men's fashion and clothing"; slug = "mens-clothing"; sortOrder = 1 },
    @{ name = "Women's Clothing"; description = "Women's fashion and clothing"; slug = "womens-clothing"; sortOrder = 2 },
    @{ name = "Kids' Clothing"; description = "Children's fashion and clothing"; slug = "kids-clothing"; sortOrder = 3 }
)

$subCategoryIds = @{}

foreach ($subCat in $subCategories) {
    $subCatData = @{
        name = $subCat.name
        description = $subCat.description
        slug = $subCat.slug
        parentCategoryId = $clothingCategoryId
        sortOrder = $subCat.sortOrder
        isActive = $true
    } | ConvertTo-Json
    
    Write-Host "Adding $($subCat.name)..."
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:7038/api/category" -Method POST -Body $subCatData -ContentType "application/json"
        $result = $response.Content | ConvertFrom-Json
        $subCategoryIds[$subCat.name] = $result.id
        Write-Host "$($subCat.name) added with ID: $($result.id)"
    }
    catch {
        Write-Host "Sub-category might already exist: $($_.Exception.Message)"
    }
}

Write-Host "Categories setup completed!"
Write-Host "Clothing Category ID: $clothingCategoryId"
Write-Host "Sub-category IDs:"
$subCategoryIds | Format-Table
