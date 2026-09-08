import {
    validateConditionClient,
    validateCondition,
    validateBulkCondition,
    validateProofOfPresenceCondition,
} from '../../Helpers/sharedHelpers';

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

        it('treats a missing property as the legacy undefined string sentinel', () => {
            expect(validateCondition({}, "_client^actionDetails != 'undefined'")).toBe(false);
        });

        it('matches the legacy undefined condition when the property is present', () => {
            expect(validateCondition({ actionDetails: 'Anomaly' }, "_client^actionDetails != 'undefined'")).toBe(true);
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

    describe('validateBulkCondition', () => {
        it('returns true when there is no condition', () => {
            expect(validateBulkCondition({}, '')).toBe(true);
        });

        it('evaluates a numeric comparison to true', () => {
            expect(validateBulkCondition({ Amount: 200 }, 'Amount > 100')).toBe(true);
        });

        it('evaluates a numeric comparison to false', () => {
            expect(validateBulkCondition({ Amount: 50 }, 'Amount > 100')).toBe(false);
        });

        it('treats an injected value as a literal string and never executes it', () => {
            let result: boolean | undefined;
            expect(() => {
                result = validateBulkCondition({ Status: INJECTION }, 'Status == "Approved"');
            }).not.toThrow();
            expect(result).toBe(false);
            expect(attackMarker).not.toHaveBeenCalled();
        });
    });

    describe('validateProofOfPresenceCondition', () => {
        it('evaluates a matching condition to true', () => {
            expect(validateProofOfPresenceCondition({ ProofValue: 'Yes' }, 'ProofValue == "Yes"')).toBe(true);
        });

        it('evaluates a non-matching condition to false', () => {
            expect(validateProofOfPresenceCondition({ ProofValue: 'No' }, 'ProofValue == "Yes"')).toBe(false);
        });

        it('treats an injected value as a literal string and never executes it', () => {
            let result: boolean | undefined;
            expect(() => {
                result = validateProofOfPresenceCondition({ ProofValue: INJECTION }, 'ProofValue == "Yes"');
            }).not.toThrow();
            expect(result).toBe(false);
            expect(attackMarker).not.toHaveBeenCalled();
        });
    });

    describe('evaluator operator support', () => {
        it('supports logical AND / OR combinations', () => {
            expect(validateBulkCondition({ Amount: 200, Status: 'Open' }, 'Amount > 100 && Status == "Open"')).toBe(
                true
            );
            expect(validateBulkCondition({ Amount: 50, Status: 'Open' }, 'Amount > 100 || Status == "Open"')).toBe(
                true
            );
        });

        it('supports inequality', () => {
            expect(validateBulkCondition({ Status: 'Open' }, 'Status != "Closed"')).toBe(true);
        });

        // A value that itself looks like a method call must stay an inert literal (never invoked).
        it('treats a method-call-looking value as an inert literal', () => {
            expect(validateBulkCondition({ Status: '"Approved,Rejected".includes("Approved")' }, 'Status == "x"')).toBe(
                false
            );
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

        // Safety net: an unparseable condition must fall back, never throw out of the validators.
        it('never throws on an unparseable condition and falls back', () => {
            let result: boolean | undefined;
            expect(() => {
                result = validateProofOfPresenceCondition({}, 'ProofValue matches /totally invalid grammar');
            }).not.toThrow();
            expect(result).toBe(true);
            expect(attackMarker).not.toHaveBeenCalled();
        });
    });
});
