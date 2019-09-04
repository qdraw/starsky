import * as React from "react";
import Button from '../components/Button';
/** Context */
import { authContext } from "../contexts/AuthContext";
import "../style/css/00-index.css";
/** Presentation */
// import ErrorMessage from "../components/ErrorMessage";
/** Custom Hooks */
import useErrorHandler from "../utils/custom-hooks/ErrorHandler";
/** Utils */
import { validateLoginForm } from "../utils/Helpers";

function Login() {
  const [userEmail, setUserEmail] = React.useState("");
  const [userPassword, setUserPassword] = React.useState("");
  const [loading, setLoading] = React.useState(false);
  const auth = React.useContext(authContext);
  const { error, showError } = useErrorHandler(null);
  const MessageApplicationName: string = "Starsky";
  const MessageWrongUsernamePassword: string = "Je gebruikersnaam of wachtwoord is niet juist. Probeer het opnieuw";
  const MessageUsername: string = "E-mailadres of telefoonnummer";
  const MessageConnection: string = "Er is geen verbinding mogelijk, probeer het later opnieuw";

  const MessagePassword: string = "Geef je wachtwoord op";
  const MessageExamplePassword: string = "superveilig";
  const MessageExampleUsername: string = "dont@mail.me";


  const MessageLogin: string = "Inloggen";


  const authHandler = async () => {
    try {
      setLoading(true);

      const response = await fetch("/account/login", {
        credentials: "include",
        method: "POST",
        headers: {
          "Content-Type": "application/x-www-form-urlencoded"
        },
        body: 'Email=' + userEmail + '&Password=' + userPassword
      })
      if (!response) {
        showError(MessageConnection);
      }
      else if (response.status === 401) {
        auth.setUnauthStatus();
        setLoading(false);
        showError(MessageWrongUsernamePassword);
      }
      else {
        auth.setAuthStatus({ login: true, username: userEmail });
      }
    } catch (err) {
      setLoading(false);
      showError(err.message);
    }
  };

  return (
    <>
      <header className="header header--main header--bluegray700">
        <div className="wrapper">
          <div className="item item--first item--detective">{MessageApplicationName}
          </div>
        </div>
      </header>
      <div className="content"><div className="content--header">{MessageLogin}</div>

        <form
          className="content--login-form form-inline form-nav"
          onSubmit={e => {
            e.preventDefault();
            showError(null);
            if (validateLoginForm(userEmail, userPassword, showError)) {
              authHandler();
            }
          }}
        >
          <label htmlFor="username">
            {MessageUsername}
          </label>
          <input
            className="form-control"
            autoComplete="off"
            type="email"
            name="email"
            value={userEmail}
            placeholder={MessageExampleUsername}
            onChange={e => setUserEmail(e.target.value)}
          />
          <label htmlFor="password">
            {MessagePassword}
          </label>
          <input
            className="form-control"
            type="password"
            name="password"
            value={userPassword}
            placeholder={MessageExamplePassword}
            onChange={e => setUserPassword(e.target.value)}
          />
          {error && <div className="content--error-true">{error}</div>}

          <Button className="btn btn--default" type="submit" disabled={loading} onClick={e => { }}>
            {loading ? "Loading..." : MessageLogin}
          </Button>
        </form>
      </div>
    </>
  );
}

export default Login;
