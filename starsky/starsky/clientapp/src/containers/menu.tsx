
import React, { memo } from 'react';
import MenuArchive from '../components/menu.archive';
import MenuDetailView from '../components/menu.detailview';
import { IMenuProps } from '../interfaces/IMenuProps';

const Menu: React.FunctionComponent<IMenuProps> = memo((props) => {

  return (
    props.isDetailMenu ? <MenuDetailView {...props} /> : <MenuArchive  {...props} />
  );
});

export default Menu
