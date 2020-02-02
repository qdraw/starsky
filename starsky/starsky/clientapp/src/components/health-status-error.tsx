import React from 'react';
import useFetch from '../hooks/use-fetch';
import { IHealthEntry } from '../interfaces/IHealthEntry';
import Notification from './notification';

const HealthStatusError: React.FunctionComponent = () => {

  var healthCheck = useFetch("/api/health", 'get');

  if (healthCheck.statusCode === 200 || !healthCheck.data || !healthCheck.data.entries) return (<></>)

  var content: JSX.Element[] = [<>{"Er zijn kritieke fouten in de volgende onderdelen:"}</>];

  healthCheck.data.entries.forEach((entry: IHealthEntry) => {
    if (entry.isHealthy) return;
    content.push(<li key={entry.name}> {entry.name}</li>)
  });

  return (
    <Notification className="notification--danger">{content}</Notification>
  );
};

export default HealthStatusError
