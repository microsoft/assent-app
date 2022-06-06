import { IStackTokens, IStackStyles } from '@fluentui/react/lib/Stack';
import { FontIcon } from '@fluentui/react/lib/Icon';
import { FontWeights, FontSizes, IStyle } from '@fluentui/react';
import { Text } from '@fluentui/react/lib/Text';
import { IMessageBarStyles } from '@fluentui/react/lib/MessageBar';
//uncomment styled import for performance testing
//import styled from "styled-components";

export const WarningViewStackTokensLargeGap: IStackTokens = { childrenGap: 6 };

export const OtherViewsStackTokensGap: IStackTokens = { childrenGap: 3 };

export const WarningViewStackStylesBottomBorder: IStackStyles = {
    root: { borderBottom: `1px solid gray` }
};

export const CollapsibleStyle: IStyle = {
    backgroundColor: '#fff4ce',
    padding: '5px 15px',
};

export const SuccessIcon = styled(FontIcon)`
    margin-top: 20vh;
    margin-bottom: 20px;
    font-size: 58px;
    color: #107c10;
`;

export const SuccessMessage = styled(Text).attrs({
    as: 'p'
})`
    font-size: ${FontSizes.size18};
    font-weight: ${FontWeights.semibold};
`;

export const DetailsMessageBarTitle = styled(Text).attrs({
    as: 'p'
})`
    font-size: ${FontSizes.size16};
    font-weight: ${FontWeights.semibold};
`;

export const ErrorViewStyle: IMessageBarStyles = {
    root: {
        maxHeight: '125px',
        overflow: 'auto',
        selectors: {
            '@media (min-device-width: 1023px) and (min-width: 320px) and (max-width: 639px)': {
                minHeight: '30px',
                height: '30px',
                marginTop:'30px'
            },
            '@media only screen and (min-device-width: 1023px) and (max-width: 380px)': {
                minHeight: '20px',
                height: '20px',
                marginTop:'30px'
            }
        }
    }
};

export const UnorderedList = styled.ul`
    list-style-type: none;
    padding: 0;
    margin: 0;
`;
