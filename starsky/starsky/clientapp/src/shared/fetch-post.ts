const FetchPost = async (url: string, body: string, method: 'post' | 'delete' = 'post'): Promise<any> => {
  const settings = {
    method: method,
    body,
    credentials: "include" as RequestCredentials,
    headers: {
      'Accept': 'application/json',
      'Content-type': 'application/x-www-form-urlencoded',
    }
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