import React from 'react';
import { ArchiveContext } from '../contexts/archive-context';
import useGlobalSettings from '../hooks/use-global-settings';
import { CastToInterface } from '../shared/cast-to-interface';
import { Language } from '../shared/language';
import ArchiveSidebarLabelEditAddOverwrite from './archive-sidebar-label-edit-add-overwrite';
import ArchiveSidebarLabelEditSearchReplace from './archive-sidebar-label-edit-search-replace';
import SwitchButton from './switch-button';

const ArchiveSidebarLabelEdit: React.FunctionComponent = () => {

  // Content
  const settings = useGlobalSettings();
  const MessageModifyName = new Language(settings.language).text("Wijzigen", "Modify");
  const MessageSearchAndReplaceName = new Language(settings.language).text("Vervangen", "Replace");

  // Toggle
  const [isReplaceMode, setReplaceMode] = React.useState(false);

  let { state } = React.useContext(ArchiveContext);

  // state without any context
  state = new CastToInterface().UndefinedIArchiveReadonly(state);

  return (
    <div className="content--text">
      <SwitchButton isEnabled={!state.isReadOnly} leftLabel={MessageModifyName}
        rightLabel={MessageSearchAndReplaceName} onToggle={(value) => setReplaceMode(value)} />
      {!isReplaceMode ? <ArchiveSidebarLabelEditAddOverwrite /> : null}
      {isReplaceMode ? <ArchiveSidebarLabelEditSearchReplace /> : null}
    </div>
  )
};
export default ArchiveSidebarLabelEdit
