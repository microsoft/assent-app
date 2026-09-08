import * as React from 'react';
import { Text } from '@fluentui/react/lib/Text';

const FilterTag = styled.div`
    display: inline-flex;
    align-items: center;
    background: #0078d4;
    color: white;
    padding: 4px 12px;
    border-radius: 16px;
    font-size: 12px;
    font-weight: 500;
    margin-right: 8px;
    margin-bottom: 4px;
    cursor: pointer;
    transition: all 0.2s ease;

    &:hover {
        background: #106ebe;
        transform: scale(1.02);
    }

    &:active {
        transform: scale(0.98);
    }
`;

const FilterTagIcon = styled.span`
    margin-left: 6px;
    font-size: 10px;
    cursor: pointer;
    opacity: 0.8;
    transition: opacity 0.2s ease;

    &:hover {
        opacity: 1;
    }
`;

const FilterTagsContainer = styled.div`
    display: flex;
    align-items: center;
    flex-wrap: wrap;
    gap: 4px;
    min-height: 24px;
`;

const FilterTagsWrapper = styled.div`
    display: flex;
    align-items: center;
    gap: 16px;
    flex-wrap: wrap;
`;

export interface IFilterTag {
    key: string;
    label: string;
    type?: 'filter' | 'category';
}

interface IFilterTagsProps {
    tags: IFilterTag[];
    onRemoveTag: (tagKey: string) => void;
    showLabel?: boolean;
    labelText?: string;
}

export const FilterTags: React.FunctionComponent<IFilterTagsProps> = ({
    tags,
    onRemoveTag,
    showLabel = true,
    labelText = 'Active Filters:',
}) => {
    if (tags.length === 0) {
        return null;
    }

    return (
        <FilterTagsWrapper>
            <FilterTagsContainer>
                {showLabel && (
                    <Text
                        variant="small"
                        styles={{
                            root: {
                                color: '#616161',
                                marginRight: '8px',
                                fontWeight: '500',
                            },
                        }}
                    >
                        {labelText}
                    </Text>
                )}
                {tags.map((tag) => (
                    <FilterTag key={tag.key} onClick={() => onRemoveTag(tag.key)}>
                        {tag.label}
                        <FilterTagIcon>×</FilterTagIcon>
                    </FilterTag>
                ))}
            </FilterTagsContainer>
        </FilterTagsWrapper>
    );
};