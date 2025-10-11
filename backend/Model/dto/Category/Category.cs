namespace Backend.Model.dto.Category
{
    public class CreateCategoryDto
    {
        public string Name { get; set; } = string.Empty;
    }

    public class UpdateCategoryDto
    {
        public string Name { get; set; } = string.Empty;
    }

    public class CreateSubCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public long CategoryId { get; set; }
    }

    public class UpdateSubCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public long CategoryId { get; set; }
    }

    public class CategoryDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public List<SubCategoryDto> SubCategories { get; set; } = new List<SubCategoryDto>();
    }

    public class SubCategoryDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public long CategoryId { get; set; }
    }
}