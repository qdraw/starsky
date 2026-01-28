export function renderModeSelection(
  select: string[],
  handleModeSelect: (mode: "offset" | "timezone") => void,
  handleExit: () => void
) {
  return (
    <>
      <div className="modal content--subheader">Shift Photo Timestamps</div>
      <div className="modal content--text">
        <p>
          You have selected {select.length} image{select.length !== 1 ? "s" : ""}
        </p>
        <p>&nbsp;</p>

        <div className="mode-selection">
          <p>What do you want to do?</p>

          <label className="radio-option">
            <input
              type="radio"
              name="shift-mode"
              value="offset"
              onChange={() => handleModeSelect("offset")}
            />
            <div>
              <strong>Correct incorrect camera timezone</strong>
              <p>The camera clock was set to the wrong timezone.</p>
            </div>
          </label>

          <label className="radio-option">
            <input
              type="radio"
              name="shift-mode"
              value="timezone"
              onChange={() => handleModeSelect("timezone")}
            />
            <div>
              <strong>I moved to a different place</strong>
              <p>I traveled and took photos in another location.</p>
            </div>
          </label>
        </div>

        <div className="modal-buttons">
          <button className="btn btn--default" onClick={handleExit}>
            Cancel
          </button>
        </div>
      </div>
    </>
  );
}
