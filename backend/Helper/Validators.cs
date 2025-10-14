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
            ValidateSlug(dto.Slug, "Product");
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
                _errors.Add("Id must be greater than 0");

            if (string.IsNullOrWhiteSpace(dto.Name))
                _errors.Add("Name is required");
            else if (dto.Name.Length > 200)
                _errors.Add("Name must not exceed 200 characters");

            if (string.IsNullOrWhiteSpace(dto.Slug))
                _errors.Add("Slug is required");

            if (string.IsNullOrWhiteSpace(dto.Brand))
                _errors.Add("Brand is required");

            if (string.IsNullOrWhiteSpace(dto.Description))
                _errors.Add("Description is required");
            else if (dto.Description.Length > 1000)
                _errors.Add("Description must not exceed 1000 characters");
        }

        private void ValidateSlug(string slug, string context)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                _errors.Add($"{context} slug is required");
                return;
            }

            // Slug must be lowercase, numbers, hyphens only
            var slugPattern = @"^[a-z0-9]+(-[a-z0-9]+)*$";
            if (!Regex.IsMatch(slug, slugPattern))
                _errors.Add($"{context} slug '{slug}' is invalid. Must contain only lowercase letters, numbers, and hyphens (no Vietnamese characters or special characters)");
        }

        private void ValidateAttributeOptions(CreateProductDocumentDto dto)
        {
            if (dto.AttributeOptions == null || !dto.AttributeOptions.Any())
            {
                _errors.Add("AttributeOptions is required and must have at least one attribute");
                return;
            }

            foreach (var attr in dto.AttributeOptions)
            {
                if (string.IsNullOrWhiteSpace(attr.Key))
                    _errors.Add("Attribute key cannot be empty");

                if (attr.Value == null || !attr.Value.Any())
                    _errors.Add($"Attribute '{attr.Key}' must have at least one value");
                else if (attr.Value.Any(string.IsNullOrWhiteSpace))
                    _errors.Add($"Attribute '{attr.Key}' contains empty values");
                else if (attr.Value.Distinct().Count() != attr.Value.Count)
                    _errors.Add($"Attribute '{attr.Key}' contains duplicate values");
            }
        }

        private void ValidateVariants(CreateProductDocumentDto dto)
        {
            if (dto.Variants == null || !dto.Variants.Any())
            {
                _errors.Add("Variants is required and must have at least one variant");
                return;
            }

            // Calculate expected variant count (cartesian product)
            var expectedVariantCount = dto.AttributeOptions.Values
                .Aggregate(1, (acc, values) => acc * values.Count);

            if (dto.Variants.Count != expectedVariantCount)
            {
                _errors.Add($"Expected {expectedVariantCount} variants (based on attribute combinations), but got {dto.Variants.Count}");
            }

            // Validate each variant
            var variantSlugs = new HashSet<string>();
            var variantCombinations = new HashSet<string>();

            for (int i = 0; i < dto.Variants.Count; i++)
            {
                var variant = dto.Variants[i];
                var context = $"Variant[{i}]";

                ValidateVariantBasic(variant, context);
                ValidateVariantAttributes(variant, dto.AttributeOptions, context);
                ValidateVariantPrices(variant, context);
                ValidateVariantSpecifications(variant, context);

                // Check duplicate slugs
                if (!string.IsNullOrWhiteSpace(variant.Slug))
                {
                    if (!variantSlugs.Add(variant.Slug))
                        _errors.Add($"{context}: Duplicate slug '{variant.Slug}'");
                }

                // Check duplicate attribute combinations
                var combination = string.Join("|", variant.Attributes.OrderBy(x => x.Key).Select(x => $"{x.Key}:{x.Value}"));
                if (!string.IsNullOrWhiteSpace(combination))
                {
                    if (!variantCombinations.Add(combination))
                        _errors.Add($"{context}: Duplicate attribute combination");
                }
            }

            // Validate all combinations are present
            ValidateAllCombinationsPresent(dto);
        }

        private void ValidateVariantBasic(CreateVariantDto variant, string context)
        {
            if (string.IsNullOrWhiteSpace(variant.Slug))
                _errors.Add($"{context}: Slug is required");
            else
                ValidateSlug(variant.Slug, context);

            if (variant.Attributes == null || !variant.Attributes.Any())
                _errors.Add($"{context}: Attributes is required");
        }

        private void ValidateVariantAttributes(CreateVariantDto variant, Dictionary<string, List<string>> attributeOptions, string context)
        {
            if (variant.Attributes == null) return;

            // Check if variant has all required attributes
            foreach (var attrKey in attributeOptions.Keys)
            {
                if (!variant.Attributes.ContainsKey(attrKey))
                    _errors.Add($"{context}: Missing attribute '{attrKey}'");
            }

            // Check if variant attributes are valid
            foreach (var attr in variant.Attributes)
            {
                if (!attributeOptions.ContainsKey(attr.Key))
                {
                    _errors.Add($"{context}: Unknown attribute '{attr.Key}'");
                }
                else if (!attributeOptions[attr.Key].Contains(attr.Value))
                {
                    _errors.Add($"{context}: Invalid value '{attr.Value}' for attribute '{attr.Key}'. Valid values: {string.Join(", ", attributeOptions[attr.Key])}");
                }
            }

            // Check for extra attributes
            if (variant.Attributes.Count != attributeOptions.Count)
                _errors.Add($"{context}: Must have exactly {attributeOptions.Count} attributes");
        }

        private void ValidateVariantPrices(CreateVariantDto variant, string context)
        {
            if (variant.OriginalPrice <= 0)
                _errors.Add($"{context}: OriginalPrice must be greater than 0");

            if (variant.DiscountedPrice <= 0)
                _errors.Add($"{context}: DiscountedPrice must be greater than 0");

            if (variant.DiscountedPrice > variant.OriginalPrice)
                _errors.Add($"{context}: DiscountedPrice ({variant.DiscountedPrice}) cannot be greater than OriginalPrice ({variant.OriginalPrice})");
        }

        private void ValidateVariantSpecifications(CreateVariantDto variant, string context)
        {
            if (variant.Specifications == null || !variant.Specifications.Any())
            {
                _errors.Add($"{context}: Specifications is required and must have at least one specification");
                return;
            }

            var specLabels = new HashSet<string>();
            for (int j = 0; j < variant.Specifications.Count; j++)
            {
                var spec = variant.Specifications[j];
                var specContext = $"{context}.Specification[{j}]";

                if (string.IsNullOrWhiteSpace(spec.Label))
                    _errors.Add($"{specContext}: Label is required");
                else if (!specLabels.Add(spec.Label))
                    _errors.Add($"{specContext}: Duplicate label '{spec.Label}'");

                if (string.IsNullOrWhiteSpace(spec.Value))
                    _errors.Add($"{specContext}: Value is required");
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
                _errors.Add($"Missing variant combinations: {string.Join(", ", missingCombinations.Select(c => $"[{c.Replace("|", ", ")}]"))}");
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