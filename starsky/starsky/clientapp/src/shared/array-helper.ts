
class ArrayHelper {
  public UniqueResults(arrayOfObj: any[], key: string) {
    // fileIndexItems.filter((v, i, a) => a.findIndex(t => (t.filePath === v.filePath)) === i) // duplicate check
    return arrayOfObj.filter((item, index, array) => {
      return array.map((mapItem) => mapItem[key]).indexOf(item[key]) === index
    })
  }
}

export default ArrayHelper;