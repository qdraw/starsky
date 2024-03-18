
export default function Button({ children, href, color }) {
  return (
    <a
      href={href}
      style={{
        backgroundColor: color,
        borderRadius: "2px",
        display :"inline-block",
        color: "#fff",
        padding: "0.5rem",
        marginBottom: "0.4rem",
        marginRight: "0.4rem",
      }}
    >
      {children}
    </a>
  );
}
