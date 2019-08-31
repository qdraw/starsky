function validateEmail(email: string) {
  var re = /^(([^<>()[\]\\.,;:\s@"]+(\.[^<>()[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
  return re.test(String(email).toLowerCase());
}

/** Handle form validation for the login form
 * @param email - user's auth email
 * @param password - user's auth password
 * @param setError - function that handles updating error state value
 */
export const validateLoginForm = (
  email: string,
  password: string,
  setError: (error: string | null) => void
): boolean => {
  // Check for undefined or empty input fields
  if (!email || !password) {
    setError("Voer een emailadres en een wachtwoord in");
    return false;
  }

  // Validate email
  if (!validateEmail(email)) {
    setError("Controleer je email adres");
    return false;
  }

  return true;
};

/** Return user auth from local storage value */
// export const getStoredUserAuth = async (): Promise<UserAuth> => {

//   await apiRequest(
//     "https://jsonplaceholder.typicode.com/users",
//     "post",
//     { email: 'userEmail', password: 'userPassword' }
//   );

//   const auth = window.localStorage.getItem("UserAuth");
//   if (auth) {
//     return JSON.parse(auth);
//   }
//   return DEFAULT_USER_AUTH;
// };

/**
 * API Request handler
 * @param url - api endpoint
 * @param method - http method
 * @param bodyParams - body parameters of request
 */

export const apiRequest = async (
  url: string,
  method: string,
  bodyParams?: { email: string; password: string }
): Promise<any> => {
  try {
    const response = await fetch(url, {
      method,
      headers: {
        Accept: "application/json",
        "Content-Type": "application/json"
      },
      body: bodyParams ? JSON.stringify(bodyParams) : undefined
    });
    return await response.json();
  } catch (error) {
    return undefined;
  }
};
