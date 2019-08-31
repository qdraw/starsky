const FetchPost = async (url: string, body: string): Promise<any> => {
  const settings = {
    method: 'POST',
    body,
    headers: {
      'Accept': 'application/json',
      'Content-type': 'application/x-www-form-urlencoded',
    }
  }

  const response = await fetch(url, settings);
  if (!response.ok) throw Error(response.status.toString());

  try {
    const data = await response.json();
    return data;
  } catch (err) {
    throw err;
  }
};

export default FetchPost;