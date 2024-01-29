export function GetMousePosition(event: React.MouseEvent | MouseEvent) {
  const targetElement = event.target as HTMLProgressElement;

  const offsetParent = targetElement.offsetParent as HTMLElement;
  const offsetParentLeft = offsetParent?.offsetLeft;

  return (
    (event.pageX - (targetElement.offsetLeft + offsetParentLeft)) /
    targetElement.offsetWidth
  );
}
