import { storiesOf } from "@storybook/react";
import DetailViewContextWrapper from "../../contexts-wrappers/detailview-wrapper";
import useLocation from "../../hooks/use-location";
import { IDetailView } from "../../interfaces/IDetailView";
import { IExifStatus } from "../../interfaces/IExifStatus";
import { ImageFormat } from "../../interfaces/IFileIndexItem";

storiesOf("containers/detailview", module).add("default", () => {
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

  const history = useLocation();

  history.location.search += "&details=true&modal=geo";

  return <DetailViewContextWrapper {...item} />;
});