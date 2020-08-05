import React, { useEffect } from 'react';
import useGlobalSettings from '../../../hooks/use-global-settings';
import useLocation from '../../../hooks/use-location';
import { Language } from '../../../shared/language';
import { URLPath } from '../../../shared/url-path';
import Modal from '../../atoms/modal/modal';
import SwitchButton from '../../atoms/switch-button/switch-button';

interface IModalDisplayOptionsProps {
  isOpen: boolean;
  handleExit: Function;
  parentFolder?: string;
}

const ModalDisplayOptions: React.FunctionComponent<IModalDisplayOptionsProps> = (props) => {

  // content
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageDisplayOptions = language.text("Weergave opties", "Display options");
  const MessageSwitchButtonCollectionsOn = language.text("Collecties aan", "Collections on");
  const MessageSwitchButtonCollectionsOff = language.text("Per bestand (uit)", "Per file (off)");
  const MessageSwitchButtonIsSingleItemOn = language.text("Alles inladen", "Load everything");
  const MessageSwitchButtonIsSingleItemOff = language.text("Klein inladen", "Small loading");

  var history = useLocation();

  // the default is true
  const [collections, setCollections] = React.useState(new URLPath().StringToIUrl(history.location.search).collections !== false);

  /** update when changing values and search */
  useEffect(() => {
    setCollections(new URLPath().StringToIUrl(history.location.search).collections !== false);
  }, [collections, history.location.search])

  function toggleCollections() {
    var urlObject = new URLPath().StringToIUrl(history.location.search);
    // set the default option
    if (urlObject.collections === undefined) urlObject.collections = true;
    urlObject.collections = !urlObject.collections;
    setCollections(urlObject.collections);
    history.navigate(new URLPath().IUrlToString(urlObject), { replace: true })
  }

  var singleItem = localStorage.getItem("issingleitem");
  const [isSingleItem, setIsSingleItem] = React.useState(singleItem === "false");

  function toggleSlowFiles() {
    setIsSingleItem(!isSingleItem);
    localStorage.setItem("issingleitem", isSingleItem.toString())
  }

  return (<Modal
    id="modal-display-options"
    isOpen={props.isOpen}
    handleExit={() => {
      props.handleExit()
    }}>
    <div className="modal content--subheader">{MessageDisplayOptions}</div>
    <div className="content--text">
      <SwitchButton isOn={!collections} data-test="toggle-collections" isEnabled={true}
        leftLabel={MessageSwitchButtonCollectionsOn}
        onToggle={() => toggleCollections()}
        rightLabel={MessageSwitchButtonCollectionsOff} />
    </div>
    <div className="modal content--subheader">
      <SwitchButton data-test="toggle-slow-files" isOn={isSingleItem} isEnabled={true}
        leftLabel={MessageSwitchButtonIsSingleItemOn}
        rightLabel={MessageSwitchButtonIsSingleItemOff}
        onToggle={() => toggleSlowFiles()} />
    </div>
    <div className="modal content--text">
    </div>
  </Modal>)
}

export default ModalDisplayOptions;
