/**
 * 
 * @param {string} id
 */
export function scrollToElement(id) {
    const element = document.getElementById(id);
    element.scrollIntoView({ behavior: "smooth", block: "center" });
}