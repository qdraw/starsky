import React from 'react';
import useGlobalSettings from '../../../hooks/use-global-settings';
import useInterval from '../../../hooks/use-interval';
import FetchGet from '../../../shared/fetch-get';
import FetchPost from '../../../shared/fetch-post';
import { Language } from '../../../shared/language';
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
 * ModalExport
 * @param props input stats
 */
const ModalPublish: React.FunctionComponent<IModalPublishProps> = (props) => {

  // content
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessagePublishSelection = language.text("Publiceer selectie", "Publish selection");
  const MessageOrginalFile = language.text("Origineel bestand", "Original file");
  const MessageThumbnailFile = "Thumbnail";
  const MessageGenericExportFail = language.text("Er is iets misgegaan met exporteren", "Something went wrong with exporting");
  const MessageExportReady = language.text("Het bestand {createZipKey} is klaar met exporteren.", "The file {createZipKey} has finished exporting.");
  const MessageDownloadAsZipArchive = language.text("Download als zip-archief", "Download as a zip archive");
  const MessageOneMomentPlease = language.text("Een moment geduld alstublieft", "One moment please");

  const [isProcessing, setProcessing] = React.useState(ProcessingState.default);
  const [createZipKey, setCreateZipKey] = React.useState("");

  async function postZip(isThumbnail: boolean) {
    if (!props.select) {
      setProcessing(ProcessingState.fail);
      return;
    }
    /*
    f	/__starsky/0001-readonly/4.jpg;/__starsky/0001-readonly/3.jpg
    json	true
    thumbnail	false
    collections	true
    */
    var bodyParams = new URLSearchParams();

    var selectString = "";
    for (let index = 0; index < props.select.length; index++) {
      const element = props.select[index];
      if (index === 0) {
        selectString = element;
        continue;
      }
      selectString += ";" + element;
    }

    bodyParams.set("f", selectString);
    bodyParams.set("json", "true");
    bodyParams.set("thumbnail", isThumbnail.toString());
    setProcessing(ProcessingState.server);

    var zipKeyResult = await FetchPost(new UrlQuery().UrlExportPostZipApi(), bodyParams.toString());

    if (zipKeyResult.statusCode !== 200 || !zipKeyResult.data) {
      setProcessing(ProcessingState.fail);
      return;
    }
    setCreateZipKey(zipKeyResult.data);
  }

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


  // const [singleFileThumbnailStatus, setSingleFileThumbnailStatus] = React.useState(true);

  // function getFirstSelectResult(): string {
  //   if (!props.select || props.select.length !== 1) return "";
  //   return props.select[0];
  // }

  // var singleFileThumbResult = useFetch(new UrlQuery().UrlAllowedTypesThumb(getFirstSelectResult()), "get");
  // useEffect(() => {
  //   setSingleFileThumbnailStatus(singleFileThumbResult.data !== false);
  // }, [singleFileThumbResult.data])

  function updateName() {

  }

  function updateSetting() {

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
        <h4><b>Item Name</b></h4>
        <FormControl contentEditable={true} onBlur={updateName} name="item-name" ></FormControl>
        <h4><b>Instelling</b></h4>
        <Select selectOptions={["_default"]} callback={updateSetting}></Select>
        <button onClick={() => {
          postZip(false)
        }} className="btn btn--default" data-test="orginal">{MessageOrginalFile}</button>

      </> : null}

      {isProcessing === ProcessingState.default && props.select && props.select.length >= 2 ? <>
        <button onClick={() => {
          postZip(false)
        }} className="btn btn--info" data-test="orginal">{MessageOrginalFile}</button>

        <button onClick={() => {
          postZip(true)
        }} className="btn btn--default" data-test="thumbnail">{MessageThumbnailFile}</button>
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