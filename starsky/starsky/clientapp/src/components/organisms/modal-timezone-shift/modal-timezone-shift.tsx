import { useCallback } from "react";
import { ArchiveAction } from "../../../contexts/archive-context";
import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import Modal from "../../atoms/modal/modal";
import {
  useFileRenameState,
  useOffsetState,
  usePreviewState,
  useResetOnClose,
  useShiftMode,
  useTimezoneState
} from "./hooks";
import { renderFileRenameMode } from "./internal/render-file-rename-mode";
import { renderModeSelection } from "./internal/render-mode-selection";
import { renderOffsetMode } from "./internal/render-offset-mode";
import { renderTimezoneMode } from "./internal/render-timezone-mode";

export interface IModalTimezoneShiftProps {
  isOpen: boolean;
  handleExit: () => void;
  select: string[];
  historyLocationSearch: string;
  state: IArchiveProps;
  dispatch: React.Dispatch<ArchiveAction>;
  undoSelection: () => void;
  collections: boolean;
}

const ModalTimezoneShift: React.FunctionComponent<IModalTimezoneShiftProps> = ({
  isOpen,
  handleExit,
  select,
  state,
  dispatch,
  undoSelection,
  historyLocationSearch,
  collections
}) => {
  // Mode and step tracking
  const shiftMode = useShiftMode();
  const { currentStep, handleBack, handleModeSelect, setCurrentStep } = shiftMode;

  // Offset mode state
  const offsetState = useOffsetState();

  // Timezone mode state
  const timezoneState = useTimezoneState();

  // Preview and execution state
  const previewState = usePreviewState();

  // File rename state
  const fileRenameState = useFileRenameState();

  // Handle mode selection with state reset
  const handleModeSelectWrapped = (mode: "offset" | "timezone") => {
    handleModeSelect(mode);
  };

  // Reset all state when modal is closed
  const handleResetAll = useCallback(() => {
    shiftMode.reset();
    offsetState.reset();
    timezoneState.reset();
    previewState.previewReset();
    fileRenameState.reset();
  }, []);

  useResetOnClose(isOpen, handleResetAll);

  return (
    <Modal isOpen={isOpen} handleExit={handleExit} dataTest="modal-timezone-shift">
      <div className="modal content scroll modal-timezone-shift">
        {currentStep === "mode-selection" &&
          renderModeSelection(select, handleModeSelectWrapped, handleExit)}
        {currentStep === "offset" &&
          renderOffsetMode({
            offsetState,
            previewState,
            select,
            state,
            handleBack,
            handleExit,
            dispatch,
            historyLocationSearch,
            undoSelection,
            collections,
            setCurrentStep
          })}
        {currentStep === "timezone" &&
          renderTimezoneMode({
            select,
            state,
            timezoneState,
            previewState,
            handleBack,
            handleExit,
            dispatch,
            historyLocationSearch,
            undoSelection,
            collections,
            setCurrentStep
          })}
        {currentStep === "file-rename-offset" &&
          renderFileRenameMode({
            select,
            state,
            fileRenameState,
            handleBack,
            handleExit,
            dispatch,
            historyLocationSearch,
            undoSelection,
            collections,
            mode: "offset",
            offsetData: {
              year: offsetState.offsetYears,
              month: offsetState.offsetMonths,
              day: offsetState.offsetDays,
              hour: offsetState.offsetHours,
              minute: offsetState.offsetMinutes,
              second: offsetState.offsetSeconds
            }
          })}
        {currentStep === "file-rename-timezone" &&
          renderFileRenameMode({
            select,
            state,
            fileRenameState,
            handleBack,
            handleExit,
            dispatch,
            historyLocationSearch,
            undoSelection,
            collections,
            mode: "timezone",
            timezoneData: {
              recordedTimezoneId: timezoneState.recordedTimezoneId,
              correctTimezoneId: timezoneState.correctTimezoneId
            }
          })}
      </div>
    </Modal>
  );
};

export default ModalTimezoneShift;
