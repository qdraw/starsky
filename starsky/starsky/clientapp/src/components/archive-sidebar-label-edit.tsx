import React from 'react';
import { ArchiveContext } from '../contexts/archive-context';
import { newIArchive } from '../interfaces/IArchive';
import ArchiveSidebarLabelEditAddOverwrite from './archive-sidebar-label-edit-add-overwrite';
import ArchiveSidebarLabelEditSearchReplace from './archive-sidebar-label-edit-search-replace';
import SwitchButton from './switch-button';

const ArchiveSidebarLabelEdit: React.FunctionComponent = () => {
  const [isReplaceMode, setReplaceMode] = React.useState(false);

  {/* feature toggle */ }
  const [isFeatureToggle, setFeatureToggle] = React.useState(localStorage.getItem('beta_replace') !== null);

  let { state } = React.useContext(ArchiveContext);

  // state without any context
  if (state === undefined) {
    state = newIArchive();
    state.isReadOnly = true;
  }

  return (
    <div className="content--text">
      <SwitchButton isEnabled={!state.isReadOnly} leftLabel="Wijzigen" rightLabel="Vervangen" onToggle={(value) => setReplaceMode(value)}></SwitchButton>
      {!isReplaceMode ? <ArchiveSidebarLabelEditAddOverwrite /> : null}
      {/* feature toggle */}
      {isReplaceMode && !isFeatureToggle ?
        <h4><button className='btn btn--default' onClick={() => {
          localStorage.setItem('beta_replace', 'true');
          setFeatureToggle(true);
        }}>Test Functionaliteit aanzetten</button></h4>
        : null}
      {isReplaceMode && isFeatureToggle ? <ArchiveSidebarLabelEditSearchReplace /> : null}
    </div>
  )

};
export default ArchiveSidebarLabelEdit