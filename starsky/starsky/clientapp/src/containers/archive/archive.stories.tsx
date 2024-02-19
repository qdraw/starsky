import { MemoryRouter } from "react-router-dom";
import { IArchiveProps } from "../../interfaces/IArchiveProps";
import { Router } from "../../router-app/router-app";
import Archive from "./archive";

export default {
  title: "containers/archive"
};

export const Default = () => {
  const item = {} as IArchiveProps;

  Router.navigate("?details=true&modal=false");

  return (
    <MemoryRouter>
      <Archive {...item} />
    </MemoryRouter>
  );
};

Default.story = {
  name: "default"
};
