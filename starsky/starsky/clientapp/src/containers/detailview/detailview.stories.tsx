import { MemoryRouter } from "react-router-dom";
import DetailViewContextWrapper from "../../contexts-wrappers/detailview-wrapper";
import { IDetailView } from "../../interfaces/IDetailView";
import { IExifStatus } from "../../interfaces/IExifStatus";
import { ImageFormat } from "../../interfaces/IFileIndexItem";
import { Router } from "../../router-app/router-app";

export default {
  title: "containers/detailview"
};

export const Default = () => {
  const item = {
    subPath: "/test.jpg",
    fileIndexItem: {
      filePath: "/test.jpg",
      imageFormat: ImageFormat.jpg,
      fileHash: "M52QR7MJEU6CHLMWVYJQ4M4AGE",
      status: IExifStatus.Ok
    },
    relativeObjects: {}
  } as IDetailView;

  Router.navigate("?details=true&modal=false");

  return (
    <MemoryRouter>
      <DetailViewContextWrapper {...item} />
    </MemoryRouter>
  );
};

Default.story = {
  name: "default"
};
