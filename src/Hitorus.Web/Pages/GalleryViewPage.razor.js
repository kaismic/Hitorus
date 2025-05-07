const MAIN_CONTAINER_ID = "main-container";
const IMAGE_CONTAINER_ID = "image-container";
const RESIZE_DEBOUNCE = 500; // in milliseconds
const CONTINUOUS_SCROLL_INTERVAL = 24; // in milliseconds
let _scrollSpeed = 1; // in pixels
let _pageTurnInterval = 8; // in seconds
let _viewMode = 0;
let _autoScrollMode = 0;
let _loop = false;
let _intervalId = null;
let _dotNetObject = null;
let _cumulativeHeights = [];
let _timeRecord = { last: Date.now() };
let _pageIndex = null;


/**
 * @typedef {Object} TimeRecord
 * @property {number} last
 */

/**
 * 
 * @param {Function} func - function to be called
 * @param {number} debounce - debounce time in milliseconds
 * @param {TimeRecord} timeRecordObj
 */
function debounce(func, debounce, timeRecordObj) {
    timeRecordObj.last = Date.now();
    setTimeout(() => {
        if (Date.now() - timeRecordObj.last >= debounce) {
            func();
        }
    }, debounce);
}

let initIntervalId = setInterval(() => {
    const imageContainer = document.getElementById(IMAGE_CONTAINER_ID);
    if (imageContainer) {
        // if imageContainer is loaded, then mainContainer is also loaded since mainContainer is parent of imageContainer
        const mainContainer = document.getElementById(MAIN_CONTAINER_ID);
        mainContainer.addEventListener("scroll", () => { onScroll() });
        new ResizeObserver(() => { debounce(setCumulativeHeights, RESIZE_DEBOUNCE, _timeRecord) }).observe(imageContainer);
        clearInterval(initIntervalId);
        setCumulativeHeights();
    }
}, 1000)

function setCumulativeHeights() {
    if (_viewMode !== 1) {
        return;
    }
    const imageContainer = document.getElementById(IMAGE_CONTAINER_ID);
    if (!imageContainer) {
        return;
    }
    _cumulativeHeights = [imageContainer.children[0].clientHeight];
    for (let i = 1; i < imageContainer.childElementCount; i++) {
        _cumulativeHeights[i] = _cumulativeHeights[i - 1] + imageContainer.children[i].clientHeight;
    }
}

function onScroll() {
    if (_viewMode !== 1) {
        return;
    }
    const current = getCurrentPageIndex();
    if (current !== _pageIndex) {
        _pageIndex = current;
        _dotNetObject.invokeMethodAsync("SetPageNumberFromJs", _pageIndex + 1);
    }
}

export function init(viewConfiguration) {
    _viewMode = viewConfiguration.viewMode;
    _autoScrollMode = viewConfiguration.autoScrollMode;
    _scrollSpeed = viewConfiguration.scrollSpeed;
    _pageTurnInterval = viewConfiguration.pageTurnInterval;
    _loop = viewConfiguration.loop;
}

export function setDotNetObject(value) { _dotNetObject = value; }
export function setViewMode(value) {
    _viewMode = value;
    if (value === 0) {
        // reset page index to prevent smooth scrolling on next scrollToIndex which will be invoked by viewMode change
        _pageIndex = null;
    }
}
export function setAutoScrollMode(value) { _autoScrollMode = value; }
export function setPageTurnInterval(value) {
    _pageTurnInterval = value;
    if (_intervalId) {
        startAutoScroll();
    }
}
export function setScrollSpeed(value) { _scrollSpeed = value; }
export function setLoop(value) { _loop = value; }

export function startAutoScroll() {
    stopAutoScroll();
    switch (_autoScrollMode) {
        // continuous
        case 0:
            _intervalId = setInterval(
                () => {
                    scrollBy(_scrollSpeed);
                },
                CONTINUOUS_SCROLL_INTERVAL
            );
            break;
        // by page
        case 1:
            _intervalId = setInterval(
                () => {
                    scrollToNextPage();
                },
                _pageTurnInterval * 1000
            );
            break;
    }
}

export function stopAutoScroll() {
    if (_intervalId) {
        clearInterval(_intervalId);
        _intervalId = null;
    }
}

/**
 * 
 * @param {number} delta
 */
function scrollBy(delta) {
    const mainContainer = document.getElementById(MAIN_CONTAINER_ID);
    // copied from: https://developer.mozilla.org/en-US/docs/Web/API/Element/scrollHeight#determine_if_an_element_has_been_totally_scrolled
    const scrollReachedBottom = Math.abs(mainContainer.scrollHeight - mainContainer.clientHeight - mainContainer.scrollTop) <= 1;
    if (scrollReachedBottom) {
        if (_loop) {
            mainContainer.scrollTop = 0;
        } else {
            stopAutoScroll();
            _dotNetObject.invokeMethodAsync("OnAutoScrollStop");
        }
    } else {
        mainContainer.scrollBy(0, delta);
    }
}

/**
 * @returns {number}
 */
function getCurrentPageIndex() {
    const mainContainer = document.getElementById(MAIN_CONTAINER_ID);
    if (!mainContainer) {
        return 0;
    }
    const centeredScrollTop = mainContainer.scrollTop + (mainContainer.clientHeight / 2);
    for (let i = 0; i < _cumulativeHeights.length; i++) {
        if (centeredScrollTop < _cumulativeHeights[i]) {
            return i;
        }
    }
    return 0;
}

function scrollToNextPage() {
    const nextIndex = getCurrentPageIndex() + 1;
    const imageContainer = document.getElementById(IMAGE_CONTAINER_ID);
    if (nextIndex >= imageContainer.childElementCount) {
        if (_loop) {
            const mainContainer = document.getElementById(MAIN_CONTAINER_ID);
            mainContainer.scrollTo(0, 0);
        } else {
            stopAutoScroll();
            _dotNetObject.invokeMethodAsync("OnAutoScrollStop");
        }
    } else {
        scrollToIndex(nextIndex);
    }
}

export function scrollToIndex(index) {
    const imageContainer = document.getElementById(IMAGE_CONTAINER_ID);
    if (index < 0 || index >= imageContainer.childElementCount) {
        return;
    }
    const item = imageContainer.children[index];
    if (item) {
        const b = _pageIndex === null ? "instant" : Math.abs(_pageIndex - index) <= 1 ? "smooth" : "instant";
        item.scrollIntoView({ behavior: b, block: "center" });
    }
}