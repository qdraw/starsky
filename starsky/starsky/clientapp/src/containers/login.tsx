import React, { useEffect } from "react";
import Button from '../components/Button';
import useLocation from '../hooks/use-location';
import BrowserDetect from '../shared/browser-detect';
import FetchGet from '../shared/fetch-get';
import FetchPost from '../shared/fetch-post';
import { URLPath } from '../shared/url-path';
import { UrlQuery } from '../shared/url-query';
import { validateLoginForm } from '../shared/validate-login-form';

export interface ILoginProps {
  defaultLoginStatus?: boolean;
}

const Login: React.FC<ILoginProps> = () => {

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
  // var defaultLoginStatus = props.defaultLoginStatus ? props.defaultLoginStatus : true;
  const [isLogin, setLogin] = React.useState(true);
  useEffect(() => {
    FetchGet(new UrlQuery().UrlAccountStatus()).then((status) => {
      setLogin(status.statusCode === 401);
    });
  }, [history.location.search]);

  const authHandler = async () => {
    try {
      setLoading(true);
      const response = await FetchPost(new UrlQuery().UrlLogin(), 'Email=' + userEmail + '&Password=' + userPassword);
      if (!response || !response.data) {
        setError(MessageConnection);
      }
      else if (response.statusCode === 401 || response.statusCode === 302) {
        setLoading(false);
        setError(MessageWrongUsernamePassword);
      }
      else {
        // redirect
        var returnUrl = new URLPath().GetReturnUrl(history.location.search);
        history.navigate(returnUrl, { replace: true });

        // for chrome navigate isn't enough
        setTimeout(() => {
          document.location.reload();
        }, 100);
      }
    } catch (err) {
      setLoading(false);
      setError(err.message);
    }
  };

  /**
   * If you use a starsky.local address in Safari then the cookies are not shared, so your login will fail
   */
  useEffect(() => {
    if (history.location.hostname && history.location.hostname.match(/\.local$/ig) && new BrowserDetect().IsIOS()) {
      setError("Probeer in te loggen via een ip-adres of een echt domein, local adressen werken niet in Safari op iOS");
    }
  }, [history.location.search]);

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
};

export default Login;
