import { MemoryRouter } from "react-router-dom";
import { IFileIndexItem } from "../../../interfaces/IFileIndexItem";
import { Router } from "../../../router-app/router-app";
import ListImageChildItem from "../../atoms/list-image-child-item/list-image-child-item";
import ListImageBox from "./list-image-view-select-container";
const fileIndexItem = {
  fileName: "test.jpg",
  colorClass: 1
} as IFileIndexItem;

export default {
  title: "components/molecules/list-image-view-select-container"
};

export const Default = () => {
  Router.navigate("/");
  return (
    <MemoryRouter>
      <ListImageBox item={fileIndexItem}>
        <ListImageChildItem {...fileIndexItem} />
      </ListImageBox>
    </MemoryRouter>
  );
  // for multiple items on page see: components/molecules/item-list-view
};

Default.story = {
  name: "default"
};

export const Select = () => {
  Router.navigate("/?select=test.jpg");
  return (
    <MemoryRouter>
      <ListImageBox item={fileIndexItem}>
        <ListImageChildItem {...fileIndexItem} />
      </ListImageBox>
    </MemoryRouter>
  );
};

Select.story = {
  name: "select"
};
