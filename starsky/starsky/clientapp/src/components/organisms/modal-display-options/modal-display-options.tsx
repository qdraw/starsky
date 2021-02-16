import React, { useEffect } from "react";
import useGlobalSettings from "../../../hooks/use-global-settings";
import useLocation from "../../../hooks/use-location";
import { SortType } from "../../../interfaces/IArchive";
import { Language } from "../../../shared/language";
import { URLPath } from "../../../shared/url-path";
import Modal from "../../atoms/modal/modal";
import Select from "../../atoms/select/select";
import SwitchButton from "../../atoms/switch-button/switch-button";

interface IModalDisplayOptionsProps {
  isOpen: boolean;
  handleExit: Function;
  parentFolder?: string;
}

const ModalDisplayOptions: React.FunctionComponent<IModalDisplayOptionsProps> = (
  props
) => {
  // content
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageDisplayOptions = language.text(
    "Weergave opties",
    "Display options"
  );
  const MessageSwitchButtonCollectionsOn = language.text(
    "Toon raw bestanden",
    "Show raw files"
  );
  const MessageSwitchButtonCollectionsOff = language.text(
    "Verberg raw bestanden",
    "Hide Raw files"
  );
  const MessageSwitchButtonIsSingleItemOn = language.text(
    "Alles inladen",
    "Load everything"
  );
  const MessageSwitchButtonIsSingleItemOff = language.text(
    "Klein inladen",
    "Small loading"
  );
  const MessageSwitchButtonIsSocketOn = language.text(
    "Realtime updates",
    "Realtime updates"
  );
  const MessageSwitchButtonIsSocketOff = language.text(
    "Ververs zelf",
    "Refresh yourself"
  );

  var history = useLocation();

  // the default is true
  const [collections, setCollections] = React.useState(
    new URLPath().StringToIUrl(history.location.search).collections !== false
  );

  /** update when changing values and search */
  useEffect(() => {
    setCollections(
      new URLPath().StringToIUrl(history.location.search).collections !== false
    );
  }, [collections, history.location.search]);

  function toggleCollections() {
    const urlObject = new URLPath().StringToIUrl(history.location.search);
    // set the default option
    if (urlObject.collections === undefined) urlObject.collections = true;
    urlObject.collections = !urlObject.collections;
    setCollections(urlObject.collections);
    history.navigate(new URLPath().IUrlToString(urlObject), { replace: true });
  }

  const [isSingleItem, setIsSingleItem] = React.useState(
    localStorage.getItem("issingleitem") === "false"
  );

  function toggleSlowFiles() {
    setIsSingleItem(!isSingleItem);
    localStorage.setItem("issingleitem", isSingleItem.toString());
  }

  const [isUseSockets, setUseSockets] = React.useState(
    localStorage.getItem("use-sockets") === "false"
  );
  function toggleSockets() {
    setUseSockets(!isUseSockets);
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
      <div className="modal content--subheader">{MessageDisplayOptions}</div>
      <div className="content--text">
        <SwitchButton
          isOn={!collections}
          data-test="toggle-collections"
          isEnabled={true}
          leftLabel={MessageSwitchButtonCollectionsOn}
          onToggle={() => toggleCollections()}
          rightLabel={MessageSwitchButtonCollectionsOff}
        />
      </div>
      <div className="modal content--subheader">
        <SwitchButton
          data-test="toggle-slow-files"
          isOn={isSingleItem}
          isEnabled={true}
          leftLabel={MessageSwitchButtonIsSingleItemOn}
          rightLabel={MessageSwitchButtonIsSingleItemOff}
          onToggle={() => toggleSlowFiles()}
        />
      </div>
      <div className="content--text">
        <SwitchButton
          isOn={isUseSockets}
          data-test="toggle-sockets"
          isEnabled={true}
          leftLabel={MessageSwitchButtonIsSocketOn}
          onToggle={() => toggleSockets()}
          rightLabel={MessageSwitchButtonIsSocketOff}
        />
      </div>
      <div className="modal content--text">
        <Select
          data-test="sort"
          selectOptions={Object.values(SortType) as any}
          callback={toggleSort}
          selected={currentSort()}
        />
      </div>
    </Modal>
  );
};

export default ModalDisplayOptions;
