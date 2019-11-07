import React, { useEffect } from "react";
import Button from '../components/Button';
import useLocation from '../hooks/use-location';
import FetchGet from '../shared/fetch-get';
import { URLPath } from '../shared/url-path';
import { validateLoginForm } from '../shared/validate-login-form';

function Login() {
  var history = useLocation();

  const [userEmail, setUserEmail] = React.useState("");
  const [userPassword, setUserPassword] = React.useState("");
  const [loading, setLoading] = React.useState(false);

  const useErrorHandler = (initialState: string | null) => { return initialState };
  const [error, setError] = React.useState(useErrorHandler(null));

  const MessageApplicationName: string = "Starsky";
  const MessageWrongUsernamePassword: string = "Je gebruikersnaam of wachtwoord is niet juist. Probeer het opnieuw";
  const MessageUsername: string = "E-mailadres of telefoonnummer";
  const MessageConnection: string = "Er is geen verbinding mogelijk, probeer het later opnieuw";
  const LogoutWarning: string = "Wil je uitloggen?";
  const MessagePassword: string = "Geef je wachtwoord op";
  const MessageExamplePassword: string = "superveilig";
  const MessageExampleUsername: string = "dont@mail.me";

  const MessageLogin: string = "Inloggen";
  const MessageLogout: string = "Uitloggen";

  // We don't want to login twich
  const [isLogin, setLogin] = React.useState(true);
  useEffect(() => {
    (async function () {
      var status = await FetchGet("/account/status");
      setLogin(status.statusCode === 401);
    })();
  }, []);


  const authHandler = async () => {
    try {
      setLoading(true);

      const response = await fetch("/account/login?json=true", {
        credentials: "include",
        method: "POST",
        headers: {
          "Content-Type": "application/x-www-form-urlencoded"
        },
        body: 'Email=' + userEmail + '&Password=' + userPassword
      })
      if (!response) {
        setError(MessageConnection);
      }
      else if (response.status === 401) {
        setLoading(false);
        setError(MessageWrongUsernamePassword);
      }
      else {
        // redirect
        var returnUrl = new URLPath().GetReturnUrl(history.location.search);
        history.navigate(returnUrl, { replace: true });
      }
    } catch (err) {
      setLoading(false);
      setError(err.message);
    }
  };

  return (
    <>
      {isLogin ?
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
                setError(null);
                if (validateLoginForm(userEmail, userPassword, setError)) {
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
        : null}
      {!isLogin ?
        <>
          <header className="header header--main header--bluegray700">
            <div className="wrapper">
              <div className="item item--first item--detective">{MessageApplicationName}
              </div>
            </div>
          </header>
          <div className="content"><div className="content--header">{MessageLogin}</div>
          </div>
          <div className="content">
            <form className="content--login-form">
              <div className="content--error-true">{LogoutWarning}</div>
              <a className="btn btn--default" href="/account/logout">{MessageLogout}</a>
            </form>
          </div>
        </>
        : null}
    </>
  );
}

export default Login;
