import React, { FunctionComponent, useEffect } from "react";
import ButtonStyled from "../components/atoms/button-styled/button-styled";
import useGlobalSettings from "../hooks/use-global-settings";
import useLocation from "../hooks/use-location/use-location";
import localization from "../localization/localization.json";
import { DocumentTitle } from "../shared/document-title";
import FetchGet from "../shared/fetch/fetch-get";
import FetchPost from "../shared/fetch/fetch-post";
import { Language } from "../shared/language";
import { UrlQuery } from "../shared/url/url-query";
import { validateLoginForm } from "../shared/validate-login-form";

const AccountRegister: FunctionComponent = () => {
  const settings = useGlobalSettings();
  const language = new Language(settings.language);

  const MessageApplicationName = "Starsky";
  const MessageCreateNewAccount = language.key(localization.MessageCreateNewAccount);
  const MessageUsername = language.key(localization.MessageUsername);
  const MessageExamplePassword = language.key(localization.MessageExamplePassword);
  const MessageExampleUsername = language.key(localization.MessageExampleUsername);
  const MessagePassword = language.key(localization.MessagePassword);
  const MessageConfirmPassword = language.key(localization.MessageConfirmPassword);
  const MessageNoUsernamePassword = language.key(localization.MessageNoUsernamePassword);
  const MessageWrongFormatEmailAddress = language.key(localization.MessageWrongFormatEmailAddress);
  const MessagePasswordToShort = language.key(localization.MessagePasswordToShort);
  const MessagePasswordNoMatch = language.key(localization.MessagePasswordNoMatch);
  const MessageConnection = language.key(localization.MessageConnection);
  const MessageRejectedBadRequest = language.key(localization.MessageRejectedBadRequest);
  const MessageRegistrationTurnedOff = language.key(localization.MessageRegistrationTurnedOff);
  const MessageSignInInstead = language.key(localization.MessageSignInInstead);
  const MessageLegalCreateAccountHtml = language.key(localization.MessageLegalCreateAccountHtml);

  const history = useLocation();

  const [userEmail, setUserEmail] = React.useState("");
  const [userPassword, setUserPassword] = React.useState("");
  const [userConfirmPassword, setUserConfirmPassword] = React.useState("");

  const useErrorHandler = (initialState: string | null) => {
    return initialState;
  };
  const [error, setError] = React.useState(useErrorHandler(null));

  const [loading, setLoading] = React.useState(false);
  new DocumentTitle().SetDocumentTitlePrefix(MessageCreateNewAccount);

  const setUpAccountHandler = async () => {
    const loginValidation = validateLoginForm(userEmail, userPassword);

    if (!loginValidation) {
      setError(
        loginValidation === null ? MessageWrongFormatEmailAddress : MessageNoUsernamePassword
      );
      return;
    }
    if (userPassword.length <= 7) {
      setError(MessagePasswordToShort);
      return;
    }
    if (userPassword !== userConfirmPassword) {
      setError(MessagePasswordNoMatch);
      return;
    }

    setLoading(true);

    const response = await FetchPost(
      new UrlQuery().UrlAccountRegisterApi(),
      `Email=${userEmail}&Password=${userPassword}&ConfirmPassword=${userConfirmPassword}`
    );

    if (response.statusCode === 400 || response.statusCode === 403) {
      setError(MessageRejectedBadRequest);
      setLoading(false);
      return;
    }

    if (!response?.data) {
      setError(MessageConnection);
      setLoading(false);
      return;
    }

    history.navigate(new UrlQuery().UrlLoginPage(), { replace: true });
  };

  const [displaySignInInstead, setDisplaySignInInstead] = React.useState(true);

  // readonly mode
  const [isFormEnabled, setIsFormEnabled] = React.useState(true);
  useEffect(() => {
    FetchGet(new UrlQuery().UrlAccountRegisterStatus()).then((response) => {
      setIsFormEnabled(response.statusCode !== 403);
      if (response.statusCode === 403) {
        setError(MessageRegistrationTurnedOff);
      }
      if (response.statusCode === 202) {
        setDisplaySignInInstead(false);
      }
    });
  }, [MessageRegistrationTurnedOff, history.location.search]);

  return (
    <>
      <header className="header header--main header--bluegray700">
        <div className="wrapper">
          <div className="item item--first item--detective">{MessageApplicationName}</div>
        </div>
      </header>

      <div className="content">
        <div className="content--header">{MessageCreateNewAccount}</div>

        <form
          method="post"
          className="content--login-form form-inline form-nav"
          data-test="account-register-form"
          onSubmit={(e) => {
            e.preventDefault();
            setError(null);
            setUpAccountHandler();
          }}
        >
          <label htmlFor="email">{MessageUsername}</label>
          <input
            className="form-control"
            disabled={!isFormEnabled}
            autoComplete="off"
            type="email"
            name="email"
            maxLength={80}
            data-test="email"
            spellCheck={false}
            value={userEmail}
            placeholder={MessageExampleUsername}
            onChange={(e) => setUserEmail(e.target.value)}
          />

          <label htmlFor="email">{MessagePassword}</label>
          <input
            className="form-control"
            disabled={!isFormEnabled}
            autoComplete="off"
            type="password"
            name="password"
            data-test="password"
            maxLength={80}
            spellCheck={false}
            placeholder={MessageExamplePassword}
            value={userPassword}
            onChange={(e) => setUserPassword(e.target.value)}
          />

          <label htmlFor="email">{MessageConfirmPassword}</label>
          <input
            className="form-control"
            disabled={!isFormEnabled}
            autoComplete="off"
            type="password"
            maxLength={100}
            spellCheck={false}
            name="confirm-password"
            data-test="confirm-password"
            value={userConfirmPassword}
            onChange={(e) => setUserConfirmPassword(e.target.value)}
          />
          <div
            className="legal-text-row"
            dangerouslySetInnerHTML={{ __html: MessageLegalCreateAccountHtml }}
          ></div>

          {error && (
            <div data-test="account-register-error" className="content--error-true">
              {error}
            </div>
          )}

          <ButtonStyled
            className="btn btn--default"
            type="submit"
            data-test="account-register-submit"
            disabled={loading || !isFormEnabled}
          >
            {loading ? "Loading..." : MessageCreateNewAccount}
          </ButtonStyled>
          {displaySignInInstead ? (
            <a
              data-test="sign-in-instead"
              className="alternative btn"
              href={new UrlQuery().UrlLoginPage()}
            >
              {MessageSignInInstead}
            </a>
          ) : (
            <div data-test="sign-in-instead" className="alternative btn disabled">
              {MessageSignInInstead}
            </div>
          )}
        </form>
      </div>
    </>
  );
};

export default AccountRegister;
