import useFetch from "../../../hooks/use-fetch";
import useGlobalSettings from "../../../hooks/use-global-settings";
import { IHealthEntry } from "../../../interfaces/IHealthEntry";
import localization from "../../../localization/localization.json";
import { Language } from "../../../shared/language";
import { UrlQuery } from "../../../shared/url/url-query";
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

  const healthCheckData = healthCheck.data as { entries: IHealthEntry[] };

  if (healthCheckData?.entries) {
    for (const entry of healthCheckData.entries) {
      if (entry.isHealthy) continue;
      content.push(<li key={entry.name}> {entry.name}</li>);
    }
  } else {
    content.push(
      <li key="backend-services">BackendServices HTTP StatusCode: {healthCheck.statusCode}</li>
    );
  }

  return <Notification type={NotificationType.danger}>{content}</Notification>;
};

export default HealthStatusError;
