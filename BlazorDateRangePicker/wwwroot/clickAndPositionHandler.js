/**
* @author: Sergey Zaikin zaikinsr@yandex.ru
* @copyright: Copyright (c) 2019 Sergey Zaikin. All rights reserved.
* @license: Licensed under the MIT license. See http://www.opensource.org/licenses/mit-license.php
*/
window.clickAndPositionHandler = {
    listeners: [],
    addClickOutsideEvent: function (elementId, parentId, dotnetHelper) {
        if (window.clickAndPositionHandler.listeners.indexOf(elementId) > -1) return;
        window.addEventListener("click", function (e) {
            if ((document.getElementById(elementId) != null && !document.getElementById(elementId).contains(e.target)) &&
                (document.getElementById(parentId) != null && !document.getElementById(parentId).contains(e.target)) &&
                (document.getElementById(elementId).parentElement == null ||
                    document.getElementById(elementId).parentElement.style == null ||
                    document.getElementById(elementId).parentElement.style.visibility == "visible")) {
                dotnetHelper.invokeMethodAsync("InvokeClickOutside");
            }
        });
        window.clickAndPositionHandler.listeners.push(elementId);
    },
    getPickerPosition: function (elementId, parentId, drops, opens, skipAddListener) {
        var resizeFunction = function () {
            window.clickAndPositionHandler.getPickerPosition(elementId, parentId, drops, opens, true);
        };
        var parentOffset = { top: 0, left: 0 },
            containerTop;
        var parentRightEdge = window.innerWidth;
        var container = document.getElementById(elementId);
        var parentEl = document.getElementById(parentId);
        var element = parentEl;
        if (element == null || container == null) {
            window.removeEventListener('resize', resizeFunction, true);
            window.removeEventListener('onwheel', resizeFunction, true);
            window.removeEventListener('onmousewheel', resizeFunction, true);
            window.removeEventListener('scroll', resizeFunction, true);
            return;
        }

        if (parentEl === document.body) {
            var rect = parentEl.getBoundingClientRect();
            parentOffset = {
                top: rect.top,
                left: rect.left
            };

            parentRightEdge = parentEl[0].clientWidth + rect.left;
        }

        var elementRect = element.getBoundingClientRect();

        var outerHeight = function (el) {
            return el.offsetHeight;
        }

        var outerWidth = function (el) {
            return el.offsetWidth;
        }

        var setStylesOnElement = function (styles, element) {
            for (var prop in styles) {
                element.style[prop] = styles[prop];
            }
        }

        if (drops == 'up')
            containerTop = elementRect.top - outerHeight(container) - parentOffset.top;
        else
            containerTop = elementRect.top + outerHeight(element) - parentOffset.top;

        var containerWidth = outerWidth(container);

        if (opens == 'left') {
            var containerRight = parentRightEdge - elementRect.left - outerWidth(element);
            if (containerWidth + containerRight > window.innerWidth) {
                setStylesOnElement({
                    position: 'fixed',
                    top: containerTop + 'px',
                    right: 'auto',
                    left: 9 + 'px'
                }, container);
            } else {
                setStylesOnElement({
                    position: 'fixed',
                    top: containerTop + 'px',
                    right: containerRight + 'px',
                    left: 'auto'
                }, container);
            }
        } else if (opens == 'center') {
            var containerLeft = elementRect.left - parentOffset.left + outerWidth(element) / 2 - containerWidth / 2;
            if (containerLeft < 0) {
                setStylesOnElement({
                    position: 'fixed',
                    top: containerTop + 'px',
                    right: 'auto',
                    left: 9 + 'px'
                }, container);
            } else if (containerLeft + containerWidth > window.innerWidth) {
                setStylesOnElement({
                    position: 'fixed',
                    top: containerTop + 'px',
                    left: 'auto',
                    right: 0 + 'px'
                }, container);
            } else {
                setStylesOnElement({
                    position: 'fixed',
                    top: containerTop + 'px',
                    left: containerLeft + 'px',
                    right: 'auto'
                }, container);
            }
        } else {
            var containerLeft = elementRect.left - parentOffset.left;
            if (containerLeft + containerWidth > window.innerWidth) {
                setStylesOnElement({
                    position: 'fixed',
                    top: containerTop + 'px',
                    left: 'auto',
                    right: 0 + 'px'
                }, container);
            } else {
                setStylesOnElement({
                    position: 'fixed',
                    top: containerTop + 'px',
                    left: containerLeft + 'px',
                    right: 'auto'
                }, container);
            }
        }

        if (skipAddListener !== true) {
            window.addEventListener('resize', resizeFunction, true);
            window.addEventListener('onwheel', resizeFunction, true);
            window.addEventListener('onmousewheel', resizeFunction, true);
            window.addEventListener('scroll', resizeFunction, true);
        };
    }
};