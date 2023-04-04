import React from "react";
import MenuDetailView from "./menu-detail-view";

export default {
  title: "components/organisms/menu-detail-view"
};

export const Default = () => {
  return (
    <MenuDetailView state={{ fileIndexItem: {} } as any} dispatch={() => {}} />
  );
};

Default.story = {
  name: "default"
};

export const Deleted = () => {
  return (
    <MenuDetailView
      state={{ fileIndexItem: { status: "Deleted" } } as any}
      dispatch={() => {}}
    />
  );
};

Deleted.story = {
  name: "deleted"
};

export const ReadOnly = () => {
  return (
    <MenuDetailView
      state={{ isReadOnly: true, fileIndexItem: {} } as any}
      dispatch={() => {}}
    />
  );
};

ReadOnly.story = {
  name: "read only"
};
