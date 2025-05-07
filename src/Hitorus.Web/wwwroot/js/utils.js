/**
 * Assumes target's parent is a flex container with flex-direction: column; OR a grid container with grid-template-rows
 * @param {string} targetValue
 * @param {('id'|'class')} targetValueType
 * @param {string} observeeValue
 * @param {('id'|'class')} observeeValueType
 */
function setFillHeightResizeObserver(targetValue, targetValueType, observeeValue, observeeValueType) {
    // set target's height to fill the remaining space of its parent
    const resizeObserver = new ResizeObserver(
        () => {
            const target = targetValueType == "id" ? document.getElementById(targetValue) : document.getElementsByClassName(targetValue)[0]
            if (!target) {
                return;
            }
            const parent = target.parentNode;
            if (!parent) {
                return;
            }
            const computedStyle = getComputedStyle(parent);
            let targetHeight = parent.clientHeight;
            targetHeight -= parseFloat(computedStyle.paddingTop) + parseFloat(computedStyle.paddingBottom); // remove padding
            const parsedGap = parseInt(computedStyle.gap)
            if (parsedGap) {
                targetHeight -= parsedGap * (parent.children.length - 1) // remove gap
            }
            for (const child of parent.children) {
                if (child !== target) {
                    targetHeight -= child.offsetHeight;
                    targetHeight -= child.marginTop ? child.marginTop : 0;
                    targetHeight -= child.marginBottom ? child.marginBottom : 0;
                }
            }
            target.style.height = targetHeight + "px";
        }
    );
    resizeObserver.observe(observeeValueType == "id" ? document.getElementById(observeeValue) : document.getElementsByClassName(observeeValue)[0]);
}

/**
 * 
 * @param {string} targetValue
 * @param {('id'|'class')} targetValueType
 * @param {string} sourceValue
 * @param {('id'|'class')} sourceValueType
 */
function setHeightToSourceHeight(targetValue, targetValueType, sourceValue, sourceValueType) {
    const target = targetValueType == "id" ? document.getElementById(targetValue) : document.getElementsByClassName(targetValue)[0]
    if (!target) {
        return;
    }
    const source = sourceValueType == "id" ? document.getElementById(sourceValue) : document.getElementsByClassName(sourceValue)[0]
    if (!source) {
        return;
    }
    target.style.height = source.clientHeight + "px";
}

function getClientWidthById(id) {
    const element = document.getElementById(id);
    if (!element) {
        return 0;
    }
    return element.clientWidth;
}

function startDeleteAnimation(elementId, galleryId, dotNetObject) {
    const element = document.getElementById(elementId);
    if (element) {
        const keyframes = [
            { transform: "translateX(0%) scale(1)", opacity: "1", offset: 0 },
            { transform: "translateX(0%) scale(0.8)", opacity: "0.6", offset: 0.1 },
            { transform: "translateX(0%) scale(0.8)", opacity: "0.6", offset: 0.3 },
            { transform: "translateX(400%) scale(0)", opacity: "0", offset: 1 },
        ];
        const animation = element.animate(keyframes, { duration: 2000, fill: "forwards", easing: "ease-out" })
        animation.finished.then(() => {
            dotNetObject.invokeMethodAsync("OnDeleteAnimationFinished", galleryId);
        })
    }
}