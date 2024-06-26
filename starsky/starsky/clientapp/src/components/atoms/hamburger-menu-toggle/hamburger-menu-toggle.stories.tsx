import { useState } from "react";
import HamburgerMenuToggle from "./hamburger-menu-toggle";

export default {
  title: "components/atoms/hamburger-menu-toggle"
};

export const Default = () => {
  const [hamburgerMenu, setHamburgerMenu] = useState(false);
  return (
    <div style={{ backgroundColor: "red" }}>
      <HamburgerMenuToggle
        select={false}
        hamburgerMenu={hamburgerMenu}
        setHamburgerMenu={setHamburgerMenu}
      />
    </div>
  );
};

Default.storyName = "default";
