import React from "react";

export default function Button({ children, href, color }) {
  return (
    <a
      href={href}
      style={{
        backgroundColor: color,
        borderRadius: "2px",
        color: "#fff",
        padding: "0.5rem",
        marginBottom: "0.5rem",
        marginRight: "0.5rem",
      }}
    >
      {children}
    </a>
  );
}
