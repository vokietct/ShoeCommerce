using ShoeCommerce.Models;

namespace ShoeCommerce.Services;

public interface ICategoryBrandService
{
    Task<List<Category>> GetAllCategoriesAsync();
    Task<List<Brand>> GetAllBrandsAsync();

    // Category CRUD
    Task<(bool Success, string Message, int? CategoryId)> CreateCategoryAsync(string name, string? description);
    Task<(bool Success, string Message)> UpdateCategoryAsync(int id, string name, string? description);
    Task<(bool Success, string Message)> DeleteCategoryAsync(int id);

    // Brand CRUD
    Task<(bool Success, string Message, int? BrandId)> CreateBrandAsync(string name, string? description);
    Task<(bool Success, string Message)> UpdateBrandAsync(int id, string name, string? description);
    Task<(bool Success, string Message)> DeleteBrandAsync(int id);
}

