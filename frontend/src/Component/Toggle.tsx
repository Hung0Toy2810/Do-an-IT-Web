import React, { useState } from "react";

interface ToggleProps {
  checked?: boolean;
  onChange?: (checked: boolean) => void;
  label?: string;
  width?: number;   // chiều rộng toggle
  height?: number;  // chiều cao toggle
}

const Toggle: React.FC<ToggleProps> = ({
  checked = false,
  onChange,
  label,
  width = 40,   // mặc định ~ w-12
  height = 18,  // mặc định ~ h-7
}) => {
  const [isChecked, setIsChecked] = useState(checked);

  const handleToggle = () => {
    const newValue = !isChecked;
    setIsChecked(newValue);
    onChange?.(newValue);
  };

  // đường kính nút tròn = height - 4px (trừ padding trên dưới)
  const knobSize = height - 4;
  const knobOffset = isChecked ? width - knobSize - 2 : 2;

  return (
    <label className="flex items-center gap-3 cursor-pointer group">
      <div
        className="relative transition-all duration-300"
        style={{
          width,
          height,
          borderRadius: height / 2,
          backgroundColor: isChecked ? "#5B21B6" : "#D1D5DB", // violet-800 : gray-300
        }}
        onClick={handleToggle}
      >
        <div
          className="absolute transition-all duration-300 bg-white shadow-md"
          style={{
            top: 2,
            left: knobOffset,
            width: knobSize,
            height: knobSize,
            borderRadius: "50%",
          }}
        />
      </div>
      {label && (
        <span className="text-sm font-medium text-gray-900 transition-colors group-hover:text-violet-800">
          {label}
        </span>
      )}
    </label>
  );
};

export default Toggle;
