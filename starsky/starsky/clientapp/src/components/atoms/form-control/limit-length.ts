export class LimitLength {
  setChildLength: React.Dispatch<React.SetStateAction<number>>;
  onBlur: ((event: React.ChangeEvent<HTMLDivElement>) => void) | undefined;
  maxlength: number;

  /**
   *
   */
  constructor(
    setChildLength: React.Dispatch<React.SetStateAction<number>>,
    onBlur: ((event: React.ChangeEvent<HTMLDivElement>) => void) | undefined,
    maxlength: number
  ) {
    this.setChildLength = setChildLength;
    this.onBlur = onBlur;
    this.maxlength = maxlength;

    // bind this to object
    this.LimitLengthBlur = this.LimitLengthBlur.bind(this);
    this.LimitLengthKey = this.LimitLengthKey.bind(this);
  }

  /**
   * Limit length before sending to onBlurEvent
   * @param element Focus event
   */
  public LimitLengthBlur(element: React.FocusEvent<HTMLDivElement>) {
    if (!element.currentTarget.textContent) {
      this.setChildLength(0);
      if (!this.onBlur) return;
      this.onBlur(element);
      return;
    }

    if (element.currentTarget.textContent.length - 1 >= this.maxlength + 1) {
      this.setChildLength(element.currentTarget.textContent.length - 1);
      return;
    }
    if (!this.onBlur) return;
    this.onBlur(element);
  }

  /**
   * Limit on keydown
   * @param element KeydownEvent
   */
  public LimitLengthKey(element: React.KeyboardEvent<HTMLDivElement>) {
    if (
      (element.metaKey || element.ctrlKey) &&
      (element.key === "a" || element.key === "e")
    ) {
      return;
    }

    if (!element.currentTarget.textContent) {
      this.setChildLength(0);
      return;
    }

    var elementLength = element.currentTarget.textContent.trim().length;

    if (
      elementLength < this.maxlength ||
      window.getSelection()?.type === "Range" ||
      (element.key === "x" && element.ctrlKey) ||
      (element.key === "x" && element.metaKey) ||
      !element.key.match(/^.{0,1}$/)
    )
      return;

    this.setChildLength(elementLength);

    element.preventDefault();
  }
}
