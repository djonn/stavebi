export const shuffle = <T>(arr: T[]): T[] => {
  return arr
    .map(x => ({ value: x, sort: Math.random() }))
    .sort((a, b) => a.sort - b.sort)
    .map(({ value }) => value);
};

export const sum = (a: number, b: number): number => a + b;

export const daysBetween = (from: Date, to: Date): number => {
  let millisBetween = to.valueOf() - from.valueOf();
  let dayMs = 1000 * 60 * 60 * 24;

  let daysBetween = Math.floor(millisBetween / dayMs);
  return daysBetween;
};
