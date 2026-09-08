import * as React from 'react';
import { Adaptive } from './Adaptive';
import { DetailsType } from './Details.types';
import Microfrontend from './Microfrontend';

interface IDetailsWrapperProps {
    detailsComponentType: number;
    detailsTemplateJSON: any;
    combinedDetailsJSON: any;
    onOpenURLActionExecuted: any;
    onSubmitActionExecuted: any;
    onToggleVisibilityActionExecuted: any;
    userAlias: string;
    shouldDetailReRender: boolean;
    tenantId: string;
    executeMicrofrontendActionRef: any;
    dispatchUpdateAdditionalData: any;
    cdnURL: string | null;
    selectedPage: string;
    highlightTerms?: string[];
}

export function DetailsWrapper(props: IDetailsWrapperProps): React.ReactElement {
    const {
        detailsComponentType,
        cdnURL,
        detailsTemplateJSON,
        combinedDetailsJSON,
        onOpenURLActionExecuted,
        onSubmitActionExecuted,
        onToggleVisibilityActionExecuted,
        userAlias,
        shouldDetailReRender,
        tenantId,
        executeMicrofrontendActionRef,
        dispatchUpdateAdditionalData,
        selectedPage,
    } = props;

    const useAdaptiveCard = detailsComponentType === DetailsType.AdaptiveCard;

    return useAdaptiveCard ? (
        <Adaptive
            template={detailsTemplateJSON}
            dataPayload={combinedDetailsJSON}
            onOpenURLActionExecuted={onOpenURLActionExecuted}
            onSubmitActionExecuted={onSubmitActionExecuted}
            onToggleVisibilityActionExecuted={onToggleVisibilityActionExecuted}
            userAlias={userAlias}
            shouldDetailReRender={shouldDetailReRender}
            highlightTerms={props.highlightTerms}
        />
    ) : (
        <Microfrontend
            tenantId={Number(tenantId)}
            executeMicrofrontendActionRef={executeMicrofrontendActionRef}
            dispatchUpdateAdditionalData={dispatchUpdateAdditionalData}
            detailsJSON={combinedDetailsJSON}
            cdnURL={cdnURL}
            selectedPage={selectedPage}
        />
    );
}
