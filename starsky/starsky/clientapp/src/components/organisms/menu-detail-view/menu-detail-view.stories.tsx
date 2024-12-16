import { MemoryRouter } from "react-router-dom";
import { IDetailView } from "../../../interfaces/IDetailView";
import MenuDetailView from "./menu-detail-view";

export default {
  title: "components/organisms/menu-detail-view"
};

export const Default = () => {
  return (
    <MemoryRouter>
      <MenuDetailView state={{ fileIndexItem: {} } as IDetailView} dispatch={() => {}} />
    </MemoryRouter>
  );
};

Default.storyName = "default";

export const Deleted = () => {
  return (
    <MemoryRouter>
      <MenuDetailView
        state={{ fileIndexItem: { status: "Deleted" } } as IDetailView}
        dispatch={() => {}}
      />
    </MemoryRouter>
  );
};

Deleted.storyName = "deleted";

export const ReadOnly = () => {
  return (
    <MemoryRouter>
      <MenuDetailView
        state={{ isReadOnly: true, fileIndexItem: {} } as IDetailView}
        dispatch={() => {}}
      />
    </MemoryRouter>
  );
};

ReadOnly.storyName = "read only";
