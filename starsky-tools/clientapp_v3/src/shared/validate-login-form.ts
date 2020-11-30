
/**
 * Handle form validation for the login form
 * false is no email or password
 * null is non valid email adres
 * true is continue
 * @param email - user's auth email
 * @param password - user's auth password
 */
export const validateLoginForm = (
  email: string,
  password: string,
): boolean | null => {
  // Check for undefined or empty input fields
  if (!email || !password) {
    return false;
  }

  if (!validateEmail(email)) {
    return null;
  }
  return true;
};

function validateEmail(email: string) {
  var re = /^(([^<>()[\]\\.,;:\s@"]+(\.[^<>()[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
  return re.test(String(email).toLowerCase());
}
