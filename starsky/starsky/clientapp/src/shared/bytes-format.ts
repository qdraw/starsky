/**
 * Display human friendly bytes sizes
 * @see: https://stackoverflow.com/a/18650828/8613589
 * @param bytes - size in bytes
 * @param decimals - number of decimals (default is 2 -> 0.20)
 */
export default function BytesFormat(bytes: number, decimals = 2): null | string {
  if (!bytes) return null;

  const k = 1024;
  const dm = decimals < 0 ? 0 : decimals;
  const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB', 'PB', 'EB', 'ZB', 'YB'];

  const i = Math.floor(Math.log(bytes) / Math.log(k));

  return parseFloat((bytes / Math.pow(k, i)).toFixed(dm)).toLocaleString() + ' ' + sizes[i];
}