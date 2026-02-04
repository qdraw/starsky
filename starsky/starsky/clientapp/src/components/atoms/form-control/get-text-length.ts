import React from "react";

export default function getTextLength(children: React.ReactNode): number {
  if (typeof children === "string" || typeof children === "number") {
    return children.toString().length;
  }
  if (Array.isArray(children)) {
    return children.reduce((sum, child) => sum + getTextLength(child), 0);
  }
  if (React.isValidElement(children)) {
    return getTextLength(children.props.children);
  }
  return 0;
}
