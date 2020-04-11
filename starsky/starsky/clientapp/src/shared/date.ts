import { SupportedLanguages } from './language';

const isValidDate = (inputDateTime: string | undefined): boolean => {
  if (inputDateTime) {
    let input = new Date(inputDateTime).valueOf();
    return input > 0 && input < 7258118400 * 1000; // 01/01/2200
  }
  return false;
}


const leftPad = (n: number) => {
  return n > 9 ? "" + n : "0" + n;
};

/**
 * Return difference in Minutes
 * @param date a Javascript Datetime stamp (unix*1000)
 * @param now Javascript now
 */
const differenceInDate = (date: number, now: number = new Date().valueOf()): number => {
  return (now - date) / 60000;
};

const IsEditedNow = (inputDateTime: undefined | string): boolean | null => {
  if (!inputDateTime) return null;
  let input = new Date(inputDateTime).valueOf();
  if (!input) return null;
  let difference = differenceInDate(input);
  return difference <= 0.2;
};

const parseRelativeDate = (inputDateTime: string | undefined, locate: SupportedLanguages): string => {
  let date = "";

  if (!inputDateTime) return date;
  let input = new Date(`${inputDateTime}`).valueOf();

  if (!input) return date;

  let difference = differenceInDate(input);

  switch (true) {
    case (difference <= 1):
      return "{lessThan1Minute}";
    case (difference < 60):
      return difference.toFixed(0) + " {minutes}";
    case (difference < 1441):
      return Math.round(difference / 60) + " {hour}";
    default:
      return parseDate(inputDateTime, locate);
  }
}

const parseDate = (dateTime: string | undefined, locate: SupportedLanguages): string => {
  if (!dateTime) return "";
  var dateTimeObject = new Date(`${dateTime}Z`);
  // We prefer British English, uses day-month-year order
  var locateString = locate === SupportedLanguages.en ? "en-GB" : locate.toString();
  // toLocaleDateString assumes that the input is UTC, which is usaly not the case
  return dateTimeObject.toLocaleDateString(locateString, { timeZone: 'UTC', weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' });
}

const parseTime = (dateTime: string | undefined): string => {

  if (!isValidDate(dateTime) || !dateTime) {
    return "";
  }

  var dateTimeObject = new Date(`${dateTime}Z`);
  return dateTimeObject.toLocaleTimeString([], { timeZone: 'UTC', hour12: false, hour: '2-digit', minute: '2-digit', second: '2-digit' })
}

const secondsToHours = (seconds: number): string => {
  if (isNaN(seconds)) return '0:00';
  const time = new Date(0);
  time.setUTCSeconds(seconds);
  if (time.getUTCHours() === 0) return `${time.getUTCMinutes()}:${leftPad(time.getUTCSeconds())}`;
  return `${time.getUTCHours()}:${leftPad(time.getUTCMinutes())}:${leftPad(time.getUTCSeconds())}`;
}

export { IsEditedNow, isValidDate, parseRelativeDate, parseDate, parseTime, leftPad, secondsToHours };

