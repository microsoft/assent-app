import { ISharedComponentsPersistentState } from './SharedComponents.types';
import { SharedComponentsAction, SharedComponentsActionType } from './SharedComponents.action-types';
import { teachingSteps, initialTeachingStep } from './Components/FeaturesIntroductionSteps';

export const sharedComponentsPersistentReducerName = 'SharedComponentsPersistentReducer';

export const SharedComponentsPersistentInitialState: ISharedComponentsPersistentState = {
    userName: '',
    userAlias: '',
    teachingBubbleVisibility: false,
    teachingBubbleStep: teachingSteps[initialTeachingStep],
};

export function sharedComponentsPersistentReducer(
    prev: ISharedComponentsPersistentState = SharedComponentsPersistentInitialState,
    action: SharedComponentsAction
): ISharedComponentsPersistentState {
    switch (action.type) {
        case SharedComponentsActionType.UPDATE_USER_ALIAS:
            return {
                ...prev,
                userAlias: action.userAlias,
                userName: action.userName,
            };
        case SharedComponentsActionType.TOGGLE_TEACHING_BUBBLE_VISIBILITY:
            return {
                ...prev,
                teachingBubbleVisibility: !prev.teachingBubbleVisibility,
                teachingBubbleStep: teachingSteps[initialTeachingStep],
            };
        case SharedComponentsActionType.UPDATE_TEACHING_STEP:
            return {
                ...prev,
                teachingBubbleStep: action.newStep,
            };
        default:
            return prev;
    }
}
