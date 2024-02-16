export const shuffle = <T>(arr: T[]): T[] => {
  return arr
    .map(x => ({ value: x, sort: Math.random() }))
    .sort((a, b) => a.sort - b.sort)
    .map(({ value }) => value);
};

export const sum = (a: number, b: number): number => a + b;
