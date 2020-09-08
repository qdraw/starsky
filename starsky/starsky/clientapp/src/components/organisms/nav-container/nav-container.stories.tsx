import { storiesOf } from "@storybook/react";
import React from "react";
import HamburgerMenuToggle from '../../atoms/hamburger-menu-toggle/hamburger-menu-toggle';
import NavContainer from './nav-container';

storiesOf("components/organisms/nav-container", module)
  .add("default", () => {
    const [hamburgerMenu, setHamburgerMenu] = React.useState(false);
    return <header className="header">
      <div style={{ backgroundColor: "red" }}>
        <HamburgerMenuToggle select={false} hamburgerMenu={hamburgerMenu} setHamburgerMenu={setHamburgerMenu} />
      </div>
      <h2>NavContainer next: {hamburgerMenu}</h2>
      <NavContainer hamburgerMenu={hamburgerMenu}>
        <h1 style={{ color: 'white' }}>Content</h1>
      </NavContainer>
    </header>
  })