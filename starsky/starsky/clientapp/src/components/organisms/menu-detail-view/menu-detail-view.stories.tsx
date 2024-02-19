import { MemoryRouter } from "react-router-dom";
import MenuDetailView from "./menu-detail-view";

export default {
  title: "components/organisms/menu-detail-view"
};

export const Default = () => {
  return (
    <MemoryRouter>
      <MenuDetailView state={{ fileIndexItem: {} } as any} dispatch={() => {}} />
    </MemoryRouter>
  );
};

Default.storyName = "default";

export const Deleted = () => {
  return (
    <MemoryRouter>
      <MenuDetailView state={{ fileIndexItem: { status: "Deleted" } } as any} dispatch={() => {}} />
    </MemoryRouter>
  );
};

Deleted.storyName = "deleted";

export const ReadOnly = () => {
  return (
    <MemoryRouter>
      <MenuDetailView state={{ isReadOnly: true, fileIndexItem: {} } as any} dispatch={() => {}} />
    </MemoryRouter>
  );
};

ReadOnly.storyName = "read only";
