import { IConnectionDefault } from '../interfaces/IConnectionDefault';

const FetchGet = async (url: string): Promise<IConnectionDefault> => {
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

    return {
      statusCode: res.status,
      data: response
    } as IConnectionDefault;

  } catch (err) {
    console.error(err);
    return {
      statusCode: res.status,
      data: null
    } as IConnectionDefault;
  }
};

export default FetchGet;