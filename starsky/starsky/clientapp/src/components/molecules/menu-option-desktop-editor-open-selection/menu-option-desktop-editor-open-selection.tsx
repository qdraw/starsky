import React, { memo, useState } from "react";
import useFetch from "../../../hooks/use-fetch";
import useGlobalSettings from "../../../hooks/use-global-settings";
import useHotKeys from "../../../hooks/use-keyboard/use-hotkeys";
import useLocation from "../../../hooks/use-location/use-location";
import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import { PageType } from "../../../interfaces/IDetailView";
import { IEnvFeatures } from "../../../interfaces/IEnvFeatures";
import localization from "../../../localization/localization.json";
import FetchPost from "../../../shared/fetch/fetch-post";
import { Language } from "../../../shared/language";
import { URLPath } from "../../../shared/url-path";
import { UrlQuery } from "../../../shared/url-query";
import MenuOption from "../../atoms/menu-option/menu-option";
import Notification, { NotificationType } from "../../atoms/notification/notification";
import ModalDesktopEditorOpenSelectionConfirmation from "../../organisms/modal-desktop-editor-open-selection-confirmation/modal-desktop-editor-open-selection-confirmation";

interface IMenuOptionDesktopEditorOpenProps {
  state: IArchiveProps;
  select: string[];
  isReadOnly: boolean;
  setEnableMoreMenu?: React.Dispatch<React.SetStateAction<boolean>>;
}

async function openDesktop(
  select: string[],
  collections: boolean,
  state: IArchiveProps,
  setIsError: React.Dispatch<React.SetStateAction<string>>,
  messageDesktopEditorUnableToOpen: string
) {
  const toDesktopOpenList = new URLPath().MergeSelectFileIndexItem(select, state.fileIndexItems);
  if (!toDesktopOpenList) return;
  const selectParams = new URLPath().ArrayToCommaSeparatedStringOneParent(toDesktopOpenList, "");
  const urlOpen = new UrlQuery().UrlApiDesktopEditorOpen();

  const bodyParams = new URLSearchParams();
  bodyParams.append("f", selectParams);
  bodyParams.append("collections", collections.toString());

  const openDesktopResult = await FetchPost(urlOpen, bodyParams.toString());
  if (openDesktopResult.statusCode >= 300) {
    setIsError(messageDesktopEditorUnableToOpen);
  }
}

export async function StartMenuOptionDesktopEditorOpenSelection(
  select: string[],
  collections: boolean,
  state: IArchiveProps,
  setIsError: React.Dispatch<React.SetStateAction<string>>,
  messageDesktopEditorUnableToOpen: string,
  setModalConfirmationOpenFiles: (value: React.SetStateAction<boolean>) => void
) {
  const toDesktopOpenList = new URLPath().MergeSelectFileIndexItem(select, state.fileIndexItems);
  if (!toDesktopOpenList) return;
  const selectParams = new URLPath().ArrayToCommaSeparatedStringOneParent(toDesktopOpenList, "");
  const urlCheck = new UrlQuery().UrlApiDesktopEditorOpenAmountConfirmationChecker();

  const bodyParams = new URLSearchParams();
  bodyParams.append("f", selectParams);

  const openWithoutConformationResult = (await FetchPost(urlCheck, bodyParams.toString())).data;
  if (openWithoutConformationResult === false) {
    setModalConfirmationOpenFiles(true);
    return;
  }
  await openDesktop(select, collections, state, setIsError, messageDesktopEditorUnableToOpen);
}

const MenuOptionDesktopEditorOpenSelection: React.FunctionComponent<IMenuOptionDesktopEditorOpenProps> =
  memo(({ state, select, isReadOnly }) => {
    const featuresResult = useFetch(new UrlQuery().UrlApiFeaturesAppSettings(), "get");
    const dataFeatures = featuresResult?.data as IEnvFeatures | undefined;
    const history = useLocation();

    // Get language keys
    const settings = useGlobalSettings();
    const language = new Language(settings.language);
    const MessageDesktopEditorUnableToOpen = language.key(
      localization.MessageDesktopEditorUnableToOpen
    );

    // for showing a notification
    const [isError, setIsError] = useState("");

    const [modalConfirmationOpenFiles, setModalConfirmationOpenFiles] = useState(false);

    const isCollections =
      state.pageType !== PageType.Search
        ? new URLPath().StringToIUrl(history.location.search).collections !== false
        : false;

    /**
     * Open editor with keys
     */
    useHotKeys({ key: "e", ctrlKeyOrMetaKey: true }, () => {
      console.log("hi");

      StartMenuOptionDesktopEditorOpenSelection(
        select,
        isCollections,
        state,
        setIsError,
        MessageDesktopEditorUnableToOpen,
        setModalConfirmationOpenFiles
      ).then(() => {
        // do nothing
      });
    });

    return (
      <>
        {/* Modal move folder to trash */}
        {modalConfirmationOpenFiles ? (
          <ModalDesktopEditorOpenSelectionConfirmation
            handleExit={() => {
              setModalConfirmationOpenFiles(!modalConfirmationOpenFiles);
            }}
            select={select}
            state={state}
            isCollections={isCollections}
            setIsLoading={() => {}}
            isOpen={modalConfirmationOpenFiles}
          />
        ) : null}

        {isError !== "" ? (
          <Notification callback={() => setIsError("")} type={NotificationType.danger}>
            {isError}
          </Notification>
        ) : null}

        {select.length >= 1 && dataFeatures?.openEditorEnabled === true ? (
          <MenuOption
            isReadOnly={isReadOnly}
            testName={"menu-option-desktop-editor-open"}
            onClickKeydown={() =>
              StartMenuOptionDesktopEditorOpenSelection(
                select,
                isCollections,
                state,
                setIsError,
                MessageDesktopEditorUnableToOpen,
                setModalConfirmationOpenFiles
              )
            }
            localization={
              select.length === 1
                ? localization.MessageDesktopEditorOpenSingleFile
                : localization.MessageDesktopEditorOpenMultipleFiles
            }
          />
        ) : null}
      </>
    );
  });

export default MenuOptionDesktopEditorOpenSelection;
