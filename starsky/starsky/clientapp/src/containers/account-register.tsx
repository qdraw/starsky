import React, { FunctionComponent, useEffect } from "react";
import ButtonStyled from "../components/atoms/button-styled/button-styled";
import useGlobalSettings from "../hooks/use-global-settings";
import useLocation from "../hooks/use-location";
import DocumentTitle from "../shared/document-title";
import FetchGet from "../shared/fetch-get";
import FetchPost from "../shared/fetch-post";
import { Language } from "../shared/language";
import { UrlQuery } from "../shared/url-query";
import { validateLoginForm } from "../shared/validate-login-form";

const AccountRegister: FunctionComponent = () => {
  const settings = useGlobalSettings();
  const language = new Language(settings.language);

  const MessageApplicationName = "Starsky";
  const MessageCreateNewAccount = language.text(
    "Maak nieuw account",
    "Create new account"
  );
  const MessageUsername = language.text("E-mailadres", "E-mail address");
  const MessageExamplePassword = language.text("superveilig", "supersafe");
  const MessageExampleUsername = "dont@mail.me";
  const MessagePassword = language.text(
    "Geef je wachtwoord op",
    "Enter your password"
  );
  const MessageConfirmPassword = language.text(
    "Vul je wachtwoord nog een keer in",
    "Enter your password again"
  );
  const MessageNoUsernamePassword = language.text(
    "Voer een emailadres en een wachtwoord in",
    "Enter an email address and password"
  );
  const MessageWrongFormatEmailAddress = language.text(
    "Controleer je email adres",
    "Check your email address"
  );
  const MessagePasswordToShort = language.text(
    "Gebruik minimaal 8 tekens voor je wachtwoord",
    "Use at least 8 characters for your password"
  );
  const MessagePasswordNoMatch = language.text(
    "Deze wachtwoorden komen niet overeen. Probeer het opnieuw",
    "These passwords do not match. Please try again"
  );
  const MessageConnection = language.text(
    "Er is geen verbinding mogelijk, probeer het later opnieuw",
    "No connection is possible, please try again later"
  );
  const MessageRejectedBadRequest = language.text(
    "Dit verzoek is afgewezen aangezien er niet voldaan is aan de beveiligingseisen (Error 400)",
    "This request was rejected because the security requirements were not met  (Error 400)"
  );
  const MessageRegistrationTurnedOff = language.text(
    "Registratie is uitgezet",
    "Registration is turned off"
  );
  const MessageSignInInstead = language.text(
    "In plaats daarvan inloggen",
    "Sign in instead"
  );

  const MessageLegalCreateAccountHtml = language.text(
    `Door het creëren van een account gaat u akkoord met de
   <a href="/legal/toc.nl.html" data-test="toc">Algemene Voorwaarden</a> van Starsky. Raadpleeg en bekijk hier onze 
   <a href="/legal/privacy-policy.nl.html" data-test="privacy">Privacykennisgeving</a> en onze 
   <a href="/legal/privacy-policy.nl.html#cookie">Cookieverklaring</a>.`,
    `By creating an account you agree to <a href="/legal/toc.en.html" data-test="toc">Starsky's Conditions of Use</a>. 
   Please see our  <a href="/legal/privacy-policy.en.html" data-test="privacy">Privacy</a> Notice and our 
   <a href="/legal/privacy-policy.en.html#cookie">Cookies Notice </a>   `
  );

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
        loginValidation === null
          ? MessageWrongFormatEmailAddress
          : MessageNoUsernamePassword
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
  const [isFormEnabled, setFormEnabled] = React.useState(true);
  useEffect(() => {
    FetchGet(new UrlQuery().UrlAccountRegisterStatus()).then((response) => {
      setFormEnabled(response.statusCode !== 403);
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
          <div className="item item--first item--detective">
            {MessageApplicationName}
          </div>
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
            <div
              data-test="account-register-error"
              className="content--error-true"
            >
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
            <div
              data-test="sign-in-instead"
              className="alternative btn disabled"
            >
              {MessageSignInInstead}
            </div>
          )}
        </form>
      </div>
    </>
  );
};

export default AccountRegister;
