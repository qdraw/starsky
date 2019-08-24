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


