import { storiesOf } from "@storybook/react";
import React from "react";
import MenuDetailView from "./menu-detail-view";

storiesOf("components/organisms/menu-detail-view", module)
  .add("default", () => {
    return (
      <MenuDetailView
        state={{ fileIndexItem: {} } as any}
        dispatch={() => {}}
      />
    );
  })
  .add("deleted", () => {
    return (
      <MenuDetailView
        state={{ fileIndexItem: { status: "Deleted" } } as any}
        dispatch={() => {}}
      />
    );
  })
  .add("read only", () => {
    return (
      <MenuDetailView
        state={{ isReadOnly: true, fileIndexItem: {} } as any}
        dispatch={() => {}}
      />
    );
  });
