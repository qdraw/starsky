// capturePosition

export interface ICaptionPosition {
  freeze: () => void;
  unfreeze: () => void;
}

const capturePosition = () => {
  const topCachedPosition = window.scrollY;
  return {
    freeze: () => {
      (document.body as HTMLElement).style.position = "fixed";
      (document.body as HTMLElement).style.top = `${topCachedPosition * -1}px`;
      (document.body as HTMLElement).style.width = "100%";
    },
    unfreeze: () => {
      (document.body as HTMLElement).style.position = "initial";
      (document.body as HTMLElement).style.top = "initial";
      (document.body as HTMLElement).style.width = "initial";
      window.scrollTo(0, topCachedPosition);
    }
  } as ICaptionPosition;
};
export default capturePosition;
