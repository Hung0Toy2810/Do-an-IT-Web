// Validators/ProductDocumentValidator.cs
using Backend.Model.dto.Product;
using System.Text.RegularExpressions;

namespace Backend.Validators
{
    public class ProductDocumentValidator
    {
        private readonly List<string> _errors = new();

        public void Validate(CreateProductDocumentDto dto)
        {
            _errors.Clear();

            ValidateBasicFields(dto);
            ValidateSlug(dto.Slug, "Sản phẩm");
            ValidateAttributeOptions(dto);
            ValidateVariants(dto);

            if (_errors.Any())
            {
                throw new Backend.Exceptions.ValidationException(string.Join("; ", _errors), _errors);
            }
        }

        private void ValidateBasicFields(CreateProductDocumentDto dto)
        {
            if (dto.Id <= 0)
                _errors.Add("Id phải lớn hơn 0");

            if (string.IsNullOrWhiteSpace(dto.Name))
                _errors.Add("Tên sản phẩm là bắt buộc");
            else if (dto.Name.Length > 200)
                _errors.Add("Tên sản phẩm không được vượt quá 200 ký tự");

            if (string.IsNullOrWhiteSpace(dto.Slug))
                _errors.Add("Slug là bắt buộc");

            if (string.IsNullOrWhiteSpace(dto.Brand))
                _errors.Add("Thương hiệu là bắt buộc");

            if (string.IsNullOrWhiteSpace(dto.Description))
                _errors.Add("Mô tả là bắt buộc");
            else if (dto.Description.Length > 1000)
                _errors.Add("Mô tả không được vượt quá 1000 ký tự");
        }

        private void ValidateSlug(string slug, string context)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                _errors.Add($"Slug của {context} là bắt buộc");
                return;
            }

