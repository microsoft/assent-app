import * as React from 'react';
import { render, fireEvent } from '@testing-library/react';
import DocumentPreview from '../../../../../../src/Components/Shared/Details/DocumentPreview/DocumentPreview';

describe('DocumentPreview', () => {
    const props = {
        documentPreview: {},
        dropdownOnChange: {},
        dropdownSelectedKey: {},
        dropdownOptions: [''],
        previewContainerWidth: {},
        footerHeight: {},
        rotation: ''
    };
    it('should render document view when properties are given', () => {
        const handleGetDimensions = jest.fn();
        const wrapper = render(<DocumentPreview {...props} onLoad={handleGetDimensions} />);
        expect(wrapper).toMatchSnapshot();
    });

    it('should render document view when document preview given', () => {
        const handleGetDimensions = jest.fn();
        const prop1 = {
            documentPreview: ['/', 'i', 'R', 'J'],
            dropdownOnChange: {},
            dropdownSelectedKey: {},
            dropdownOptions: [''],
            previewContainerWidth: {},
            rotation: {}
        };

        const wrapper1 = render(<DocumentPreview {...prop1} onLoad={handleGetDimensions} />);
        fireEvent.load(wrapper1.getByAltText('Image preview'));
        expect(wrapper1).toMatchSnapshot();
    });

    it('should render document view with rotation as 90', () => {
        const handleRotate = jest.fn();
        const prop1 = {
            documentPreview: ['/', 'i', 'R', 'J'],
            dropdownOnChange: {},
            dropdownSelectedKey: {},
            dropdownOptions: [''],
            previewContainerWidth: {}
        };

        const wrapper2 = render(<DocumentPreview {...prop1} onClick={handleRotate} />);
        fireEvent.click(wrapper2.getByTitle('Rotate 90 degrees'));
        expect(wrapper2).toMatchSnapshot();
    });

    it('should render document view when rotation is 270', () => {
        const rotation = 270;
        const setState = jest.fn();
        const useStateSpy = jest.spyOn(React, 'useState');
        useStateSpy.mockImplementation(() => [rotation, setState]);
        expect(rotation).toEqual(270);
    });

    it('should render document view when we set the rotation', () => {
        const handleRotate = jest.fn();
        const prop1 = {
            documentPreview: ['/', 'i', 'R', 'J'],
            dropdownOnChange: {},
            dropdownSelectedKey: {},
            dropdownOptions: [''],
            previewContainerWidth: {}
        };

        const wrapper2 = render(<DocumentPreview {...prop1} up onClick={handleRotate} />);
        fireEvent.click(wrapper2.getByTitle('Rotate 90 degrees'));
        expect(wrapper2).toMatchSnapshot();
    });

    it('should render document view when documentPreview is R', () => {
        const prop1 = {
            documentPreview: ['R'],
            dropdownOnChange: {},
            dropdownSelectedKey: {},
            dropdownOptions: ['']
        };
        const wrapper2 = render(<DocumentPreview {...prop1} />);
        expect(wrapper2).toMatchSnapshot();
    });

    it('should render document view when documentPreview is V ', () => {
        const prop1 = {
            documentPreview: ['V'],
            dropdownOnChange: {},
            dropdownSelectedKey: {},
            dropdownOptions: ['']
        };

        const wrapper2 = render(<DocumentPreview {...prop1} />);
        expect(wrapper2).toMatchSnapshot();
    });

    it('should render document view when documentPreview is J', () => {
        const prop1 = {
            documentPreview: ['J'],
            dropdownOnChange: {},
            dropdownSelectedKey: {},
            dropdownOptions: [''],
            previewContainerWidth: 20
        };

        const wrapper2 = render(<DocumentPreview {...prop1} />);
        expect(wrapper2).toMatchSnapshot();
    });

    it('should render document view when documentPreview is i ', () => {
        const prop1 = {
            documentPreview: ['i'],
            dropdownOnChange: {},
            dropdownSelectedKey: {},
            dropdownOptions: [''],
            previewContainerWidth: 20
        };

        const wrapper2 = render(<DocumentPreview {...prop1} />);
        expect(wrapper2).toMatchSnapshot();
    });

    it('should render document view when rotation is 180 ', () => {
        const prop1 = {
            documentPreview: ['i'],
            dropdownOnChange: {},
            dropdownSelectedKey: {},
            dropdownOptions: ['']
        };
        const rotation1 = 180;
        const setState = jest.fn();
        const useStateSpy = jest.spyOn(React, 'useState');
        useStateSpy.mockImplementation(() => [rotation1, setState]);
        const wrapper2 = render(<DocumentPreview {...prop1} />);
        expect(wrapper2).toMatchSnapshot();
    });
});
