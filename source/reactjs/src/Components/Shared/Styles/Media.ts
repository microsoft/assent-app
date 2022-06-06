type Breakpoint = 's' | 'm' | 'l' | 'xl' | 'xxl' | 'xxxl';

type BreakpointMap = { [breakpoint in Breakpoint]: number };

export const breakpointMap: BreakpointMap = {
    s: 300,
    m: 480,
    l: 640,
    xl: 1024,
    xxl: 1366,
    xxxl: 1920
};

export const maxWidth = (Object.keys(breakpointMap) as (keyof BreakpointMap)[]).reduce((accumulator, label) => {
    accumulator[label] = `@media (max-width: ${breakpointMap[label] - 1}px)`;
    return accumulator;
}, {} as { [key in keyof BreakpointMap]: string });

export const minWidth = (Object.keys(breakpointMap) as (keyof BreakpointMap)[]).reduce((accumulator, label) => {
    accumulator[label] = `@media (min-width: ${breakpointMap[label]}px)`;
    return accumulator;
}, {} as { [key in keyof BreakpointMap]: string });
