// styled is a global (webpack externals) — no import needed

export const DropdownContainer = styled.div`
    position: absolute;
    top: 100%;
    left: 0;
    width: 100%;
    z-index: 1000;
    background-color: #fff;
    border: 1px solid #e0e0e0;
    border-top: none;
    box-shadow: 0 8px 24px rgba(0, 0, 0, 0.14);
    border-radius: 0 0 6px 6px;
    max-height: 400px;
    overflow-y: auto;
    padding: 4px 0;
`;

export const SectionHeader = styled.div`
    padding: 8px 14px 4px;
    font-size: 11px;
    font-weight: 600;
    color: #707070;
    text-transform: uppercase;
    letter-spacing: 0.5px;
`;

export const Separator = styled.div`
    height: 1px;
    background-color: #ededed;
    margin: 4px 0;
`;

export const TermItem = styled.div<{ isActive: boolean }>`
    display: flex;
    align-items: center;
    padding: 7px 14px;
    cursor: pointer;
    font-size: 14px;
    color: #242424;
    border-left: 3px solid transparent;
    background: ${(props) => (props.isActive ? '#f0f6ff' : 'transparent')};
    border-left-color: ${(props) => (props.isActive ? '#0078d4' : 'transparent')};

    &:hover {
        background: ${(props) => (props.isActive ? '#f0f6ff' : '#f5f5f5')};
    }
`;

export const RequestItem = styled.div<{ isActive: boolean }>`
    display: flex;
    align-items: flex-start;
    padding: 8px 14px;
    cursor: pointer;
    border-left: 3px solid transparent;
    background: ${(props) => (props.isActive ? '#f0f6ff' : 'transparent')};
    border-left-color: ${(props) => (props.isActive ? '#0078d4' : 'transparent')};

    &:hover {
        background: ${(props) => (props.isActive ? '#f0f6ff' : '#f5f5f5')};
    }
`;

export const RequestTitle = styled.div`
    font-size: 14px;
    color: #242424;
    font-weight: 500;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
`;

export const RequestSubtitle = styled.div`
    font-size: 12px;
    color: #707070;
    margin-top: 2px;
`;

export const FooterHint = styled.div`
    padding: 6px 14px;
    font-size: 11px;
    color: #6e6e6e;
    border-top: 1px solid #f0f0f0;
    background: #fafafa;
`;

export const HighlightMatch = styled.strong`
    font-weight: 600;
`;

const shimmerPulse = `
    @keyframes shimmerPulse {
        0%, 100% { opacity: 0.4; }
        50% { opacity: 1; }
    }
`;

export const ShimmerBar = styled.div`
    height: 14px;
    background: #e8e8e8;
    border-radius: 4px;
    margin: 10px 14px;
    animation: shimmerPulse 1.5s ease-in-out infinite;

    ${shimmerPulse}

    &:nth-child(2) {
        width: 60%;
        animation-delay: 0.2s;
    }

    &:nth-child(3) {
        width: 45%;
        animation-delay: 0.4s;
    }
`;

export const ScreenReaderOnly = styled.div`
    position: absolute;
    width: 1px;
    height: 1px;
    overflow: hidden;
    clip: rect(0, 0, 0, 0);
`;
