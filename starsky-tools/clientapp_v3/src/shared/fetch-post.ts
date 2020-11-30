import { IConnectionDefault } from '../interfaces/IConnectionDefault';

const FetchPost = async (url: string, body: string | FormData, method: 'post' | 'delete' = 'post', headers: object = {}): Promise<IConnectionDefault> => {

  function getCookie(name: string): string {
    var match = document.cookie.match(new RegExp('(^| )' + name + '=([^;]+)'));
    if (match) return match[2];
    return "X-XSRF-TOKEN";
  }

  const settings: RequestInit = {
    method: method,
    body,
    credentials: "include" as RequestCredentials,
    headers: {
      'X-XSRF-TOKEN': getCookie("X-XSRF-TOKEN"),
      'Accept': 'application/json',
      ...headers
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