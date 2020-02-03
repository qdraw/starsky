import React from 'react';
import useFetch from '../hooks/use-fetch';
import useGlobalSettings from '../hooks/use-global-settings';
import { IHealthEntry } from '../interfaces/IHealthEntry';
import { Language } from '../shared/language';
import Notification from './notification';

const HealthStatusError: React.FunctionComponent = () => {

  var healthCheck = useFetch("/api/health/details", 'get');

  const settings = useGlobalSettings();
  const MessageCriticalErrors = new Language(settings.language).text("Er zijn kritieke fouten in de volgende onderdelen:",
    "There are critical errors in the following components:");

  console.log(healthCheck.statusCode);

  if (healthCheck.statusCode === 200 || healthCheck.statusCode === 999) return (<></>)

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
    <Notification className="notification--danger">{content}</Notification>
  );
};

export default HealthStatusError
