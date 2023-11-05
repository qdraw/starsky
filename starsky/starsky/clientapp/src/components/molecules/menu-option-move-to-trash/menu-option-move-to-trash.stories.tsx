import { newIArchive } from "../../../interfaces/IArchive";
import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import { IFileIndexItem } from "../../../interfaces/IFileIndexItem";
import MoreMenu from "../../atoms/more-menu/more-menu";
import MenuOptionMoveToTrash from "./menu-option-move-to-trash";

export default {
  title: "components/molecules/menu-option-move-to-trash"
};

export const Default = () => {
  const test = {
    ...newIArchive(),
    fileIndexItems: [
      {
        parentDirectory: "/",
        fileName: "test.jpg"
      } as IFileIndexItem
    ]
  } as IArchiveProps;
  return (
    <>
      <MoreMenu enableMoreMenu={true} setEnableMoreMenu={() => {}}>
        <MenuOptionMoveToTrash
          setSelect={() => {
            alert("done");
          }}
          select={["test.jpg"]}
          isReadOnly={false}
          state={test}
          dispatch={() => {
            alert("done");
          }}
        />
      </MoreMenu>
    </>
  );
};

Default.story = {
  name: "default"
};
