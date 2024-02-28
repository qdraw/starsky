import useFetch from "../../../hooks/use-fetch";
import useGlobalSettings from "../../../hooks/use-global-settings";
import { IHealthEntry } from "../../../interfaces/IHealthEntry";
import localization from "../../../localization/localization.json";
import { Language } from "../../../shared/language";
import { UrlQuery } from "../../../shared/url-query";
import Notification, { NotificationType } from "../../atoms/notification/notification";

const HealthStatusError: React.FunctionComponent = () => {
  const healthCheck = useFetch(new UrlQuery().UrlHealthDetails(), "get");

  const settings = useGlobalSettings();
  const MessageHealthStatusCriticalErrors = new Language(settings.language).key(
    localization.MessageHealthStatusCriticalErrorsWithTheFollowingComponents
  );

  if (
    healthCheck.statusCode === 200 ||
    healthCheck.statusCode === 999 ||
    healthCheck.statusCode === 401
  )
    return null;

  const content: React.JSX.Element[] = [
    <span key="warning">{MessageHealthStatusCriticalErrors}</span>
  ];

  if (!healthCheck.data?.entries) {
    content.push(
      <li key="backend-services">BackendServices HTTP StatusCode: {healthCheck.statusCode}</li>
    );
  } else {
    healthCheck.data.entries.forEach((entry: IHealthEntry) => {
      if (entry.isHealthy) return;
      content.push(<li key={entry.name}> {entry.name}</li>);
    });
  }

  return <Notification type={NotificationType.danger}>{content}</Notification>;
};

export default HealthStatusError;
