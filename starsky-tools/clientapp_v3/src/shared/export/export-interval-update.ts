import { Dispatch } from 'react';
import FetchGet from '../fetch-get';
import { UrlQuery } from '../url-query';
import { ProcessingState } from './processing-state';

export async function ExportIntervalUpdate(zipKey: string, setProcessing: Dispatch<ProcessingState>) {
  // need to check if ProcessingState = server
  if (!zipKey) return;
  var result = await FetchGet(new UrlQuery().UrlExportZipApi(zipKey, true));

  switch (result.statusCode) {
    case 200:
      setProcessing(ProcessingState.ready);
      // not ready jet
      return;
    case 206:
    case 404:
      // not ready yet status 404 and 206
      break;
    default:
      setProcessing(ProcessingState.fail);
  }
}