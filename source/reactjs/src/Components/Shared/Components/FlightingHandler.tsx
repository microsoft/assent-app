import * as React from 'react';
import { Coachmark, TeachingBubbleContent, DirectionalHint } from '@fluentui/react';
import { useBoolean } from '@fluentui/react-hooks';
import { IEmployeeExperienceContext } from '@micro-frontend-react/employee-experience/lib/IEmployeeExperienceContext';
import { Context } from '@micro-frontend-react/employee-experience/lib/Context';
import {
    getIsFeatureEnabledForUser,
    getQuickTourData,
    getItemsRead,
    getIsTeachingCoachEnabledForFeature,
} from '../SharedComponents.selectors';
import { postQuickTourInfo } from '../SharedComponents.actions';
import { IQuickTourListItem, IComponentsAppState } from '../SharedComponents.types';

type CoachmarkTarget = string | HTMLElement | React.RefObject<HTMLElement> | null;

/**
 * Declarative one-time teaching coachmark attached to a flighted feature. Supplied
 * to `FlightingHandler` via the `coachmark` prop: the handler already owns the
 * feature name and flighting, and the flighting row owns the TeachingCoach flag,
 * so the caller only provides what the data can't — where to anchor and what copy
 * to show.
 */
export interface IFlightingCoachmark {
    /** Anchor the bubble points at: a CSS selector, a DOM element, or a ref. */
    target: CoachmarkTarget;
    /** Bold headline of the teaching bubble. */
    headline: string;
    /** Body content of the teaching bubble. */
    content: React.ReactNode;
    /**
     * Extra visibility gate for layout conditions the handler can't infer — e.g.
     * the anchor element only renders at certain breakpoints. Defaults to true.
     * Flighting, the TeachingCoach flag, and one-time persistence are always
     * enforced regardless.
     */
    when?: boolean;
    /** Direction the bubble opens relative to the target. Defaults to bottomCenter. */
    directionalHint?: DirectionalHint;
    /** Delay before the coachmark appears once eligible. Defaults to 1000ms. */
    delayMs?: number;
    /** Screen-reader alert announced when the coachmark appears. */
    ariaAlertText?: string;
    /** Label for the primary confirm button that dismisses the coachmark. Defaults to 'Got it'. */
    primaryButtonText?: string;
}

/**
 * Props for the standalone `FeatureCoachmark` component. Identical to the
 * `coachmark` object accepted by `FlightingHandler`, plus the `featureName` the
 * handler would otherwise supply.
 */
export interface IFeatureCoachmarkProps extends IFlightingCoachmark {
    /** Feature whose flighting + TeachingCoach flag + unread state gate this coachmark. */
    featureName: string;
}

export interface IFlightingHandlerProps {
    featureName: string;
    children?: React.ReactNode;
    /**
     * Optional one-time teaching coachmark for this feature. Renders only when the
     * feature is flighted, its FlightingFeature row has TeachingCoach=true, and it
     * is still unread — persisting dismissal so it never shows again. Adding it is
     * entirely optional: a feature can be flighted with no coachmark.
     */
    coachmark?: IFlightingCoachmark;
}

/**
 * Gates its children on whether the given feature is flighted for the current
 * user, and optionally attaches a one-time teaching coachmark for that same
 * feature (see the `coachmark` prop). This is the single entry point for both
 * flighting a feature and teaching it — the coachmark decision is owned by the
 * flighting data (TeachingCoach), not by hardcoded ids.
 */
function FlightingHandler({ featureName, children, coachmark }: IFlightingHandlerProps): React.ReactElement | null {
    const isShown = useFlighting(featureName);
    if (!isShown) {
        return null;
    }
    return (
        <>
            {children}
            {coachmark && <FeatureCoachmark featureName={featureName} {...coachmark} />}
        </>
    );
}

export function useFlighting(featureName: string): boolean {
    const { useSelector } = React.useContext(Context as React.Context<IEmployeeExperienceContext>);
    return useSelector((state: any) => getIsFeatureEnabledForUser(state, featureName));
}

export interface IFeatureCoachmarkState {
    /**
     * True when the feature is flighted to the user, is marked as a coachmark in
     * the flighting data (TeachingCoach=true), AND is still unread. The single
     * signal to decide whether to surface any one-time teaching UI for the feature.
     */
    isActive: boolean;
    /** The resolved quick-tour item backing the teaching coach, if any. */
    tourItem?: IQuickTourListItem;
    /**
     * Persist the acknowledgement so the teaching coach never shows again for this
     * user (across devices), via the shared server-backed quick-tour read list.
     * Safe to call even when inactive (no-op).
     */
    dismiss: () => void;
}

/**
 * Headless hook that answers "should I teach this feature to this user right now,
 * and how do I mark it seen?" — driven by the flighting data, which owns the
 * per-feature TeachingCoach flag (projected into allFlightingData alongside
 * FlightingStatus). No tour ids are maintained in code.
 *
 * Eligibility is the conjunction of three signals, each from its natural owner:
 *   1. `useFlighting(featureName)` — is the feature on for this user?
 *   2. `getIsTeachingCoachEnabledForFeature` — does the flighting row mark this
 *      feature as a coachmark (TeachingCoach=true)? This authoritatively separates
 *      coachmark features from slide-based quick-tour features.
 *   3. a matching item in `getQuickTourData` — the quick-tour feed holds only the
 *      *unread* items and supplies the per-user "not yet seen" state plus the tour
 *      id used to persist the acknowledgement (the one thing flighting lacks).
 *
 * Exposed for advanced cases where a team wants to attach a non-coachmark teaching
 * affordance; for the common case pass a `coachmark` to `FlightingHandler`.
 */
