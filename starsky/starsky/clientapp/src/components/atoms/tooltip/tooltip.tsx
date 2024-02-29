import React, { useState } from "react";

interface TooltipProps {
  text: string;
  children?: React.ReactNode;
  left: boolean;
}

const Tooltip: React.FC<TooltipProps> = ({ text, children, left }) => {
  const [showTooltip, setShowTooltip] = useState(false);

  const handleMouseEnter = () => {
    setShowTooltip((prevState) => !prevState);
  };

  const handleMouseLeave = () => {
    setShowTooltip(false);
  };

  return (
    <button
      className={left ? "tooltip-container left" : "tooltip-container"}
      onMouseEnter={handleMouseEnter}
      onClick={handleMouseEnter}
      onMouseLeave={handleMouseLeave}
    >
      {children}
      {showTooltip && (
        <span data-test="tooltip" className="tooltip">
          {text}
        </span>
      )}
    </button>
  );
};

export default Tooltip;
