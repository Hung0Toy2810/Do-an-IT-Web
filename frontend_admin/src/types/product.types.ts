// src/types/product.types.ts

export interface SubCategoryDto {
  id: number;
  name: string;
  slug: string;
  categoryId: number;
}

export interface CategoryDto {
  id: number;
  name: string;
  slug: string;
  subCategories: SubCategoryDto[];
}

export interface ProductCardDto {
  id: number;
  name: string;
  slug: string;
  brand: string;
  firstImage: string | null;
  minDiscountedPrice: number;
  originalPriceOfMinVariant: number;
  isDiscontinued: boolean;
  rating: number;
  totalRatings: number;
}

export interface CreateCategoryDto { 
  name: string; 
}

export interface UpdateCategoryDto { 
  name: string; 
}

export interface CreateSubCategoryDto { 
  name: string; 
  categoryId: number; 
}

export interface UpdateSubCategoryDto { 
  name: string; 
  categoryId: number; 
}
// src/types/product.types.ts
export interface CreateProductDocumentDto {
  id: number;
  name: string;
  slug: string;
  brand: string;
  description: string;
  attributeOptions: Record<string, string[]>;
  variants: CreateVariantDto[];
}

export interface CreateVariantDto {
  slug: string;
  attributes: Record<string, string>;
  originalPrice: number;
  discountedPrice: number;
  specifications: { label: string; value: string }[];
}