import React, { useEffect } from "react";
import ButtonStyled from "../components/atoms/button-styled/button-styled";
import Preloader from "../components/atoms/preloader/preloader";
import useFetch from "../hooks/use-fetch";
import useGlobalSettings from "../hooks/use-global-settings";
import useLocation from "../hooks/use-location/use-location";
import localization from "../localization/localization.json";
import { BrowserDetect } from "../shared/browser-detect";
import { DocumentTitle } from "../shared/document-title";
import FetchPost from "../shared/fetch/fetch-post";
import { Language } from "../shared/language";
import { UrlQuery } from "../shared/url/url-query";
import { validateLoginForm } from "../shared/validate-login-form";

interface ILoginProps {
  defaultLoginStatus?: boolean;
}

export const Login: React.FC<ILoginProps> = () => {
  const history = useLocation();
  const [userEmail, setUserEmail] = React.useState("");
  const [userPassword, setUserPassword] = React.useState("");
  const [loading, setLoading] = React.useState(false);

  const useErrorHandler = (initialState: string | null) => {
    return initialState;
  };
  const [error, setError] = React.useState(useErrorHandler(null));

  const settings = useGlobalSettings();
  const language = new Language(settings.language);

  const MessageApplicationName = "Starsky";
  const MessageWrongUsernamePassword = language.key(localization.MessageWrongUsernamePassword);
  const MessageLockedOut = language.key(localization.MessageLockedOut);
  const MessageNoUsernamePassword = language.key(localization.MessageNoUsernamePassword);
  const MessageWrongFormatEmailAddress = language.key(localization.MessageWrongFormatEmailAddress);
  const MessageUsername = language.key(localization.MessageUsername);
  const MessageConnection = language.key(localization.MessageConnection);
  const MessageDatabaseConnection = language.key(localization.MessageDatabaseConnection);
  const LogoutWarning = language.key(localization.LogoutWarning);
  const MessageStayLoggedIn = language.key(localization.MessageStayLoggedIn);
  const MessagePassword = language.key(localization.MessagePassword);
  const MessageExamplePassword = language.key(localization.MessageExamplePassword);
  const MessageExampleUsername = language.key(localization.MessageExampleUsername);
  const MessageLogin = language.key(localization.MessageLogin);
  const MessageLogout = language.key(localization.MessageLogout);
  const MessageCreateAccount = language.key(localization.MessageCreateAccount);

  // We don't want to login twice
  const [isLogin, setIsLogin] = React.useState(true);

  const accountStatus = useFetch(new UrlQuery().UrlAccountStatus(), "get");

  useEffect(() => {
    if (!accountStatus) return;
    setIsLogin(accountStatus.statusCode === 401);
    new DocumentTitle().SetDocumentTitlePrefix(
      accountStatus.statusCode === 401 ? MessageLogin : MessageLogout
    );
    // to help new users find the register screen
    if (
      accountStatus.statusCode === 406 &&
      history.location.search.indexOf(new UrlQuery().UrlAccountRegisterPage()) === -1
    ) {
      history.navigate(new UrlQuery().UrlAccountRegisterPage(), {
        replace: true
      });
    }
  }, [accountStatus, history, MessageLogin, MessageLogout]);

  const authHandler = async () => {
    try {
      setLoading(true);
      const response = await FetchPost(
        new UrlQuery().UrlLoginApi(),
        "Email=" + userEmail + "&Password=" + userPassword
      );
      setLoading(false);
      if (!response?.data) {
        setError(MessageConnection);
      } else if (response.statusCode === 423) {
        setError(MessageLockedOut);
      } else if (response.statusCode === 401 || response.statusCode === 302) {
        setError(MessageWrongUsernamePassword);
      } else {
        // redirect
        let returnUrl = new UrlQuery().GetReturnUrl(history.location.search);
        // only used in the dev, because you have the same url
        if (`/${history.location.search}` === returnUrl) {
          returnUrl += "&details=true";
        }

        history.navigate(returnUrl, { replace: true });
      }
    } catch (err: any) {
      setLoading(false);
      setError(err.message);
    }
  };

  if (!accountStatus) return <Preloader isOverlay={true}></Preloader>;

  return (
    <>
      <header className="header header--main header--bluegray700">
        <div className="wrapper">
          <div className="item item--first item--detective">{MessageApplicationName}</div>
        </div>
      </header>

      {!accountStatus.data && new BrowserDetect().IsLegacy() ? (
        <div className="content">
          <div className="warning-box">
            Your browser is not supported, please try the latest version of Firefox or Chrome
          </div>
        </div>
      ) : null}

      {accountStatus.statusCode === 500 || accountStatus.statusCode === 503 ? (
        <div className="content">
          <div data-test="message-database-connection" className="warning-box">
            {MessageDatabaseConnection}
          </div>
        </div>
      ) : null}

      {isLogin ? (
        <div className="content" data-test="login-content">
          <div className="content--header">{MessageLogin}</div>
          <form
            className="content--login-form form-inline form-nav"
            onSubmit={(e) => {
              e.preventDefault();
              setError(null);
              const loginValidation = validateLoginForm(userEmail, userPassword);
              if (!loginValidation) {
                setError(
                  loginValidation === null
                    ? MessageWrongFormatEmailAddress
                    : MessageNoUsernamePassword
                );
                return;
              }
              authHandler();
            }}
          >
            <label htmlFor="email">{MessageUsername}</label>
            <input
              className="form-control"
              autoComplete="off"
              type="email"
              data-test="email"
              name="email"
              maxLength={80}
              value={userEmail}
              placeholder={MessageExampleUsername}
              onChange={(e) => setUserEmail(e.target.value)}
            />
            <label htmlFor="password">{MessagePassword}</label>
            <input
              className="form-control"
              type="password"
              data-test="password"
              name="password"
              maxLength={80}
              value={userPassword}
              placeholder={MessageExamplePassword}
              onChange={(e) => setUserPassword(e.target.value)}
            />
            {error && (
              <div data-test="login-error" className="content--error-true">
                {error}
              </div>
            )}

            <ButtonStyled
              className="btn btn--default"
              type="submit"
              data-test="login-submit"
              disabled={loading}
            >
              {loading ? "Loading..." : MessageLogin}
            </ButtonStyled>
            <a className="alternative" href={new UrlQuery().UrlAccountRegisterPage()}>
              {MessageCreateAccount}
            </a>
          </form>
        </div>
      ) : null}
      {!isLogin && accountStatus.data && accountStatus.statusCode <= 400 ? (
        <>
          <div className="content" data-test="logout-content">
            <div className="content--header">{MessageLogout}</div>
          </div>
          <div className="content">
            <form className="content--login-form">
              <div className="content--error-true">{LogoutWarning}</div>
              <a
                className="btn btn--default"
                data-test="logout"
                href={new UrlQuery().UrlLogoutPage(
                  new UrlQuery().UrlHomeIndexPage(
                    new UrlQuery().GetReturnUrl(history.location.search)
                  )
                )}
              >
                {MessageLogout}
              </a>
              <a
                className="btn btn--info"
                data-test="stayLoggedin"
                href={new UrlQuery().UrlHomeIndexPage(
                  new UrlQuery().GetReturnUrl(history.location.search)
                )}
              >
                {MessageStayLoggedIn}
              </a>
              <a
                className="btn btn--info"
                data-test="stayLoggedin"
                href={new UrlQuery().UrlAccountRegisterPage()}
              >
                {MessageCreateAccount}
              </a>
            </form>
          </div>
        </>
      ) : null}
    </>
  );
};
