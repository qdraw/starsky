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
        const response = await res.json();
        response.statusCode = res.status;
        response.url = res.url;

        if (res.status === 404) {
          return {
            pageType: PageType.NotFound,
          }
        }
        else if (res.status >= 400 && res.status <= 550) {
          return {
            pageType: PageType.ApplicationException,
          }
        }

        if (mounted) {
          setData(response);
        }
      } catch (e) {
        console.error(e);
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


