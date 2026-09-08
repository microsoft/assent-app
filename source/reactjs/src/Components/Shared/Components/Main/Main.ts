// Adds space for nav and header
// flex for stick footer
export const Main = styled.main`
    width: 100%;
    flex: 1 0 auto;
    margin-top: 0px;
    background: #f2f2f2;

    @media (min-width: 1025px) {
        width: calc(100% - 48px);
        margin-left: 48px;
    }

    /* At 200% zoom, headers stack taller — make Main fill remaining viewport and scroll */
    @media (max-width: 640px) {
        height: calc(100vh - 100px);
        overflow-y: auto;
        overflow-x: hidden;
    }
`;
