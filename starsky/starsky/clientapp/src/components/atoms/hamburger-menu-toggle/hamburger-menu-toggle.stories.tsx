import { storiesOf } from "@storybook/react";
import React from "react";
import HamburgerMenuToggle from './hamburger-menu-toggle';

storiesOf("components/atoms/hamburger-menu-toggle", module)
  .add("default", () => {
    const [hamburgerMenu, setHamburgerMenu] = React.useState(false);
    return <div style={{ backgroundColor: "red" }}>
      <HamburgerMenuToggle select={false} hamburgerMenu={hamburgerMenu} setHamburgerMenu={setHamburgerMenu} />
    </div>
  })