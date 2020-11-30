import React from 'react';
import useFetch from '../../../hooks/use-fetch';
import useGlobalSettings from '../../../hooks/use-global-settings';
import { IHealthEntry } from '../../../interfaces/IHealthEntry';
import { Language } from '../../../shared/language';
import { UrlQuery } from '../../../shared/url-query';
import Notification, { NotificationType } from '../../atoms/notification/notification';

const HealthStatusError: React.FunctionComponent = () => {

  var healthCheck = useFetch(new UrlQuery().UrlHealthDetails(), 'get');

  const settings = useGlobalSettings();
  const MessageCriticalErrors = new Language(settings.language).text("Er zijn kritieke fouten in de volgende onderdelen:",
    "There are critical errors in the following components:");

  if (healthCheck.statusCode === 200 || healthCheck.statusCode === 999 || healthCheck.statusCode === 401) return (null)

  var content: JSX.Element[] = [<span key="warning">{MessageCriticalErrors}</span>];

  if (!healthCheck.data || !healthCheck.data.entries) {
    content.push(<li key="backend-services">BackendServices HTTP StatusCode: {healthCheck.statusCode}</li>)
  }
  else {
    healthCheck.data.entries.forEach((entry: IHealthEntry) => {
      if (entry.isHealthy) return;
      content.push(<li key={entry.name}> {entry.name}</li>)
    });
  }

  return (
    <Notification type={NotificationType.danger}>{content}</Notification>
  );
};

export default HealthStatusError
