export class Keyboard {
  /**
   * to prevent keystrokes in a form element
   * @param event Keyboard Event
   */
  public isInForm(event?: Event): boolean {
    // to prevent keystrokes in a form element
    if (event) {
      const target = event.target as HTMLElement;

      if (target.matches(".form-control")) {
        return true;
      }
    }
    return false;
  }
}