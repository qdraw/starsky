import useGlobalSettings from "../../../hooks/use-global-settings";
import { IFileIndexItem } from "../../../interfaces/IFileIndexItem";
import localization from "../../../localization/localization.json";
import { Language } from "../../../shared/language";
import Modal from "../../atoms/modal/modal";
import ItemTextListView from "../../molecules/item-text-list-view/item-text-list-view";

interface IModalDropAreaFilesAddedProps {
  isOpen: boolean;
  handleExit: () => void;
  uploadFilesList: IFileIndexItem[];
}

const ModalDropAreaFilesAdded: React.FunctionComponent<IModalDropAreaFilesAddedProps> = (props) => {
  const settings = useGlobalSettings();
  const MessageFilesAdded = new Language(settings.language).key(localization.MessageFilesAdded);

  return (
    <Modal
      id="modal-drop-area-files-added"
      isOpen={props.isOpen}
      dataTest="modal-drop-area-files-added"
      handleExit={() => {
        props.handleExit();
      }}
    >
      <div className="modal content--subheader">{MessageFilesAdded}</div>
      <div className="modal modal-move content content--text" data-test="upload-files">
        <ItemTextListView fileIndexItems={props.uploadFilesList} />
      </div>
    </Modal>
  );
};

export default ModalDropAreaFilesAdded;
