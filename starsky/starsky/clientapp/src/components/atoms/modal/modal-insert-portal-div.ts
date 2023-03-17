export default function modalInserPortalDiv(
  modal: React.MutableRefObject<HTMLDivElement | null>,
  hasUpdated: boolean,
  forceUpdate: React.Dispatch<React.SetStateAction<boolean>>,
  id: string
) {
  modal.current = document.createElement("div");
  modal.current.id = id;

  if (!document.body.querySelector(`#${id}`)) {
    document.body.insertBefore(modal.current, document.body.firstChild);
  }

  if (!hasUpdated) forceUpdate(true);

  return () => {
    if (modal.current) {
      document.body.removeChild(modal.current);
    }
  };
}
