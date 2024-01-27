export function GetMousePosition(event: React.MouseEvent | MouseEvent) {
  const target = event.target as HTMLProgressElement;
  if (!target.offsetLeft) return 0.1;
  return (
    (event.pageX -
      (target.offsetLeft + (target.offsetParent as HTMLElement).offsetLeft)) /
    target.offsetWidth
  );
}
