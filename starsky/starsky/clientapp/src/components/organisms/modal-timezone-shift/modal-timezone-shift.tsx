import { useCallback } from "react";
import { ArchiveAction } from "../../../contexts/archive-context";
import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import Modal from "../../atoms/modal/modal";
import {
  useLoadTimezones,
  useOffsetState,
  usePreviewState,
  useResetOnClose,
  useShiftMode,
  useTimezoneState
} from "./hooks";
import { loadTimezones } from "./internal/load-timezones";
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
}

const ModalTimezoneShift: React.FunctionComponent<IModalTimezoneShiftProps> = ({
  isOpen,
  handleExit,
  select,
  state
}) => {
  // Mode and step tracking
  const shiftMode = useShiftMode();
  const { currentStep, setCurrentStep, handleBack: handleBackMode, handleModeSelect } = shiftMode;

  // Offset mode state
  const offsetState = useOffsetState();

  // Timezone mode state
  const timezoneState = useTimezoneState();
  const { timezones } = timezoneState;

  // Preview and execution state
  const previewState = usePreviewState();
  const { setPreview, setError } = previewState;

  // Load timezones when entering timezone mode
  const handleLoadTimezones = useCallback(() => {
    loadTimezones();
  }, []);

  useLoadTimezones(currentStep, timezones.length === 0, handleLoadTimezones);

  // Handle back with state reset
  const handleBack = () => {
    handleBackMode();
    setPreview([]);
    setError(null);
  };

  // Handle mode selection with state reset
  const handleModeSelectWrapped = (mode: "offset" | "timezone") => {
    handleModeSelect(mode);
    setPreview([]);
    setError(null);
  };

  // Reset all state when modal is closed
  const handleResetAll = useCallback(() => {
    shiftMode.reset();
    offsetState.reset();
    timezoneState.reset();
    previewState.reset();
  }, []);

  useResetOnClose(isOpen, handleResetAll);

  return (
    <Modal isOpen={isOpen} handleExit={handleExit} dataTest="modal-timezone-shift">
      <div className="modal content scroll modal-timezone-shift">
        {currentStep === "mode-selection" &&
          renderModeSelection(select, handleModeSelectWrapped, handleExit)}
        {currentStep === "offset" &&
          renderOffsetMode(offsetState, previewState, select, state, handleBack)}
        {currentStep === "timezone" &&
          renderTimezoneMode(select, timezoneState, previewState, handleBack)}
      </div>
    </Modal>
  );
};

export default ModalTimezoneShift;
