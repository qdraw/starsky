export const debounce = (func: any, wait: number) => {
  let timeout: any;
  return function (...args: any) {
    // @ts-ignore
    const context = this;
    clearTimeout(timeout);
    timeout = setTimeout(() => func.apply(context, args), wait);
  };
};
