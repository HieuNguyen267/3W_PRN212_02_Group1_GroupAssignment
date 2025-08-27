using System.Collections.Generic;
using System.Linq;
using DAL.Entities;

namespace DAL.Repositories
{
    public class CategoryRepo : ICategoryRepo
    {
        private readonly ShoppingOnlineContext _context;

        public CategoryRepo()
        {
            _context = new ShoppingOnlineContext();
        }

        public List<Category> GetAll()
        {
            return _context.Categories.ToList();
        }

        public Category? GetById(string id)
        {
            return _context.Categories.FirstOrDefault(c => c.CategoryId == id);
        }

        public void Add(Category category)
        {
            _context.Categories.Add(category);
            _context.SaveChanges();
        }

        public void Update(Category category)
        {
            _context.Categories.Update(category);
            _context.SaveChanges();
        }

        public void Delete(string id)
        {
            var entity = _context.Categories.Find(id);
            if (entity != null)
            {
                _context.Categories.Remove(entity);
                _context.SaveChanges();
            }
        }

        public string GenerateNewCategoryId()
        {
            var lastCategory = _context.Categories
                .OrderByDescending(c => c.CategoryId)
                .FirstOrDefault();

            if (lastCategory == null)
                return "CAT000";

            // Lấy phần số, giả sử ID là CAT001 -> lấy "001"
            string numberPart = lastCategory.CategoryId.Substring(3);
            if (int.TryParse(numberPart, out int num))
            {
                return "CAT" + (num + 1).ToString("D3"); // luôn có 3 chữ số
            }

            return "CAT000"; // fallback
        }

    }
}
