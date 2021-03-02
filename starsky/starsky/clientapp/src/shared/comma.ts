export class Comma {
  public CommaSpaceLastDot(inputs: string[]): string {
    let output = "";
    for (let index = 0; index < inputs.length; index++) {
      const element = inputs[index];
      output += element.split(".")[element.split(".").length - 1];
      if (index !== inputs.length - 1) {
        output += ", ";
      }
    }
    return output;
  }
}
