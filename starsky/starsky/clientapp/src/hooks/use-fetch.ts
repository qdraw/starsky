import { useEffect, useState } from 'react';
import { IConnectionDefault, newIConnectionDefault } from '../interfaces/IConnectionDefault';

/**
 * With abort signal
 * @param url http/https url
 * @param method ;get or post
 */
const useFetch = (url: string, method: 'get' | 'post'): IConnectionDefault => {
  const [data, setData] = useState(newIConnectionDefault());

  useEffect(() => {
    let mounted = true;
    const abortController = new AbortController();

    fetchContent(url, method, mounted, abortController, setData);

    return () => {
      mounted = false;
      abortController.abort();
    };
  }, [url, method]);
  return data;
};


export const fetchContent = async (url: string,
  method: 'get' | 'post',
  mounted: boolean,
  abortController: AbortController,
  setData: React.Dispatch<React.SetStateAction<IConnectionDefault>>
): Promise<void> => {
  const res: Response = await fetch(url, {
    signal: abortController.signal,
    credentials: "include",
    method: method
  });

  let data;
  try {
    data = await res.json();
  } catch (event) {
    console.error("use-fetch", event);
  }

  var response = {
    statusCode: res.status,
    data
  } as IConnectionDefault;

  if (mounted) {
    setData(response);
  }

}

export default useFetch;

// Source:
// https://github.com/bghveding/use-fetch


