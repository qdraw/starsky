// capturePosition

const capturePosition = () => {
  let cachedPosition = window.pageYOffset;
  return {
    freeze: () => {
      // @ts-ignore
      document.body.style =
        `position: fixed; top: ${cachedPosition * -1}px; width: 100%;`;
    },
    unfreeze: () => {
      document.body.removeAttribute("style");
      window.scrollTo({
        top: cachedPosition
      });
    }
  };
};
export default capturePosition;