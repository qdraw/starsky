import React from "react";
import MenuSearch from "../../components/organisms/menu-search/menu-search";
import { ArchiveContext } from "../../contexts/archive-context";

const MenuMenuSearchContainer: React.FunctionComponent = () => {
  let { state, dispatch } = React.useContext(ArchiveContext);
  return <MenuSearch state={state} dispatch={dispatch}></MenuSearch>;
};

export default MenuMenuSearchContainer;
