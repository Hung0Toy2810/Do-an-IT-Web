dotnet ef migrations add InitialCreate
dotnet ef database update
docker-compose up -d
docker ps
docker-compose down
git init
git add .
git commit -m "Create change password page"
git remote add origin https://github.com/Hung0Toy2810/Do-an-IT-Web.git
git branch -M main
git push -u origin main
dotnet ef database drop
{
  "id": 1,
  "name": "iPhone 15 Pro Max",
  "slug": "iphone-15-pro-max",
  "subcategory": "Điện thoại",
  "brand": "Apple",
  "description": "iPhone 15 Pro Max mang đến hiệu năng đỉnh cao với chip A17 Pro và thiết kế titan sang trọng.",
  "attributeOptions": {
    "Màu sắc": ["Titan Tự Nhiên", "Titan Xanh", "Titan Trắng"],
    "Dung lượng": ["256GB", "512GB", "1TB"]
  },
  "variants": [
    {
      "attributes": { "Màu sắc": "Titan Tự Nhiên", "Dung lượng": "256GB" },
      "stock": 10,
      "originalPrice": 34990000,
      "discountedPrice": 29990000,
      "images": [
        "https://images.unsplash.com/photo-1695048133142-1a20484d2569?w=800&q=80",
        "https://images.unsplash.com/photo-1696446702403-d4eb1f6c3e7c?w=800&q=80"
      ],
      "specifications": [
        { "label": "Màn hình", "value": "6.7\" Super Retina XDR OLED, 120Hz" },
        { "label": "Chip xử lý", "value": "Apple A17 Pro 6 nhân" },
        { "label": "RAM", "value": "8GB" },
        { "label": "Bộ nhớ trong", "value": "256GB" },
        { "label": "Camera sau", "value": "48MP + 12MP + 12MP" },
        { "label": "Camera trước", "value": "12MP" },
        { "label": "Pin", "value": "4422 mAh" },
        { "label": "Trọng lượng", "value": "221g" }
      ]
    },
    {
      "attributes": { "Màu sắc": "Titan Tự Nhiên", "Dung lượng": "512GB" },
      "stock": 5,
      "originalPrice": 37990000,
      "discountedPrice": 32990000,
      "images": [
        "https://images.unsplash.com/photo-1703084480082-bc8736ef764b?w=800&q=80",
        "https://images.unsplash.com/photo-1703093141513-13a2b4058a4b?w=800&q=80"
      ],
      "specifications": [
        { "label": "Màn hình", "value": "6.7\" Super Retina XDR OLED, 120Hz" },
        { "label": "Chip xử lý", "value": "Apple A17 Pro 6 nhân" },
        { "label": "RAM", "value": "8GB" },
        { "label": "Bộ nhớ trong", "value": "512GB" },
        { "label": "Camera sau", "value": "48MP + 12MP + 12MP" },
        { "label": "Camera trước", "value": "12MP" },
        { "label": "Pin", "value": "4422 mAh" },
        { "label": "Trọng lượng", "value": "222g" }
      ]
    },
    {
      "attributes": { "Màu sắc": "Titan Tự Nhiên", "Dung lượng": "1TB" },
      "stock": 0,
      "originalPrice": 40990000,
      "discountedPrice": 35990000,
      "images": [
        "https://images.unsplash.com/photo-1703083537253-c3c7fbb7c90b?w=800&q=80",
        "https://images.unsplash.com/photo-1703083567835-5e44719fba57?w=800&q=80"
      ],
      "specifications": [
        { "label": "Màn hình", "value": "6.7\" Super Retina XDR OLED, 120Hz" },
        { "label": "Chip xử lý", "value": "Apple A17 Pro 6 nhân" },
        { "label": "RAM", "value": "8GB" },
        { "label": "Bộ nhớ trong", "value": "1TB" },
        { "label": "Camera sau", "value": "48MP + 12MP + 12MP" },
        { "label": "Camera trước", "value": "12MP" },
        { "label": "Pin", "value": "4422 mAh" },
        { "label": "Trọng lượng", "value": "223g" }
      ]
    },
    {
      "attributes": { "Màu sắc": "Titan Xanh", "Dung lượng": "256GB" },
      "stock": 8,
      "originalPrice": 34990000,
      "discountedPrice": 29990000,
      "images": [
        "https://images.unsplash.com/photo-1703104458879-72f6b4f5cf4a?w=800&q=80",
        "https://images.unsplash.com/photo-1703104571444-049c4ad9c24b?w=800&q=80"
      ],
      "specifications": [
        { "label": "Màn hình", "value": "6.7\" Super Retina XDR OLED, 120Hz" },
        { "label": "Chip xử lý", "value": "Apple A17 Pro 6 nhân (tối ưu cho gam màu xanh)" },
        { "label": "RAM", "value": "8GB" },
        { "label": "Bộ nhớ trong", "value": "256GB" },
        { "label": "Camera sau", "value": "48MP + 12MP + 12MP (Deep Fusion tối ưu màu xanh)" },
        { "label": "Camera trước", "value": "12MP" },
        { "label": "Pin", "value": "4420 mAh" },
        { "label": "Trọng lượng", "value": "220g" }
      ]
    },
    {
      "attributes": { "Màu sắc": "Titan Xanh", "Dung lượng": "512GB" },
      "stock": 3,
      "originalPrice": 37990000,
      "discountedPrice": 32990000,
      "images": [
        "https://images.unsplash.com/photo-1703095536422-b2a374b60d7e?w=800&q=80",
        "https://images.unsplash.com/photo-1703095631221-34d2c0989fcb?w=800&q=80"
      ],
      "specifications": [
        { "label": "Màn hình", "value": "6.7\" Super Retina XDR OLED, 120Hz" },
        { "label": "Chip xử lý", "value": "Apple A17 Pro 6 nhân" },
        { "label": "RAM", "value": "8GB" },
        { "label": "Bộ nhớ trong", "value": "512GB" },
        { "label": "Camera sau", "value": "48MP + 12MP + 12MP" },
        { "label": "Camera trước", "value": "12MP" },
        { "label": "Pin", "value": "4418 mAh" },
        { "label": "Trọng lượng", "value": "221g" }
      ]
    },
    {
      "attributes": { "Màu sắc": "Titan Xanh", "Dung lượng": "1TB" },
      "stock": 2,
      "originalPrice": 40990000,
      "discountedPrice": 35990000,
      "images": [
        "https://images.unsplash.com/photo-1703096221772-9dd6df66a92a?w=800&q=80",
        "https://images.unsplash.com/photo-1703096332254-b7aaf830f923?w=800&q=80"
      ],
      "specifications": [
        { "label": "Màn hình", "value": "6.7\" Super Retina XDR OLED, 120Hz" },
        { "label": "Chip xử lý", "value": "Apple A17 Pro 6 nhân" },
        { "label": "RAM", "value": "8GB" },
        { "label": "Bộ nhớ trong", "value": "1TB" },
        { "label": "Camera sau", "value": "48MP + 12MP + 12MP" },
        { "label": "Camera trước", "value": "12MP" },
        { "label": "Pin", "value": "4425 mAh" },
        { "label": "Trọng lượng", "value": "223g" }
      ]
    },
    {
      "attributes": { "Màu sắc": "Titan Trắng", "Dung lượng": "256GB" },
      "stock": 15,
      "originalPrice": 34990000,
      "discountedPrice": 29990000,
      "images": [
        "https://images.unsplash.com/photo-1703084622021-b5d4b43dc41d?w=800&q=80",
        "https://images.unsplash.com/photo-1703084742245-16ab4dcda10d?w=800&q=80"
      ],
      "specifications": [
        { "label": "Màn hình", "value": "6.7\" Super Retina XDR OLED, 120Hz" },
        { "label": "Chip xử lý", "value": "Apple A17 Pro 6 nhân" },
        { "label": "RAM", "value": "8GB" },
        { "label": "Bộ nhớ trong", "value": "256GB" },
        { "label": "Camera sau", "value": "48MP + 12MP + 12MP" },
        { "label": "Camera trước", "value": "12MP" },
        { "label": "Pin", "value": "4430 mAh" },
        { "label": "Trọng lượng", "value": "220g" }
      ]
    },
    {
      "attributes": { "Màu sắc": "Titan Trắng", "Dung lượng": "512GB" },
      "stock": 0,
      "originalPrice": 37990000,
      "discountedPrice": 32990000,
      "images": [
        "https://images.unsplash.com/photo-1703084890120-90d6a1d25a53?w=800&q=80",
        "https://images.unsplash.com/photo-1703084989991-8f1b84a2fd35?w=800&q=80"
      ],
      "specifications": [
        { "label": "Màn hình", "value": "6.7\" Super Retina XDR OLED, 120Hz" },
        { "label": "Chip xử lý", "value": "Apple A17 Pro 6 nhân" },
        { "label": "RAM", "value": "8GB" },
        { "label": "Bộ nhớ trong", "value": "512GB" },
        { "label": "Camera sau", "value": "48MP + 12MP + 12MP (tối ưu ánh sáng trắng)" },
        { "label": "Camera trước", "value": "12MP" },
        { "label": "Pin", "value": "4432 mAh" },
        { "label": "Trọng lượng", "value": "221g" }
      ]
    },
    {
      "attributes": { "Màu sắc": "Titan Trắng", "Dung lượng": "1TB" },
      "stock": 1,
      "originalPrice": 40990000,
      "discountedPrice": 35990000,
      "images": [
        "https://images.unsplash.com/photo-1703085058992-bfa5a4a1a7e2?w=800&q=80",
        "https://images.unsplash.com/photo-1703085163341-f6482c1e23b8?w=800&q=80"
      ],
      "specifications": [
        { "label": "Màn hình", "value": "6.7\" Super Retina XDR OLED, 120Hz" },
        { "label": "Chip xử lý", "value": "Apple A17 Pro 6 nhân" },
        { "label": "RAM", "value": "8GB" },
        { "label": "Bộ nhớ trong", "value": "1TB" },
        { "label": "Camera sau", "value": "48MP + 12MP + 12MP" },
        { "label": "Camera trước", "value": "12MP" },
        { "label": "Pin", "value": "4435 mAh" },
        { "label": "Trọng lượng", "value": "223g" }
      ]
    }
  ]
}


