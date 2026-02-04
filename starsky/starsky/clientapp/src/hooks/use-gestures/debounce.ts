export const debounce = (func: (...args: unknown[]) => void, wait: number) => {
  let timeout: ReturnType<typeof setTimeout>;
  return (...args: unknown[]) => {
    clearTimeout(timeout);
    timeout = setTimeout(() => func(...args), wait);
  };
};
