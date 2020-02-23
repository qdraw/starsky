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
}

/**
 * Return difference in Minutes
 * @param date a Javascript Datetime stamp (unix*1000)
 */
const differenceInDate = (date: number): number => {
  let now = new Date().valueOf();
  let difference = (now - date) / 60000;
  return difference;
}

const IsEditedNow = (inputDateTime: undefined | string): boolean | null => {
  if (!inputDateTime) return null;
  let input = new Date(inputDateTime).valueOf();
  if (!input) return null;
  let difference = differenceInDate(input);
  return difference <= 0.2;
}

const parseRelativeDate = (inputDateTime: string | undefined, locate: SupportedLanguages): string => {
  let date = "";

  if (!inputDateTime) return date;
  let input = new Date(inputDateTime).valueOf();

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
  var dateTimeObject = new Date(dateTime);
  // We prefer British English, uses day-month-year order
  var locateString = locate === SupportedLanguages.en ? "en-GB" : locate.toString()
  return dateTimeObject.toLocaleDateString(locateString, { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' });
}

const parseTime = (dateTime: string | undefined): string => {

  let date = "";
  if (dateTime) {
    date += leftPad(new Date(dateTime).getHours());
    date += ":";
    date += leftPad(new Date(dateTime).getMinutes());
    date += ":";
    date += leftPad(new Date(dateTime).getSeconds());
  }
  return date;
}

export { IsEditedNow, isValidDate, parseRelativeDate, parseDate, parseTime, leftPad, };

