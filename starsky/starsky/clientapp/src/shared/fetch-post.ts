const FetchPost = async (url: string, body: string | FormData, method: 'post' | 'delete' = 'post'): Promise<any> => {
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

  const response = await fetch(url, settings);
  if (!response.ok) {
    console.error(response.status.toString())
    return null;
  }

  try {
    const data = await response.json();
    return data;
  } catch (err) {
    throw err;
  }
};

export default FetchPost;