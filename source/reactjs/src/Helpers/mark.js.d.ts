declare module 'mark.js' {
    interface MarkOptions {
        element?: string;
        className?: string;
        exclude?: string[];
        separateWordSearch?: boolean;
        accuracy?: 'partially' | 'complementary' | 'exactly';
        caseSensitive?: boolean;
        acrossElements?: boolean;
        each?: (element: HTMLElement) => void;
        done?: (totalMarks: number) => void;
    }

    interface UnmarkOptions {
        element?: string;
        className?: string;
        exclude?: string[];
        done?: () => void;
    }

    class Mark {
        constructor(context: HTMLElement | HTMLElement[] | NodeList | string);
        mark(keyword: string | string[], options?: MarkOptions): void;
        unmark(options?: UnmarkOptions): void;
    }

    export = Mark;
}
