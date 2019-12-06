import { IConnectionDefault } from '../interfaces/IConnectionDefault';

const FetchPost = async (url: string, body: string | FormData, method: 'post' | 'delete' = 'post'): Promise<IConnectionDefault> => {
  const settings: RequestInit = {
    method: method,
    body,
    credentials: "include" as RequestCredentials,
    headers: {
      'Accept': 'application/json',
    },
  }

  if (typeof body === "string") {
    (settings.headers as any)['Content-type'] = 'application/x-www-form-urlencoded';
  }

  const res = await fetch(url, settings);

  try {
    const data = await res.json();
    return {
      statusCode: res.status,
      data
    } as IConnectionDefault;
  } catch (err) {
    console.error(err);
    return {
      statusCode: res.status,
      data: null
    } as IConnectionDefault;
  }
};

export default FetchPost;