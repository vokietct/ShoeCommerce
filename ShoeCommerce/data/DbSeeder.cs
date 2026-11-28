using Microsoft.EntityFrameworkCore;
using ShoeCommerce.Models;

namespace ShoeCommerce.Data;

public static class DbSeeder
{
    public static async Task EnsureProductImagesTableAsync(ShoeStoreDbContext context)
    {
        await context.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS product_images (
                image_id INT AUTO_INCREMENT PRIMARY KEY,
                product_id INT NOT NULL,
                image_url VARCHAR(500) NOT NULL,
                sort_order INT NOT NULL DEFAULT 0,
                created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                INDEX ix_product_images_product_sort (product_id, sort_order),
                CONSTRAINT fk_product_images_product
                    FOREIGN KEY (product_id)
                    REFERENCES products(product_id)
                    ON DELETE CASCADE
            ) ENGINE = InnoDB
            """);
    }

    public static async Task SeedAsync(ShoeStoreDbContext context)
    {
        // 1. Roles
        if (!await context.Roles.AnyAsync())
        {
            context.Roles.AddRange(
                new Role { RoleId = 1, RoleName = "Admin" },
                new Role { RoleId = 2, RoleName = "Employee" },
                new Role { RoleId = 3, RoleName = "Customer" }
            );
            await context.SaveChangesAsync();
        }

        // 2. Users
        if (!await context.Users.AnyAsync())
        {
            var adminUser = new User
            {
                Email = "admin@shoestore.vn",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                FullName = "Administrator",
                Phone = "0901234567",
                RoleId = 1,
                IsActive = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            var employeeUser = new User
            {
                Email = "linh@shoestore.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("employee123"),
                FullName = "Trần Thảo Linh",
                Phone = "0912345678",
                RoleId = 2,
                IsActive = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            var custUser = new User
            {
                Email = "minhanh@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("customer123"),
                FullName = "Nguyễn Minh Anh",
                Phone = "0987654321",
                RoleId = 3,
                IsActive = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            context.Users.AddRange(adminUser, employeeUser, custUser);
            await context.SaveChangesAsync();

            // Link Employee records
            context.Employees.AddRange(
                new Employee { EmployeeId = adminUser.UserId, HireDate = new DateOnly(2024, 1, 15) },
                new Employee { EmployeeId = employeeUser.UserId, HireDate = new DateOnly(2024, 6, 1) }
            );

            // Link Customer record & Cart
            var customer = new Customer { CustomerId = custUser.UserId };
            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            context.Carts.Add(new Cart
            {
                CustomerId = customer.CustomerId,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            });

            // Customer address
            context.CustomerAddresses.Add(new CustomerAddress
            {
                CustomerId = customer.CustomerId,
                ReceiverName = "Nguyễn Minh Anh",
                Phone = "0987654321",
                AddressLine = "123 Nguyễn Trãi, Phường Bến Thành, Quận 1, TP. Hồ Chí Minh",
                IsDefault = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            });

            await context.SaveChangesAsync();
        }
        else
        {
            var adminUser = await context.Users
                .FirstOrDefaultAsync(u => u.Email == "admin@shoestore.vn");
            if (adminUser != null && adminUser.FullName == "Hoàng Gia Long")
            {
                adminUser.FullName = "Administrator";
                adminUser.UpdatedAt = DateTime.Now;
                await context.SaveChangesAsync();
            }
        }

        // 3. Categories
        if (!await context.Categories.AnyAsync())
        {
            context.Categories.AddRange(
                new Category { CategoryName = "Giày thể thao", Description = "Giày thể thao năng động, tập luyện đa năng" },
                new Category { CategoryName = "Giày chạy bộ", Description = "Giày chạy bộ chuyên nghiệp, siêu êm và nhẹ" },
                new Category { CategoryName = "Giày thời trang", Description = "Giày sneaker phong cách lifestyle, đi chơi hằng ngày" },
                new Category { CategoryName = "Giày tây", Description = "Giày da sang trọng, lịch sự cho công sở và tiệc" },
                new Category { CategoryName = "Sandal & Dép", Description = "Dép và sandal quai ngang thoáng mát, tiện dụng" }
            );
            await context.SaveChangesAsync();
        }

        // 4. Brands
        if (!await context.Brands.AnyAsync())
        {
            context.Brands.AddRange(
                new Brand { BrandName = "Nike", Description = "Thương hiệu thể thao hàng đầu thế giới từ Mỹ" },
                new Brand { BrandName = "Adidas", Description = "Thương hiệu thể thao ba sọc huyền thoại từ Đức" },
                new Brand { BrandName = "Puma", Description = "Thương hiệu thể thao thời trang và tốc độ" },
                new Brand { BrandName = "Converse", Description = "Biểu tượng giày canvas kinh điển của giới trẻ" },
                new Brand { BrandName = "Vans", Description = "Thương hiệu trượt ván đường phố kinh điển" },
                new Brand { BrandName = "New Balance", Description = "Thương hiệu giày êm ái và thiết kế retro cổ điển" }
            );
            await context.SaveChangesAsync();
        }

        // 5. Suppliers
        if (!await context.Suppliers.AnyAsync())
        {
            context.Suppliers.AddRange(
                new Supplier
                {
                    SupplierName = "Công ty TNHH Nike Việt Nam",
                    Phone = "02838221234",
                    Email = "supply@nike.com.vn",
                    Address = "Tầng 8, Tòa nhà Bitexco, Q.1, TP. Hồ Chí Minh",
                    IsCooperating = true
                },
                new Supplier
                {
                    SupplierName = "Công ty TNHH Adidas Việt Nam",
                    Phone = "02839115678",
                    Email = "vietnam@adidas.com",
                    Address = "Tòa nhà Deutsches Haus, Q.1, TP. Hồ Chí Minh",
                    IsCooperating = true
                },
                new Supplier
                {
                    SupplierName = "Công ty CP Thời trang thể thao Đông Á",
                    Phone = "02837449999",
                    Email = "contact@donga-sport.vn",
                    Address = "Lô B2, KCN Tân Bình, TP. Hồ Chí Minh",
                    IsCooperating = true
                }
            );
            await context.SaveChangesAsync();
        }

        // 6. Products & Variants
        if (!await context.Products.AnyAsync())
        {
            var catTheThao = await context.Categories.FirstAsync(c => c.CategoryName == "Giày thể thao");
            var catChayBo = await context.Categories.FirstAsync(c => c.CategoryName == "Giày chạy bộ");
            var catThoiTrang = await context.Categories.FirstAsync(c => c.CategoryName == "Giày thời trang");
            var catGiayTay = await context.Categories.FirstAsync(c => c.CategoryName == "Giày tây");
            var catSandal = await context.Categories.FirstAsync(c => c.CategoryName == "Sandal & Dép");

            var brandNike = await context.Brands.FirstAsync(b => b.BrandName == "Nike");
            var brandAdidas = await context.Brands.FirstAsync(b => b.BrandName == "Adidas");
            var brandPuma = await context.Brands.FirstAsync(b => b.BrandName == "Puma");
            var brandConverse = await context.Brands.FirstAsync(b => b.BrandName == "Converse");
            var brandVans = await context.Brands.FirstAsync(b => b.BrandName == "Vans");
            var brandNB = await context.Brands.FirstAsync(b => b.BrandName == "New Balance");

            var supNike = await context.Suppliers.FirstAsync(s => s.SupplierName.Contains("Nike"));
            var supAdidas = await context.Suppliers.FirstAsync(s => s.SupplierName.Contains("Adidas"));
            var supDongA = await context.Suppliers.FirstAsync(s => s.SupplierName.Contains("Đông Á"));

            var p1 = new Product
            {
                ProductCode = "AF1-07",
                ProductName = "Nike Air Force 1 '07",
                CategoryId = catThoiTrang.CategoryId,
                BrandId = brandNike.BrandId,
                SupplierId = supNike.SupplierId,
                Description = "Đôi giày kinh điển của Nike mang phong cách thời thượng, đệm Air êm ái suốt cả ngày dài và chất liệu da bền bỉ.",
                ThumbnailUrl = "/images/products/nike_air_force_1.png",
                Status = ProductStatus.Selling,
                Variants = new List<ProductVariant>
                {
                    new ProductVariant { Sku = "AF1-07-WHT-40", Size = "40", Color = "Trắng", Price = 2890000, StockQuantity = 15, LowStockThreshold = 5, Status = ProductStatus.Selling },
                    new ProductVariant { Sku = "AF1-07-WHT-41", Size = "41", Color = "Trắng", Price = 2890000, StockQuantity = 12, LowStockThreshold = 5, Status = ProductStatus.Selling },
                    new ProductVariant { Sku = "AF1-07-WHT-42", Size = "42", Color = "Trắng", Price = 2890000, StockQuantity = 20, LowStockThreshold = 5, Status = ProductStatus.Selling },
                    new ProductVariant { Sku = "AF1-07-BLK-41", Size = "41", Color = "Đen", Price = 2990000, StockQuantity = 8, LowStockThreshold = 5, Status = ProductStatus.Selling }
                }
            };

            var p2 = new Product
            {
                ProductCode = "UB-LIGHT",
                ProductName = "Adidas Ultraboost Light",
                CategoryId = catChayBo.CategoryId,
                BrandId = brandAdidas.BrandId,
                SupplierId = supAdidas.SupplierId,
                Description = "Mẫu giày chạy bộ hàng đầu với đệm Light BOOST nhẹ hơn 30%, phản hồi lực tối đa cho từng bước chạy.",
                ThumbnailUrl = "/images/products/adidas_ultraboost.png",
                Status = ProductStatus.Selling,
                Variants = new List<ProductVariant>
                {
                    new ProductVariant { Sku = "UB-LIGHT-BLK-40", Size = "40", Color = "Đen", Price = 3850000, StockQuantity = 10, LowStockThreshold = 4, Status = ProductStatus.Selling },
                    new ProductVariant { Sku = "UB-LIGHT-BLK-41", Size = "41", Color = "Đen", Price = 3850000, StockQuantity = 14, LowStockThreshold = 4, Status = ProductStatus.Selling },
                    new ProductVariant { Sku = "UB-LIGHT-WHT-42", Size = "42", Color = "Trắng", Price = 3950000, StockQuantity = 6, LowStockThreshold = 4, Status = ProductStatus.Selling },
                    new ProductVariant { Sku = "UB-LIGHT-BLU-42", Size = "42", Color = "Xanh Navy", Price = 3850000, StockQuantity = 3, LowStockThreshold = 5, Status = ProductStatus.Selling } // Sắp hết hàng
                }
            };

            var p3 = new Product
            {
                ProductCode = "PUMA-VELO",
                ProductName = "Puma Velocity Nitro 3",
                CategoryId = catChayBo.CategoryId,
                BrandId = brandPuma.BrandId,
                SupplierId = supDongA.SupplierId,
                Description = "Được trang bị bọt NITRO siêu nhẹ và đàn hồi, Puma Velocity Nitro 3 hoàn hảo cho các bài tập chạy hằng ngày.",
                ThumbnailUrl = "/images/products/puma_velocity.png",
                Status = ProductStatus.Selling,
                Variants = new List<ProductVariant>
                {
                    new ProductVariant { Sku = "PUMA-VN3-ORG-40", Size = "40", Color = "Cam", Price = 2590000, StockQuantity = 9, LowStockThreshold = 4, Status = ProductStatus.Selling },
                    new ProductVariant { Sku = "PUMA-VN3-ORG-41", Size = "41", Color = "Cam", Price = 2590000, StockQuantity = 11, LowStockThreshold = 4, Status = ProductStatus.Selling },
                    new ProductVariant { Sku = "PUMA-VN3-BLK-42", Size = "42", Color = "Đen", Price = 2650000, StockQuantity = 7, LowStockThreshold = 4, Status = ProductStatus.Selling }
                }
            };

            var p4 = new Product
            {
                ProductCode = "CV-CHUCK70",
                ProductName = "Converse Chuck 70 Vintage Canvas",
                CategoryId = catThoiTrang.CategoryId,
                BrandId = brandConverse.BrandId,
                SupplierId = supDongA.SupplierId,
                Description = "Phiên bản Chuck 70 tôn vinh thiết kế nguyên bản thập niên 70 với chất liệu vải canvas cao cấp và lót giày OrthoLite cực êm.",
                ThumbnailUrl = "/images/products/converse_chuck_70.png",
                Status = ProductStatus.Selling,
                Variants = new List<ProductVariant>
                {
                    new ProductVariant { Sku = "CV-C70-BLK-39", Size = "39", Color = "Đen", Price = 1950000, StockQuantity = 18, LowStockThreshold = 5, Status = ProductStatus.Selling },
                    new ProductVariant { Sku = "CV-C70-BLK-40", Size = "40", Color = "Đen", Price = 1950000, StockQuantity = 22, LowStockThreshold = 5, Status = ProductStatus.Selling },
                    new ProductVariant { Sku = "CV-C70-PAR-41", Size = "41", Color = "Vàng Kem", Price = 2050000, StockQuantity = 10, LowStockThreshold = 5, Status = ProductStatus.Selling }
                }
            };

            var p5 = new Product
            {
                ProductCode = "VANS-OLDSKOOL",
                ProductName = "Vans Old Skool Classic",
                CategoryId = catThoiTrang.CategoryId,
                BrandId = brandVans.BrandId,
                SupplierId = supDongA.SupplierId,
                Description = "Đôi giày trượt ván biểu tượng với sọc Jazz Stripe đặc trưng, đế bánh quế Waffle bám đường siêu tốt.",
                ThumbnailUrl = "/images/products/vans_old_skool.png",
                Status = ProductStatus.Selling,
                Variants = new List<ProductVariant>
                {
                    new ProductVariant { Sku = "VN-OS-BW-40", Size = "40", Color = "Đen Trắng", Price = 1850000, StockQuantity = 16, LowStockThreshold = 5, Status = ProductStatus.Selling },
                    new ProductVariant { Sku = "VN-OS-BW-41", Size = "41", Color = "Đen Trắng", Price = 1850000, StockQuantity = 19, LowStockThreshold = 5, Status = ProductStatus.Selling },
                    new ProductVariant { Sku = "VN-OS-BW-42", Size = "42", Color = "Đen Trắng", Price = 1850000, StockQuantity = 15, LowStockThreshold = 5, Status = ProductStatus.Selling }
                }
            };

            var p6 = new Product
            {
                ProductCode = "NB-574",
                ProductName = "New Balance 574 Core",
                CategoryId = catTheThao.CategoryId,
                BrandId = brandNB.BrandId,
                SupplierId = supDongA.SupplierId,
                Description = "Mẫu giày kinh điển và linh hoạt nhất của New Balance với đệm ENCAP hỗ trợ tối đa cho bàn chân.",
                ThumbnailUrl = "/images/products/new_balance_574.png",
                Status = ProductStatus.Selling,
                Variants = new List<ProductVariant>
                {
                    new ProductVariant { Sku = "NB-574-GRY-40", Size = "40", Color = "Xám", Price = 2390000, StockQuantity = 14, LowStockThreshold = 5, Status = ProductStatus.Selling },
                    new ProductVariant { Sku = "NB-574-GRY-41", Size = "41", Color = "Xám", Price = 2390000, StockQuantity = 12, LowStockThreshold = 5, Status = ProductStatus.Selling },
                    new ProductVariant { Sku = "NB-574-NVY-42", Size = "42", Color = "Xanh Navy", Price = 2450000, StockQuantity = 4, LowStockThreshold = 5, Status = ProductStatus.Selling } // Sắp hết hàng
                }
            };

            var p7 = new Product
            {
                ProductCode = "NIKE-PEG40",
                ProductName = "Nike Air Zoom Pegasus 40",
                CategoryId = catChayBo.CategoryId,
                BrandId = brandNike.BrandId,
                SupplierId = supNike.SupplierId,
                Description = "Giày chạy bộ quốc dân với 2 túi đệm Zoom Air kết hợp bọt React êm ái, thích hợp mọi cự ly.",
                ThumbnailUrl = "/images/products/nike_pegasus.png",
                Status = ProductStatus.Selling,
                Variants = new List<ProductVariant>
                {
                    new ProductVariant { Sku = "PEG40-WHT-41", Size = "41", Color = "Trắng Xanh", Price = 3490000, StockQuantity = 10, LowStockThreshold = 4, Status = ProductStatus.Selling },
                    new ProductVariant { Sku = "PEG40-BLK-42", Size = "42", Color = "Đen", Price = 3490000, StockQuantity = 15, LowStockThreshold = 4, Status = ProductStatus.Selling }
                }
            };

            var p8 = new Product
            {
                ProductCode = "AD-SAMBA",
                ProductName = "Adidas Samba OG",
                CategoryId = catThoiTrang.CategoryId,
                BrandId = brandAdidas.BrandId,
                SupplierId = supAdidas.SupplierId,
                Description = "Cơn sốt thời trang đường phố toàn cầu, thiết kế da mềm viền da lộn chữ T kinh điển.",
                ThumbnailUrl = "/images/products/adidas_samba.png",
                Status = ProductStatus.Selling,
                Variants = new List<ProductVariant>
                {
                    new ProductVariant { Sku = "SAMBA-OG-WHT-40", Size = "40", Color = "Trắng Sọc Đen", Price = 2700000, StockQuantity = 8, LowStockThreshold = 5, Status = ProductStatus.Selling },
                    new ProductVariant { Sku = "SAMBA-OG-WHT-41", Size = "41", Color = "Trắng Sọc Đen", Price = 2700000, StockQuantity = 7, LowStockThreshold = 5, Status = ProductStatus.Selling }
                }
            };

            context.Products.AddRange(p1, p2, p3, p4, p5, p6, p7, p8);
            await context.SaveChangesAsync();
        }

        // Bổ sung dữ liệu demo đến đủ 42 sản phẩm, không tạo trùng khi app khởi động lại.
        var currentProductCount = await context.Products.CountAsync();
        if (currentProductCount < 42)
        {
            var categories = await context.Categories.ToListAsync();
            var brands = await context.Brands.ToListAsync();
            var suppliers = await context.Suppliers.ToListAsync();

            var demoNames = new[]
            {
                "Nike Revolution 7", "Nike Court Vision Low", "Nike Air Max SC",
                "Nike Dunk Low Retro", "Nike Downshifter 12", "Nike Precision 6",
                "Adidas Breaknet 2.0", "Adidas Grand Court 2", "Adidas Run Falcon 3",
                "Adidas Duramo Speed", "Adidas Advantage Base", "Adidas Response Runner",
                "Puma Skyrocket Lite", "Puma Softride Enzo", "Puma Rebound Layup",
                "Puma Slipstream", "Puma Caven 2.0", "Puma Court Pure",
                "Converse Run Star Hike", "Converse One Star Pro", "Converse Chuck Taylor All Star",
                "Converse Pro Leather", "Converse Jack Purcell", "Converse Star Player 76",
                "Vans Authentic Classic", "Vans Era ComfyCush", "Vans Knu Skool",
                "Vans Slip-On Checkerboard", "Vans Filmore Hi", "Vans Skate Old Skool",
                "New Balance 530", "New Balance 327", "New Balance 1906R", "New Balance Fresh Foam Arishi"
            };

            var thumbnails = new[]
            {
                "/images/products/nike_air_force_1.png",
                "/images/products/adidas_ultraboost.png",
                "/images/products/puma_velocity.png",
                "/images/products/converse_chuck_70.png",
                "/images/products/vans_old_skool.png",
                "/images/products/new_balance_574.png",
                "/images/products/nike_pegasus.png",
                "/images/products/adidas_samba.png"
            };

            var demoColors = new[]
            {
                new[] { "Trắng", "Đen" },
                new[] { "Xám", "Xanh Navy" },
                new[] { "Đen Trắng", "Be" }
            };

            var existingCodes = await context.Products
                .Select(p => p.ProductCode)
                .ToHashSetAsync(StringComparer.OrdinalIgnoreCase);

            var addedProducts = new List<Product>();
            for (var index = 0; index < demoNames.Length && currentProductCount + addedProducts.Count < 42; index++)
            {
                var brandIndex = index % brands.Count;
                var brand = brands[brandIndex];
                var category = (index % 4) switch
                {
                    0 => categories.First(c => c.CategoryName == "Giày thể thao"),
                    1 => categories.First(c => c.CategoryName == "Giày chạy bộ"),
                    2 => categories.First(c => c.CategoryName == "Giày thời trang"),
                    _ => categories.First(c => c.CategoryName == "Sandal & Dép")
                };
                var supplier = suppliers.First(s => s.IsCooperating);
                if (brand.BrandName == "Nike")
                    supplier = suppliers.First(s => s.SupplierName.Contains("Nike"));
                else if (brand.BrandName == "Adidas")
                    supplier = suppliers.First(s => s.SupplierName.Contains("Adidas"));
                else
                    supplier = suppliers.First(s => s.SupplierName.Contains("Đông Á"));

                var code = $"DEMO-{(index + 1):D3}";
                if (existingCodes.Contains(code))
                    continue;

                var basePrice = 1450000m + ((index % 9) * 180000m);
                var colors = demoColors[index % demoColors.Length];
                var variants = new List<ProductVariant>();
                var sizes = new[] { "39", "40", "41", "42" };
                foreach (var (size, sizeIndex) in sizes.Select((size, sizeIndex) => (size, sizeIndex)))
                {
                    var color = colors[sizeIndex % colors.Length];
                    var colorCode = color
                        .Replace(" ", "")
                        .Replace("Trắng", "WHT")
                        .Replace("Đen", "BLK")
                        .Replace("Xám", "GRY")
                        .Replace("XanhNavy", "NVY")
                        .Replace("Be", "BE");
                    if (colorCode.Length > 8)
                        colorCode = colorCode[..8];

                    variants.Add(new ProductVariant
                    {
                        Sku = $"{code}-{colorCode}-{size}",
                        Size = size,
                        Color = color,
                        Price = basePrice + (sizeIndex % 2 * 50000m),
                        StockQuantity = 6 + ((index + sizeIndex) % 18),
                        LowStockThreshold = 5,
                        Status = ProductStatus.Selling
                    });
                }

                var product = new Product
                {
                    ProductCode = code,
                    ProductName = demoNames[index],
                    CategoryId = category.CategoryId,
                    BrandId = brand.BrandId,
                    SupplierId = supplier.SupplierId,
                    Description = $"{demoNames[index]} thuộc bộ sưu tập thời trang thể thao. Thiết kế năng động, đệm êm ái, phù hợp đi hàng ngày.",
                    ThumbnailUrl = thumbnails[index % thumbnails.Length],
                    Status = ProductStatus.Selling,
                    Variants = variants
                };
                addedProducts.Add(product);
                existingCodes.Add(code);
            }

            if (addedProducts.Count > 0)
            {
                context.Products.AddRange(addedProducts);
                await context.SaveChangesAsync();
            }
        }

        var productsForVariantSeed = await context.Products
            .Include(p => p.Variants)
            .ToListAsync();
        var existingVariantSkus = productsForVariantSeed
            .SelectMany(p => p.Variants)
            .Select(v => v.Sku)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var defaultColors = new[] { "Trắng", "Đen", "Xanh Navy", "Xám", "Be" };
        var shoeSizes = new[] { "39", "40", "41", "42", "43" };
        var addedVariants = 0;

        foreach (var seededProduct in productsForVariantSeed)
        {
            var colors = seededProduct.Variants
                .Select(v => v.Color)
                .Where(color => !string.IsNullOrWhiteSpace(color))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var defaultColor in defaultColors)
            {
                if (colors.Count >= 2)
                    break;
                if (!colors.Contains(defaultColor, StringComparer.OrdinalIgnoreCase))
                    colors.Add(defaultColor);
            }

            var basePrice = seededProduct.Variants.Count > 0
                ? seededProduct.Variants.Average(v => v.Price)
                : 1990000m;

            for (var colorIndex = 0; colorIndex < colors.Count; colorIndex++)
            {
                for (var sizeIndex = 0; sizeIndex < shoeSizes.Length; sizeIndex++)
                {
                    var size = shoeSizes[sizeIndex];
                    var color = colors[colorIndex];
                    if (seededProduct.Variants.Any(v =>
                        string.Equals(v.Size, size, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(v.Color, color, StringComparison.OrdinalIgnoreCase)))
                    {
                        continue;
                    }

                    var sku = $"{seededProduct.ProductCode}-SEED-C{colorIndex + 1}-{size}";
                    if (!existingVariantSkus.Add(sku))
                        continue;

                    var isOutOfStock = (seededProduct.ProductId + sizeIndex + colorIndex) % 13 == 0;
                    var variant = new ProductVariant
                    {
                        ProductId = seededProduct.ProductId,
                        Sku = sku,
                        Size = size,
                        Color = color,
                        Price = basePrice,
                        StockQuantity = isOutOfStock ? 0 : 8 + ((seededProduct.ProductId + sizeIndex + colorIndex) % 13),
                        LowStockThreshold = 5,
                        Status = ProductStatus.Selling
                    };
                    context.ProductVariants.Add(variant);
                    seededProduct.Variants.Add(variant);
                    addedVariants++;
                }
            }
        }

        if (addedVariants > 0)
            await context.SaveChangesAsync();

        // 7. Promotions
        if (!await context.Promotions.AnyAsync())
        {
            context.Promotions.AddRange(
                new Promotion
                {
                    PromoCode = "CHAO100",
                    DiscountAmount = 100000,
                    MinOrderValue = 1500000,
                    StartDate = DateTime.Now.AddDays(-30),
                    EndDate = DateTime.Now.AddYears(1),
                    IsActive = true
                },
                new Promotion
                {
                    PromoCode = "TET2026",
                    DiscountAmount = 200000,
                    MinOrderValue = 2000000,
                    StartDate = DateTime.Now.AddDays(-10),
                    EndDate = DateTime.Now.AddYears(1),
                    IsActive = true
                },
                new Promotion
                {
                    PromoCode = "VIP50",
                    DiscountAmount = 50000,
                    MinOrderValue = 500000,
                    StartDate = DateTime.Now.AddDays(-30),
                    EndDate = DateTime.Now.AddYears(1),
                    IsActive = true
                }
            );
            await context.SaveChangesAsync();
        }

        // 8. Seed sample orders, reviews & initial stock documents
        var admin = await context.Users.FirstAsync(u => u.RoleId == 1);
        var customerUser = await context.Users.FirstAsync(u => u.RoleId == 3);

        if (!await context.Orders.AnyAsync())
        {
            var p1 = await context.Products.Include(p => p.Variants).FirstAsync(p => p.ProductCode == "AF1-07");
            var v1 = p1.Variants.First();

            var promo = await context.Promotions.FirstAsync(p => p.PromoCode == "CHAO100");

            // Order 1: Completed order (so customer can review product)
            var order1 = new Order
            {
                OrderCode = "SS26110101",
                CustomerId = customerUser.UserId,
                PromotionId = promo.PromotionId,
                ReceiverName = "Nguyễn Minh Anh",
                Phone = "0987654321",
                ShippingAddress = "123 Nguyễn Trãi, Phường Bến Thành, Quận 1, TP. Hồ Chí Minh",
                TotalAmount = v1.Price,
                DiscountAmount = 100000,
                PromoCodeSnapshot = promo.PromoCode,
                OrderStatus = OrderStatus.Completed,
                Note = "Giao giờ hành chính",
                CompletedAt = DateTime.Now.AddDays(-5),
                CreatedAt = DateTime.Now.AddDays(-7),
                UpdatedAt = DateTime.Now.AddDays(-5)
            };

            context.Orders.Add(order1);
            await context.SaveChangesAsync();

            context.OrderDetails.Add(new OrderDetail
            {
                OrderId = order1.OrderId,
                VariantId = v1.VariantId,
                ProductNameSnapshot = p1.ProductName,
                SkuSnapshot = v1.Sku,
                SizeSnapshot = v1.Size,
                ColorSnapshot = v1.Color,
                Quantity = 1,
                UnitPrice = v1.Price
            });

            context.Payments.Add(new Payment
            {
                OrderId = order1.OrderId,
                PaymentMethod = PaymentMethod.COD,
                Amount = v1.Price - 100000,
                PaymentStatus = PaymentStatus.Paid,
                ConfirmedByUserId = admin.UserId,
                ConfirmedAt = DateTime.Now.AddDays(-5),
                CreatedAt = DateTime.Now.AddDays(-7),
                UpdatedAt = DateTime.Now.AddDays(-5)
            });

            context.OrderStatusHistories.AddRange(
                new OrderStatusHistory
                {
                    OrderId = order1.OrderId,
                    PreviousStatus = null,
                    NewStatus = OrderStatus.Pending,
                    ChangedByUserId = customerUser.UserId,
                    Note = "Khách hàng đặt hàng",
                    ChangedAt = DateTime.Now.AddDays(-7)
                },
                new OrderStatusHistory
                {
                    OrderId = order1.OrderId,
                    PreviousStatus = OrderStatus.Pending,
                    NewStatus = OrderStatus.Confirmed,
                    ChangedByUserId = admin.UserId,
                    Note = "Xác nhận đơn hàng",
                    ChangedAt = DateTime.Now.AddDays(-6)
                },
                new OrderStatusHistory
                {
                    OrderId = order1.OrderId,
                    PreviousStatus = OrderStatus.Confirmed,
                    NewStatus = OrderStatus.Shipping,
                    ChangedByUserId = admin.UserId,
                    Note = "Bàn giao đơn vị vận chuyển",
                    ChangedAt = DateTime.Now.AddDays(-6)
                },
                new OrderStatusHistory
                {
                    OrderId = order1.OrderId,
                    PreviousStatus = OrderStatus.Shipping,
                    NewStatus = OrderStatus.Completed,
                    ChangedByUserId = admin.UserId,
                    Note = "Giao hàng thành công và thu tiền COD",
                    ChangedAt = DateTime.Now.AddDays(-5)
                }
            );

            // Seed review for product 1 by this customer
            context.ProductReviews.Add(new ProductReview
            {
                ProductId = p1.ProductId,
                CustomerId = customerUser.UserId,
                Rating = 5,
                Comment = "Giày đi rất êm và ôm chân, da xịn bóng đẹp, form chuẩn chính hãng. Giao hàng nhanh đóng gói cẩn thận 5 sao!",
                CreatedAt = DateTime.Now.AddDays(-4),
                UpdatedAt = DateTime.Now.AddDays(-4)
            });

            // Order 2: Pending COD order (can test cancel, confirm, update shipping info)
            var p2 = await context.Products.Include(p => p.Variants).FirstAsync(p => p.ProductCode == "UB-LIGHT");
            var v2 = p2.Variants.First();

            var order2 = new Order
            {
                OrderCode = "SS26110702",
                CustomerId = customerUser.UserId,
                PromotionId = null,
                ReceiverName = "Nguyễn Minh Anh",
                Phone = "0987654321",
                ShippingAddress = "123 Nguyễn Trãi, Phường Bến Thành, Quận 1, TP. Hồ Chí Minh",
                TotalAmount = v2.Price,
                DiscountAmount = 0,
                PromoCodeSnapshot = null,
                OrderStatus = OrderStatus.Pending,
                Note = "Gọi trước khi giao",
                CreatedAt = DateTime.Now.AddHours(-2),
                UpdatedAt = DateTime.Now.AddHours(-2)
            };

            context.Orders.Add(order2);
            await context.SaveChangesAsync();

            context.OrderDetails.Add(new OrderDetail
            {
                OrderId = order2.OrderId,
                VariantId = v2.VariantId,
                ProductNameSnapshot = p2.ProductName,
                SkuSnapshot = v2.Sku,
                SizeSnapshot = v2.Size,
                ColorSnapshot = v2.Color,
                Quantity = 1,
                UnitPrice = v2.Price
            });

            context.Payments.Add(new Payment
            {
                OrderId = order2.OrderId,
                PaymentMethod = PaymentMethod.COD,
                Amount = v2.Price,
                PaymentStatus = PaymentStatus.Unpaid,
                CreatedAt = DateTime.Now.AddHours(-2),
                UpdatedAt = DateTime.Now.AddHours(-2)
            });

            context.OrderStatusHistories.Add(new OrderStatusHistory
            {
                OrderId = order2.OrderId,
                PreviousStatus = null,
                NewStatus = OrderStatus.Pending,
                ChangedByUserId = customerUser.UserId,
                Note = "Khách hàng đặt hàng",
                ChangedAt = DateTime.Now.AddHours(-2)
            });

            await context.SaveChangesAsync();
        }

        // 9. Initial Stock Document if none exists
        if (!await context.StockDocuments.AnyAsync())
        {
            var supNike = await context.Suppliers.FirstAsync(s => s.SupplierName.Contains("Nike"));
            var allVariants = await context.ProductVariants.Take(5).ToListAsync();

            var doc = new StockDocument
            {
                DocumentCode = "PN-20261101-01",
                DocumentType = StockDocumentType.PurchaseReceipt,
                DocumentStatus = StockDocumentStatus.Confirmed,
                SupplierId = supNike.SupplierId,
                Reason = "Nhập hàng đầu kỳ từ nhà cung cấp Nike Việt Nam",
                CreatedByUserId = admin.UserId,
                ConfirmedByUserId = admin.UserId,
                CreatedAt = DateTime.Now.AddDays(-10),
                ConfirmedAt = DateTime.Now.AddDays(-10)
            };

            context.StockDocuments.Add(doc);
            await context.SaveChangesAsync();

            foreach (var v in allVariants)
            {
                context.StockDocumentItems.Add(new StockDocumentItem
                {
                    DocumentId = doc.DocumentId,
                    VariantId = v.VariantId,
                    Quantity = v.StockQuantity,
                    UnitCost = v.Price * 0.6m,
                    Note = "Lô hàng mới nguyên hộp"
                });
            }

            await context.SaveChangesAsync();
        }
    }
}
