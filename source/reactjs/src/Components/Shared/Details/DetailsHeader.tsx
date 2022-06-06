import * as React from 'react';
import { Adaptive } from './Adaptive';
import { DetailsType } from './Details.types';

interface IDetailsHeaderProps {
    isPullModelEnabled: boolean;
    headerTemplateJSON: any;
    headerJSON: any;
    onOpenURLActionExecuted: any;
    onSubmitActionExecuted: any;
    userAlias: string;
    shouldDetailReRender: boolean;
    summaryDataMapping: string | null;
}

export function DetailsWrapper(props: IDetailsHeaderProps): React.ReactElement {
    const {
        headerTemplateJSON,
        headerJSON,
        onOpenURLActionExecuted,
        onSubmitActionExecuted,
        userAlias,
        shouldDetailReRender,
        isPullModelEnabled,
        summaryDataMapping
    } = props;

    const headerDataJSON = isPullModelEnabled ? {} : headerJSON;

    return (
        <Adaptive
            template={headerTemplateJSON}
            dataPayload={headerDataJSON}
            onOpenURLActionExecuted={onOpenURLActionExecuted}
            onSubmitActionExecuted={onSubmitActionExecuted}
            userAlias={userAlias}
            shouldDetailReRender={shouldDetailReRender}
        />
    );
}
