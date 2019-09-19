const FetchGet = async (url: string): Promise<any> => {
  const settings = {
    method: 'GET',
    credentials: "include" as RequestCredentials,
    headers: {
      'Accept': 'application/json',
    }
  }

  const res = await fetch(url, settings);
  const response = await res.json();

  if (typeof response === "string") {
    return {
      statusCode: res.status,
      data: response
    } as any;
  }

  if (!response.ok) {
    console.error(response.status)
    return null;
  }

  try {
    const data = await response.json();
    return data;
  } catch (err) {
    throw err;
  }
};

export default FetchGet;