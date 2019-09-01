import React, { memo } from 'react';
import { IMenuProps } from '../interfaces/IMenuProps';

const MenuSearch: React.FunctionComponent<IMenuProps> = memo((props) => {
  var sidebar = false;
  var select = false;
  return (
    <>
      <header className={sidebar ? "header header--main header--select header--edit" : select ? "header header--main header--select" : "header header--main "}>
      </header>
    </>);
});

export default MenuSearch
