export class Keyboard {
  /**
   * to prevent keystrokes in a form element
   * @param event Keyboard Event
   */
  public isInForm(event?: Event): boolean | null {
    if (!event) return null;

    const target = event.target as HTMLElement;
    if (!target || !target.className) return null;
    return target.className.indexOf("form-control") !== -1 || target.className.indexOf("modal") !== -1;
  }

  public SetFocusOnEndField(current: HTMLDivElement): void {
    // no content in field
    if (current.childNodes.length === 0) {
      current.focus();
      return;
    }
    if (!current.childNodes[0].textContent) return;
    // Set selection to the end of the element
    var range = document.createRange();
    var sel = window.getSelection();
    range.setStart(current.childNodes[0], current.childNodes[0].textContent.length);
    range.collapse(true);
    if (!sel) return;
    sel.removeAllRanges();
    sel.addRange(range);
  }
}