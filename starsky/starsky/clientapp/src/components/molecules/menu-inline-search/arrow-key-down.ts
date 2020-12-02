export default function ArrowKeyDown(
  event: React.KeyboardEvent<HTMLInputElement>,
  keyDownIndex: number,
  setKeyDownIndex: React.Dispatch<React.SetStateAction<number>>,
  inputFormControlReferenceCurrent: HTMLInputElement | null,
  suggest: string[]
): void {
  if (event.key !== "ArrowDown" && event.key !== "ArrowUp") {
    setKeyDownIndex(-1);
    return;
  }
  if (inputFormControlReferenceCurrent === null || suggest.length === 0) return;

  if (event.key === "ArrowUp" && keyDownIndex <= 0) return;

  var value = event.key === "ArrowDown" ? keyDownIndex + 1 : keyDownIndex - 1;

  if (value < suggest.length) {
    inputFormControlReferenceCurrent.value = suggest[value];
    setKeyDownIndex(value);
  }
}
