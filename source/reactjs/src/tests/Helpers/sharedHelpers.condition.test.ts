import { validateConditionClient, validateCondition } from '../../Helpers/sharedHelpers';

// Security regression tests: injected values (e.g. `a"-attackMarker()-"a`) must stay inert literals, never execute.
describe('sharedHelpers condition DSL security', () => {
    // Marker that only a code break-out would call; must never be invoked.
    const attackMarker = jest.fn();

    beforeEach(() => {
        attackMarker.mockClear();
        (globalThis as any).attackMarker = attackMarker;
    });

    afterAll(() => {
        delete (globalThis as any).attackMarker;
    });

    // A payload that closes the string literal, injects a call, and reopens it.
    const INJECTION = 'a"-attackMarker()-"a';

    describe('validateConditionClient', () => {
        it('evaluates a matching condition to true', () => {
            expect(validateConditionClient({ status: 'Approved' }, 'Status == "Approved"')).toBe(true);
        });

        it('evaluates a non-matching condition to false', () => {
            expect(validateConditionClient({ status: 'Rejected' }, 'Status == "Approved"')).toBe(false);
        });

        it('treats an injected value as a literal string and never executes it', () => {
            let result: boolean | undefined;
            expect(() => {
                result = validateConditionClient({ status: INJECTION }, 'Status == "Approved"');
            }).not.toThrow();
            expect(result).toBe(false);
            expect(attackMarker).not.toHaveBeenCalled();
        });
    });

    describe('validateCondition', () => {
        it('returns true when there is no condition', () => {
            expect(validateCondition({}, '')).toBe(true);
        });

        it('evaluates a _client condition to true when matching', () => {
            expect(validateCondition({ status: 'Approved' }, '_client^Status == "Approved"')).toBe(true);
        });

        it('evaluates a _client condition to false when not matching', () => {
            expect(validateCondition({ status: 'Rejected' }, '_client^Status == "Approved"')).toBe(false);
        });

        it('treats an injected value as a literal string and never executes it', () => {
            let result: boolean | undefined;
            expect(() => {
                result = validateCondition({ status: INJECTION }, '_client^Status == "Approved"');
            }).not.toThrow();
            expect(result).toBe(false);
            expect(attackMarker).not.toHaveBeenCalled();
        });
    });

    describe('evaluator security hardening', () => {
        // Member access is limited to a read-only allowlist; constructor/prototype breakouts must throw, never execute.
        it('blocks constructor breakout attempts without executing them', () => {
            expect(() =>
                validateConditionClient({ status: '"".constructor("attackMarker()")()' }, 'Status == "x"')
            ).not.toThrow();
            expect(attackMarker).not.toHaveBeenCalled();
        });
    });
});
