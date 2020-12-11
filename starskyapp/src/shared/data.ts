/**
 * Return difference in Minutes
 * @param date a Javascript Datetime stamp (unix*1000)
 * @param now Javascript now
 */
const DifferenceInDate = (
  date: number,
  now: number = new Date().valueOf()
): number => {
  return (now - date) / 60000;
};
