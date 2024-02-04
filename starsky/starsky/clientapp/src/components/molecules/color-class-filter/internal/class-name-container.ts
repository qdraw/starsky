export function ClassNameContainer(propsSticky?: boolean) {
  return !propsSticky
    ? "colorclass colorclass--filter"
    : "colorclass colorclass--filter colorclass--sticky";
}
