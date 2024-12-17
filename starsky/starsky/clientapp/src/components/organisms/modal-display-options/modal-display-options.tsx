import React, { useEffect } from "react";
import useGlobalSettings from "../../../hooks/use-global-settings";
import useLocation from "../../../hooks/use-location/use-location";
import { SortType } from "../../../interfaces/IArchive";
import localization from "../../../localization/localization.json";
import { Language } from "../../../shared/language";
import { URLPath } from "../../../shared/url/url-path";
import Modal from "../../atoms/modal/modal";
import Select from "../../atoms/select/select";
import SwitchButton from "../../atoms/switch-button/switch-button";

interface IModalDisplayOptionsProps {
  isOpen: boolean;
  handleExit: () => void;
}

const ModalDisplayOptions: React.FunctionComponent<IModalDisplayOptionsProps> = (props) => {
  // content
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageDisplayOptions = language.key(localization.MessageDisplayOptions);
  const MessageDisplayOptionsSwitchButtonCollectionsOff = language.key(
    localization.MessageDisplayOptionsSwitchButtonCollectionsOff
  );
  const MessageDisplayOptionsSwitchButtonCollectionsOn = language.key(
    localization.MessageDisplayOptionsSwitchButtonCollectionsOn
  );
  const MessageDisplayOptionsSwitchButtonIsSingleItemOff = language.key(
    localization.MessageDisplayOptionsSwitchButtonIsSingleItemOff
  );
  const MessageDisplayOptionsSwitchButtonIsSingleItemOn = language.key(
    localization.MessageDisplayOptionsSwitchButtonIsSingleItemOn
  );
  const MessageDisplayOptionsSwitchButtonIsSocketOn = language.key(
    localization.MessageDisplayOptionsSwitchButtonIsSocketOn
  );
  const MessageDisplayOptionsSwitchButtonIsSocketOff = language.key(
    localization.MessageDisplayOptionsSwitchButtonIsSocketOff
  );

  const history = useLocation();

  // the default is true
  const [collections, setCollections] = React.useState(
    new URLPath().StringToIUrl(history.location.search).collections !== false
  );

  /** update when changing values and search */
  useEffect(() => {
    setCollections(new URLPath().StringToIUrl(history.location.search).collections !== false);
  }, [collections, history.location.search]);

  function toggleCollections() {
    const urlObject = new URLPath().StringToIUrl(history.location.search);
    // set the default option
    if (urlObject.collections === undefined) urlObject.collections = true;
    urlObject.collections = !urlObject.collections;
    setCollections(urlObject.collections);
    history.navigate(new URLPath().IUrlToString(urlObject), { replace: true });
  }

  const [isAlwaysLoadImage, setIsAlwaysLoadImage] = React.useState(
    localStorage.getItem("alwaysLoadImage") === "true"
  );

  function toggleSlowFiles() {
    localStorage.setItem("alwaysLoadImage", (!isAlwaysLoadImage).toString());
    setIsAlwaysLoadImage(!isAlwaysLoadImage);
  }

  const [isUseSockets, setIsUseSockets] = React.useState(
    localStorage.getItem("use-sockets") === "false"
  );

  function toggleSockets() {
    setIsUseSockets(!isUseSockets);
    if (isUseSockets) {
      localStorage.removeItem("use-sockets");
      return;
    }
    localStorage.setItem("use-sockets", "false");
  }

  function currentSort(): string | undefined {
    const urlObject = new URLPath().StringToIUrl(history.location.search);
    return urlObject.sort?.toString();
  }

  function toggleSort(option: string) {
    const urlObject = new URLPath().StringToIUrl(history.location.search);
    urlObject.sort = SortType[option as keyof typeof SortType];
    console.log(urlObject.sort);

    history.navigate(new URLPath().IUrlToString(urlObject), {
      replace: true
    });
  }

  return (
    <Modal
      id="modal-display-options"
      isOpen={props.isOpen}
      handleExit={() => {
        props.handleExit();
      }}
    >
      <div data-test="modal-display-options">
        <div className="modal content--subheader">{MessageDisplayOptions}</div>
        <div className="content--text">
          <SwitchButton
            isOn={!collections}
            data-test="toggle-collections"
            isEnabled={true}
            leftLabel={MessageDisplayOptionsSwitchButtonCollectionsOn}
            onToggle={() => toggleCollections()}
            rightLabel={MessageDisplayOptionsSwitchButtonCollectionsOff}
          />
        </div>
        <div className="modal content--subheader">
          <SwitchButton
            data-test="toggle-slow-files"
            isOn={isAlwaysLoadImage}
            isEnabled={true}
            leftLabel={MessageDisplayOptionsSwitchButtonIsSingleItemOn}
            rightLabel={MessageDisplayOptionsSwitchButtonIsSingleItemOff}
            onToggle={() => toggleSlowFiles()}
          />
        </div>
        <div className="content--text">
          <SwitchButton
            isOn={isUseSockets}
            data-test="toggle-sockets"
            isEnabled={true}
            leftLabel={MessageDisplayOptionsSwitchButtonIsSocketOn}
            onToggle={() => toggleSockets()}
            rightLabel={MessageDisplayOptionsSwitchButtonIsSocketOff}
          />
        </div>
        <div className="modal content--text">
          <Select
            data-test="sort"
            selectOptions={Object.values(SortType) as string[]}
            callback={toggleSort}
            selected={currentSort()}
          />
        </div>
      </div>
    </Modal>
  );
};

export default ModalDisplayOptions;
