import React, { useEffect } from 'react';
import useFetch from '../../../hooks/use-fetch';
import useGlobalSettings from '../../../hooks/use-global-settings';
import useInterval from '../../../hooks/use-interval';
import FetchGet from '../../../shared/fetch-get';
import FetchPost from '../../../shared/fetch-post';
import { Language } from '../../../shared/language';
import { URLPath } from '../../../shared/url-path';
import { UrlQuery } from '../../../shared/url-query';
import FormControl from '../../atoms/form-control/form-control';
import Modal from '../../atoms/modal/modal';
import Select from '../../atoms/select/select';

interface IModalPublishProps {
  isOpen: boolean;
  select: Array<string> | undefined;
  handleExit: Function;
}

enum ProcessingState {
  default,
  server,
  ready,
  fail
}

/**
 * ModalPublish
 * @param props input stats
 */
const ModalPublish: React.FunctionComponent<IModalPublishProps> = (props) => {

  // content
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessagePublishSelection = language.text("Publiceer selectie", "Publish selection");
  const MessageGenericExportFail = language.text("Er is iets misgegaan met exporteren",
    "Something went wrong with exporting");
  const MessageExportReady = language.text("Het bestand {createZipKey} is klaar met exporteren.",
    "The file {createZipKey} has finished exporting.");
  const MessageDownloadAsZipArchive = language.text("Download als zip-archief", "Download as a zip archive");
  const MessageOneMomentPlease = language.text("Een moment geduld alstublieft", "One moment please");

  const [isProcessing, setProcessing] = React.useState(ProcessingState.default);
  const [createZipKey, setCreateZipKey] = React.useState("");
  const [itemName, setItemName] = React.useState("");
  const [publishProfileName, setPublishProfileName] = React.useState("");

  async function postZip() {
    console.log(props.select);

    if (!props.select) {
      setProcessing(ProcessingState.fail);
      return;
    }

    var bodyParams = new URLSearchParams();
    bodyParams.set("f", new URLPath().ArrayToCommaSeperatedString(props.select));
    bodyParams.set("itemName", itemName);
    bodyParams.set("publishProfileName", publishProfileName);

    setProcessing(ProcessingState.server);

    var zipKeyResult = await FetchPost(new UrlQuery().UrlPublishCreate(), bodyParams.toString());

    if (zipKeyResult.statusCode !== 200 || !zipKeyResult.data) {
      setProcessing(ProcessingState.fail);
      return;
    }
    setCreateZipKey(zipKeyResult.data);
  }

  var allPublishProfiles = useFetch(new UrlQuery().UrlPublish(), 'get').data;
  useEffect(() => {
    // set the default option
    if (!allPublishProfiles) return;
    setPublishProfileName(allPublishProfiles[0])
  }, [allPublishProfiles])

  useInterval(async () => {
    if (isProcessing !== ProcessingState.server) return;
    if (!createZipKey) return;
    var result = await FetchGet(new UrlQuery().UrlExportZipApi(createZipKey, true));
    if (result.statusCode === 200) {
      setProcessing(ProcessingState.ready);
      return;
    }
    else if (result.statusCode === 206) {
      return; // not ready jet
    }
    setProcessing(ProcessingState.fail);
  }, 1500);


  function updateItemName(event: React.ChangeEvent<HTMLDivElement>) {
    setItemName(event.target.textContent ? event.target.textContent.trim() : "")
  }

  return (<Modal
    id="detailview-publish-modal"
    isOpen={props.isOpen}
    handleExit={() => {
      props.handleExit()
    }}>

    <div className="modal content--subheader">{isProcessing !== ProcessingState.server
      ? MessagePublishSelection : MessageOneMomentPlease}</div>
    <div className="modal content--text publish">

      {/* when selecting one file */}
      {isProcessing === ProcessingState.default && props.select ? <>
        <h4>Item Name</h4>
        <FormControl contentEditable={true} onInput={updateItemName} name="item-name"></FormControl>
        <h4>Instelling </h4>
        <Select selectOptions={allPublishProfiles} callback={setPublishProfileName}></Select>
        <button disabled={!itemName} onClick={postZip} className="btn btn--default" data-test="publish">{MessagePublishSelection}</button>

      </> : null}

      {isProcessing === ProcessingState.server ? <>
        <div className="preloader preloader--inside"></div>
      </> : null}

      {isProcessing === ProcessingState.fail ? MessageGenericExportFail : null}

      {isProcessing === ProcessingState.ready ? <>
        {language.token(MessageExportReady, ["{createZipKey}"], [createZipKey])}
        <a className="btn btn--default" href={new UrlQuery().UrlExportZipApi(createZipKey, false)}
          download rel="noopener noreferrer" target="_blank">
          {MessageDownloadAsZipArchive}
        </a>
      </> : null}
    </div>
  </Modal>)
};

export default ModalPublish