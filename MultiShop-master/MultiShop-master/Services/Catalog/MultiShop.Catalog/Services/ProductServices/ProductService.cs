using AutoMapper;
using MongoDB.Driver;
using MultiShop.Catalog.Dtos.CategoryDtos;
using MultiShop.Catalog.Dtos.ProductDtos;
using MultiShop.Catalog.Entites;
using MultiShop.Catalog.Settings;

namespace MultiShop.Catalog.Services.ProductServices
{
    public class ProductService : IProductService
    {
        private readonly IMapper _mapper;
        private readonly IMongoCollection<Product> _productCollection;
        private readonly IMongoCollection<Category> _categoryCollection;
        public ProductService(IMapper mapper, IDatabaseSettings _databaseSettings)
        {
            var client = new MongoClient(_databaseSettings.ConnectionString);
            var database = client.GetDatabase(_databaseSettings.DatabaseName);
            _productCollection = database.GetCollection<Product>(_databaseSettings.ProductCollectionName);
            _categoryCollection = database.GetCollection<Category>(_databaseSettings.CategoryCollectionName);
            _mapper = mapper;
        }
        public async Task CreateProductAsync(CreateProductDto createProductDto)
        {
            var values = _mapper.Map<Product>(createProductDto);
            await _productCollection.InsertOneAsync(values);
        }
        public async Task DeleteProductAsync(string id)
        {
            await _productCollection.DeleteOneAsync(x => x.ProductId == id);
        }
        public async Task<GetByIdProductDto> GetByIdProductAsync(string id)
        {
            var values = await _productCollection.Find<Product>(x => x.ProductId == id).FirstOrDefaultAsync();
            return _mapper.Map<GetByIdProductDto>(values);
        }

        public async Task<List<ResultProductsWithCategoryDto>> GetProductsWithCategoryAsync()
        {
            var values = await _productCollection.Find(x => true).ToListAsync();

            foreach (var item in values)
            {
                item.Category = await _categoryCollection.Find<Category>(x => x.CategoryId == item.CategoryId).FirstAsync();
            }

            return _mapper.Map<List<ResultProductsWithCategoryDto>>(values);
        }

        public async Task<List<ResultProductsWithCategoryDto>> GetProductsWithCategoryByCatetegoryIdAsync(string CategoryId)
        {
            var values = await _productCollection.Find(x => x.CategoryId == CategoryId).ToListAsync();

            foreach (var item in values)
            {
                item.Category = await _categoryCollection.Find<Category>(x => x.CategoryId == item.CategoryId).FirstAsync();
            }

            return _mapper.Map<List<ResultProductsWithCategoryDto>>(values);
        }

        public async Task<List<ResultProductDto>> GettAllProductAsync()
        {
            var values = await _productCollection.Find(x => true).ToListAsync();
            return _mapper.Map<List<ResultProductDto>>(values);
        }
        public async Task UpdateProductAsync(UpdateProductDto updateProductDto)
        {
            var values = _mapper.Map<Product>(updateProductDto);
            await _productCollection.FindOneAndReplaceAsync(x => x.ProductId == updateProductDto.ProductId, values);
        }

        public async Task<List<ResultProductsWithCategoryDto>> GetFilteredProductsAsync(ProductFilterRequestDto filter)
        {
            filter ??= new ProductFilterRequestDto();

            var builder = Builders<Product>.Filter;
            var filters = new List<FilterDefinition<Product>>();

            if (!string.IsNullOrWhiteSpace(filter.CategoryId))
                filters.Add(builder.Eq(x => x.CategoryId, filter.CategoryId.Trim()));

            if (!string.IsNullOrWhiteSpace(filter.Color))
            {
                var colorPattern = "^" + System.Text.RegularExpressions.Regex.Escape(filter.Color.Trim()) + "$";
                filters.Add(builder.Regex(x => x.Color, new MongoDB.Bson.BsonRegularExpression(colorPattern, "i")));
            }

            if (!string.IsNullOrWhiteSpace(filter.Size))
            {
                var sizePattern = "^" + System.Text.RegularExpressions.Regex.Escape(filter.Size.Trim()) + "$";
                filters.Add(builder.Regex(x => x.Size, new MongoDB.Bson.BsonRegularExpression(sizePattern, "i")));
            }

            if (filter.MinPrice.HasValue)
                filters.Add(builder.Gte(x => x.ProductPrice, filter.MinPrice.Value));

            if (filter.MaxPrice.HasValue)
                filters.Add(builder.Lte(x => x.ProductPrice, filter.MaxPrice.Value));

            if (filter.InStockOnly)
                filters.Add(builder.Gt(x => x.Stock, 0));

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var regex = new MongoDB.Bson.BsonRegularExpression(filter.Search, "i");
                filters.Add(builder.Regex(x => x.ProductName, regex));
            }

            var combined = filters.Count == 0 ? builder.Empty : builder.And(filters);
            var values = await _productCollection.Find(combined).ToListAsync();

            foreach (var item in values)
            {
                if (!string.IsNullOrWhiteSpace(item.CategoryId))
                {
                    item.Category = await _categoryCollection
                        .Find<Category>(x => x.CategoryId == item.CategoryId)
                        .FirstOrDefaultAsync();
                }
            }

            return _mapper.Map<List<ResultProductsWithCategoryDto>>(values);
        }

        public async Task<List<string>> GetDistinctColorsAsync()
        {
            var distinct = await _productCollection.DistinctAsync(x => x.Color, Builders<Product>.Filter.Empty);
            var list = await distinct.ToListAsync();
            return list
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .GroupBy(x => x, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public async Task<List<string>> GetDistinctSizesAsync()
        {
            var distinct = await _productCollection.DistinctAsync(x => x.Size, Builders<Product>.Filter.Empty);
            var list = await distinct.ToListAsync();
            return list
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .GroupBy(x => x, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
