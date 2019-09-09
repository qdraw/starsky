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

function validateEmail(email: string) {
  var re = /^(([^<>()[\]\\.,;:\s@"]+(\.[^<>()[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
  return re.test(String(email).toLowerCase());
}
