/* eslint-disable @typescript-eslint/no-unsafe-member-access */
/* eslint-disable @typescript-eslint/no-unsafe-member-access */
/* eslint-disable @typescript-eslint/no-unsafe-assignment */
/* eslint-disable @typescript-eslint/no-unsafe-return */
/* eslint-disable @typescript-eslint/no-unsafe-argument */

const orDie = (arg) => {
  if (!arg) throw new Error(`Argument is empty`);
  return arg;
};

export const unfreezeImport = <T>(module: T, key: keyof T): void => {
  const meta = orDie(Object.getOwnPropertyDescriptor(module, key));
  const getter = orDie(meta.get);

  const originalValue = getter() as T[typeof key];
  let currentValue = originalValue;
  let isMocked = false;

  Object.defineProperty(module, key, {
    ...meta,
    get: () => (isMocked ? currentValue : getter()),
    set(newValue: T[typeof key]) {
      isMocked = newValue !== originalValue;
      currentValue = newValue;
    },
  });
};
