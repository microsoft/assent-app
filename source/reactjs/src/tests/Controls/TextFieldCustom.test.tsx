import * as React from 'react';
import { render } from '@testing-library/react';
import TextFieldCustom from '../../Controls/TextFieldCustom';

describe("Lob App - TextFieldCustom", () => {
    it("will render TextFieldCustom", () => {
        let onSAPCostChange = (newValue: boolean) => { }
        let errorMessage = "";
        let editSAPCost = "";
        let iconProps = { iconName: 'CheckMark' };
        const wrapper = render(
            <TextFieldCustom
                onChange={onSAPCostChange}
                errorMessage={errorMessage}
                value={editSAPCost}
                ariaLabel="Enter new SAP Cost Object value"
                iconProps={iconProps}
            />
        )
        expect(wrapper).toMatchSnapshot();
    });
});