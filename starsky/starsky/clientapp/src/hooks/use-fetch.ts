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

        if (res.status >= 400 && res.status <= 550 && res.status !== 404) {
          setData({
            pageType: PageType.ApplicationException,
          } as any)
          return;
        }

        if (mounted) {
          setData(response);
        }
      } catch (event) {
        console.error("use-fetch", event);
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


