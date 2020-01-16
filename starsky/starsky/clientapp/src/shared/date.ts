
const isValidDate = (inputDateTime: string | undefined): boolean => {
  if (inputDateTime) {
    let input = new Date(inputDateTime).valueOf();
    return input > 0;
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

const parseRelativeDate = (inputDateTime: string | undefined): string => {
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
      return parseDate(inputDateTime);
  }
}

const parseDate = (dateTime: string | undefined): string => {

  let date = "";
  if (dateTime) {
    date += new Date(dateTime).getDate();
    date += "-";
    date += new Date(dateTime).getMonth() + 1;
    date += "-";
    date += new Date(dateTime).getFullYear();
  }
  return date;
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

