import { MemoryRouter } from "react-router-dom";
import { IArchiveProps } from "../../interfaces/IArchiveProps";
import { Router } from "../../router-app/router-app";
import Archive from "./archive";

export default {
  title: "containers/archive"
};

export const Default = () => {
  const archive = {
    colorClassUsage: [1],
    colorClassActiveList: [1],
    fileIndexItems: [{ fileName: "test", filePath: "/test.jpg", colorClass: 1 }]
  } as IArchiveProps;

  Router.navigate("?details=true&modal=false");

  return (
    <MemoryRouter>
      <Archive {...archive} />
    </MemoryRouter>
  );
};

Default.story = {
  name: "default"
};
