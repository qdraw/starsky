import { IConnectionDefault } from "../../interfaces/IConnectionDefault";
import { GetCookie } from "../cookie/get-cookie.ts";

const FetchPost = async (
  url: string,
  body: string | FormData,
  method: "post" | "delete" = "post",
  headers: Record<string, string | undefined> = {}
): Promise<IConnectionDefault> => {
  const settings: RequestInit = {
    method: method,
    body,
    credentials: "include" as RequestCredentials,
    headers: {
      "X-XSRF-TOKEN": GetCookie("X-XSRF-TOKEN"),
      Accept: "application/json",
      ...headers
    }
  };

  if (typeof body === "string") {
    (settings.headers as Record<string, string>)["Content-Type"] =
      "application/x-www-form-urlencoded";
  }

  let res: Response;
  try {
    res = await fetch(url, settings);
  } catch {
    return {
      statusCode: 999,
      data: null
    };
  }

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
