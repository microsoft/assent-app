import React = require("react");

export const useActiveElement = () => {
    const [active, setActive] = React.useState(document.activeElement);

    const handleFocusIn = () => {
        setActive(document.activeElement);
    }

    React.useEffect(() => {
        document.addEventListener('focusin', handleFocusIn)
        return () => {
        document.removeEventListener('focusin', handleFocusIn)
    };
    }, [])

    return active;
}