            // Slug must be lowercase, numbers, hyphens only
            var slugPattern = @"^[a-z0-9]+(-[a-z0-9]+)*$";
            if (!Regex.IsMatch(slug, slugPattern))
                _errors.Add($"Slug của {context} '{slug}' không hợp lệ. Chỉ được chứa chữ thường, số và dấu gạch ngang (không chứa ký tự tiếng Việt hoặc ký tự đặc biệt)");
        }

        private void ValidateAttributeOptions(CreateProductDocumentDto dto)
        {
            if (dto.AttributeOptions == null || !dto.AttributeOptions.Any())
            {
                _errors.Add("AttributeOptions là bắt buộc và phải có ít nhất một thuộc tính");
                return;
            }

            foreach (var attr in dto.AttributeOptions)
            {
                if (string.IsNullOrWhiteSpace(attr.Key))
                    _errors.Add("Khóa thuộc tính không được rỗng");

                if (attr.Value == null || !attr.Value.Any())
                    _errors.Add($"Thuộc tính '{attr.Key}' phải có ít nhất một giá trị");
                else if (attr.Value.Any(string.IsNullOrWhiteSpace))
                    _errors.Add($"Thuộc tính '{attr.Key}' chứa giá trị rỗng");
                else if (attr.Value.Distinct().Count() != attr.Value.Count)
                    _errors.Add($"Thuộc tính '{attr.Key}' chứa giá trị trùng lặp");
            }
        }

        private void ValidateVariants(CreateProductDocumentDto dto)
        {
            if (dto.Variants == null || !dto.Variants.Any())
            {
                _errors.Add("Danh sách biến thể là bắt buộc và phải có ít nhất một biến thể");
                return;
            }

            // Calculate expected variant count (cartesian product)
            var expectedVariantCount = dto.AttributeOptions.Values
                .Aggregate(1, (acc, values) => acc * values.Count);

            if (dto.Variants.Count != expectedVariantCount)
            {
                _errors.Add($"Kỳ vọng có {expectedVariantCount} biến thể (dựa trên tổ hợp thuộc tính), nhưng chỉ nhận được {dto.Variants.Count}");
            }

            // Validate each variant
            var variantSlugs = new HashSet<string>();
            var variantCombinations = new HashSet<string>();

            for (int i = 0; i < dto.Variants.Count; i++)
            {
                var variant = dto.Variants[i];
                var context = $"Biến thể[{i}]";

                ValidateVariantBasic(variant, context);
                ValidateVariantAttributes(variant, dto.AttributeOptions, context);
                ValidateVariantPrices(variant, context);
                ValidateVariantSpecifications(variant, context);

                // Check duplicate slugs
                if (!string.IsNullOrWhiteSpace(variant.Slug))
                {
                    if (!variantSlugs.Add(variant.Slug))
                        _errors.Add($"{context}: Slug trùng lặp '{variant.Slug}'");
                }

                // Check duplicate attribute combinations
                var combination = string.Join("|", variant.Attributes.OrderBy(x => x.Key).Select(x => $"{x.Key}:{x.Value}"));
                if (!string.IsNullOrWhiteSpace(combination))
                {
                    if (!variantCombinations.Add(combination))
                        _errors.Add($"{context}: Tổ hợp thuộc tính trùng lặp");
                }
            }

            // Validate all combinations are present
            ValidateAllCombinationsPresent(dto);
        }

        private void ValidateVariantBasic(CreateVariantDto variant, string context)
        {
            if (string.IsNullOrWhiteSpace(variant.Slug))
                _errors.Add($"{context}: Slug là bắt buộc");
            else
                ValidateSlug(variant.Slug, context);

            if (variant.Attributes == null || !variant.Attributes.Any())
                _errors.Add($"{context}: Thuộc tính là bắt buộc");
        }

        private void ValidateVariantAttributes(CreateVariantDto variant, Dictionary<string, List<string>> attributeOptions, string context)
        {
            if (variant.Attributes == null) return;

            // Check if variant has all required attributes
            foreach (var attrKey in attributeOptions.Keys)
            {
                if (!variant.Attributes.ContainsKey(attrKey))
                    _errors.Add($"{context}: Thiếu thuộc tính '{attrKey}'");
            }

            // Check if variant attributes are valid
            foreach (var attr in variant.Attributes)
            {
                if (!attributeOptions.ContainsKey(attr.Key))
                {
                    _errors.Add($"{context}: Thuộc tính không xác định '{attr.Key}'");
                }
                else if (!attributeOptions[attr.Key].Contains(attr.Value))
                {
                    _errors.Add($"{context}: Giá trị không hợp lệ '{attr.Value}' cho thuộc tính '{attr.Key}'. Giá trị hợp lệ: {string.Join(", ", attributeOptions[attr.Key])}");
                }
            }

            // Check for extra attributes
            if (variant.Attributes.Count != attributeOptions.Count)
                _errors.Add($"{context}: Phải có chính xác {attributeOptions.Count} thuộc tính");
        }

        private void ValidateVariantPrices(CreateVariantDto variant, string context)
        {
            if (variant.OriginalPrice <= 0)
                _errors.Add($"{context}: Giá gốc phải lớn hơn 0");

            if (variant.DiscountedPrice <= 0)
                _errors.Add($"{context}: Giá giảm phải lớn hơn 0");

            if (variant.DiscountedPrice > variant.OriginalPrice)
                _errors.Add($"{context}: Giá giảm ({variant.DiscountedPrice}) không được lớn hơn giá gốc ({variant.OriginalPrice})");
        }

        private void ValidateVariantSpecifications(CreateVariantDto variant, string context)
        {
            if (variant.Specifications == null || !variant.Specifications.Any())
            {
                _errors.Add($"{context}: Thông số kỹ thuật là bắt buộc và phải có ít nhất một thông số");
                return;
            }

            var specLabels = new HashSet<string>();
            for (int j = 0; j < variant.Specifications.Count; j++)
            {
                var spec = variant.Specifications[j];
                var specContext = $"{context}.Thông số[{j}]";

                if (string.IsNullOrWhiteSpace(spec.Label))
                    _errors.Add($"{specContext}: Nhãn là bắt buộc");
                else if (!specLabels.Add(spec.Label))
                    _errors.Add($"{specContext}: Nhãn trùng lặp '{spec.Label}'");

                if (string.IsNullOrWhiteSpace(spec.Value))
                    _errors.Add($"{specContext}: Giá trị là bắt buộc");
            }
        }

        private void ValidateAllCombinationsPresent(CreateProductDocumentDto dto)
        {
            var allCombinations = GenerateAllCombinations(dto.AttributeOptions);
            var providedCombinations = dto.Variants
                .Select(v => string.Join("|", v.Attributes.OrderBy(x => x.Key).Select(x => $"{x.Key}:{x.Value}")))
                .ToHashSet();

            var missingCombinations = allCombinations.Except(providedCombinations).ToList();
            if (missingCombinations.Any())
            {
                _errors.Add($"Thiếu các tổ hợp biến thể: {string.Join(", ", missingCombinations.Select(c => $"[{c.Replace("|", ", ")}]"))}");
            }
        }

        private HashSet<string> GenerateAllCombinations(Dictionary<string, List<string>> attributeOptions)
        {
            var result = new HashSet<string>();
            var keys = attributeOptions.Keys.ToList();
            var values = attributeOptions.Values.ToList();

            GenerateCombinationsRecursive(keys, values, 0, new Dictionary<string, string>(), result);
            return result;
        }

        private void GenerateCombinationsRecursive(
            List<string> keys,
            List<List<string>> values,
            int depth,
            Dictionary<string, string> current,
            HashSet<string> result)
        {
            if (depth == keys.Count)
            {
                var combination = string.Join("|", current.OrderBy(x => x.Key).Select(x => $"{x.Key}:{x.Value}"));
                result.Add(combination);
                return;
            }

            foreach (var value in values[depth])
            {
                current[keys[depth]] = value;
                GenerateCombinationsRecursive(keys, values, depth + 1, current, result);
            }
        }
    }
}