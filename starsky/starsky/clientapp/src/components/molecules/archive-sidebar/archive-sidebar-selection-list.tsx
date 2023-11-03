import React, { memo, useEffect } from "react";
import useGlobalSettings from "../../../hooks/use-global-settings";
import useLocation from "../../../hooks/use-location/use-location";
import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import { IFileIndexItem } from "../../../interfaces/IFileIndexItem";
import { Language } from "../../../shared/language";
import { Select } from "../../../shared/select";
import { URLPath } from "../../../shared/url-path";

interface IDetailViewSidebarSelectionListProps {
  fileIndexItems: Array<IFileIndexItem>;
}

const ArchiveSidebarSelectionList: React.FunctionComponent<IDetailViewSidebarSelectionListProps> =
  // eslint-disable-next-line react/display-name
  memo((props) => {
    // content
    const settings = useGlobalSettings();
    const language = new Language(settings.language);
    const MessageNoneSelected = language.text(
      "Niets geselecteerd",
      "Nothing selected"
    );
    const MessageAllName = language.text("Alles", "All");

    const history = useLocation();
    const [select, setSelect] = React.useState(
      new URLPath().StringToIUrl(history.location.search).select
    );
    useEffect(() => {
      setSelect(new URLPath().StringToIUrl(history.location.search).select);
    }, [history.location.search]);

    const allSelection = () =>
      new Select(
        select,
        setSelect,
        props as IArchiveProps,
        history
      ).allSelection();
    const undoSelection = () =>
      new Select(
        select,
        setSelect,
        props as IArchiveProps,
        history
      ).undoSelection();
    const toggleSelection = (item: string) =>
      new Select(
        select,
        setSelect,
        props as IArchiveProps,
        history
      ).toggleSelection(item);

    // noinspection HtmlUnknownAttribute
    return (
      <div className="sidebar-selection">
        <div className="content--header content--subheader">
          {!select || select.length !== props.fileIndexItems.length ? (
            <button
              data-test="select-all"
              className="btn btn--default"
              onClick={() => allSelection()}
            >
              {MessageAllName}
            </button>
          ) : (
            ""
          )}
          {!select || select.length !== 0 ? (
            <button
              className="btn btn--default"
              onClick={() => undoSelection()}
            >
              Undo
            </button>
          ) : (
            ""
          )}
        </div>
        <ul data-test="sidebar-selection-list">
          {!select || select.length === 0 ? (
            <li className="warning-box" data-test="sidebar-selection-none">
              {MessageNoneSelected}
            </li>
          ) : (
            ""
          )}
          {select
            ? select.map(
                (
                  item // item is filename
                ) => (
                  <li key={item}>
                    <button
                      onKeyDown={(event) => {
                        event.key === "Enter" && toggleSelection(item);
                      }}
                      onClick={() => toggleSelection(item)}
                      className="close"
                      title={item}
                    />
                    <span>{item}</span>
                  </li>
                )
              )
            : ""}
        </ul>
      </div>
    );
  });
export default ArchiveSidebarSelectionList;
