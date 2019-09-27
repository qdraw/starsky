const FetchGet = async (url: string): Promise<any> => {
  const settings = {
    method: 'GET',
    credentials: "include" as RequestCredentials,
    headers: {
      'Accept': 'application/json',
    }
  }

  const res = await fetch(url, settings);
  try {

    const response = await res.json();

    if (typeof response === "string") {
      return {
        statusCode: res.status,
        data: response
      } as any;
    }

    if (!res.ok) {
      console.error(response)
      return null;
    }

    return response;
  } catch (err) {
    throw err;
  }
};

export default FetchGet;