export function useFeatureCoachmark(featureName: string): IFeatureCoachmarkState {
    const { useSelector, dispatch } = React.useContext(Context as React.Context<IEmployeeExperienceContext>);
    const isFlighted = useFlighting(featureName);
    const hasTeachingCoach = useSelector((state: IComponentsAppState) =>
        getIsTeachingCoachEnabledForFeature(state, featureName)
    );
    const quickTourData = useSelector(getQuickTourData);
    const readQuickTours = useSelector(getItemsRead);

    const tourItem = React.useMemo(
        () => (quickTourData || []).find((item: IQuickTourListItem) => item.name === featureName),
        [quickTourData, featureName]
    );
    const isActive = isFlighted && hasTeachingCoach && !!tourItem;

    const dismiss = React.useCallback(() => {
        if (!tourItem) {
            return;
        }
        // Backend's QuickTourFeatureList is the full read set (InsertOrReplace),
        // so merge this item's id into the existing server-truth read list rather
        // than posting just [id] which would wipe other acknowledged tours.
        const readIds = readQuickTours ? readQuickTours.map((i: IQuickTourListItem) => i.id.toString()) : [];
        const idStr = tourItem.id.toString();
        if (!readIds.includes(idStr)) {
            readIds.push(idStr);
            const sortedReadIds = readIds.sort((a, b) => parseInt(a) - parseInt(b));
            dispatch(postQuickTourInfo(sortedReadIds));
        }
    }, [readQuickTours, tourItem, dispatch]);

    return { isActive, tourItem, dismiss };
}

function resolveTarget(target: CoachmarkTarget): string | HTMLElement | null {
    if (!target) {
        return null;
    }
    if (typeof target === 'string') {
        return target;
    }
    if (target instanceof HTMLElement) {
        return target;
    }
    return target.current ?? null;
}

/**
 * Reusable one-time teaching coachmark for a flighted feature. Two ways to use it:
 *   1. Preferred — pass a `coachmark` object to `FlightingHandler`. For features
 *      that already gate their UI with the handler, this attaches the coachmark
 *      without a second declaration; the handler renders this for you.
 *   2. Standalone — render `<FeatureCoachmark featureName=… target=… />` directly.
 *      Only needed when a feature flights via the `useFlighting` hook or the
 *      selector, so there is no `FlightingHandler` in its tree to hang it off.
 * Both paths share the same eligibility (flighted + TeachingCoach + unread) and the
 * same one-time persistence, so behaviour is identical however it is declared.
 */
export function FeatureCoachmark(props: IFeatureCoachmarkProps): React.ReactElement | null {
    const {
        featureName,
        target,
        headline,
        content,
        when = true,
        directionalHint = DirectionalHint.bottomCenter,
        delayMs = 1000,
        ariaAlertText = 'A coachmark has appeared',
        primaryButtonText = 'Got it',
    } = props;

    const { isActive, dismiss } = useFeatureCoachmark(featureName);
    const [isVisible, { setTrue: show, setFalse: hide }] = useBoolean(false);

    const shouldShow = when && isActive;

    React.useEffect(() => {
        if (!shouldShow) {
            // Eligibility (or the caller's `when`) turned off while mounted — make sure a
            // coachmark that was already shown is dismissed rather than lingering on screen.
            hide();
            return undefined;
        }
        const timer = setTimeout(() => show(), delayMs);
        return () => clearTimeout(timer);
    }, [shouldShow, delayMs, show, hide]);

    const handleDismiss = React.useCallback(() => {
        hide();
        dismiss();
    }, [hide, dismiss]);

    const resolvedTarget = resolveTarget(target);
    if (!isVisible || !resolvedTarget) {
        return null;
    }

    return (
        <Coachmark
            target={resolvedTarget}
            positioningContainerProps={{ directionalHint, directionalHintFixed: true, doNotLayer: false }}
            ariaAlertText={ariaAlertText}
            ariaDescribedBy={`feature-coachmark-desc-${featureName}`}
            ariaLabelledBy={`feature-coachmark-label-${featureName}`}
            ariaDescribedByText="Press enter or alt + C to open the coachmark notification"
            ariaLabelledByText="Coachmark notification"
        >
            <TeachingBubbleContent
                headline={headline}
                hasCloseButton
                closeButtonAriaLabel="Close"
                onDismiss={handleDismiss}
                primaryButtonProps={{ children: primaryButtonText, onClick: handleDismiss }}
                ariaDescribedBy={`feature-coachmark-content-desc-${featureName}`}
                ariaLabelledBy={`feature-coachmark-content-label-${featureName}`}
            >
                {content}
            </TeachingBubbleContent>
        </Coachmark>
    );
}

export default FlightingHandler;
