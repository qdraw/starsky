class ArrayHelper {
  public UniqueResults<T>(arrayOfObj: T[], key: keyof T): T[] {
    if (!arrayOfObj) return arrayOfObj;
    return arrayOfObj.filter((item, index, array) => {
      return array.map((mapItem) => mapItem[key]).indexOf(item[key]) === index;
    });
  }
}

export default ArrayHelper;
