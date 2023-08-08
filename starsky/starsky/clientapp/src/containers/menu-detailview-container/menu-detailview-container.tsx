import { useContext } from "react";
import MenuDetailView from "../../components/organisms/menu-detail-view/menu-detail-view";
import { DetailViewContext } from "../../contexts/detailview-context";
import { IDetailView, PageType } from "../../interfaces/IDetailView";

const MenuDetailViewContainer: React.FunctionComponent = () => {
  // eslint-disable-next-line prefer-const
  let { state, dispatch } = useContext(DetailViewContext);

  // fallback state
  if (!state) {
    state = {
      pageType: PageType.Loading,
      isReadOnly: true,
      fileIndexItem: {
        parentDirectory: "/",
        fileName: "",
        filePath: "/",
        lastEdited: new Date(1970, 1, 1).toISOString(),
      },
    } as IDetailView;
  }

  return <MenuDetailView state={state} dispatch={dispatch}></MenuDetailView>;
};

export default MenuDetailViewContainer;
