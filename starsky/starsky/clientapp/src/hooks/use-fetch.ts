import { useEffect, useState } from 'react';
import { PageType } from '../interfaces/IDetailView';

const useFetch = (url: string, method: 'get' | 'post'): any | null => {
  const [data, setData] = useState(null);
  useEffect(() => {
    let mounted = true;
    const abortController = new AbortController();
    (async () => {
      try {
        const res: Response = await fetch(url, {
          signal: abortController.signal,
          credentials: "include",
          method: method
        });

        if (res.status === 404 || res.status === 500) {
          return {
            pageType: PageType.Unknown,
            statusCode: res.status
          }
        }

        const response = await res.json();
        response.statusCode = res.status;
        response.url = res.url;

        if (mounted) {
          setData(response);
        }
      } catch (e) {
        console.log(e);
      }
    })();
    const cleanup = () => {
      mounted = false;
      abortController.abort();
    };
    return cleanup;
  }, [url, method]);
  return data;
};

export default useFetch;

// ????? https://github.com/bghveding/use-fetch













// const defaultInit = {};
// const defaultReadBody = body => body.json();

// const useAsyncRun = (asyncTask) => {
//   const start = asyncTask && asyncTask.start;
//   const abort = asyncTask && asyncTask.abort;
//   useEffect(() => {
//     if (start) start();
//     const cleanup = () => {
//       if (abort) abort();
//     };
//     return cleanup;
//   }, [start]);
// };

// const useAsyncTaskFetch = (
//   input,
//   init = defaultInit,
//   readBody = defaultReadBody,
// ) => useAsyncTask(
//   async (abortController) => {
//     const response = await fetch(input, {
//       signal: abortController.signal,
//       ...init,
//     });
//     if (!response.ok) {
//       throw new Error(response.statusText);
//     }
//     const body = await readBody(response);
//     return body;
//   },
//   [input, init, readBody],
// );

// const useFetch = (...args) => {
//   const asyncTask = useAsyncTaskFetch(...args);
//   useAsyncRun(asyncTask);
//   return asyncTask;
// };


