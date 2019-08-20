import * as React from "react";

export function usePromise<T>(
  promiseOrFunction: (() => Promise<T>) | Promise<T>,
  defaultValue: T
): [T, Error | string | null, boolean] {
  const [state, setState] = React.useState({
    value: defaultValue,
    error: null,
    isPending: true
  });

  React.useEffect(() => {
    const promise =
      typeof promiseOrFunction === "function"
        ? promiseOrFunction()
        : promiseOrFunction;

    let isSubscribed = true;
    promise
      .then(value =>
        isSubscribed ? setState({ value, error: null, isPending: false }) : null
      )
      .catch(error =>
        isSubscribed
          ? setState({ value: defaultValue, error: error, isPending: false })
          : null
      );

    return () => {
      isSubscribed = false;
    };
  }, [promiseOrFunction, defaultValue]);

  const { value, error, isPending } = state;
  return [value, error, isPending];
}
