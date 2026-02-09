export class Keyboard {
  public unBlur(event?: KeyboardEvent): boolean | null {
    if (!event || !this.isInForm(event)) return null;

    const target = event.target as HTMLElement;
    if (!target?.blur) return null;
    target.blur();
    return true;
  }

  public focus(event?: KeyboardEvent): boolean | null {
    if (!event || !this.isInForm(event)) return null;

    const target = event.target as HTMLElement;
    if (!target?.focus) return null;
    target.focus();
    return true;
  }

  public setCaretToEnd(event?: KeyboardEvent): boolean | null {
    if (!event || !this.isInForm(event)) return null;

    // Set caret to end after focusing
    const target = event.target as HTMLElement;
    if (target.isContentEditable) {
      const range = document.createRange();
      range.selectNodeContents(target);
      range.collapse(false); // Move caret to end
      const sel = window.getSelection();
      if (sel) {
        sel.removeAllRanges();
        sel.addRange(range);
        return true;
      }
    }
    return false;
  }

  /**
   * to prevent keystrokes in a form element
   * @param event Keyboard Event
   */
  public isInForm(event?: Event): boolean | null {
    if (!event) return null;

    const target = event.target as HTMLElement;
    if (!target?.className) return null;
    return target.className.includes("form-control") || target.className.includes("modal");
  }

  /**
   * When press an key like I and it should focus on the end of the div contenteditable
   * @param current html element
   * @returns void
   */
  public SetFocusOnEndField(current: HTMLDivElement): void {
    // no content in field
    if (current.childNodes.length === 0) {
      current.focus();
      return;
    }
    if (!current.childNodes[0].textContent) return;
    // Set selection to the end of the element
    const range = document.createRange();
    const sel = globalThis.getSelection();
    range.setStart(current.childNodes[0], current.childNodes[0].textContent.length);
    range.collapse(true);
    if (!sel) return;
    sel.removeAllRanges();
    sel.addRange(range);
  }
}
