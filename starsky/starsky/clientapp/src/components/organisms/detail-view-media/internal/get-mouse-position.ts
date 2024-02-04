export function GetMousePosition(event: React.MouseEvent | MouseEvent) {
  const targetElement = event.target as HTMLProgressElement;

  const offsetParent = targetElement?.offsetParent as HTMLElement;
  const offsetParentLeft = offsetParent?.offsetLeft;

  console.log(offsetParentLeft);
  console.log(targetElement?.offsetLeft);
  console.log(event?.pageX);
  console.log(targetElement?.offsetWidth);

  return (
    (event.pageX - (targetElement?.offsetLeft + offsetParentLeft)) / targetElement?.offsetWidth
  );
}
