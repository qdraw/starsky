import { useContext } from "react";
import MenuSearch from "../../components/organisms/menu-search/menu-search";
import { ArchiveContext } from "../../contexts/archive-context";

const MenuSearchContainer: React.FunctionComponent = () => {
  const { state, dispatch } = useContext(ArchiveContext);
  return <MenuSearch state={state} dispatch={dispatch}></MenuSearch>;
};

export default MenuSearchContainer;
