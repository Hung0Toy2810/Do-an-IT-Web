import React, { useState } from "react";
import { Search, X } from "lucide-react";

interface SearchBarProps {
  placeholder?: string;
  onSearch?: (value: string) => void;
  width?: string | number;
  height?: string | number;
}

const SearchBar: React.FC<SearchBarProps> = ({
  placeholder = "Tìm kiếm...",
  onSearch,
  width = "100%",
  height = "44px",
}) => {
  const [value, setValue] = useState("");
  const [isMobileSearchOpen, setIsMobileSearchOpen] = useState(false);

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === "Enter") {
      onSearch?.(value);
      setIsMobileSearchOpen(false);
    }
  };

  const handleSearchClick = () => {
    onSearch?.(value);
    setIsMobileSearchOpen(false);
  };

  // Chuẩn hóa giá trị width và height
  const normalizedWidth = typeof width === "number" ? `${width}px` : width;
  const normalizedHeight = typeof height === "number" ? `${height}px` : height;

  return (
    <div className="relative w-full max-w-2xl p-4 mx-auto">
      {/* Desktop / Tablet */}
      <div
        className="relative hidden w-full md:block"
        style={{ height: normalizedHeight }}
      >
        <input
          type="search"
          value={value}
          onChange={(e) => setValue(e.target.value)}
          onKeyDown={handleKeyDown}
          placeholder={placeholder}
          className="block w-full h-full px-4 pr-24 text-sm font-medium text-gray-900 placeholder-gray-500 transition-all bg-gray-100 border border-gray-200 focus:outline-none focus:ring-2 focus:ring-violet-800/20 focus:border-violet-800 rounded-xl [&::-webkit-search-cancel-button]:hidden [&::-webkit-search-decoration]:hidden"
        />
        <button
          type="button"
          onClick={handleSearchClick}
          className="absolute top-0 right-0 flex items-center justify-center h-full gap-2 px-4 text-sm font-medium text-white transition-all border bg-violet-800 border-violet-800 rounded-r-xl hover:bg-violet-900 focus:ring-4 focus:outline-none focus:ring-violet-300"
        >
          <Search size={18} />
        </button>
      </div>

      {/* Mobile - nút mở search */}
      <div className="md:hidden">
        <button
          type="button"
          onClick={() => setIsMobileSearchOpen(!isMobileSearchOpen)}
          className="flex items-center justify-center w-10 h-10 text-white transition-all rounded-lg bg-violet-800 hover:bg-violet-900 focus:ring-4 focus:outline-none focus:ring-violet-300"
        >
          <Search size={20} />
        </button>

        {/* Mobile search dropdown */}
        {isMobileSearchOpen && (
          <div className="absolute left-0 right-0 z-50 p-4 mt-2 bg-white border border-gray-200 shadow-lg rounded-xl">
            <div className="flex items-center gap-2 mb-3">
              <h3 className="text-sm font-semibold text-gray-900">Tìm kiếm</h3>
              <button
                type="button"
                onClick={() => setIsMobileSearchOpen(false)}
                className="ml-auto text-gray-400 hover:text-gray-600"
              >
                <X size={20} />
              </button>
            </div>
            <div className="relative" style={{ height: normalizedHeight }}>
              <input
                type="search"
                value={value}
                onChange={(e) => setValue(e.target.value)}
                onKeyDown={handleKeyDown}
                placeholder={placeholder}
                className="block w-full h-full px-4 pr-24 text-sm font-medium text-gray-900 placeholder-gray-500 transition-all bg-gray-100 border border-gray-200 focus:outline-none focus:ring-2 focus:ring-violet-800/20 focus:border-violet-800 rounded-xl"
                autoFocus
              />
              <button
                type="button"
                onClick={handleSearchClick}
                className="absolute top-0 right-0 flex items-center justify-center h-full gap-2 px-4 text-sm font-medium text-white transition-all border bg-violet-800 border-violet-800 rounded-r-xl hover:bg-violet-900 focus:ring-4 focus:outline-none focus:ring-violet-300"
              >
                <Search size={18} />
              </button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default SearchBar;