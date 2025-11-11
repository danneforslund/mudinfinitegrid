window.scrollHelper = {
    attachScrollListener: function (elementId, dotNetRef) {
        const element = document.getElementById(elementId).querySelector('.mud-table-container');
        if (!element) return;

        let isHandling = false;

        async function onScroll() {
            const bottomReached = element.scrollTop > 0 &&
                element.scrollTop + element.clientHeight >= element.scrollHeight - 1;
            if (bottomReached && !isHandling) {
                isHandling = true;
                dotNetRef.invokeMethodAsync('NotifyScrollBottomAsync');
                setTimeout(() => { isHandling = false; }, 200);
            }
        }

        element.addEventListener('scroll', onScroll);
        element._scrollHandler = onScroll;
    },

    removeScrollListener: function (elementId) {
        const element = document.getElementById(elementId).querySelector('.mud-table-container');
        if (element && element._scrollHandler) {
            element.removeEventListener('scroll', element._scrollHandler);
            delete element._scrollHandler;
        }
    }
};
