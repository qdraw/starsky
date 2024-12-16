// https://github.com/dashed/shallowequal/blob/master/index.js

const shallowEqual = (
  objA: unknown,
  objB: unknown,
  compare?: (objA: unknown, objB: unknown, indexOrKey?: number | string) => boolean | undefined,
  compareContext?: unknown
): boolean => {
  let ret = compare ? compare.call(compareContext, objA, objB) : void 0;

  if (ret !== void 0) {
    return !!ret;
  }

  if (objA === objB) {
    return true;
  }

  if (typeof objA !== "object" || !objA || typeof objB !== "object" || !objB) {
    return false;
  }

  const keysA = Object.keys(objA);
  const keysB = Object.keys(objB);

  if (keysA.length !== keysB.length) {
    return false;
  }

  const bHasOwnProperty = Object.prototype.hasOwnProperty.bind(objB);

  // Test for A's keys different from B.
  for (const key of keysA) {
    if (!bHasOwnProperty(key)) {
      return false;
    }

    const valueA = (objA as { [key: string]: unknown })[key];
    const valueB = (objB as { [key: string]: unknown })[key];

    ret = compare ? compare.call(compareContext, valueA, valueB, key) : void 0;

    if (ret === false || (ret === void 0 && valueA !== valueB)) {
      return false;
    }
  }

  return true;
};

export default shallowEqual;
